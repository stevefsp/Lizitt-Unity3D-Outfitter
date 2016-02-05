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
    /// Defines a group of ordered accessory mounters.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Designed for use as a field in a Unity component.  It provides a
    /// better editor experience than is available for arrays.
    /// </para>
    /// <para>
    /// WARNING: This class can't be used in an array.  E.g. An array of AccessoryMounterGroup objects, 
    /// or an array of objects that contain AccessoryMounterGroup objects.
    /// </para>
    /// </remarks>
    [System.Serializable]
    public class AccessoryMounterGroup
    {
        [SerializeField]
        private AccessoryMounter[] m_Items;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bufferSize">The size of the internal buffer.</param>
        public AccessoryMounterGroup(int bufferSize)
        {
            m_Items = new AccessoryMounter[Mathf.Max(0, bufferSize)];
        }

        /// <summary>
        /// The items at the specified index, or null if there is none.
        /// </summary>
        /// <param name="index">
        /// The index. [Limit: 0 &lt;= value &lt; <see cref="BufferSize"/>]
        /// </param>
        /// <returns>
        /// The item at the specified index, or null if there is none.
        /// </returns>
        public AccessoryMounter this[int index]
        {
            get { return m_Items[index]; }
            set { m_Items[index] = value; }
        }

        /// <summary>
        /// The size of the buffer.
        /// </summary>
        public int BufferSize
        {
            get { return m_Items.Length; }
        }

        /// <summary>
        /// Replace the items in the buffer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// To use this method safely, either set <paramref name="asReference"/> to false, or
        /// discard all external references to the <paramref name="items"/> array.
        /// </para>
        /// </remarks>
        /// <param name="mounters">The object to udpate.  (Required)</param>
        /// <param name="asReference">
        /// If true, the inernal buffer will be replaced by a referecne to <paramref name="items"/>.  
        /// Otherwise <paramref name="items"/> will be copied.
        /// </param>
        /// <param name="items">The items to put into the buffer.</param>
        public static void UnsafeReplaceItems(
            AccessoryMounterGroup mounters, bool asReference, params AccessoryMounter[] items)
        {
            // Null and null elements allowed.
            if (items == null)
                mounters.Clear(0);
            else
                mounters.m_Items = asReference ? items : (AccessoryMounter[])items.Clone();
        }

        /// <summary>
        /// Clear all items and optionally resize the internal buffer.
        /// </summary>
        /// <param name="bufferSize">
        /// The new size of the internal buffer or -1 to keep the current buffer.
        /// [Limites: >= 0, or -1]
        /// </param>
        public void Clear(int bufferSize = -1)
        {
            if (bufferSize == -1)
                bufferSize = m_Items.Length;

            if (m_Items.Length != bufferSize)
                m_Items = new AccessoryMounter[Mathf.Max(0, bufferSize)];
            else
                System.Array.Clear(m_Items, 0, m_Items.Length);
        }

        /// <summary>
        /// True if the buffer contains the specified item.
        /// </summary>
        /// <param name="item">The item to check. (Required)</param>
        /// <returns>
        /// True if the buffer contains the specified item.
        /// </returns>
        public bool Contains(AccessoryMounter item)
        {
            if (!item)
                return false;

            if (item)
            {
                for (int i = 0; i < m_Items.Length; i++)
                {
                    if (m_Items[i] == item)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// The default location type of the first non-null mounter in the group, or 'root' if there isn't one.
        /// </summary>
        public MountPointType DefaultLocationType
        {
            get
            {
                for (int i = 0; i < m_Items.Length; i++)
                {
                    if (m_Items[i])
                        return m_Items[i].DefaultLocationType;
                }

                return (MountPointType)0;
            }
        }
    }
}