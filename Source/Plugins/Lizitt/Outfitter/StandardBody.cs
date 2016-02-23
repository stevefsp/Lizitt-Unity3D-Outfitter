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

            SendOutfitChangedPre();

            var origOutfit = m_Outfit;
            UnlinkCurrentOutfit();

            SendOutfitChange(origOutfit);

            return origOutfit;
        }

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
            // Do nothing.
        }

        void IOutfitObserver.OnReleaseAccessory(Outfit sender, Accessory accessory)
        {
            // Do nothing.
        }

        #endregion

        #region Outfit Utilities

        private void LinkOutfit(Outfit outfit)
        {
            AccessoriesLocal.SetOutfit(outfit);
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

            // Outfit may have been improperly destroyed, so always run these.
            AccessoriesLocal.SetOutfit(null);
            m_Outfit = null;
            m_HasOutfit = false;
        }

        private void CheckNeedsSoftReset()
        {
            // Don't worry about losing ownership.  That is non-fatal and much less likely to occur.
            if (m_HasOutfit && !m_Outfit)
            {
                Debug.LogError("Outfit was improperly destroyed after assignment to body."
                    + " Soft reset performed.", this);
                SoftReset();
            }
        }

        #endregion

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

            // TODO: EVAL: Should only the mounted accessories be purged?
            AccessoriesLocal.Reset();

            if (outfit)
                outfit.RemoveObserver(this);

            Observers.SendSoftReset(this);
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

//namespace com.lizitt.outfitter
//{
//    /// <summary>
//    /// Implements all standard <see cref="Body"/> features.
//    /// </summary>
//    public class StandardBody
//        : Body, IOutfitObserver, IAccessoryObserver
//    {
//        #region Core

//        [Space]

//        [SerializeField]
//        [Tooltip("The motion root when there is no outfit assigned.  The body's transform will be used if not assigned"
//            + " prior to initialization.")]
//        [LocalComponentPopupAttribute(typeof(Transform), true)]
//        private Transform m_DefaultMotionRoot = null;

//        /// <summary>
//        /// The motion root to use when there is no outfit assigned.
//        /// </summary>
//        /// <remarks>
//        /// <para>
//        /// The body's transform will be used if not assigned prior to initialization.
//        /// </para>
//        /// </remarks>
//        public Transform DefaultMotionRoot
//        {
//            // Must always be available, even in editor mode.
//            get
//            {
//                if (!m_DefaultMotionRoot)
//                    m_DefaultMotionRoot = transform;

//                return m_DefaultMotionRoot;
//            }
//            set { m_DefaultMotionRoot = value ? value : transform; }
//        }

//        public override Transform MotionRoot
//        {
//            // Must always be avaiable, even in editor mode.
//            get { return Outfit ? Outfit.MotionRoot : DefaultMotionRoot; }
//        }

//        public override Collider PrimaryCollider
//        {
//            get { return Outfit ? Outfit.PrimaryCollider : null; }
//        }

//        public override Rigidbody PrimaryRigidBody
//        {
//            get { return Outfit ? Outfit.PrimaryRigidbody : null; }
//        }

//        #endregion

//        #region Outfit

//        [SerializeField]
//        [HideInInspector]
//        private Outfit m_Outfit;

//        [SerializeField]
//        [HideInInspector]
//        private bool m_HasOutfit;  // Used to detect if the outfit has been improperly destroyed.

//        public override Outfit Outfit
//        {
//            get 
//            {
//                CheckNeedsSoftReset();
//                return m_Outfit; 
//            }
//        }

//        public sealed override Outfit SetOutfit(Outfit outfit)
//        {
//            CheckNeedsSoftReset();

//            if (!outfit)
//                return RemoveOutfit();

//            if (outfit == m_Outfit)
//            {
//                Debug.LogError("Outfit already set. Outfit set ignored.", this);
//                return null;
//            }

//            SendOutfitChangedPre();

//            var origOutfit = m_Outfit;
//            UnlinkCurrentOutfit();
//            LinkOutfit(outfit);

//            SendOutfitChange(origOutfit);

//            return origOutfit;
//        }

//        private Outfit RemoveOutfit()
//        {
//            if (!Outfit)
//                return null;

