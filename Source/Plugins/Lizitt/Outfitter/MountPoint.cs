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
    /// A mount point to which items, such as accessories, can be attached.
    /// </summary>
    /// <remarks>
    /// <para>
    /// While mount points are normally associated with an <see cref="Outfit"/>, the owner may any GameObject.
    /// Standard components don't care.
    /// </para>
    /// </remarks>
    /// <seealso cref="Accessory"/>
    /// <seealso cref="Outfit"/>
    [AddComponentMenu(LizittUtil.LizittMenu + "Mount Point", OutfitterUtil.BaseMenuOrder + 5)]
    public class MountPoint
        : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The location type of the mount point.")]
        private MountPointType m_Location = (MountPointType)1;

        /// <summary>
        /// The location type of the mount point.
        /// </summary>
        public MountPointType LocationType
        {
            get { return m_Location; }
            set { m_Location = value; }
        }

        [SerializeField]
        [Tooltip("The data context of the mount point.  (Optional)")]
        private GameObject m_Context = null;

        /// <summary>
        /// The data context of the mount point.  (Optional)
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is an informational field.  It can be used to provide information to users of the mount point.  For 
        /// example, it can be set to the agent that owns the outfit that owns the mount point.
        /// </para>
        /// <para>
        /// Standard components automatically initialize this value to their own GameObject if the value is not
        /// aleady assigned.  This is meant as a helpful automation.  The value can be reassigned as desired
        /// without impacting standard component behavior.
        /// </para>
        /// </remarks>
        public GameObject Context
        {
            get { return m_Context; }
            set { m_Context = value; }
        }

        [SerializeField]
        [Tooltip("The mount point is blocked and should not be used.")]
        private bool m_IsBlocked = false;

        /// <summary>
        /// The mount point is blocked and should not be used.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is a hint.  How other components handle this flag is implementation specific.  The general rule is
        /// that the component responsbile for asking an accessory to mount to a mount point is responsible for 
        /// first checking and responding to its blocked status.
        /// </para>
        /// </remarks>
        public bool IsBlocked
        {
            get { return m_IsBlocked; }
            set { m_IsBlocked = value; }
        }        
        
        /// <summary>
        /// Synchronize the state to the specified mount point.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The 'is blocked' state and context are synchronized, depending on the parameter values.  Other properties 
        /// such as location type, transform values, etc., are ignored.
        /// </para>
        /// </remarks>
        /// <param name="to">The mount point to sync to. (Required)</param>
        /// <param name="from">The mount point to sync from. (Required)</param>
        /// <param name="includeBlocked">Synchronize the mount point 'is blocked' state.</param>
        /// <param name="includeContext">Synchronize the context unless it is <paramref name="ignoreContext"/>.</param>
        /// <param name="ignoreContext">
        /// The context that should never be synchronized. (Usually the object <paramref name="to"/> is a member of,
        /// such at its Outfit. (Required if <paramref name="includeContext"/> is true.)
        /// </param>
        public static void Synchronize(MountPoint to, MountPoint from, 
            bool includeBlocked, bool includeContext, GameObject ignoreContext)
        {
            if (includeBlocked)
                to.IsBlocked = from.IsBlocked;

            if (includeContext && from.Context != ignoreContext)
                to.Context = from.Context;
        }
    }
}