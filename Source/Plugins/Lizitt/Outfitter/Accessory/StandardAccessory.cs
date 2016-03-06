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
    /// This component is useful when the accessory needs to support multiple and/or complex mount senarios.  It
    /// supports up to three levels of mounters that are tried in the following order:
    /// </para>
    /// </remarks>
    [AddComponentMenu(OutfitterUtil.Menu + "Standard Accessory", OutfitterUtil.AccessoryComponentMenuOrder + 0)]
    [SelectionBase]
    public class StandardAccessory
        : AccessoryCore
    {
        #region Coverage

        public sealed override BodyCoverage GetCoverageFor(MountPoint location)
        {
            if (Accessory.CanMount(this, PriorityMounter, location, 0))
                return PriorityMounter.GetCoverageFor(location);

            for (int i = 0; i < m_Mounters.Count; i++)
            {
                if (Accessory.CanMount(this, m_Mounters[i], location, 0))
                    return m_Mounters[i].GetCoverageFor(location);
            }

            return 0;
        }

        #endregion

        #region Mounters & Limits

        [SerializeField]
        [Tooltip("Use the default mounter if no other mounter is available. (Will immediately parent the accessory"
            + " to any mount point with no offsets and no coverage.")]
        private bool m_UseDefaultMounter = false;

        /// <summary>
        /// Use the default mounter if no other mounter is available.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The default mounter will immediately parent the accessory to any mount point with no offsets and no
        /// coverage. (Though the mount method's 'additional coverage' parameter can be used to apply 
        /// coverage.)   
        /// </para>
        /// <para>
        /// <ol>
        /// <li>The mounter provided to the mount method.</li>
        /// <li><see cref="PriorityMounter"/></li>
        /// <li>The 'normal' mounters in the order of their list.</li>
        /// </ol>
        /// </para>
        /// </remarks>
        public bool UseDefaultMounter
        {
            get { return m_UseDefaultMounter; }
            set { m_UseDefaultMounter = value; }
        }

        /// <summary>
        /// Only access through the property.
        /// </summary>
        [SerializeField]
        [ObjectList("IAccessoryMounter Objects", typeof(IAccessoryMounter))]
        private AccessoryMounterGroup m_Mounters = new AccessoryMounterGroup(1);

        /// <summary>
        /// The maximum number of mounters the accessory contains.  (Some may be null.)
        /// </summary>
        public int MounterCount
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
        /// The index [0 &tl= value &lt; <see cref="MounterCount"/>]
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
        /// </remarks>
        /// <param name="accessory">The accessory. (Required)</param>
        /// <param name="mountPoints">The replacement mounters. (Required)</param>
        public static bool UnsafeReplaceMounters(StandardAccessory accessory, params IAccessoryMounter[] mounters)
        {
            AccessoryMounterGroup.UnsafeReplaceItems(accessory, accessory.m_Mounters, mounters);
            return true;
        }

        /// <summary>
        /// Clear all mounters.  (Except <see cref="PriorityMounter"/>.)
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
        /// When mounting, mounters are tried in the following order:
        /// </para>
        /// <para>
        /// <ol>
        /// <li>The mounter provided to the mount method.</li>
        /// <li>This mounter.</li>
        /// <li>The 'normal' mounters in the order of their list.</li>
        /// </ol>
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
                    Debug.LogError(value.GetType().Name + " is not a UnityEngine.Object.");
            }
        }

        [SerializeField]
        [Tooltip("The location the accessory can always mount to. (The default mounter will be used if no"
            + " other mounter is avaiable.)")]
        [SortedEnumPopup(typeof(MountPointType))]
        private MountPointType m_DefaultLocation;  // Type is by custom editor. <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

        /// <summary>
        /// The location the accessory can always mount to.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The default mounter will be used if no other mounter is avaiable.  (Even if it is disabled.)
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

        public sealed override bool CanMount(MountPoint location, BodyCoverage restrictions)
        {
            if (!location)
                return false;

            if (m_UseDefaultMounter)
                return true;

            if (Accessory.CanMount(this, PriorityMounter, location, restrictions))
                return true;

            return m_Mounters.CanMount(this, location, restrictions) != -1;
        }

        #endregion

        #region Mounting

        protected sealed override IAccessoryMounter GetInitializedMounter(MountPoint location, GameObject owner)
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

        #region Editor Only

        protected override void GetUndoObjects(System.Collections.Generic.List<Object> list)
        {
            base.GetUndoObjects(list);

            if (m_PriorityMounter)
                list.Add(m_PriorityMounter);

            for (int i = 0; i < m_Mounters.Count; i++)
            {
                var item = m_Mounters[i];

                if (item != null)
                    list.Add(item as Object);
            }
        }

        #endregion

#endif
    }
}