//            SendOutfitChangedPre();

//            var origOutfit = m_Outfit;
//            UnlinkCurrentOutfit();

//            SendOutfitChange(origOutfit);

//            return origOutfit;
//        }

//        #region Outfit Observer

//        void IOutfitObserver.OnDestroy(Outfit sender, DestroyType typ, Outfit referenceOutfit)
//        {
//            if (m_Outfit == sender)
//            {
//                Debug.LogError("Outfit destroyed while owned by body.  Soft reset performed.", this);
//                SoftReset();
//            }
//            else
//            {
//                // Overkill?
//                Debug.LogError("Internal error: Body is observing external outfit: " + sender.name,
//                    this);
//                sender.RemoveObserver(this);
//            }
//        }

//        void IOutfitObserver.OnStateChange(Outfit sender)
//        {

//            if (sender.Owner != gameObject)
//            {
//                Debug.LogErrorFormat(this, "Unexpected outfit ownership change.  Soft reset"
//                    + " performed: Outfit: {0}, New owner: {1}", sender.name,
//                    sender.Owner ? sender.Owner.name : "None");

//                SoftReset();

//                return;
//            }

//            switch (sender.Status)
//            {
//                case OutfitStatus.Stored:
//                case OutfitStatus.Unmanaged:

//                    Debug.LogErrorFormat(this, "Unsupported outfit status change.  Soft reset"
//                        + " performed: Outfit: {0}, Status: {1}", sender.name, sender.Status);

//                    SoftReset();

//                    return;
//            }
//        }

//        void IOutfitObserver.OnMountAccessory(Outfit sender, Accessory accessory)
//        {
//            // Do nothing.
//        }

//        void IOutfitObserver.OnReleaseAccessory(Outfit sender, Accessory accessory)
//        {
//            // Do nothing.
//        }

//        #endregion

//        #region Outfit Utilities

//        private void LinkOutfit(Outfit outfit)
//        {
//            AccessoriesSetOutfit(outfit);
//            m_Outfit = outfit;
//            m_HasOutfit = m_Outfit;

//            if (m_HasOutfit)
//            {
//                outfit.SetState(OutfitStatus.InUse, gameObject);
//                outfit.transform.parent = transform;

//                // Persist the 'last' outfit position.
//                outfit.MotionRoot.position = DefaultMotionRoot.position;
//                outfit.MotionRoot.rotation = DefaultMotionRoot.rotation;

//                outfit.AddObserver(this);
//            }
//        }

//        private void UnlinkCurrentOutfit()
//        {
//            if (m_Outfit)
//            {
//                m_Outfit.RemoveObserver(this);

//                if (m_Outfit.Owner == gameObject)
//                    m_Outfit.SetState(OutfitStatus.Unmanaged, null);

//                if (m_Outfit.transform.parent == transform)
//                    m_Outfit.transform.parent = null;

//                // Preserve the outfit's position.
//                DefaultMotionRoot.position = m_Outfit.MotionRoot.position;
//                DefaultMotionRoot.rotation = m_Outfit.MotionRoot.rotation;
//            }

//            // Outfit may have been improperly destroyed, so always run these.
//            AccessoriesSetOutfit(null);
//            m_Outfit = null;
//            m_HasOutfit = false;
//        }

//        private void CheckNeedsSoftReset()
//        {
//            // Don't worry about losing ownership.  That is non-fatal and much less likely to occur.
//            if (m_HasOutfit && !m_Outfit)
//            {
//                Debug.LogError("Outfit was improperly destroyed after assignment to body."
//                    + " Soft reset performed.", this);
//                SoftReset();
//            }
//        }

//        #endregion

//        #endregion

//        #region Accessories

//        [SerializeField]
//        [HideInInspector]    // Refactor warning: Field name is used in the body editor.
//        private List<AccessoryMountInfo> m_Accessories = new List<AccessoryMountInfo>(4); 

//        public override int AccessoryCount
//        {
//            get { return m_Accessories.Count; }
//        }

//        public override AccessoryMountInfo GetAccessoryInfo(int index)
//        {
//            return m_Accessories[index];
//        }

