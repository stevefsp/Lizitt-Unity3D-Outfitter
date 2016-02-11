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
    /// An accessory that supports multiple <see cref="IAccessoryMounter"/> components.
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
            if (Accessory.CanMount(this, PriorityMounter, locationType, 0))
                return PriorityMounter.GetCoverageFor(locationType);

            for (int i = 0; i < m_Mounters.Count; i++)
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

        [SerializeField]
        [ObjectList("IAccessoryMounter Objects", typeof(IAccessoryMounter))]
        private AccessoryMounterGroup m_Mounters = new AccessoryMounterGroup(0);  // Required by custom editor. <<<<<<<<

        /// <summary>
        /// The size of the mounter buffer.
        /// </summary>
        public int MounterBufferSize
        {
            get { return m_Mounters.Count; }
        }

        /// <summary>
        /// Get the mounter at the specified index, or null if there is none.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method will never return a reference to a destroyed object.  (Always returns a true null.)
        /// </para>
        /// </remarks>
        /// <param name="index">
        /// The index [0 &tl= value &lt; <see cref="MounterBufferSize"/>]
        /// </param>
        /// <returns>The mounter at the specified index, or null if there is none.</returns>
        public IAccessoryMounter GetMounter(int index)
        {
            return m_Mounters[index];
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
        public static bool UnsafeReplaceMounters(StandardAccessory accessory, params IAccessoryMounter[] mounters)
        {
            AccessoryMounterGroup.UnsafeReplaceItems(accessory, accessory.m_Mounters, mounters);
            return true;
        }

        /// <summary>
        /// Clear all mounters.
        /// </summary>
        /// <para>
        /// Behavior is undefined if this method if used after accessory initialization.
        /// </para>
        /// <param name="accessory">The accessory. (Required.)</param>
        public static void UnSafeClearMounters(StandardAccessory accessory)
        {
            accessory.m_Mounters.Clear();
        }

        [SerializeField]
        [HideInInspector]
        private Object m_PriorityMounter = null;

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
        public IAccessoryMounter PriorityMounter
        {
            get { return m_PriorityMounter as IAccessoryMounter; }
            set
            {
                if (value == null)
                    m_PriorityMounter = null;
                else if (value is Object)
                {
                    var obj = value as Object;
                    m_PriorityMounter = obj ? obj : null;
                }
                else
                    Debug.LogError(value.GetType().Name + " is not a Unity Object.");
            }
        }

        [SerializeField]
        [Tooltip("The location type the accessory can always mount to. (The default mounter will be used if no"
            + " other mounter is avaiable.)")]
        private MountPointType m_DefaultLocation;  // Required by custom editor. <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

        /// <summary>
        /// The location type the accessory can always mount to.
        /// </summary>
        /// <remarks>
        /// <para>
        /// It no other mounter is available, then the default mounter will be used, even if it is disabled.
        /// </para>
        /// </remarks>
        public sealed override MountPointType DefaultLocationType
        {
            get { return m_DefaultLocation; }
        }

        /// <summary>
        /// Sets the value of <see cref="DefaultLocationType"/>.
        /// </summary>
        /// <param name="locationType">The new default location type.</param>
        public void SetDefaultLocationType(MountPointType locationType)
        {
            m_DefaultLocation = locationType;
        }

        public sealed override bool CanMount(MountPointType locationType, BodyCoverage restrictions)
        {
            if (m_UseDefaultMounter)
                return true;

            if (Accessory.CanMount(this, PriorityMounter, locationType, restrictions))
                return true;

            return m_Mounters.CanMount(this, locationType, restrictions) != -1;
        }

        #endregion

        #region Mounting

        protected override IAccessoryMounter GetInitializedMounter(MountPoint location, GameObject owner)
        {
            if (!LizittUtil.IsUnityDestroyed(m_PriorityMounter) && PriorityMounter.InitializeMount(this, location))
                return PriorityMounter;

            return m_Mounters.GetInitializedMounter(this, location);
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
                PriorityMounter.OnAccessoryDestroy(this, typ);

            m_Mounters.SendAccessoryDestroy(this, typ);
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

            m_Mounters.PurgeDestroyed();

            var seen = new System.Collections.Generic.List<IAccessoryMounter>();

            // TODO: Remove this once the GUI properly prevents duplicates.
            for (int i = m_Mounters.Count - 1; i >= 0; i--)
            {
                if (seen.Contains(m_Mounters[i]))
                    m_Mounters.Remove(m_Mounters[i]);
                else
                    seen.Add(m_Mounters[i]);
            }

            var refreshItems = GetComponents<IAccessoryMounter>();
            if (refreshItems.Length == 0)
                return;  // Leave existing alone.

            var items = new System.Collections.Generic.List<IAccessoryMounter>(refreshItems.Length);

            for (int i = 0; i < m_Mounters.Count; i++)
            {
                if (m_Mounters[i] != null)
                    items.Add(m_Mounters[i]);
            }

            // Add new items to end.
            foreach (var refreshItem in refreshItems)
            {
                if (!items.Contains(refreshItem))
                    items.Add(refreshItem);
            }

            AccessoryMounterGroup.UnsafeReplaceItems(this, m_Mounters, items.ToArray());
        }

        [ContextMenu("Reset Mounters")]
        private void ResetMounters_Menu()
        {
            m_Mounters.Clear();
        }

        #endregion

#endif
    }
}

