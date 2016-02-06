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
    /// An accessory that supports multiple <see cref="AccessoryMounter"/> components.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This component is useful when the accessory needs to support multiple and/or complex mount senarios.
    /// </para>
    /// </remarks>
    public class StandardAccessory
        : AccessoryCore
    {
        #region Coverage

        public sealed override BodyCoverage GetCoverageFor(MountPointType locationType)
        {
            if (Accessory.CanMount(this, m_PriorityMounter, locationType, 0))
                return m_PriorityMounter.GetCoverageFor(locationType);

            for (int i = 0; i < m_Mounters.BufferSize; i++)
            {
                if (Accessory.CanMount(this, m_Mounters[i], locationType, 0))
                    return m_Mounters[i].GetCoverageFor(locationType);
            }

            return 0;
        }

        #endregion

        #region Mounters & Limits

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

        [Space]
        [SerializeField]
        private AccessoryMounterGroup m_Mounters = new AccessoryMounterGroup(0);

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

        /// <summary>
        /// The location type the accessory can mount to.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value is the default location of the highest priority mounter assigned to the accessory, either
        /// <see cref="PriorityMounter"/> or the first avaiable 'normal' mounter.  If, due to a configuration issue,
        /// there are no mounters, then 'root' will be returned.  In this case the accessory will use the default
        /// mounter to mount to 'root' no matter the value of <see cref="UseDefaultMounter"/>.
        /// </para>
        /// </remarks>
        public sealed override MountPointType DefaultLocationType
        {
            get
            {
                if (m_PriorityMounter)
                    return m_PriorityMounter.DefaultLocationType;

                return m_Mounters.DefaultLocationType;
            }
        }

        public sealed override bool CanMount(MountPointType locationType, BodyCoverage restrictions)
        {
            if (m_UseDefaultMounter)
                return true;

            if (Accessory.CanMount(this, m_PriorityMounter, locationType, restrictions))
                return true;

            // TODO: Transfer this functionality tot he mounter group.
            for (int i = 0; i < m_Mounters.BufferSize; i++)
            {
                if (Accessory.CanMount(this, m_Mounters[i], locationType, restrictions))
                    return true;
            }

            return false;
        }

        #endregion

        #region Mounting

        protected override AccessoryMounter GetInitializedMounter(MountPoint location, GameObject owner)
        {
            if (m_PriorityMounter != null && m_PriorityMounter.InitializeMount(this, location))
                return m_PriorityMounter;

            // TODO: EVAL: Move this to the mounter group?
            for (int i = 0; i < m_Mounters.BufferSize; i++)
            {
                if (m_Mounters[i] && m_Mounters[i].InitializeMount(this, location))
                    return m_Mounters[i];
            }

            return null;
        }

        protected sealed override bool CanMountInternal(MountPoint location, GameObject owner)
        {
            return (m_UseDefaultMounter || location.LocationType == DefaultLocationType);
        }

        protected sealed override BodyCoverage MountInternal(MountPoint location, GameObject owner)
        {
            if (!m_UseDefaultMounter)
            {
                Debug.LogWarning("Unexpected configuation: No mounter found for default mount location."
                    + " Used default mounter: " + location.LocationType, this);
            }

            transform.parent = location.transform;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            // The default mounter never has coverage.
            return 0;
        }

        #endregion

        #region Destroy

        protected override void OnDestroyLocal(DestroyType typ)
        {
            if (m_PriorityMounter)
                m_PriorityMounter.OnAccessoryDestroy(this, typ);

            // TODO: Move this to the mounter group.
            for (int i = 0; i < m_Mounters.BufferSize; i++)
            {
                if (m_Mounters[i])
                    m_Mounters[i].OnAccessoryDestroy(this, typ);
            }
        }

        #endregion

#if UNITY_EDITOR

        #region Context Menu

        [ContextMenu("Refresh All Settings")]
        private void RefreshAllSettings_Menu()
        {
            RefreshMounters_Menu();
            RefreshObservers_Menu();
        }

        [ContextMenu("Refresh Mounters")]
        private void RefreshMounters_Menu()
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

        [ContextMenu("Reset Mounters")]
        private void ResetMounters_Menu()
        {
            m_Mounters.Clear(0);
        }

        #endregion

#endif
    }
}

