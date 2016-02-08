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
    /// Provides common settings and behavior for a mounter that performs position and rotation
    /// easing during accessory mount operations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A mounter based on this class can only be used for one mount operation at a time, so it 
    /// is either assigned to a single accessory or shared using a pooling system in a 
    /// manner that prevents concurrent use.
    /// </para>
    /// <para>
    /// Will complete immediately if used outside of play mode. (No animation.)
    /// </para>
    /// <para>
    /// Does not implement unmounting.
    /// </para>
    /// </remarks>
    public abstract class EasingMounter
        : OffsetMounter
    {
        /*
         * Design notes:
         * 
         * It is technically ok for both mount points (to/from) to be the same.  The purpose may 
         * be to use the mounter to transfer from the same location but on two different outfits.
         */

        [Space(10)]

        [SerializeField]
        [Tooltip("The location the mounter can transfer the accessory from.")]
        private MountPointType m_From = (MountPointType)0;

        /// <summary>
        /// The location the mounter can transfer the accessory from.
        /// </summary>
        public MountPointType From
        {
            get { return m_From; }
            set { m_From = value; }
        }

        [SerializeField]
        [Tooltip("The location the mounter can mount the accessory to.")]
        private MountPointType m_To = (MountPointType)0;

        /// <summary>
        /// The location the mounter can mount the accessory to.
        /// </summary>
        public MountPointType To
        {
            get { return m_To; }
            set { m_To = value; }
        }

        /// <summary>
        /// Set the to and from locations.
        /// </summary>
        /// <param name="from">The location the mounter can transfer the accessory from.</param>
        /// <param name="to">The location the mounter can mount the accessory to.</param>
        public void SetLocations(MountPointType from, MountPointType to)
        {
            m_From = from;
            m_To = to;
        }

        public override MountPointType DefaultLocationType
        {
            get { return m_To; }
        }

        public override bool CanMount(Accessory accessory, MountPointType locationType)
        {
            return locationType == m_To && accessory && accessory.CurrentLocation 
                && accessory.CurrentLocation.LocationType == m_From;
        }

        public override BodyCoverage GetCoverageFor(MountPointType locationType)
        {
            return (locationType == m_To) ? Coverage : 0;   
        }

        [Space]

        [SerializeField]
        [Tooltip("The length of the ease operation, in seconds.")]
        [ClampMinimum(0)]
        private float m_EaseDuration = 1;

        /// <summary>
        /// The length of the ease operation, in seconds. [Limit: >= 0]
        /// </summary>
        public float EaseDuration
        {
            get { return m_EaseDuration; }
            set { m_EaseDuration = Mathf.Max(0, value); }
        }

        private float m_EaseTime = 0;
        private Vector3 m_LocalStartPosition;
        private Vector3 m_LocalStartRotation;

        public override bool InitializeMount(Accessory accessory, MountPoint location)
        {
            if (location && CanMount(accessory, location.LocationType))
            {
                m_EaseTime = 0;

                accessory.transform.parent = location.transform;

                m_LocalStartPosition = accessory.transform.localPosition;
                m_LocalStartRotation = accessory.transform.localEulerAngles;

                return true;
            }

            return false;
        }

        public override bool UpdateMount(Accessory accessory, MountPoint location, bool immediateComplete)
        {
            m_EaseTime += Time.deltaTime;

            var ntime = Mathf.Clamp01(m_EaseTime / m_EaseDuration);

            if (!Application.isPlaying || immediateComplete || ntime >= 1)
            {
                FinalizeMount(accessory, location);
                return false;
            }

            accessory.transform.localPosition =
                GetLocalPosition(m_LocalStartPosition, PositionOffset, ntime);

            accessory.transform.localEulerAngles = 
                GetLocalEulerAngles(m_LocalStartRotation, RotationOffset, ntime);

            return true;
        }

        /// <summary>
        /// Get the local position for the specified time.
        /// </summary>
        /// <remarks>
        /// <para>
        /// All positions are local to the target location.
        /// </para>
        /// </remarks>
        /// <param name="start">The local start position.</param>
        /// <param name="end">The local end position.</param>
        /// <param name="normalizedTime">The normalized time. [0 &lt;= value &lt;= 1]</param>
        /// <returns>The local position for the specified time.</returns>
        public abstract Vector3 GetLocalPosition(Vector3 start, Vector3 end, float normalizedTime);

        /// <summary>
        /// Get the local rotation for the specified time.
        /// </summary>
        /// <remarks>
        /// <para>
        /// All rotations are local to the target location.  (I.e. The mount point.)
        /// </para>
        /// <para>
        /// Note: <see cref="Easing.Clerp"/> is useful for obtaining the shortest rotation 
        /// between angles.
        /// </para>
        /// </remarks>
        /// <param name="start">The local start rotation.</param>
        /// <param name="end">The local end rotation.</param>
        /// <param name="normalizedTime">The normalized time. [0 &lt;= value &lt;= 1]</param>
        /// <returns>The local position for the specified time.</returns>
        public abstract Vector3 GetLocalEulerAngles(
            Vector3 start, Vector3 end, float normalizedTime);
    }
}
