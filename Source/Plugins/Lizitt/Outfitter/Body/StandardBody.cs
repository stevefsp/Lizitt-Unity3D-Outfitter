/*
 * Copyright (c) 2015-2016 Stephen A. Pratt
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
using UnityEngine;
using System.Collections.Generic;

namespace com.lizitt.outfitter
{
    /// <summary>
    /// Implements all standard <see cref="Body"/> features.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The body expectes that it will retain control of its outfit until the outfit is removed using
    /// <see cref="SetOutfit(Outfit)"/> or one of its overloads.  If the body detects a loss of control, then it 
    /// will behave as follows:
    /// </para>
    /// <para>
    /// If the outfit bake event is received the body will perform a forced release.
    /// </para>
    /// <para>
    /// If a non-bake outfit destroy event is received, the body will attempt a normal release in order to recover
    /// its assets as best it can.
    /// </para>
    /// <para>
    /// If the body loses ownership of its outfit, or the outfit transtions to the 'stored' or 'unmanaged' state, the
    /// body will perform a normal release.
    /// </para>
    /// <para>
    /// If the body detects that the outfit was destroyed improperly using Object.Destroy() instead of 
    /// <see cref="Outfit.Destroy"/> then it will perform a forced release in order to clean itself up.  Observers
    /// will receive the event so they also have a chance to cleanup. What is permanently lost depends on the 
    /// process used to destroy the outfit.
    /// </para>
    /// </remarks>
    [RequireComponent(typeof(BodyAccessoryManager))]
    [AddComponentMenu(OutfitterMenu.Menu + "Standard Body", OutfitterMenu.BodyComponentMenuOrder + 0)]
    [SelectionBase]
    public class StandardBody
        : Body, IOutfitObserver
    {
        #region Core

        [Space]

        [SerializeField]
        [Tooltip("The motion root when there is no outfit is assigned.  (Defaults to the body's transform.)")]
        [LocalComponentPopupAttribute(typeof(Transform), true)]
        private Transform m_DefaultMotionRoot = null;

        /// <summary>
        /// The motion root when there is no outfit assigned. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// Defaults to the body's transform.
        /// </para>
        /// </remarks>
        public Transform DefaultMotionRoot
        {
            // Must always be available.
            get
            {
                if (!m_DefaultMotionRoot)
                {
                    if (Application.isPlaying)
                        m_DefaultMotionRoot = transform;
                    return transform;
                }

                return m_DefaultMotionRoot;
            }
            set { m_DefaultMotionRoot = value ? value : transform; }
        }

        public sealed override Transform MotionRoot
        {
            // Must always be avaiable.
            get { return Outfit ? Outfit.MotionRoot : DefaultMotionRoot; }
        }

        public sealed override Collider PrimaryCollider
        {
            get { return Outfit ? Outfit.PrimaryCollider : null; }
        }

        public sealed override Rigidbody PrimaryRigidBody
        {
            get { return Outfit ? Outfit.PrimaryRigidbody : null; }
        }

        #endregion

        #region Outfit

        [SerializeField]
        [HideInInspector]
        private Outfit m_Outfit;

        [SerializeField]
        [HideInInspector]
        private bool m_HasOutfit;  // Used to detect if the outfit has been improperly destroyed.

        public sealed override Outfit Outfit
        {
            get
            {
                CheckOutfitLost();
                return m_Outfit;
            }
        }

        public sealed override Outfit SetOutfit(Outfit outfit, bool forceRelease)
        {
            // Warning: This method can be called by CheckOutfitLost(), so make sure no code paths trigger that
            // method.

            if (outfit)
            {
                if (outfit == m_Outfit)
                {
                    Debug.LogWarning("Outfit is already set. No action taken.", this);
                    return null;
                }

                if (outfit && outfit.IsManaged && (outfit.Owner && outfit.Owner != gameObject))
                {
                    Debug.LogErrorFormat(this, 
                        "Outfit is managed by another object.  Can't set outfit.  Outfit: {0}, Owner: {1}", 
                        outfit.name, outfit.Owner.name);
                    return outfit;
                }
            }

            if (forceRelease && !m_HasOutfit)
            {
                Debug.LogWarning("Force release ignored.  Body has no outfit to release.", this);
                forceRelease = false;
            }

            forceRelease = forceRelease || (m_HasOutfit && !m_Outfit);

            var origOutfit = m_Outfit ? m_Outfit : null;  // Get rid of potential destoryed reference early.

            if (m_Outfit)
            {
                m_Outfit.RemoveObserver(this);  // Keep this early.

                // Note: The state of the outfit it set at the end of the method, just before final release.

                if (m_Outfit.transform.parent == transform)
                    m_Outfit.transform.parent = null;

                // Preserve the outfit's position.
                DefaultMotionRoot.position = m_Outfit.MotionRoot.position;
                DefaultMotionRoot.rotation = m_Outfit.MotionRoot.rotation;

                // Assumption: The body should never be the context of outfit compoenents for an outfit it isn't
                // managing.

                for (int i = 0; i < m_Outfit.BodyPartCount; i++)
                {
                    var item = m_Outfit.GetBodyPart(i);
                    if (item && item.Context == gameObject)
                        item.Context = null;
                }

                for (int i = 0; i < m_Outfit.MountPointCount; i++)
                {
                    var item = m_Outfit.GetMountPoint(i);
                    if (item && item.Context == gameObject)
                        item.Context = null;
                }
            }

            m_Outfit = outfit;
            m_HasOutfit = m_Outfit;

            if (m_Outfit)
            {
                m_Outfit.SetState(OutfitStatus.InUse, gameObject);
                m_Outfit.transform.parent = transform;

                // Persist the previous outfit's position.
                m_Outfit.MotionRoot.position = DefaultMotionRoot.position;
                m_Outfit.MotionRoot.rotation = DefaultMotionRoot.rotation;

                m_Outfit.AddObserver(this);  // Keep this late.
            }

            AccessoriesLocal.SetOutfit(outfit, forceRelease);
            SendOutfitChange(origOutfit, forceRelease);

            // Keep this last.  There may be outfit observers that take action when the outfit transitions state.
            // Don't want to trigger them until the outfit is truley free.
            if (origOutfit && origOutfit.Owner == gameObject)
                origOutfit.SetState(OutfitStatus.Unmanaged, null);

            return origOutfit;
        }

        #endregion

        #region Outfit Observer

        void IOutfitObserver.OnDestroy(Outfit sender, DestroyType typ, Outfit referenceOutfit)
        {
            if (m_Outfit && m_Outfit == sender)
            {
                Debug.LogError("Outfit destroyed while owned by body.  Forced an unlink.", this);
                sender.RemoveObserver(this);
                SetOutfit(null, typ == DestroyType.Bake);
            }
            else
            {
                // Overkill?
                Debug.LogError("Internal error: Body is observing external outfit: " + sender.name,
                    this);
                sender.RemoveObserver(this);
            }
        }

        void IOutfitObserver.OnStateChange(Outfit sender)
        {
            if (sender.Owner != gameObject)
            {
                Debug.LogWarningFormat(this, "Unexpected loss of outfit ownership.  Outfit auto-released."
                    + " Outfit: {0}, New owner: {1}", sender.name,
                    sender.Owner ? sender.Owner.name : "None");

                sender.RemoveObserver(this);
                SetOutfit(null);

                return;
            }

            switch (sender.Status)
            {
                case OutfitStatus.Stored:
                case OutfitStatus.Unmanaged:

                    Debug.LogErrorFormat(this, "Unsupported outfit status change.   Outfit auto-released."
                        + " Outfit: {0}, Status: {1}", sender.name, sender.Status);

                    sender.RemoveObserver(this);
                    SetOutfit(null);

                    return;
            }
        }

        void IOutfitObserver.OnMountAccessory(Outfit sender, Accessory accessory)
        {
            // Do nothing.
        }

        void IOutfitObserver.OnReleaseAccessory(Outfit sender, Accessory accessory)
        {
            // Do nothing.
        }

        #endregion

        #region Outfit Utilities Members

        /// <summary>
        /// Do not call this method from <see cref="SetOutfit"/>!!!!! It can result in an infinite loop.
        /// </summary>
        private void CheckOutfitLost()
        {
            // Don't worry about losing ownership.  That is detected by an event.
            if (m_HasOutfit && !m_Outfit)
            {
                Debug.LogError("Outfit was improperly destroyed after assignment to body."
                    + " Recovery performed.  Assets may have been lost.", this);
                SetOutfit(null, true);
            }
        }

        #endregion

        #region Accessories

        /// <summary>
        /// Do not access directly.  Use <see cref="AccessoriesLocal"/>.
        /// </summary>
        private BodyAccessoryManager m_Accessories;

        /// <summary>
        /// The accessory manager.  (Its true type.)
        /// </summary>
        protected BodyAccessoryManager AccessoriesLocal
        {
            get 
            {
                if (!m_Accessories)
                    m_Accessories = GetComponent<BodyAccessoryManager>();

                return m_Accessories; 
            }
        }

        public sealed override IBodyAccessoryManager Accessories
        {
            get { return AccessoriesLocal; }
        }

        #endregion

        #region Body Observers

        [Space]

        [SerializeField]
        [ObjectList("IBodyObserver Objects", typeof(IBodyObserver))]
        private BodyObserverGroup m_Observers = new BodyObserverGroup(1);

        protected BodyObserverGroup Observers
        {
            get { return m_Observers; }
        }

        public sealed override bool AddObserver(IBodyObserver observer)
        {
            return Observers.Add(observer, this) != -1;
        }

        public sealed override void RemoveObserver(IBodyObserver observer)
        {
            Observers.Remove(observer);
        }

        #endregion

        #region Initialization

        public virtual void Initialize()
        {
            CheckOutfitLost();
        }

        protected void Awake()
        {
            Initialize();
        }

        #endregion

        #region Events

        private void SendOutfitChange(Outfit previous, bool wasForced)
        {
            OnOutfitChange(previous, wasForced);
            Observers.SendOutfitChange(this, previous, wasForced);
        }

        /// <summary>
        /// Called at the end of an outfit change operation.
        /// </summary>
        /// <remarks>
        /// <para>
        /// There are three situations when a forced release is performed:
        /// </para>
        /// <para>
        /// <ul>
        /// <li><see cref="SetOutfit"/> was used to initiate the forced release.</li>
        /// <li>A bake event was received from the outfit.</li>
        /// <li>
        /// The body detected that the outfit was improperly destroyed using Object.Destroy() directly, instead of
        /// <see cref="Outfit.Destroy"/>.
        /// </li>
        /// </ul>
        /// </para>
        /// </remarks>
        /// <param name="previous">The previous outfit, or null if there is none.</param>
        /// <param name="wasForced">True if a forced release was performed.</param>
        protected virtual void OnOutfitChange(Outfit previous, bool wasForced)
        {
            // Do nothing.
        }

        #endregion

#if UNITY_EDITOR

        #region Editor Only

        protected override void GetUndoObjects(System.Collections.Generic.List<Object> list)
        {
            Initialize();

            for (int i = 0; i < Observers.Count; i++)
            {
                var item = Observers[i];

                if (item != null)
                    list.Add(item as Object);
            }

            if (DefaultMotionRoot != transform)
                list.Add(DefaultMotionRoot);

            list.Add(AccessoriesLocal);  // Property causes initialization.
            for (int i = 0; i < AccessoriesLocal.Count; i++)
            {
                var info = AccessoriesLocal[i];

                // Only include stored.  The outfit is responsible for including its accessories.
                if (info.Accessory && info.Accessory.Status == AccessoryStatus.Stored)
                    Accessory.UnsafeGetUndoObjects(info.Accessory);
            }

            if (m_Outfit)
                Outfit.UnsafeGetUndoObjects(m_Outfit, list);

            list.Add(transform);  // For completeness
            list.Add(gameObject); // For completeness
            list.Add(this);
        }

        #endregion

#endif
    }
}
