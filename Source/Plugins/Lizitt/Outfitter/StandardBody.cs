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

namespace com.lizitt.outfitter
{
    /// <summary>
    /// Implements all standard <see cref="Body"/> features.
    /// </summary>
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
                CheckNeedsSoftReset();
                return m_Outfit; 
            }
        }

        public sealed override Outfit SetOutfit(Outfit outfit)
        {
            CheckNeedsSoftReset();

            if (!outfit)
                return RemoveOutfit();

            CheckAccessoriesInitialized();

            if (outfit == m_Outfit)
            {
                Debug.LogError("Outfit already set. Outfit set ignored.", this);
                return null;
            }

            SendOutfitChangedPre();

            var origOutfit = m_Outfit;
            UnlinkCurrentOutfit();
            LinkOutfit(outfit);

            SendOutfitChange(origOutfit);

            return origOutfit;
        }

        private Outfit RemoveOutfit()
        {
            if (!Outfit)
                return null;

            CheckAccessoriesInitialized();

            SendOutfitChangedPre();

            var origOutfit = m_Outfit;
            UnlinkCurrentOutfit();

            SendOutfitChange(origOutfit);

            return origOutfit;
        }

        private void LinkOutfit(Outfit outfit)
        {
            m_Accessories.SetOutfit(outfit);
            m_Outfit = outfit;
            m_HasOutfit = m_Outfit;

            if (m_HasOutfit)
            {
                outfit.SetState(OutfitStatus.InUse, gameObject);
                outfit.transform.parent = transform;

                // Persist the 'last' outfit position.
                outfit.MotionRoot.position = DefaultMotionRoot.position;
                outfit.MotionRoot.rotation = DefaultMotionRoot.rotation;

                outfit.AddObserver(this);
            }
        }

        private void UnlinkCurrentOutfit()
        {
            if (m_Outfit)
            {
                m_Outfit.RemoveObserver(this);

                if (m_Outfit.Owner == gameObject)
                    m_Outfit.SetState(OutfitStatus.Unmanaged, null);

                if (m_Outfit.transform.parent == transform)
                    m_Outfit.transform.parent = null;

                // Preserve the outfit's position.
                DefaultMotionRoot.position = m_Outfit.MotionRoot.position;
                DefaultMotionRoot.rotation = m_Outfit.MotionRoot.rotation;
            }

            // Outfit may have been improperly destroyed.  So always run these.
            m_Accessories.SetOutfit(null);
            m_Outfit = null;
            m_HasOutfit = false;
        }

        private void CheckNeedsSoftReset()
        {
            if (m_HasOutfit && !m_Outfit)
            {
                Debug.LogError( "Outfit was improperly destroyed after assignment to body."
                    + " Soft reset performed.", this);
                SoftReset();
            }
        }

        #endregion

        #region Accessories

        [SerializeField]
        [HideInInspector]
        private BodyAccessoryManager m_Accessories;

        public override int AccessoryCount
        {
            get 
            {
                CheckAccessoriesInitialized();
                CheckNeedsSoftReset();
                return m_Accessories.Count; 
            }
        }

        public override AccessoryMountInfo GetAccessoryInfo(int index)
        {
            CheckAccessoriesInitialized();
            CheckNeedsSoftReset();

            return m_Accessories[index];
        }

        public override AccessoryMountInfo GetAccessoryInfo(Accessory accessory)
        {
            CheckAccessoriesInitialized();
            CheckNeedsSoftReset();

            return m_Accessories[accessory];
        }

        public override bool ContainsAccessory(Accessory accessory)
        {
            CheckAccessoriesInitialized();
            CheckNeedsSoftReset();

            return m_Accessories.Contains(accessory);
        }

        public override MountResult AddAccessory(
            Accessory accessory, AccessoryAddSettings settings, bool mustMount)
        {
            CheckAccessoriesInitialized();
            CheckNeedsSoftReset();
            return m_Accessories.Add(accessory, settings, mustMount);
        }

        public override MountResult ModifyAccessory(
            Accessory accessory, AccessoryAddSettings settings)
        {
            CheckAccessoriesInitialized();
            CheckNeedsSoftReset();
            return m_Accessories.Modify(accessory, settings);
        }

        public override bool RemoveAccessory(Accessory accessory)
        {
            CheckAccessoriesInitialized();
            CheckNeedsSoftReset();
            return m_Accessories.Remove(accessory);
        }

        private void CheckAccessoriesInitialized()
        {
            if (m_Accessories == null || !m_Accessories.Owner)
                m_Accessories = new BodyAccessoryManager(this);
        }

        #endregion

        #region Soft Reset

        /// <summary>
        /// Force a soft reset of the body.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A soft reset will purge all non-setting related assets from the body.  This includes the assigned
        /// outfit and added accessories.  The only event that will be sent is <see cref="IBodyObserver.OnSoftReset"/>.
        /// No cleanup of external objects is attempted.
        /// </para>
        /// <para>
        /// The body will normally detect when a soft reset is required.  The only reason for performing one
        /// manually in the editor when you know you've done something wrong, like manually deleting the outfit 
        /// assigned to the body.
        /// </para>
        /// </remarks>
        /// <param name="body">The body to reset.</param>
        public static void UnsafeSoftReset(StandardBody body)
        {
            body.SoftReset();
        }

        protected virtual void SoftReset()
        {
            // Keep this self contained.  There is too much risk of a recursive call if other methods called.
            // Only do the absolute minimum to protect the body and its observers.  That's all.  Don't care
            // about the outfit or the accessories.

            var outfit = m_Outfit;
            m_Outfit = null;
            m_HasOutfit = false;

            if (m_Accessories != null)
                m_Accessories.Reset();

            if (outfit)
                outfit.RemoveObserver(this);

            m_Observers.SendSoftReset(this);
        }

        #endregion

        #region Body Observers

        [Space]

        [SerializeField]
        [ObjectList("IBodyObserver Objects", typeof(IBodyObserver))]
        private BodyObserverGroup m_Observers = new BodyObserverGroup(0);

        protected BodyObserverGroup Observers
        {
            get { return m_Observers; }
        }

        public sealed override bool AddObserver(IBodyObserver observer)
        {
            return m_Observers.Add(observer, this) != -1;
        }

        public sealed override void RemoveObserver(IBodyObserver observer)
        {
            m_Observers.Remove(observer);
        }

        #endregion

        #region Initialization

        public virtual void Initialize()
        {
            CheckAccessoriesInitialized();
            CheckNeedsSoftReset();
        }

        protected void Awake()
        {
            Initialize();
        }

        #endregion

        #region Events

        private void SendOutfitChangedPre()
        {
            LocalOutfitChangePre();
            Observers.SendOutfitChangePre(this);
        }

        protected virtual void LocalOutfitChangePre()
        {
            // Do nothing.
        }

        private void SendOutfitChange(Outfit previous)
        {
            LocalOutfitChange(previous);
            Observers.SendOutfitChange(this, previous);
        }

        protected virtual void LocalOutfitChange(Outfit previous)
        {
            // Do nothing.
        }

        #endregion

        #region Outfit Observer

        void IOutfitObserver.OnDestroy(Outfit sender, DestroyType typ, Outfit referenceOutfit)
        {
            if (m_Outfit == sender)
            {
                Debug.LogError("Outfit destroyed while owned by body.  Soft reset performed.", this);
                SoftReset();
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

                SoftReset();

                return;
            }

            switch (sender.Status)
            {
                case OutfitStatus.Stored:
                case OutfitStatus.Unmanaged:

                    Debug.LogErrorFormat(this, "Unsupported outfit status change.  Soft reset"
                        + " performed: Outfit: {0}, Status: {1}", sender.name, sender.Status);

                    SoftReset();

                    return;
            }
        }

        void IOutfitObserver.OnMountAccessory(Outfit sender, Accessory accessory)
        {
            // Handled by accesory manager.
        }

        void IOutfitObserver.OnReleaseAccessory(Outfit sender, Accessory accessory)
        {
            // Handled by accesory manager.
        }

        #endregion

#if UNITY_EDITOR

        #region Context Menus

        [ContextMenu("Perform Soft Reset")]
        protected virtual void SoftReset_Menu()
        {
            var accCount = AccessoryCount;
            var outfit = m_Outfit;

            SoftReset();

            if (accCount > 0 || outfit)
            {
                Debug.LogWarningFormat(this, "Soft reset performed on body. Orphaned outfit: {0},"
                    + " Potential orphaned accessories: {1}",  outfit ? outfit.name : "None", 
                    accCount);
            }
            else
                Debug.Log("Soft reset performed on body. No components were orphaned.", this);
        }

        #endregion

#endif
    }
}


