﻿/*
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
    /// A mounter that will snap the accessory to a location with optional position and
    /// rotation offsets.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Update completes immediately.  Instances of this type supports multiple concurrent mount 
    /// operations.  (E.g. It an be safely shared by multiple accessories.)
    /// </para>
    /// <para>
    /// Does not support unmounting.
    /// </para>
    /// </remarks>
    public class StandardMounter
        : OffsetMounter
    {
        [Space(8)]

        [SerializeField]
        [Tooltip("The location the mounter can mount to.")]
        private MountPointType m_Location = (MountPointType)0;

        /// <summary>
        /// The location the mounter can mount to.
        /// </summary>
        public MountPointType LocationType
        {
            get { return m_Location; }
            set { m_Location = value; }
        }

        public override bool CanMount(Accessory accessory, MountPointType locationType)
        {
            return (accessory && locationType == m_Location);
        }

        public sealed override BodyCoverage GetCoverageFor(MountPointType locationType)
        {
            return (locationType == m_Location) ? Coverage : 0;   
        }

        public override bool InitializeMount(Accessory accessory, MountPoint location)
        {
            if (location == null)
                return CanUnmount(accessory);

            return CanMount(accessory, location.LocationType);
        }

        public override bool UpdateMount(Accessory accessory, MountPoint location)
        {
            FinalizeMount(accessory, location);
            return false;
        }
    }
}
