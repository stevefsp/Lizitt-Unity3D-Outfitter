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
using System.Collections.Generic;

namespace com.lizitt.outfitter
{
    /// <summary>
    /// Provides common settings and behavior for a mounter that performs position and rotation
    /// easing during an accessory mount operation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="UpdateMount"/> will complete immediately if used outside of play mode.
    /// </para>
    /// <para>
    /// Supports concurrent mount operations. Does not support mount state serialization.
    /// </para>
    /// </remarks>
    public abstract class EasingMounter
        : OffsetMounter
    {
        /*
         * Design notes:
         * 
         * It is technically ok for both mount points (to/from) to be the same.  This is useful for mounters 
         * designed to transfer an accessory between the same mount point type on different outfits.
         */

        #region Configuration Settings

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
        [Tooltip("The initial size of the mount state buffer.  (Set to the maximum expected number of"
            + " concurrent mount operations.  Too small of a value will result in play mode memory"
            + " allocation(s)/garbage collection.  Too large of a value will waste memory.)")]
        [ClampMinimum(0)]
        private int m_MountBufferSize = 5;


        /// <summary>
        /// The initial size of the mount state buffer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Set to the maximum expected number of concurrent mount operations.  Too small of a value will result
        /// in play mode memory allocation(s)/garbage collection.  Too large of a value will waste memory.
        /// </para>
        /// </remarks>
        public int MountBufferSize
        {
            get { return m_MountBufferSize; }
            set { m_MountBufferSize = Mathf.Max(0, value); }
        }

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

        #endregion

        #region  Mount State

        // Design note:  Serialization friendly. (Hence the use of a list rather than a dictionary.)
        // But not worth fully implementing until properly supported by the accessory core. 

        private struct MountState
        {
            public Accessory accessory;
            public float easeTime;
            public Vector3 localStartPosition;
            public Vector3 localStartEulers;
        }

        private List<MountState> m_MountState;

        /// <summary>
        /// The number of currently active mount operations.
        /// </summary>
        public int ActiveMountCount
        {
            get 
            {
                CheckStateInitialized();
                return m_MountState.Count; 
            }
        }

        private MountState GetMountState(Accessory accessory)
        {
            CheckStateInitialized();
            for (int i = 0; i < m_MountState.Count; i++)
            {
                var item = m_MountState[i];

                if (item.accessory == accessory)
                    return item;
            }

            // Do not set accessory.  An unssigned accessory indicates a new state.
            return new MountState();
        }

        private void SetMountState(MountState state)
        {
            CheckStateInitialized();
            for (int i = 0; i < m_MountState.Count; i++)
            {
                var item = m_MountState[i];

                if (item.accessory == state.accessory)
                {
                    m_MountState[i] = state;
                    return;
                }
            }

            m_MountState.Add(state);
        }

        private void RemoveMountState(Accessory accessory)
        {
            CheckStateInitialized();
            for (int i = 0; i < m_MountState.Count; i++)
            {
                var item = m_MountState[i];

                if (item.accessory == accessory)
                {
                    m_MountState.RemoveAt(i);
                    return;
                }
            }
        }

        private void CheckStateInitialized()
        {
            if (m_MountState == null)
                m_MountState = new List<MountState>(m_MountBufferSize);
        }

        #endregion

        #region Mounting

        public override bool InitializeMount(Accessory accessory, MountPoint location)
        {
            if (location && CanMount(accessory, location.LocationType))
            {
                var state = GetMountState(accessory);

                state.accessory = accessory;  // Might be a new state.
                state.easeTime = state.easeTime == 0 ? 0 : state.easeTime;

                accessory.transform.parent = location.transform;

                state.localStartPosition = accessory.transform.localPosition;
                state.localStartEulers = accessory.transform.localEulerAngles;

                SetMountState(state);

                return true;
            }

            return false;
        }

        public override bool UpdateMount(Accessory accessory, MountPoint location, bool immediateComplete)
        {
            float deltaTime = (Application.isPlaying && !immediateComplete) ? Time.deltaTime : m_EaseDuration + 0.1f; 
            return UpdateMount(accessory, location, deltaTime);
        }

        /// <summary>
        /// Process the mount operation until it completes using the provided time increment.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="InitializeMount"/> must be called and return true before calling this method, then either
        /// this method must be called through to completion or <see cref="CancelMount"/> used to cancel the operation.
        /// </para>
        /// <para>
        /// No matter the value of <see cref="deltaTime"/>, the normalized time is clamped to the zero to one range.
        /// </para>
        /// </remarks>
        /// <param name="accessory">The accessory to update. (Required)</param>
        /// <param name="location">The mount location. (Required)</param>
        /// <param name="deltaTime">The number of seconds to to apply to the update.</param>
        /// <returns>True while the mount operation is in-progress.  False when the operation is complete.</returns>
        public bool UpdateMount(Accessory accessory, MountPoint location, float deltaTime)
        {
            var state = GetMountState(accessory);

            if (!state.accessory)
            {
                Debug.LogError(
                    "Accessory not initialized. Update failed: " + (accessory ? accessory.name : "Null"), accessory);
                return false;
            }

            state.easeTime += deltaTime;
            
            var ntime = Mathf.Max(0, state.easeTime / m_EaseDuration);
            if (ntime >= 1)
            {
                FinalizeMount(accessory, location.transform);
                RemoveMountState(accessory);
                return false;
            }

            accessory.transform.localPosition =
                GetLocalPosition(state.localStartPosition, PositionOffset, ntime);

            accessory.transform.localEulerAngles =
                GetLocalEulerAngles(state.localStartEulers, RotationOffset, ntime);

            SetMountState(state);

            return true;
        }

        public override void CancelMount(Accessory accessory, MountPoint location)
        {
            // Don't finalize the mount.  Assume that another mounter is taking over and that it is designed to take
            // over easing as needed.  (Since that is the best design.)
            RemoveMountState(accessory);
        }

        public override void OnAccessoryDestroy(Accessory accessory, DestroyType type)
        {
            RemoveMountState(accessory);
        }

        #endregion

        #region Abstract Members

        /// <summary>
        /// Get the local position for the specified time.
        /// </summary>
        /// <remarks>
        /// <para>
        /// All positions are local offsets to the target location.
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
        /// All rotations are local offsets from the target.
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
        public abstract Vector3 GetLocalEulerAngles(Vector3 start, Vector3 end, float normalizedTime);

        #endregion
    }
}
