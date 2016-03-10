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
    /// Provides common settings for a mounter that parents an accessory to a location with a position and rotation 
    /// offset.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Mounters based on this class can support both concurrent mount operations and full state serialization.
    /// Actual support dependeds on the concrete implemenation.
    /// </para>
    /// </remarks>
    public abstract class OffsetMounterObject
        : AccessoryMounterObject
    {
        [SerializeField]
        [Tooltip("The coverage of the accessory on a successful mount operation.")]
        [EnumFlags(typeof(BodyCoverage), OutfitterUtil.SortBodyCoverage)]
        private BodyCoverage m_Coverage = 0;

        /// <summary>
        /// The coverage of the accessory on a successful mount operation.
        /// </summary>
        public BodyCoverage MountedCoverage
        {
            get { return m_Coverage; }
            set { m_Coverage = value; }
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

        /// <summary>
        /// Parents the accessory to the location and applies the offsets.
        /// </summary>
        /// <param name="accessory">The accessory</param>
        /// <param name="location">The mount location's transform.</param>
        protected void FinalizeMount(Accessory accessory, Transform parent)
        {
            if (accessory && parent)
            {
                accessory.transform.parent = parent;
                accessory.transform.localPosition = PositionOffset;
                accessory.transform.localEulerAngles = RotationOffset;
            }
        }
    }
}
