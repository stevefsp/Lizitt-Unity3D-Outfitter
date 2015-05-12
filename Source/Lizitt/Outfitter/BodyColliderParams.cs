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
    /*
     * Design notes:
     * 
     * Some naughty OOP here.  But I decided to do inheritance for Unity's sake.  It makes things 
     * much nicer in the editor.  No extra code for drawers, no embedded dropdowns, etc.
     */

    /// <summary>
    /// Capsule body collider settings.
    /// </summary>
    [System.Serializable]
    public class CapsuleBodyColliderParams
        : CapsuleColliderParams
    {
        [Header("Body Settings")]

        [SerializeField]
        private MountPointType m_MountPoint = MountPointType.Root;

        public MountPointType MountPoint
        {
            get { return m_MountPoint; }
        }
    }

    /// <summary>
    /// Box body collider settings.
    /// </summary>
    [System.Serializable]
    public class BoxBodyColliderParams
        : BoxColliderParams
    {
        [Header("Body Settings")]

        [SerializeField]
        private MountPointType m_MountPoint = MountPointType.Root;

        public MountPointType MountPoint
        {
            get { return m_MountPoint; }
        }
    }

    /// <summary>
    /// Sphere body collider settigns.
    /// </summary>
    [System.Serializable]
    public class SphereBodyColliderParams
        : SphereColliderParams
    {
        [Header("Body Settings")]

        [SerializeField]
        private MountPointType m_MountPoint = MountPointType.Root;

        public MountPointType MountPoint
        {
            get { return m_MountPoint; }
        }
    }
}
