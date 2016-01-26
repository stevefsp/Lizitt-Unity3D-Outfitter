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
using UnityEngine;

namespace com.lizitt.outfitter
{
    /// <summary>
    /// An outfit mount point.
    /// </summary>
    [System.Serializable]
    public class MountPoint
    {
        [SerializeField]
        [Tooltip("The mount point's transform.")]
        private Transform m_Transform = null;

        [SerializeField]
        [Tooltip("The type of the mount point.")]
        private MountPointType m_Type = MountPointType.Head;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MountPoint() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">The type of the mount point.</param>
        /// <param name="transform">The mount point's transform.</param>
        public MountPoint(MountPointType type, Transform transform)
        {
            if (!transform)
                throw new System.ArgumentNullException("transform");
            m_Type = type;
            m_Transform = transform;
        }

        /// <summary>
        /// The type of the mount point.
        /// </summary>
        public MountPointType MountType
        {
            get { return m_Type; }
        }

        /// <summary>
        /// The mount point's transform.
        /// </summary>
        public Transform Transform
        {
            get { return m_Transform; }
        }
    }
}