//        public override AccessoryMountInfo GetAccessoryInfo(Accessory accessory)
//        {
//            if (accessory)
//            {
//                for (int i = 0; i < m_Accessories.Count; i++)
//                {
//                    if (m_Accessories[i].Accessory == accessory)
//                        return m_Accessories[i];
//                }
//            }

//            return new AccessoryMountInfo();
//        }

//        public override bool ContainsAccessory(Accessory accessory)
//        {
//            if (accessory)
//            {
//                for (int i = 0; i < m_Accessories.Count; i++)
//                {
//                    if (m_Accessories[i].Accessory == accessory)
//                        return true;
//                }
//            }

//            return false;
//        }

//        public override MountResult AddAccessory(Accessory accessory, AccessoryAddSettings addSettings, bool mustMount)
//        {
//            if (!accessory)
//            {
//                Debug.LogError("Accessory is null.");
//                return MountResult.FailedOnError;
//            }

//            // Remember: Don't need to check mounter validity.  Settings setter does that.

//            for (int i = 0; i < m_Accessories.Count; i++)
//            {
//                if (m_Accessories[i].Accessory && m_Accessories[i].Accessory == accessory)
//                {
//                    // This is an error.  Must use modify method to change configuration.
//                    Debug.LogError("Accessory is already added: " + accessory.name, this);
//                    return MountResult.FailedOnError;
//                }
//            }

//            MountResult status = MountResult.FailedOnError;

//            var mountInfo = new AccessoryMountInfo();
//            mountInfo.Accessory = accessory;
//            mountInfo.Apply(addSettings);

//            if (m_Outfit)
//                status = MountToOutfit(ref mountInfo);
//            else if (mustMount)
//            {
//                Debug.LogError("Must succeed failure.  No outfit: " + accessory.name, this);
//                return MountResult.FailedOnError;
//            }

//            bool isMounted = (status == MountResult.Success);
//            if (!isMounted)
//            {
//                if (mustMount)
//                {
//                    Debug.LogErrorFormat(this,
//                        "Must succeed failure.  Failed to mount to outfit: Accessory: {0}, Status: {1}",
//                        accessory.name, status);

//                    return MountResult.FailedOnError;
//                }

//                StoreAccessory(ref mountInfo);
//            }

//            LinkAccessory(mountInfo);

//            return isMounted ? MountResult.Success : MountResult.Stored;
//        }

//        public override MountResult ModifyAccessory(Accessory accessory, AccessoryAddSettings addSettings)
//        {
//            if (!accessory)
//            {
//                Debug.LogError("Can't modify a null accessory.", this);
//                return MountResult.FailedOnError;
//            }

//            // Remember: Don't need to check mounter validity.  Settings setter does that.

//            for (var i = 0; i < m_Accessories.Count; i++)
//            {
//                var mountInfo = m_Accessories[i];

//                if (mountInfo.Accessory && mountInfo.Accessory == accessory)
//                {
//                    /*
//                     * Design note:
//                     * 
//                     * There can be lots of reasons for modifying an accessory, some of which don't need/want a
//                     * re-mount.  But it is better to keep it simple.  All modify calls for a mounted accessory
//                     * result in a remount.)
//                     */

//                    mountInfo.Apply(addSettings);

//                    if (m_Outfit)
//                    {
//                        if (MountToOutfit(ref mountInfo) != MountResult.Success)
//                            StoreAccessory(ref mountInfo);
//                    }
//                    // else it is already in storage.

//                    m_Accessories[i] = mountInfo;

//                    return mountInfo.Outfit ? MountResult.Success : MountResult.Stored;
//                }
//            }

//            Debug.LogError("Attempt made to modify an unknown accessory: " + accessory.name, this);
//            return MountResult.FailedOnError;
//        }

//        public override bool RemoveAccessory(Accessory accessory)
//        {
//            if (!accessory)
//                return false;

//            for (int i = 0; i < m_Accessories.Count; i++)
//            {
//                var mountInfo = m_Accessories[i];

//                if (mountInfo.Accessory == accessory)
//                {
//                    var outfit = mountInfo.Outfit;

//                    UnlinkAccessory(i);

//                    if (outfit)
//                        outfit.Release(accessory);  // Ignore failure.
//                    else
//                        accessory.Release();  // From storage.

