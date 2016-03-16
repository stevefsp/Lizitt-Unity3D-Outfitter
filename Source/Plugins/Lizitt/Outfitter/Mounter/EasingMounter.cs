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
    /// Provides common settings and behavior for a mounter that performs position and rotation easing during an 
    /// accessory mount operation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Supports multiple concurrent mount operations. Will complete immediately if used outside of play mode. 
    /// (No animation.) 
    /// </para>
    /// </remarks>
    public abstract class EasingMounter
        : OffsetMounterObject
    {
        /*
         * Design notes:
         * 
         * It is technically ok for both mount points (to/from) to be the same.  This is useful for mounters 
         * designed to transfer an accessory between the same mount point type on different outfits.
         */

        #region Settings

        [Space(10)]
        [SerializeField]
        [Tooltip("The location the mounter can transfer the accessory from.")]
        [SortedEnumPopup(typeof(MountPointType))]
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
        [SortedEnumPopup(typeof(MountPointType))]
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
        /// Set the 'to' and 'from' locations.
        /// </summary>
        /// <param name="from">The location the mounter can transfer the accessory from.</param>
        /// <param name="to">The location the mounter can mount the accessory to.</param>
        public void SetLocations(MountPointType from, MountPointType to)
        {
            m_From = from;
            m_To = to;
        }

        public override MountPointType DefaultLocation
        {
            get { return m_To; }
        }

        public sealed override bool CanMount(Accessory accessory, MountPoint location)
        {
            return location && location.LocationType == m_To
                && accessory && accessory.CurrentLocation && accessory.CurrentLocation.LocationType == m_From;
        }

        public sealed override BodyCoverage GetCoverageFor(MountPoint location)
        {
            return (location && location.LocationType == m_To) ? MountedCoverage : 0;
        }

        [Space]

        // TODO: v0.3  Add a property and editor visualization for current buffer size.  That will allow
        // optimization of the initialization size.

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

        [SerializeField]
        [Tooltip("Ease in the local space of the nearest shared parent of the 'from' and 'to' mount points."
            + " Otherwise perform the ease while parented to the 'to' mount point.")]
        private bool m_UseSharedSpace = true;

        /// <summary>
        /// Ease in the local space of the nearest shared parent of the 'from' and 'to' mount points. Otherwise"
        /// perform the ease while parented to the 'to' mount point.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Easing usually works best when it takes place in a local space shared by both the original and target
        /// and mount points. When a lot of motion is occurs, such as when a character is animating, the closer 
        /// the shared local space, the more accurate the easing will be.
        /// </para>
        /// </remarks>
        public bool UseSharedSpace
        {
            get { return m_UseSharedSpace; }
            set { m_UseSharedSpace = value; }
        }

        // TODO: EVAL: v0.4:  Make decision to remove or restore.
        // Added complexity for not much benefit?
        ///// <summary>
        ///// The ease space.
        ///// </summary>
        ///// <remarks>
        ///// <para>
        ///// Easing works best when it takes place in a local space shared by both the accessory and mount point.
        ///// When a lot of motion is occurs, such as when a character is animating, the closer the shared local 
        ///// space, the more accurate the easing will be.
        ///// </para>
        ///// </remarks>
        //public enum EaseSpaceType
        //{
        //    /// <summary>
        //    /// Use the closest shared parent of the accessory and mount point. (Lowest common ancestor.)
        //    /// </summary>
        //    FirstShared = 0,

        //    /// <summary>
        //    /// The mount point's local space.  (The accessory will act as a child of the mount point.)
        //    /// </summary>
        //    MountPoint,

        //    /// <summary>
        //    /// The local space of the mount point's parent.
        //    /// </summary>
        //    MountParent,

        //    /// <summary>
        //    /// The local space of the mount point's context.
        //    /// </summary>
        //    MountContext,

        //    /// <summary>
        //    /// The local space of the accessory's current parent.
        //    /// </summary>
        //    AccessoryParent,

        //    /// <summary>
        //    /// The motion root of the mount point's outfit, or world space if no outfit could be found.
        //    /// </summary>
        //    /// <remarks>
        //    /// <para>
        //    /// A parent search will be performed first, followed by a check of the mount point's context.
        //    /// </para>
        //    /// </remarks>
        //    MotionRoot,

        //    /// <summary>
        //    /// World space.
        //    /// </summary>
        //    World,
        //}

        //[SerializeField]
        //private EaseSpaceType m_Space = EaseSpaceType.FirstShared;

        ///// <summary>
        ///// The local space of the ease operation.  (Should be shared by the accessory and mountpoint.)
        ///// </summary>
        //public EaseSpaceType EaseSpace
        //{
        //    get { return m_Space; }
        //    set { m_Space = value; }
        //}

        #endregion

        #region  Mount State

        // Design note:  Serialization friendly. (Hence the use of a list rather than a dictionary.)
        // But not worth fully implementing until properly supported by the accessory core. 

        // TODO:  v0.3: Convert this to a simpler design. No longer planning to support mount serialization.

        private struct MountState
        {
            public Accessory accessory;
            public float easeTime;
            public Vector3 startPosition;
            public Vector3 startEulers;
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

        public sealed override bool InitializeMount(Accessory accessory, MountPoint location)
        {
            if (location && CanMount(accessory, location))
            {
                var state = GetMountState(accessory);

                state.accessory = accessory;  // Might be a new state.
                state.easeTime = state.easeTime == 0 ? 0 : state.easeTime;

                if (m_UseSharedSpace)
                {
                    if (accessory.transform.IsChildOf(location.transform) 
                        || location.transform.IsChildOf(accessory.transform))  // Unexpected.  But who knows.
                    {
                        accessory.transform.parent = null;
                    }
                    else
                        accessory.transform.parent = accessory.transform.GetSharedParent(location.transform);
                }
                else
                    accessory.transform.parent = location.transform;

                // See notes for EaseSpaceType.  Restore or remove by v0.4.
                //switch (m_Space)
                //{
                //    case EaseSpaceType.FirstShared:

                //        if (accessory.transform.IsChildOf(location.transform)
                //            || location.transform.IsChildOf(accessory.transform))  // Unexpected.  But who knows.
                //        {
                //            accessory.transform.parent = null;
                //        }
                //        else
                //            accessory.transform.parent = accessory.transform.GetSharedParent(location.transform);

                //        break;

                //    case EaseSpaceType.MotionRoot:

                //        var outfit = location.GetComponentInParent<Outfit>();
                //        if (!outfit && location.Context)
                //            outfit = location.Context.GetComponent<Outfit>();

                //        if (outfit)
                //            accessory.transform.parent = outfit.transform;
                //        else
                //        {
                //            accessory.transform.parent = null;

                //            Debug.LogWarning(
                //                "Could not locate the mount point's Outfit.  Falling back to world space. MountPoint: "
                //                + location.name, location);
                //        }

                //        break;

                //    case EaseSpaceType.MountPoint:

                //        accessory.transform.parent = location.transform;
                //        break;

                //    case EaseSpaceType.MountParent:

                //        accessory.transform.parent = location.transform.parent;
                //        break;

                //    case EaseSpaceType.MountContext:

                //        accessory.transform.parent = location.Context ? location.Context.transform : null;

                //        break;

                //    case EaseSpaceType.AccessoryParent:

                //        // Do nothing.
                //        break;

                //    default:

                //        accessory.transform.parent = null;
                //        break;

                //}

                state.startPosition = accessory.transform.localPosition;
                state.startEulers = accessory.transform.localEulerAngles;

                SetMountState(state);

                return true;
            }

            return false;
        }

        public sealed override bool UpdateMount(Accessory accessory, MountPoint location, bool immediateComplete)
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
        public sealed override bool UpdateMount(Accessory accessory, MountPoint location, float deltaTime)
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

            // Local offset -> Global space.
            var endPos = location.transform.TransformPoint(PositionOffset);
            var endRot = location.transform.rotation * Quaternion.Euler(RotationOffset);

            if (accessory.transform.parent)
            {
                // Global space -> Same space as accessory.
                endPos = accessory.transform.parent.InverseTransformPoint(endPos);
                endRot = (Quaternion.Inverse(accessory.transform.parent.rotation) * endRot);
            }

            accessory.transform.localPosition = GetPosition(state.startPosition, endPos, ntime);
            accessory.transform.localEulerAngles = GetEulerAngles(state.startEulers, endRot.eulerAngles, ntime);

            SetMountState(state);

            return true;
        }

        public sealed override void CancelMount(Accessory accessory, MountPoint location)
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
        /// Get the position for the specified time.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="start">The start position.</param>
        /// <param name="end">The end position.</param>
        /// <param name="normalizedTime">The normalized time. [0 &lt;= value &lt;= 1]</param>
        /// <returns>The position for the specified time.</returns>
        public abstract Vector3 GetPosition(Vector3 start, Vector3 end, float normalizedTime);

        /// <summary>
        /// Get the rotation for the specified time.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Note: <see cref="Easing.Clerp"/> is useful for obtaining the shortest rotation 
        /// between angles.
        /// </para>
        /// </remarks>
        /// <param name="start">The start rotation.</param>
        /// <param name="end">The end rotation.</param>
        /// <param name="normalizedTime">The normalized time. [0 &lt;= value &lt;= 1]</param>
        /// <returns>The position for the specified time.</returns>
        public abstract Vector3 GetEulerAngles(Vector3 start, Vector3 end, float normalizedTime);

        #endregion
    }
}
