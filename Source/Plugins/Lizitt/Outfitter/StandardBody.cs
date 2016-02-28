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
    [RequireComponent(typeof(BodyAccessoryManager))]
    [AddComponentMenu(LizittUtil.LizittMenu + "StandardBody", OutfitterUtil.BaseMenuOrder + 3)]
    public class StandardBody
        : Body, IOutfitObserver
    {
        #region Core

        [Space]

        [SerializeField]
        [Tooltip("The motion root when there is no outfit assigned.  The body's transform will be used if not assigned"
            + " prior to initialization.")]
        [LocalComponentPopupAttribute(typeof(Transform), true)]
        private Transform m_DefaultMotionRoot = null;

        /// <summary>
        /// The motion root to use when there is no outfit assigned.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The body's transform will be used if not assigned prior to initialization.
        /// </para>
        /// </remarks>
        public Transform DefaultMotionRoot
        {
            // Must always be available, even in editor mode.
            get
            {
                if (!m_DefaultMotionRoot)
                    m_DefaultMotionRoot = transform;

                return m_DefaultMotionRoot;
            }
            set { m_DefaultMotionRoot = value ? value : transform; }
        }

        public override Transform MotionRoot
        {
            // Must always be avaiable, even in editor mode.
            get { return Outfit ? Outfit.MotionRoot : DefaultMotionRoot; }
        }

        public override Collider PrimaryCollider
        {
            get { return Outfit ? Outfit.PrimaryCollider : null; }
        }

        public override Rigidbody PrimaryRigidBody
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

        public override Outfit Outfit
        {
            get
            {
                CheckOutfitLost();
                return m_Outfit;
            }
        }

        public sealed override Outfit SetOutfit(Outfit outfit, bool forceRelease)
        {
            // Warning: This method can be called by CheckOutfitLost().  So make sure not code paths trigger that
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

            var origOutfit = m_Outfit ? m_Outfit : null;

            if (m_Outfit)
            {
                m_Outfit.RemoveObserver(this);  // Keep this early.

                if (m_Outfit.Owner == gameObject)
                    m_Outfit.SetState(OutfitStatus.Unmanaged, null);

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
                Debug.LogErrorFormat(this, "Unexpected outfit ownership change.  Soft reset"
                    + " performed: Outfit: {0}, New owner: {1}", sender.name,
                    sender.Owner ? sender.Owner.name : "None");

                sender.RemoveObserver(this);
                SetOutfit(null);

                return;
            }

            switch (sender.Status)
            {
                case OutfitStatus.Stored:
                case OutfitStatus.Unmanaged:

                    Debug.LogErrorFormat(this, "Unsupported outfit status change.  Soft reset"
                        + " performed: Outfit: {0}, Status: {1}", sender.name, sender.Status);

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

        #region Outfit Utilities

        /// <summary>
        /// Do not call this method from <see cref="SetOutfit"/>!!!!! It can cause reversion.
        /// </summary>
        private void CheckOutfitLost()
        {
            // Don't worry about losing ownership.  That is non-fatal and much less likely to occur.
            if (m_HasOutfit && !m_Outfit)
            {
                Debug.LogError("Outfit was improperly destroyed after assignment to body."
                    + " Recovery performed.  Assets may have been lost.", this);
                SetOutfit(null, true);
            }
        }

        #endregion

        #region Accessories

        private BodyAccessoryManager m_Accessories;

        public BodyAccessoryManager AccessoriesLocal
        {
            get 
            {
                if (!m_Accessories)
                    m_Accessories = GetComponent<BodyAccessoryManager>();

                return m_Accessories; 
            }
        }

        public override IBodyAccessoryManager Accessories
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
