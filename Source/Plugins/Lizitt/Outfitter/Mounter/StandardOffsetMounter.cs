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
    /// A mounter that immediately parents to its location with optional position and rotation offsets.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The mount operation will always succeed if the mount point's location type is the same as 
    /// <see cref="DefaultLocation"/>.
    /// </para>
    /// <para>
    /// Update completes immediately.  Supports multiple concurrent mount operations.
    /// </para>
    /// </remarks>
    [CreateAssetMenu(
        menuName = OutfitterMenu.AssetMenu + "Standard Offset Mounter", order = OutfitterMenu.MounterAssetMenuOrder + 0)]
    public class StandardOffsetMounter
        : OffsetMounterObject
    {
        [Space(8)]

        [SerializeField]
        [Tooltip("The location the accessory can mount to.")]
        [SortedEnumPopup(typeof(MountPointType))]
        private MountPointType m_Location = (MountPointType)0;

        /// <summary>
        /// The location the accessory can mount to.
        /// </summary>
        public sealed override MountPointType DefaultLocation
        {
            get { return m_Location; }
        }

        /// <summary>
        /// Set the location the accessory can mount to.
        /// </summary>
        public void SetDefaultLocation(MountPointType value)
        {
            m_Location = value;
        }

        // Do not seal.  Allow extra restrictions.
        public override bool CanMount(Accessory accessory, MountPoint location)
        {
            return (accessory && location && location.LocationType == m_Location);
        }

        public sealed override BodyCoverage GetCoverageFor(MountPoint location)
        {
            return (location && location.LocationType == m_Location) ? MountedCoverage : 0;   
        }

        // Do not seal.  Allow overrides to add functionality.
        public override bool InitializeMount(Accessory accessory, MountPoint location)
        {
            if (!location)
                return false;

            return CanMount(accessory, location);
        }

        // Do not seal.  Allow overrides to add functionality.
        public override bool UpdateMount(Accessory accessory, MountPoint location, bool immediateComplete)
        {
            FinalizeMount(accessory, location.transform);
            return false;
        }

        public sealed override bool UpdateMount(Accessory accessory, MountPoint location, float deltaTime)
        {
            FinalizeMount(accessory, location.transform);
            return false;
        }

        /*
         * Design notes:
         * 
         * Considered defining and sealing CancelMount().  Desided against it.  While update completes immediately,
         * it is valid to initialize then immediately cancel.  As long as initialize is overridable, cancel must
         * remain overridable.
         * 
         */
    }
}
