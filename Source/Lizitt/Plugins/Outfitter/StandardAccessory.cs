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
    /// An generalized accessory that implements all of the standard accessory behavior.
    /// </summary>
    public class StandardAccessory
        : Accessory
    {
        /*
         * Design note:
         * 
         * It is not worth implementing an abstract core class for accessories.  There isn't 
         * enough potential code reuse to justify it since the vast majority of code revolves 
         * around mounting, baking, and storage, none of which are appropriate for a core class.
         */

        #region State

        /// <summary>
        /// Don't mutate outside of SetState();
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private GameObject m_Owner = null;

        public override GameObject Owner
        {
            get { return m_Owner; }
        }

        /// <summary>
        /// Don't mutate outside of SetState();
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private AccessoryStatus m_Status = AccessoryStatus.Unmanaged;

        public override AccessoryStatus Status
        {
            get { return m_Status; }
        }

        /// <summary>
        /// Don't mutate outside of SetState();
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private MountPoint m_CurrentLocation = null;

        public override MountPoint CurrentLocation
        {
            get { return m_CurrentLocation; }
        }

        public override bool IsMountedTo(MountPointType locationType)
        {
            return m_CurrentLocation != null && m_CurrentLocation.LocationType == locationType;
        }

        private void SetState(AccessoryStatus status, GameObject owner, MountPoint location)
        {
            if (m_Status == status && m_Owner == owner && m_CurrentLocation == location)
                return;

            m_Status = status;
            m_Owner = owner;
            m_CurrentLocation = location;

            m_Observers.SendStateChange(this);
        }

        #endregion

        #region Coverage

        [SerializeField]
        [HideInInspector]
        private BodyCoverage m_CurrentCoverage;
        public override BodyCoverage CurrentCoverage
        {
            get { return m_CurrentCoverage; }
        }

        public override BodyCoverage GetCoverageFor(MountPointType locationType)
        {
            if (CanMount(m_PriorityMounter, locationType, 0))
                return m_PriorityMounter.GetCoverageFor(locationType);

            for (int i = 0; i < m_Mounters.BufferSize; i++)
            {
                if (CanMount(m_Mounters[i], locationType, 0))
                    return m_Mounters[i].GetCoverageFor(locationType);
            }

            return 0;
        }

        #endregion

        #region Mounters & Limits

        [Space]
        [SerializeField]
        private AccessoryMounterGroup m_Mounters = new AccessoryMounterGroup(0);

        [SerializeField]
        [Tooltip("This accessory will ignore 'accessories limited' flags.")]
        private bool m_IgnoreLimited = false;

        public override bool IgnoreLimited
        {
            get { return m_IgnoreLimited; }
            set { m_IgnoreLimited = value; }
        }

        [SerializeField]
        [Tooltip("If true and no other mounter is available, use the default mounter."
            + " (The default mounter will immediately parent and snap the accesory to any"
            + " mount point.)")]
        private bool m_UseDefaultMounter = false;

        /// <summary>
        /// If true and no other mounter is available, use the default mounter.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The default mounter will immediately parent and snap the accesory to any mount point
        /// with no offsets and no coverage.  (Though the <see cref="Mount"/> method's 
        /// additional coverage parameter can be used to apply coverage.)   
        /// </para>
        /// </remarks>
        public bool UseDefaultMounter
        {
            get { return m_UseDefaultMounter; }
            set { m_UseDefaultMounter = value; }
        }

        /// <summary>
        /// The size of the mounter buffer.
        /// </summary>
        public int MounterBufferSize
        {
            get { return m_Mounters.BufferSize; }
        }

        /// <summary>
        /// Get the mounter at the specified index, or null if there is none.
        /// </summary>
        /// <param name="index">
        /// The index [0 &tl= value &lt; <see cref="MounterBufferSize"/>]
        /// </param>
        /// <returns>The mounter at the specified index, or null if there is none.</returns>
        public AccessoryMounter GetMounter(int index)
        {
            return m_Mounters[index];
        }

        /// <summary>
        /// Set the mounter at the specified index.  (Nulls allowed.)
        /// </summary>
        /// <param name="index">
        /// The index  [0 &tl= value &lt; <see cref="MounterBufferSize"/>]
        /// </param>
        /// <param name="mounter">The mounter. (Null allowed.)</param>
        public void SetMounter(int index, AccessoryMounter mounter)
        {
            m_Mounters[index] = mounter;
        }

        /// <summary>
        /// Replaces all current mounters with the provided mounters.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Behavior is undefined if this method if used after accessory initialization.
        /// </para>
        /// <para>
        /// If <paramref name="asReference"/> is true, then all external references to the
        /// mounters array must be discared or behavior will be undefined.
        /// </para>
        /// </remarks>
        /// <param name="accessory">The accessory. (Required)</param>
        /// <param name="asReference">If true the accessory will use the reference to the array.  
        /// Otherwise the array will be copied.</param>
        /// <param name="mountPoints">The mounters, or null to clear all mounters.</param>
        public static bool UnsafeReplaceMounters(
            StandardAccessory accessory, bool asReference, params AccessoryMounter[] mounters)
        {
            AccessoryMounterGroup.UnsafeReplaceItems(accessory.m_Mounters, asReference, mounters);
            return true;
        }

        /// <summary>
        /// Clear all mounters.
        /// </summary>
        /// <para>
        /// Behavior is undefined if this method if used after accessory initialization.
        /// </para>
        /// <param name="accessory">The accessory. (Required.)</param>
        /// <param name="bufferSize">
        /// The new buffer size or -1 for no change in the buffer size. [Limit: >= 0, or -1]
        /// </param>
        public static void UnSafeClearMounters(StandardAccessory accessory, int bufferSize = -1)
        {
            accessory.m_Mounters.Clear(bufferSize);
        }

        [SerializeField]
        [HideInInspector]
        private AccessoryMounter m_PriorityMounter = null;

        /// <summary>
        /// The mounter that will be tried before the 'normal' mounters.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When mounting, mounters are tried in the following order:  The mount method's
        /// priority mounter, this priority mounter, then the 'normal' mounters in the order of their
        /// list.
        /// </para>
        /// </remarks>
        public AccessoryMounter PriorityMounter
        {
            get { return m_PriorityMounter; }
            set { m_PriorityMounter = value; }
        }

        public override bool CanMount(MountPointType locationType, BodyCoverage restrictions)
        {
            if (m_UseDefaultMounter)
                return true;

            if (CanMount(m_PriorityMounter, locationType, restrictions))
                return true;

            for (int i = 0; i < m_Mounters.BufferSize; i++)
            {
                if (CanMount(m_Mounters[i], locationType, restrictions))
                    return true;
            }

            return false;
        }

        private bool CanMount(
            AccessoryMounter mounter, MountPointType locationType, BodyCoverage restrictions)
        {
            if (mounter != null && (mounter.GetCoverageFor(locationType) & restrictions) == 0)
                return mounter.CanMount(this, locationType);
                        
            return false;                
        }

        #endregion

        #region Mounting

        /// <summary>
        /// Mount the accessory to the specified mount point.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is guarenteed to return true if <see cref="CanMount"/> returns true and
        /// no <paramref name="priorityMounter"/> is used. But it is valid to use a call to this
        /// method without pre-checking mountability. E.g. As an optimitation, it is valid to
        /// simply call this method on a list of all available accessories to let the accessory 
        /// decide whether or not it can attach.
        /// </para>
        /// <para>
        /// <paramref name="additionalCoverage"/> is useful in situations where an accessory 
        /// uses a generic mounter that doesn't provide coverage information.
        /// </para>
        /// <para>
        /// When mounting, mounters are tried in the following order:  This method's
        /// priority mounter, <see cref="PriortyMounter"/>, then the 'normal' mounters in the order 
        /// of their list.
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
        public override bool Mount(MountPoint location, GameObject owner, 
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
                LocalMount(priorityMounter, owner, location, additionalCoverage);
                return true;
            }

            if (m_PriorityMounter != null && m_PriorityMounter.InitializeMount(this, location))
            {
                LocalMount(m_PriorityMounter, owner, location, additionalCoverage);
                return true;
            }

            for (int i = 0; i < m_Mounters.BufferSize; i++)
            {
                if (m_Mounters[i] && m_Mounters[i].InitializeMount(this, location))
                {
                    LocalMount(m_Mounters[i], owner, location, additionalCoverage);
                    return true;
                }
            }

            if (m_UseDefaultMounter)
            {
                CleanupCurrentState();

                transform.parent = location.transform;
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;

                m_CurrentCoverage = additionalCoverage;

                SetState(AccessoryStatus.Mounted, owner, location);  // Keep last for event.

                return true;
            }

            return false;
        }

        [SerializeField]
        [HideInInspector]
        private AccessoryMounter m_CurrentMounter = null;

        /// <summary>
        /// Used so mouter coroutines know if they are still valid.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private int m_MounterId = 0;

        public void LocalMount(AccessoryMounter mounter, GameObject owner, MountPoint location, 
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

        public override void Release()
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

        public override bool Store(GameObject owner)
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

        public override void Destroy(DestroyType typ)
        {
            m_Observers.SendDestroy(this, typ);

            if (m_PriorityMounter)
                m_PriorityMounter.OnAccessoryDestroy(this, typ);

            for (int i = 0; i < m_Mounters.BufferSize; i++)
            {
                if (m_Mounters[i])
                    m_Mounters[i].OnAccessoryDestroy(this, typ);
            }
             
            if (typ == DestroyType.GameObject)
                gameObject.SafeDestroy();
            else
                this.SafeDestroy();
        }

        #endregion

        #region Transitions

        /// <summary>
        /// Performs cleanups of settings unique to the current state.  
        /// (Call before all state transitions.)
        /// </summary>
        private void CleanupCurrentState()
        {
            // Remember, transition is out of the current status.

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

        [ContextMenu("Refresh All Settings")]
        private void RefreshAll()
        {
            RefreshMounters();
            RefreshObservers();
        }

        [ContextMenu("Refresh Mounters")]
        private void RefreshMounters()
        {
            // Design note: This process is designed to support mounters that have been linked
            // from other game objects.

            var refreshItems = GetComponents<AccessoryMounter>();
            if (refreshItems.Length == 0)
                return;  // Leave existing alone.

            var items = new System.Collections.Generic.List<AccessoryMounter>(refreshItems.Length);

            for (int i = 0; i < m_Mounters.BufferSize; i++)
            {
                if (m_Mounters[i])
                    items.Add(m_Mounters[i]);
            }

            // Add new items to end.
            foreach (var refreshItem in refreshItems)
            {
                if (!items.Contains(refreshItem))
                    items.Add(refreshItem);
            }

            AccessoryMounterGroup.UnsafeReplaceItems(m_Mounters, true, items.ToArray());
        }

        [ContextMenu("Refresh Observers")]
        private void RefreshObservers()
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

        [ContextMenu("Reset Mounters")]
        private void ResetMounters()
        {
            m_Mounters.Clear(0);
        }

        [ContextMenu("Reset Observers")]
        private void ResetObservers()
        {
            m_Observers.Clear();
        }

        #endregion

#endif
    }
}
