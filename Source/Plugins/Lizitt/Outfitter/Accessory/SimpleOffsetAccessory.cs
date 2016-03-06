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
    /// An accessory with a built-in mounter that immediately mounts to its location with optional position and 
    /// rotation offsets.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The mount operation will always succeed if the mount point's location type is the same as 
    /// <see cref="DefaultLocationType"/> and the supplied owner is non-null.
    /// </para>
    /// </remarks>
    [AddComponentMenu(OutfitterUtil.Menu + "Simple Offset Accessory", OutfitterUtil.AccessoryComponentMenuOrder + 1)]
    [SelectionBase]
    public class SimpleOffsetAccessory
        : AccessoryCore
    {
        [SerializeField]
        [Tooltip("The location the accessory can mount to.")]
        [SortedEnumPopup(typeof(MountPointType))]
        private MountPointType m_Location = (MountPointType)0;  // Type is required by custom editor. <<<<<<<<<<<<<<<<<<

        /// <summary>
        /// The location the accesory can mount to.
        /// </summary>
        public sealed override MountPointType DefaultLocationType
        {
            get { return m_Location; }
        }

        /// <summary>
        /// Set the location the accessory can mount to.
        /// </summary>
        public void SetDefaultLocationType(MountPointType value)
        {
            m_Location = value;
        }

        [SerializeField]
        [Tooltip("The coverage of the accessory on a successful mount operation.")]
        [EnumFlags(typeof(BodyCoverage), OutfitterUtil.SortBodyCoverage)]
        private BodyCoverage m_Coverage = 0;  // Type is required by custom editor. <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

        /// <summary>
        /// The coverage of the accessory on a successful mount operation.
        /// </summary>
        public BodyCoverage MountedCoverage
        {
            get { return m_Coverage; }
            set { m_Coverage = value; }
        }

        /// <summary>
        /// Returns <see cref="MountedCoverage"/> if the <paramref name="location"/>s type is equal to 
        /// <see cref="DefaultLocationType"/>.  Otherwise zero.
        /// </summary>
        /// <param name="location">The location to check.</param>
        /// <returns>
        /// <see cref="MountedCoverage"/> if the <paramref name="location"/>s type is equal to 
        /// <see cref="DefaultLocationType"/>.  Otherwise zero.
        /// </returns>
        public sealed override BodyCoverage GetCoverageFor(MountPoint location)
        {
            return (location && location.LocationType == m_Location) ? MountedCoverage : 0;   
        }

        // Don't seal this.  Extensions may need to put in place additional checks.
        public override bool CanMount(MountPoint location, BodyCoverage restrictions)
        {
            return (location && location.LocationType == m_Location && (m_Coverage & restrictions) == 0);
        }

        [Space(8)]
        [SerializeField]
        [Tooltip("The local position offset to apply to the accessory when it is mounted.")]
        private Vector3 m_PositionOffset = Vector3.zero;

        /// <summary>
        /// The local position offset to apply to the accessory when it is mounted.
        /// </summary>
        public Vector3 PositionOffset
        {
            get { return m_PositionOffset; }
            set { m_PositionOffset = value; }
        }

        [SerializeField]
        [Tooltip("The local euler rotation offset to apply to the accessory when it is mounted.")]
        private Vector3 m_RotationOffset = Vector3.zero;

        /// <summary>
        /// The local euler rotation offset to apply to the accessory when it is mounted.
        /// </summary>
        public Vector3 RotationOffset
        {
            get { return m_RotationOffset; }
            set { m_RotationOffset = value; }
        }

        protected sealed override bool CanMountInternal(MountPoint location, GameObject owner)
        {
            return location.LocationType == m_Location;
        }

        protected sealed override BodyCoverage MountInternal(MountPoint location, GameObject owner)
        {
            transform.parent = location.transform;
            transform.localPosition = PositionOffset;
            transform.localEulerAngles = RotationOffset;

            return m_Coverage;
        }

        protected sealed override IAccessoryMounter GetInitializedMounter(MountPoint location, GameObject owner)
        {
            return null;
        }
    }
}
