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
    /// An mount point to which accessories can be attached.
    /// </summary>
    /// <remarks>
    /// <para>
    /// While mount points are normally associated with an Outfit, the owner may any Component
    /// or GameObject.
    /// </para>
    /// </remarks>
    public class MountPoint
        : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The location of the mount point.")]
        private MountPointType m_Location = (MountPointType)1;

        /// <summary>
        /// The location of the mount point.
        /// </summary>
        public MountPointType LocationType
        {
            get { return m_Location; }
            set { m_Location = value; }
        }

        [SerializeField]
        [Tooltip("The owner of the mount point. (E.g. The mount point's outfit.)  (Optional.)")]
        private GameObject m_Owner = null;

        /// <summary>
        /// TThe owner of the mount point. (E.g. The mount point's outfit.)  (Optional.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is a data field meant for use by custom accessories and mounters.  It provides 
        /// a target to search for needed components.   E.g. Detecting the status of the outfit
        /// that owns the mount point.
        /// </para>
        /// </remarks>
        public GameObject Owner
        {
            get { return m_Owner; }
            set { m_Owner = value; }
        }

        [SerializeField]
        [Tooltip("The mount point is blocked and should not be used.")]
        private bool m_IsBlocked = false;

        /// <summary>
        /// The mount point is blocked and should not be used.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is a hint.  How accessories, mounters, and the mount point's owner handles
        /// this flag is implementation specific.  The general rule is that the component
        /// responsbile for asking an accessory to mount to the mount point is responsible 
        /// for first checking the blocked status.
        /// </para>
        /// </remarks>
        public bool IsBlocked
        {
            get { return m_IsBlocked; }
            set { m_IsBlocked = value; }
        }
    }
}