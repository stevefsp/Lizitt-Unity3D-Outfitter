/*
 * Copyright (c) 2015 Stephen A. Pratt
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
using com.lizitt.u3d;
using UnityEngine;

namespace com.lizitt.outfitter
{
    /// <summary>
    /// A body accessory based on a prefab or GameObject.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Warning: Do not make this class or sub-classes a required component.
    /// I.e. Don't do this: [RequireComponent(typeof(StandardAccessory))]
    /// Doing so can prevent proper conversion to a static non-accessory object which is 
    /// required for baking an outfit.
    /// </para>
    /// </remarks>
    [System.Serializable]
    public class StandardAccessory
        : BodyAccessory
    {
        #region Unity Editor Fields

        [Header("General Accessory Settings")]

        [SerializeField]
        [Tooltip("The accessory's mount point.")]
        private MountPointType m_MountPoint = MountPointType.Head;

        [SerializeField]
        [Tooltip("The body coverage for this accessory.  (Determines blocking behavior.)")]
        [EnumFlags(typeof(BodyCoverage), OutfitterUtil.SortBodyCoverage)]
        private BodyCoverage m_Coverage = 0;

        [SerializeField]
        [Tooltip("Permit coverage to be changed at runtime.")]
        private bool m_DynamicCoverage = true;

        [SerializeField]
        [Tooltip("If true, will attach to outfits flagged as limited.")]
        private bool m_IgnoreLimited = false;

        [SerializeField]
        [Tooltip("If true, the accessory will automatically enable/disable renders based on the"
            + " current state.")]
        private bool m_ManageRenderers = false;

        [Header("Accessory Local Offsets")]

        [SerializeField]
        [Tooltip("The local position offset to apply when attached.")]
        private Vector3 m_AttachPosition = Vector3.zero;

        [SerializeField]
        [Tooltip("The local rotation offset to apply when attached.")]
        private Vector3 m_AttachRotation = Vector3.zero;

        [SerializeField]
        [Tooltip("If true the attach position and rotation will be ignored."
            + " (Useful when the accessory's transform will be manually interpolated to the"
            + " mount point.)")]
        private bool m_PreserveWorldTransform = false;

        #endregion

        #region Transform Related

        public sealed override Vector3 AttachPosition
        {
            get { return m_AttachPosition; }
        }

        /// <summary>
        /// Sets the attach position offset.
        /// </summary>
        /// <param name="offset">The offset.</param>
        public void SetAttachPosition(Vector3 offset)
        {
            m_AttachPosition = offset;
        }
        public sealed override Vector3 AttachRotation
        {
            get { return m_AttachRotation; }
        }

        /// <summary>
        /// Sets the attach rotation offset.
        /// </summary>
        /// <param name="offset">The offset.</param>
        public void SetAttachRotation(Vector3 offset)
        {
            m_AttachRotation = offset;
        }

        public sealed override bool PreserveWorldTransform
        {
            get { return m_PreserveWorldTransform; }
        }

        #endregion

        #region Mount and Coverage

        public sealed override MountPointType MountPoint
        {
            get { return m_MountPoint; }
            set { m_MountPoint = value; }
        }

        public sealed override bool IgnoreLimited
        {
            get { return m_IgnoreLimited; }
            set { m_IgnoreLimited = value; }
        }

        public sealed override BodyCoverage Coverage
        {
            get { return m_Coverage; }
            set
            {
                if (m_DynamicCoverage)
                    m_Coverage = value;
                else
                    Debug.LogError("Coverage changes are not permitted.", this);
            }
        }

        public sealed override bool IsCoverageDynamic
        {
            get { return m_DynamicCoverage; }
        }

        #endregion

        #region Initialization

        protected override void OnInitialize()
        {
            SetRenderers(false);
        }

        #endregion

        #region Control Methods

        protected override void OnAttachPost()
        {
            SetRenderers(true);
        }

        protected override void OnStored()
        {
            SetRenderers(false);
        }

        protected override void OnReleaseFromAttachedPre()
        {
            SetRenderers(false);
        }

        protected override void OnReleaseFromStoragePre()
        {
            SetRenderers(false);
        }

        protected override void OnPurge()
        {
        }

        #endregion

        #region Utility Methods

        private void SetRenderers(bool enabled)
        {
            if (m_ManageRenderers)
            {
                foreach (var renderer in GetComponentsInChildren<Renderer>())
                    renderer.enabled = enabled;
            }
        }

        #endregion
    }
}
