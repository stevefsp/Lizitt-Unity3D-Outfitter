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
    /// Implements the most common shared <see cref="Accessory"/> features.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class manages state, state transtions, observers, and events.  It also handles storage, release, and 
    /// implements a framework for mounting.  Concrete implementations can intercept observer events by overriding 
    /// local event methods.  (E.g. <see cref="OnStateChangeLocal"/>.  Mount behavior is implemented by overriding 
    /// <see cref="GetInitializedMounter"/> and/or the 'internal mount' methods.  (At least one mount method must
    /// be overridden.)
    /// </para>
    /// <para>
    /// Mount priority is as follows:  The priority mounter supplied by the mount method, 
    /// the mounter provided by <see cref="GetInitializedMounter"/>, <see cref="MountInternal"/> if 
    /// <see cref="CanMountInteral"/> is true.  <see cref="MountInternal"/> only supports immediate completion
    /// mounting.
    /// </para>
    /// <para>
    /// See <see cref="StandardMounter"/> and <see cref="SimpleMounter"/> for example implementations.
    /// </para>
    /// </remarks>
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

            OnStateChangeLocal();

            m_Observers.SendStateChange(this);
        }

        /// <summary>
        /// Called on the state change event, just before the observers are notified.
        /// </summary>
        protected virtual void OnStateChangeLocal()
        {
            // Do nothing.
            // Don't use OnStateChange() naming convension because can't use OnDestory() for the destory event.
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
        /// Mount the accessory to the specified mount point.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is guarenteed to return true if <see cref="CanMount"/> returns true. 
        /// But it is valid to use a call to this method without pre-checking mountability. E.g. As an optimitation, 
        /// it is valid to simply call this method on a list of all available accessories to let the accessory 
        /// decide whether or not it can attach
        /// </para>
        /// <para>
        /// <paramref name="additionalCoverage"/> is useful in when an accessory  uses a generic mounter that doesn't
        /// provide coverage information.
        /// </para>
        /// <para>
        /// Mount priority is as follows:  The priority mounter supplied by the mount method, 
        /// the mounter provided by <see cref="GetInitializedMounter"/>, <see cref="MountInternal"/> if 
        /// <see cref="CanMountInteral"/> is true.  <see cref="MountInternal"/> only supports immediate completion
        /// mounting.
        /// </para>
        /// </remarks>
        /// <param name="location">The mount location. (Required)</param>
        /// <param name="owner">
        /// The object that will own the accessory after a successful mount. (Required)
        /// </param>
        /// <param name="priorityMounter">
        /// The mounter to attempt before any others are tried.  (I.e. A custom mounter.)
        /// </param>
        /// <param name="additionalCoverage">
        /// Additional coverage to apply on a successful mount, above and beyond the coverage
        /// supplied by the mounter or built into the accessory.
        /// </param>
        /// <returns>True if the mount succeeded, otherwise false.</returns>
        public sealed override bool Mount(MountPoint location, GameObject owner, 
            AccessoryMounter priorityMounter, BodyCoverage additionalCoverage)
        {
            // While not expected to be common, it is technically ok to re-attach to the same
            // mount location.  So there is no optimization check for that.

            if (!(location && owner))
            {
                Debug.LogError("Null mount location and/or owner.", this);
                return false;
            };

            if (priorityMounter && priorityMounter.InitializeMount(this, location))
            {
                RunMounter(priorityMounter, owner, location, additionalCoverage);
                return true;
            }

            var mounter = GetInitializedMounter(location, owner);
            if (mounter)
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
        /// This method will only be called if the <see cref="Mount"/> method's priority mounter was not selected.
        /// It is called before trying <see cref="CanMountInternal"/>.
        /// </para>
        /// </remarks>
        /// <param name="location">The mount location. (Required)</param>
        /// <param name="owner">The owner on a successful mount. (Required)</param>
        /// <returns>A mounter that initialized and ready to update, or null if none is avaiable.</returns>
        protected abstract AccessoryMounter GetInitializedMounter(MountPoint location, GameObject owner);

        /// <summary>
        /// True if <see cref="MountInternal"/> can mount to the specified location using an immediate completion
        /// internal mount process.
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
        /// <param name="owner">The owener on a successful mount. (Required.</param>
        /// <param name="resultCoverage">The resulting coverage of the mount.</param>
        protected abstract BodyCoverage MountInternal(MountPoint location, GameObject owner);

        // Don't serialize.  Will only be assigned if a coroutine is required.
        private AccessoryMounter m_CurrentMounter = null;

        /// <summary>
        /// Used so mouter coroutines know if they are still valid.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private int m_MounterId = 0;

        /// <summary>
        /// Performs the first update on the mounter and takes the appropriate action if it completes or needs
        /// more updates.
        /// </summary>
        private void RunMounter(AccessoryMounter mounter, GameObject owner, MountPoint location, 
            BodyCoverage additionalCoverage)
        {
            CleanupCurrentState();

            int id = m_MounterId + 1;
            m_MounterId = id;

            m_CurrentCoverage = mounter.GetCoverageFor(location.LocationType) | additionalCoverage;

            if (mounter.UpdateMount(this, location))
                StartCoroutine(DoDurationMount(mounter, owner, location));
            else
                SetState(AccessoryStatus.Mounted, owner, location);
        }

        // TODO: EVAL: Convert mounting to a method that supports serialization.  
        // This is not a high proiority, especially since the standard mounters support
        // immediate completion outside of play mode.  But having all major features except
        // mounting provide support for serialization might be an issue. 

        /// <summary>
        /// Kicks off a coroutine to run the mounter through to completion.
        /// </summary>
        private System.Collections.IEnumerator DoDurationMount(
            AccessoryMounter mounter, GameObject owner, MountPoint location)
        {
            m_CurrentMounter = mounter;
            var id = m_MounterId;

            SetState(AccessoryStatus.Mounting, owner, location);

            yield return null;

            while (m_MounterId == id && mounter.UpdateMount(this, location))
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

            SetState(AccessoryStatus.Unmanaged, null, null);
        }

        #endregion

        #region Store

        [SerializeField]
        [Tooltip("Deactivate and activate the accessory's GameObject when it transitions into"
            + " and out of the 'stored' state. (This will happen after the 'store' event and"
            + " before the 'unstore' event.)")]
        private bool m_UseDefaultStorage = true;

        /// <summary>
        /// Deactivate and activate the accessory's GameObject when it transitions into
        /// and out of the 'stored' state.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Deactivation/activation will happen after the 'store' event and before the 
        /// 'unstore' event.
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

            SetState(AccessoryStatus.Stored, owner, null);

            if (m_UseDefaultStorage)  // After status change event.
                gameObject.SetActive(false);

            return true;
        }

        #endregion

        #region Destroy

        public sealed override void Destroy(DestroyType typ)
        {
            OnDestroyLocal(typ);

            m_Observers.SendDestroy(this, typ);
             
            if (typ == DestroyType.GameObject)
                gameObject.SafeDestroy();
            else
                this.SafeDestroy();
        }

        /// <summary>
        /// Called on the destory event before any other action is taken.
        /// </summary>
        /// <param name="typ"></param>
        protected virtual void OnDestroyLocal(DestroyType typ)
        {
            // Do nothing.
        }

        #endregion

        #region Transitions

        /// <summary>
        /// Performs cleanups of settings unique to the current state.  
        /// (Call before all state transitions.)
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

                    transform.parent = null;
                    m_CurrentCoverage = 0;

                    if (m_CurrentMounter)  // Is mounting.
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
        private AccessoryObserverGroup m_Observers = new AccessoryObserverGroup(0);

        public sealed override bool AddObserver(IAccessoryObserver observer)
        {
            return m_Observers.Add(observer, this) != -1;
        }

        public sealed override void RemoveObserver(IAccessoryObserver observer)
        {
            m_Observers.Remove(observer);
        }

        #endregion

#if UNITY_EDITOR

        #region Context Menu

        [ContextMenu("Refresh Observers")]
        protected void RefreshObservers_Menu()
        {
            // Design note: This process is designed to support observers that have been linked
            // from other game objects.

            m_Observers.PurgeNulls();

            var refreshItems = this.GetComponents<IAccessoryObserver>();
            if (refreshItems.Length == 0)
                return;  // Leave existing alone.

            // Add new items to end.
            foreach (var refreshItem in refreshItems)
            {
                if (!m_Observers.Contains(refreshItem))
                    m_Observers.Add(refreshItem);
            }
        }

        [ContextMenu("Reset Observers")]
        protected void ResetObservers_Menu()
        {
            m_Observers.Clear();
        }

        #endregion

#endif
    }
}
