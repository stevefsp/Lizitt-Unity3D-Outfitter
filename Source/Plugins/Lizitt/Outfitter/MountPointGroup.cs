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
    /// Defines a group of ordered mount points.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Designed for use as a field in a Unity component.  It provides a better editor experience 
    /// than is available for arrays.
    /// </para>
    /// <para>
    /// WARNING: This class can't be used in an array.  E.g. An array of MountPointGroup objects, 
    /// or an array of objects that contain MountPointGroup objects.
    /// </para>
    /// </remarks>
    [System.Serializable]
    public class MountPointGroup
    {
        [SerializeField]
        private MountPoint[] m_Items;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bufferSize">The internal buffer size.</param>
        public MountPointGroup(int bufferSize)
        {
            m_Items = new MountPoint[Mathf.Max(0, bufferSize)];
        }

        /// <summary>
        /// The item at the specified index, or null if there is none.
        /// </summary>
        /// <param name="index">
        /// Index. [Limit: 0 &lt;= value &lt; <see cref="BufferSize"/>]
        /// </param>
        /// <returns>The item at the specified index, or null if there is none.</returns>
        public MountPoint this[int index]
        {
            get 
            {
                if (!m_Items[index])
                    // Auto-clean.
                    m_Items[index] = null;
                    
                return m_Items[index];
            }
            set 
            { 
                // Never store destroyed reference.
                m_Items[index] = value ? value : null;
            }
        }

        /// <summary>
        /// The item associated with the specified type, or null if there is none.
        /// </summary>
        /// <param name="typ">The item type.</param>
        /// <returns>
        /// The item associated with the specified type, or null if there is none.
        /// </returns>
        public MountPoint this[MountPointType locationType]
        {
            get
            {
                for (int i = 0; i < m_Items.Length; i++)
                {
                    if (m_Items[i] && m_Items[i].LocationType == locationType)
                        return m_Items[i];
                }

                return null;
            }
        }

        /// <summary>
        /// The maximum number of items in the group. [Limit: >= 0]
        /// </summary>
        public int BufferSize
        {
            // Can be null during initial instantiation in the editor.
            get { return m_Items == null ? 0 : m_Items.Length; }
        }

        /// <summary>
        /// Replaces the current items with the specified items.
        /// </summary>
        /// <remarks>
        /// <para>
        /// To use this method safely either set <paramref name="asReference"/> to false or
        /// discard all external refrences to <paramref name="items"/>.
        /// </para>
        /// </remarks>
        /// <param name="mountPoints">The object to update. (Required)</param>
        /// <param name="items">The items.</param>
        /// <param name="asReference">
        /// If true the internal buffer will be replaced with a reference to 
        /// <paramref name="items"/>, otherwise <paramref name="items"/> will be copied.
        /// </param>
        public static void UnsafeReplaceItems(
            MountPointGroup mountPoints, bool asReference, params MountPoint[] items)
        {
            if (items == null)
                mountPoints.Clear();
            else
                mountPoints.m_Items = asReference ? items : (MountPoint[])items.Clone();
        }

        /// <summary>
        /// Clear all items, optionally resizing the internal buffer in the process.
        /// </summary>
        /// <param name="bufferSize">
        /// The new buffer size or -1 to keep the current buffer. [Limites: >= 0, or -1]
        /// </param>
        public void Clear(int bufferSize = 0)
        {
            m_Items = new MountPoint[Mathf.Max(0, bufferSize)];
        }

        /// <summary>
        /// Sets the ownership of the items.
        /// </summary>
        /// <param name="gameObject">The owner, or null.</param>
        /// <param name="unassignedOnly">
        /// If true, ownership will only be set for items that have no owner assigned.  
        /// Otherwise the new owner will be applied to all items.
        /// </param>
        public void SetOwnership(GameObject gameObject, bool unassignedOnly = false)
        {
            for (int i = 0; i < m_Items.Length; i++)
            {
                if (m_Items[i])
                {
                    if (!unassignedOnly || m_Items[i].Owner)
                        m_Items[i].Owner = gameObject;
                }
                else
                    m_Items[i] = null;  // Auto-clean.  A good place to do it.
            }
        }

        /// <summary>
        /// True if the group contains at least one non-null item.
        /// </summary>
        public bool HasItem
        {
            get
            {
                for (int i = 0; i < m_Items.Length; i++)
                {
                    if (m_Items[i])
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// The number of assigned items in the group.  (Non-null, non-destroyed)
        /// </summary>
        public int ItemCount
        {
            get
            {
                var count = 0;

                for (int i = 0; i < m_Items.Length; i++)
                {
                    if (m_Items[i])
                        count++;
                }

                return count;
            }
        }

        /// <summary>
        /// True if the item is in the group.
        /// </summary>
        /// <remarks>
        /// <para>
        /// a null or destroyed <paramref name="item"/> always returns false.
        /// </para>
        /// </remarks>
        /// <param name="item">The item to check. (Required)</param>
        /// <returns>True if the item is in the group.</returns>
        public bool Contains(MountPoint item)
        {
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
        /// Removes all null and destroyed items from the group.
        /// </summary>
        public void Compress()
        {
            m_Items = m_Items.GetCompressed();
        }

        /// <summary>
        /// Remove null and destroyed items from the group and add new, non-duplicate items.
        /// </summary>
        /// <param name="newItems">The potential new items to add.</param>
        public void CompressAndAdd(params MountPoint[] newItems)
        {
            if (newItems == null || newItems.Length == 0)
            {
                m_Items = m_Items.GetCompressed();
                return;
            }

            m_Items = m_Items.CompressAndAddDistinct(true, newItems);
        }
    }
}
