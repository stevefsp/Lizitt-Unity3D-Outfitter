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
    /// Defines a group of ordered body parts.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Designed for use as a field in a Unity component.  It provides a better editor experience when used with 
    /// <see cref="ObjectListAttribute"/>.
    /// </para>
    /// </remarks>
    [System.Serializable]
    public sealed class BodyPartGroup
    {
        #region Core

        [SerializeField]
        private BodyPart[] m_Items;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bufferSize">The internal buffer size. [Limit: >= 0]</param>
        public BodyPartGroup(int bufferSize)
        {
            m_Items = new BodyPart[Mathf.Max(0, bufferSize)];
        }

        #endregion

        #region Item Related

        /// <summary>
        /// The maximum number of items in the group. (Some entries may be null.)
        /// </summary>
        public int Count
        {
            // Can be null during initial instantiation in the editor.
            get { return m_Items == null ? 0 : m_Items.Length; }
        }

        /// <summary>
        /// The item at the specified index, or null if there is none.
        /// </summary>
        /// <param name="index"> Index. [Limit: 0 &lt;= value &lt; <see cref="Count"/>]</param>
        /// <returns>The item at the specified index, or null if there is none.</returns>
        public BodyPart this[int index]
        {
            get 
            {
                if (!m_Items[index])
                    // Auto-clean.
                    m_Items[index] = null;

                return m_Items[index]; 
            }
            set { m_Items[index] = value ? value : null; }
        }

        /// <summary>
        /// The item associated with the specified type, or null if there is none.
        /// </summary>
        /// <param name="typ">The item type.</param>
        /// <returns>
        /// The item associated with the specified type, or null if there is none.
        /// </returns>
        public BodyPart this[BodyPartType typ]
        {
            get
            {
                for (int i = 0; i < m_Items.Length; i++)
                {
                    if (m_Items[i] && m_Items[i].PartType == typ)
                        return m_Items[i];
                }

                return null;
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
        public int AssignedCount
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
        /// <param name="item">The item to check. (Required)</param>
        /// <returns>True if the item is in the group.</returns>
        public bool Contains(BodyPart item)
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
        /// Clear all items, optionally resizing the internal buffer in the process.
        /// </summary>
        /// <param name="bufferSize">
        /// The new buffer size or -1 to keep the current buffer. [Limites: >= 0, or -1]
        /// </param>
        public void Clear(int bufferSize = -1)
        {
            if (bufferSize == -1)
                System.Array.Clear(m_Items, 0, m_Items.Length);
            else
                m_Items = new BodyPart[Mathf.Max(0, bufferSize)];
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
        public void CompressAndAdd(params BodyPart[] newItems)
        {
            if (newItems == null || newItems.Length == 0)
            {
                m_Items = m_Items.GetCompressed();
                return;
            }

            m_Items = m_Items.CompressAndAddDistinct(true, newItems);
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
        /// <param name="bodyParts">The object to update. (Required)</param>
        /// <param name="items">The items.</param>
        /// <param name="asReference">
        /// If true the internal buffer will be replaced with a reference to <paramref name="items"/>, otherwise 
        /// <paramref name="items"/> will be copied.
        /// </param>
        public static void UnsafeReplaceItems(
            BodyPartGroup bodyParts, bool asReference, params BodyPart[] items)
        {
            // Null elements are allowed.

            if (items == null)
                bodyParts.Clear(0);
            else
                bodyParts.m_Items = asReference ? items : (BodyPart[])items.Clone();
        }

        #endregion

        #region Item State and Mutators

        /// <summary>
        /// Sets the ownership of the items.
        /// </summary>
        /// <param name="gameObject">The owner, or null.</param>
        /// <param name="unassignedOnly">
        /// If true, ownership will only be set for items that have no owner assigned. Otherwise the new owner will
        /// be applied to all items.
        /// </param>
        public void SetOwnership(GameObject gameObject, bool unassignedOnly = false)
        {
            for (int i = 0; i < m_Items.Length; i++)
            {
                if (m_Items[i])
                {
                    if (!unassignedOnly || m_Items[i].Context)
                        m_Items[i].Context = gameObject;
                }
                else
                    m_Items[i] = null;  // Auto-clean destroyed.  A good time to do it.
            }
        }

        /// <summary>
        /// Apply the specified layer to all items in the group.
        /// </summary>
        /// <param name="layer">The layer. [0 &lt;= value &tl;= 31]</param>
        public void ApplyLayerToAll(int layer)
        {
            if (layer < 0 || layer > 31)
            {
                Debug.LogError("ApplyBodyPartLayer: Body collider layer is out of range: " + layer);
                return;
            }

            for (int i = 0; i < m_Items.Length; i++)
            {
                if (m_Items[i])
                    m_Items[i].ColliderLayer = layer;
            }
        }

        #endregion
    }
}