//                    return true;
//                }
//            }

//            return false;
//        }

//        #region Accessory Observer

//        void IAccessoryObserver.OnStateChange(Accessory sender)
//        {
//            switch (sender.Status)
//            {
//                case AccessoryStatus.Unmanaged:

//                    UnlinkAccessory(sender);
//                    break;

//                case AccessoryStatus.Stored:

//                    if (sender.Owner != gameObject)
//                        UnlinkAccessory(sender);

//                    break;

//                default:

//                    if (!m_Outfit || sender.Owner != m_Outfit.gameObject)
//                        UnlinkAccessory(sender);

//                    break;
//            }
//        }

//        void IAccessoryObserver.OnDestroy(Accessory sender, DestroyType typ)
//        {
//            UnlinkAccessory(sender);
//        }

//        #endregion

//        #region Accessory Utility Members

//        private void ResetAccessories()
//        {
//            m_Outfit = null;

//            for (int i = m_Accessories.Count - 1; i >= 0; i--)
//            {
//                if (m_Accessories[i].Accessory)
//                {
//                    if (m_Accessories[i].Accessory.Owner == this)
//                    {
//                        UnlinkAccessory(i);
//                        m_Accessories[i].Accessory.Release();
//                    }
//                }
//            }

//            m_Accessories.Clear();
//        }

//        /// <summary>
//        /// Attempts to mount the accessory to the current outfit and updates its information.
//        /// (The outfit must be non-null!)
//        /// </summary>
//        private MountResult MountToOutfit(ref AccessoryMountInfo mountInfo)
//        {
//            var status = m_Outfit.Mount(mountInfo.Accessory, mountInfo.LocationType,
//                mountInfo.IgnoreRestrictions, mountInfo.Mounter, mountInfo.AdditionalCoverage);

//            if (status == MountResult.Success)
//                mountInfo.Outfit = Outfit;

//            return status;
//        }

//        private bool StoreAccessory(ref AccessoryMountInfo mountInfo)
//        {
//            mountInfo.Outfit = null;

//            mountInfo.Accessory.Store(gameObject);  // Only fails on null argument.

//            mountInfo.Accessory.transform.parent = transform;

//            // This helps with visual troubleshooting. (E.g. For accessories that don't properly store.)
//            mountInfo.Accessory.transform.localPosition = Vector3.zero;
//            mountInfo.Accessory.transform.localRotation = Quaternion.identity;

//            return true;
//        }
//        private void LinkAccessory(AccessoryMountInfo info)
//        {
//            info.Accessory.AddObserver(this);
//            m_Accessories.Add(info);
//        }

//        private void UnlinkAccessory(Accessory accessory)
//        {
//            for (int i = 0; i < m_Accessories.Count; i++)
//            {
//                if (m_Accessories[i].Accessory == accessory)
//                {
//                    UnlinkAccessory(i);
//                    return;
//                }
//            }
//        }

//        private void UnlinkAccessory(int index)
//        {
//            var info = m_Accessories[index];

//            if (info.Accessory)
//                info.Accessory.RemoveObserver(this);

//            m_Accessories.RemoveAt(index);
//        }        
        
//        /// <summary>
//        /// Set the current outfit and update accessory mounting as appropriate.
//        /// </summary>
//        /// <remarks>
//        /// The behavior of each accessory during a change in the outfit is determined by
//        /// the method used to add it to the outfit manager.
//        /// </remarks>
//        /// <param name="outfit">The outfit, or null if there is no outfit.</param>
//        public void AccessoriesSetOutfit(Outfit outfit)
//        {
//            // The loop has to be forward.  A user may expect that behavior.  E.g. Accessories
//            // earlier in the list have priority.

//            bool purgeNeeded = false;
//            for (int i = 0; i < m_Accessories.Count; i++)
//            {
//                var mountInfo = m_Accessories[i];

//                if (mountInfo.Accessory)
//                {
//                    if (!outfit || MountToOutfit(ref mountInfo) != MountResult.Success)
//                        StoreAccessory(ref mountInfo);
//                }
//                else
//                {
//                    Debug.LogError("Improperly destroyed accessory detected. Will purge.", this);
//                    purgeNeeded = true;
//                    continue;
//                }

