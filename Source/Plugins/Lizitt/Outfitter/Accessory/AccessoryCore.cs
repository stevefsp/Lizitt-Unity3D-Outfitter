/*
 * Copyright (c) 2016 Stephen A. Pratt
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
    /// Implements the most common <see cref="Accessory"/> features.
    /// </summary>
    /// <remarks>
    /// <para>
    /// With a few minor exceptions the only feature that extensions of this class need to implemment is
    /// mounting, and most of the plumbing for mounting is handled by this class.
    /// </para>
    /// <para>
    /// This class manages state, state transtions, observers, and events.  It also handles storage, release, and 
    /// implements a framework for mounting.  Concrete implementations can intercept observer events by overriding 
    /// local event methods.  (E.g. <see cref="OnStateChange"/>.  Mount behavior is implemented by overriding 
    /// <see cref="GetInitializedMounter"/> and/or the 'internal mount' methods.  (At least one mount method must
    /// be overridden.)
    /// </para>
    /// <para>
    /// Mounter priority is as follows:  The priority mounter supplied to the mount method, 
    /// then the mounter provided by <see cref="GetInitializedMounter"/>, then <see cref="MountInternal"/> if 
    /// <see cref="CanMountInteral"/> is true.  <see cref="MountInternal"/> only supports immediate completion.
    /// </para>
    /// <para>
    /// See <see cref="StandardAccesory"/> and <see cref="SimpleOffsetAccessory"/> for example implementations.
    /// </para>
    /// </remarks>
    /// <seealso cref="Accessory"/>
    /// <seealso cref="StandardAccessory"/>
    /// <see cref="SimpleOffsetAccessory"/>
    public abstract class AccessoryCore
        : Accessory
    {
        #region State

        /// <summary>
        /// Don't mutate outside of SetState();
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private GameObject m_Owner = null;

        public sealed override GameObject Owner
        {
            get { return m_Owner; }
        }

        /// <summary>
        /// Don't mutate outside of SetState();
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private AccessoryStatus m_Status = AccessoryStatus.Unmanaged;

        public sealed override AccessoryStatus Status
        {
            get { return m_Status; }
        }

        /// <summary>
        /// Don't mutate outside of SetState();
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private MountPoint m_CurrentLocation = null;

        public sealed override MountPoint CurrentLocation
        {
            get { return m_CurrentLocation; }
        }

        private void SetState(AccessoryStatus status, GameObject owner, MountPoint location)
        {
            if (m_Status == status && m_Owner == owner && m_CurrentLocation == location)
                return;

            m_Status = status;
            m_Owner = owner;
            m_CurrentLocation = location;

            OnStateChange();

            m_Observers.SendStateChange(this);
        }

        /// <summary>
        /// A state change event called just before the observers are notified.
        /// </summary>
        protected virtual void OnStateChange()
        {
            // Do nothing.
        }

        #endregion

        #region Coverage

        [SerializeField]
        [HideInInspector]
        private BodyCoverage m_CurrentCoverage;
        public sealed override BodyCoverage CurrentCoverage
        {
            get { return m_CurrentCoverage; }
        }

        #endregion

        #region Limits

        [SerializeField]
        [Tooltip("This accessory will ignore 'accessories limited' flags.")]
        private bool m_IgnoreLimited = false;

        public sealed override bool IgnoreLimited
        {
            get { return m_IgnoreLimited; }
            set { m_IgnoreLimited = value; }
        }

        #endregion

        #region Mounting

        /// <summary>
        /// Mount the accessory to the specified location.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method will always succeed if <see cref="CanMount(MountPoint, BodyCoverage)"/> returns true.
        /// </para>
        /// <para>
        /// Supports lazy calling.  E.g. As an optimitation it is valid to simply call this method on a list of 
        /// available accessories to let the accessory decide whether or not it can mount.
        /// </para>
        /// <para>
        /// <paramref name="additionalCoverage"/> is useful in situations where an accessory uses a generic mounter
        /// that doesn't provide coverage information.  On a successful mount the additional coverage will be
        /// added to the coverage supplied by the mounter and/or built into the accessory.
        /// </para>
        /// <para>
        /// Mounter priority is as follows:
        /// </para>
        /// <para>
        /// <ol>
        /// <li>The priority mounter supplied by the mount method.</li>
        /// <li>The mounter provided by <see cref="GetInitializedMounter"/></li>
        /// <li><see cref="MountInternal"/> if <see cref="CanMountInteral"/> is true</li>
        /// </ol>
        /// </para>
        /// <para>
        /// <see cref="MountInternal"/> only supports immediate completion.
        /// </para>
        /// </remarks>
        /// <param name="location">The mount location. (Required)</param>
        /// <param name="owner">The object that will own the accessory after a successful mount. (Required)</param>
        /// <param name="priorityMounter">
        /// The mounter to attempt before any others are tried. (Optional)
        /// </param>
        /// <param name="additionalCoverage">
        /// Additional coverage to apply on a successful mount, above and beyond the coverage supplied by the 
        /// mounter and/or built into the accessory. (Optional)
        /// </param>
        /// <returns>True if the mount succeeded, otherwise false.</returns>
        public sealed override bool Mount(MountPoint location, GameObject owner, IAccessoryMounter priorityMounter, 
            BodyCoverage additionalCoverage)
        {
            // While not expected to be common, it is technically ok to re-attach to the same
            // mount location, so there is no optimization check for that.

            if (!(location && owner))
            {
                Debug.LogError("Null mount location and/or owner.", this);
                return false;
            };

            if (!LizittUtil.IsUnityDestroyed(priorityMounter) && priorityMounter.InitializeMount(this, location))
            {
                RunMounter(priorityMounter, owner, location, additionalCoverage);
                return true;
            }

            var mounter = GetInitializedMounter(location, owner);
            if (!LizittUtil.IsUnityDestroyed(mounter))
            {
                RunMounter(mounter, owner, location, additionalCoverage);
                return true;
            }

            if (CanMountInternal(location, owner))
            {
                CleanupCurrentState();

                m_CurrentCoverage = MountInternal(location, owner) | additionalCoverage;
                SetState(AccessoryStatus.Mounted, owner, location);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Select and return a mounter that is initialized and ready to update, or null if none is avaiable.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method will only be called if the mount method's priority mounter was not selected.
        /// It is called before trying <see cref="CanMountInternal"/>.
        /// </para>
        /// </remarks>
        /// <param name="location">The mount location. (Required)</param>
        /// <param name="owner">The owner on a successful mount. (Required)</param>
        /// <returns>A mounter that is initialized and ready to update, or null if none is avaiable.</returns>
        protected abstract IAccessoryMounter GetInitializedMounter(MountPoint location, GameObject owner);

        /// <summary>
        /// True if <see cref="MountInternal"/> can mount to the specified location using an immediate completion
        /// mount process.
        /// </summary>
        /// <param name="location">The mount location. (Required)</param>
        /// <param name="owner">The owner on a successful mount. (Required)</param>
        /// <returns>True if <see cref="MountInternal"/> can mount to the specified location.</returns>
        protected abstract bool CanMountInternal(MountPoint location, GameObject owner);

        /// <summary>
        /// Perform an immediate custom mount.  (Must succeed.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method will only be called if all other mount methods have failed and <see cref="CanMountInternal"/>
        /// returns true.
        /// </para>
        /// </remarks>
        /// <param name="location">The mount location. (Required)</param>
        /// <param name="owner">The owner on a successful mount. (Required.)</param>
        /// <param name="resultCoverage">The resulting coverage of the mount.</param>
        protected abstract BodyCoverage MountInternal(MountPoint location, GameObject owner);

        // Will only be assigned if a coroutine is required.
        private IAccessoryMounter m_CurrentMounter = null;

        /// <summary>
        /// Used to detect if a mouter coroutine is still valid.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private int m_MounterId = 0;

        /// <summary>
        /// Performs the first update on the mounter and takes the appropriate action if it completes or needs
        /// more updates.
        /// </summary>
        private void RunMounter(IAccessoryMounter mounter, GameObject owner, MountPoint location, 
            BodyCoverage additionalCoverage)
        {
            CleanupCurrentState();

            int id = m_MounterId + 1;
            m_MounterId = id;

            m_CurrentCoverage = mounter.GetCoverageFor(location) | additionalCoverage;

            if (mounter.UpdateMount(this, location, !Application.isPlaying))
                StartCoroutine(DoDurationMount(mounter, owner, location));
            else
                SetState(AccessoryStatus.Mounted, owner, location);
        }

        /// <summary>
        /// Kicks off a coroutine to run the mounter through to completion.
        /// </summary>
        private System.Collections.IEnumerator DoDurationMount(
            IAccessoryMounter mounter, GameObject owner, MountPoint location)
        {
            m_CurrentMounter = mounter;
            var id = m_MounterId;

            SetState(AccessoryStatus.Mounting, owner, location);

            yield return null;

            while (m_MounterId == id && !LizittUtil.IsUnityDestroyed(mounter) && mounter.UpdateMount(this, location))
                yield return null;

            if (m_MounterId == id)
            {
                SetState(AccessoryStatus.Mounted, owner, location);
                m_CurrentMounter = null;
            }
        }

        #endregion

        #region Release

        public sealed override void Release()
        {
            if (Status != AccessoryStatus.Unmanaged)
                CleanupCurrentState();

            transform.parent = null;
            SetState(AccessoryStatus.Unmanaged, null, null);
        }

        #endregion

        #region Store

        [SerializeField]
        [Tooltip("Deactivate and activate the accessory's GameObject when it transitions into"
            + " and out of the 'stored' state.")]
        private bool m_UseDefaultStorage = true;

        /// <summary>
        /// Deactivate and activate the accessory's GameObject when it transitions into and out of the 'stored' state.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Deactivation will happen after the 'store' event.  Activation will happen before the 'unstore' event.
        /// </para>
        /// </remarks>
        public bool UseDefaultStorage
        {
            get { return m_UseDefaultStorage; }
            set { m_UseDefaultStorage = value; }
        }

        public sealed override bool Store(GameObject owner)
        {
            if (!owner)
            {
                Debug.LogError("Can't store with a null owner.", this);
                return false;
            }

            if (Status == AccessoryStatus.Stored)
            {
                if (Owner != owner)
                    SetState(AccessoryStatus.Stored, owner, null);

                return true;
            }

            CleanupCurrentState();
            transform.parent = null;

            SetState(AccessoryStatus.Stored, owner, null);

            if (m_UseDefaultStorage)  // After status change event.
                gameObject.SetActive(false);

            return true;
        }

        #endregion

        #region Destroy

        public sealed override void Destroy(DestroyType typ, bool prepareOnly)
        {
            OnDestroyLocal(typ);

            m_Observers.SendDestroy(this, typ);

            if (!prepareOnly)
            {
                if (typ == DestroyType.GameObject)
                    gameObject.SafeDestroy();
                else
                    this.SafeDestroy();
            }
        }

        /// <summary>
        /// A destroy event called before any other action is taken.  (Pre-destroy)
        /// </summary>
        /// <param name="typ">The destroy type.</param>
        protected virtual void OnDestroyLocal(DestroyType typ)
        {
            // Design note: Can't use 'OnDestroy' because is conflicts with the Monobehaviour event signature.
            // Do nothing.
        }

        #endregion

        #region Transitions

        /// <summary>
        /// Performs cleanups of settings unique to the current state.  (Call before all state transitions.)
        /// </summary>
        private void CleanupCurrentState()
        {
            // Remember, transition is out of the current state.

            switch (Status)
            {
                case AccessoryStatus.Stored:

                    if (m_UseDefaultStorage)
                        gameObject.SetActive(true);

                    break;

                case AccessoryStatus.Mounted:
                case AccessoryStatus.Mounting:

                    m_CurrentCoverage = 0;

                    if (!LizittUtil.IsUnityDestroyed(m_CurrentMounter))  // Is mounting.
                    {
                        m_CurrentMounter.CancelMount(this, CurrentLocation);
                        m_CurrentMounter = null;
                    }

                    break;
            }
        }

        #endregion

        #region Observer Features

        [Space(5)]
        [SerializeField]
        [ObjectList("IAccessoryObserver Objects", typeof(IAccessoryObserver))]
        private AccessoryObserverGroup m_Observers = new AccessoryObserverGroup(2);

        public sealed override bool AddObserver(IAccessoryObserver observer)
        {
            return m_Observers.Add(observer, this) != -1;
        }

        public sealed override void RemoveObserver(IAccessoryObserver observer)
        {
            m_Observers.Remove(observer);
        }

        #endregion

        #region Initialization

        public virtual void Initialize()
        {
            // Do nothing.
        }

        protected void Awake()
        {
            Initialize();
        }

        #endregion

#if UNITY_EDITOR

        #region Editor Only

        protected override void GetUndoObjects(System.Collections.Generic.List<Object> list)
        {
            for (int i = 0; i < m_Observers.Count; i++)
            {
                var item = m_Observers[i];

                if (item != null)
                    list.Add(item as Object);
            }

            list.Add(transform);  // Repositioning
            list.Add(gameObject);  // Activate
            list.Add(this);
        }

        #endregion

#endif
    }
}