//                m_Accessories[i] = mountInfo;
//            }

//            if (purgeNeeded)
//                PurgeNullAcessories();
//        }

//        private void PurgeNullAcessories()
//        {
//            for (int i = m_Accessories.Count - 1; i >= 0; i--)
//            {
//                if (!m_Accessories[i].Accessory)
//                    m_Accessories.RemoveAt(i);
//            }
//        }

//        #endregion

//        #endregion

//        #region Soft Reset

//        /// <summary>
//        /// Force a soft reset of the body.
//        /// </summary>
//        /// <remarks>
//        /// <para>
//        /// A soft reset will purge all non-setting related assets from the body.  This includes the assigned
//        /// outfit and added accessories.  The only event that will be sent is <see cref="IBodyObserver.OnSoftReset"/>.
//        /// No cleanup of external objects is attempted.
//        /// </para>
//        /// <para>
//        /// The body will normally detect when a soft reset is required.  The only reason for performing one
//        /// manually in the editor when you know you've done something wrong, like manually deleting the outfit 
//        /// assigned to the body.
//        /// </para>
//        /// </remarks>
//        /// <param name="body">The body to reset.</param>
//        public static void UnsafeSoftReset(StandardBody body)
//        {
//            body.SoftReset();
//        }

//        protected virtual void SoftReset()
//        {
//            // Keep this self contained.  There is too much risk of a recursive call if other methods called.
//            // Only do the absolute minimum to protect the body and its observers.  That's all.  Don't care
//            // about the outfit or the accessories.

//            var outfit = m_Outfit;
//            m_Outfit = null;
//            m_HasOutfit = false;

//            ResetAccessories();

//            if (outfit)
//                outfit.RemoveObserver(this);

//            Observers.SendSoftReset(this);
//        }

//        #endregion

//        #region Body Observers

//        [Space]

//        [SerializeField]
//        [ObjectList("IBodyObserver Objects", typeof(IBodyObserver))]
//        private BodyObserverGroup m_Observers = new BodyObserverGroup(1);

//        protected BodyObserverGroup Observers
//        {
//            get { return m_Observers; }
//        }

//        public sealed override bool AddObserver(IBodyObserver observer)
//        {
//            return Observers.Add(observer, this) != -1;
//        }

//        public sealed override void RemoveObserver(IBodyObserver observer)
//        {
//            Observers.Remove(observer);
//        }

//        #endregion

//        #region Initialization

//        public virtual void Initialize()
//        {
//            CheckNeedsSoftReset();
//        }

//        protected void Awake()
//        {
//            Initialize();
//        }

//        #endregion

//        #region Events

//        private void SendOutfitChangedPre()
//        {
//            LocalOutfitChangePre();
//        }

//        protected virtual void LocalOutfitChangePre()
//        {
//            // Do nothing.
//        }

//        private void SendOutfitChange(Outfit previous)
//        {
//            LocalOutfitChange(previous);
//            Observers.SendOutfitChange(this, previous);
//        }

//        protected virtual void LocalOutfitChange(Outfit previous)
//        {
//            // Do nothing.
//        }

//        #endregion

//#if UNITY_EDITOR

//        #region Editor Only

//        protected override void GetUndoObjects(System.Collections.Generic.List<Object> list)
//        {
//            Initialize();

//            for (int i = 0; i < Observers.Count; i++)
//            {
//                var item = Observers[i];

//                if (item != null)
//                    list.Add(item as Object);
//            }

//            if (DefaultMotionRoot != transform)
//                list.Add(DefaultMotionRoot);

//            for (int i = 0; i < m_Accessories.Count; i++)
//            {
//                var info = m_Accessories[i];

//                if (!info.Outfit)  // Outfit returns mounted accessories.
//                    Accessory.UnsafeGetUndoObjects(info.Accessory);
//            }

//            if (m_Outfit)
//                Outfit.UnsafeGetUndoObjects(m_Outfit, list);

//            list.Add(transform);  // For completeness
//            list.Add(gameObject); // For completeness
//            list.Add(this);
//        }

//        #endregion

//#endif
//    }
//}
