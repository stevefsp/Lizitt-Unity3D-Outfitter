﻿/*
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
using System.Collections.Generic;
using com.lizitt.u3d;

namespace com.lizitt.outfitter
{
    /// <summary>
    /// Defines a read-only group of material overrides.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Designed for use as a field in a Unity component.  When used with 
    /// <see cref="MaterialOverrideGroupAttribute"/>, it provides a better editor experience than 
    /// is available with arrays.
    /// </para>
    /// </remarks>
    [System.Serializable]
    public struct MaterialOverrideGroup
        : IEnumerable<MaterialOverride>
    {
        /*
         * Design notes:
         * 
         * This is a hack that uses an anti-pattern.  I decided the benefits outweight the
         * evil in this case.
         * 
         * The issue is that Unity doesn't support custom drawers for array fields.  The drawer
         * applies to the array elements, not the array.  I don't see that being fixed any
         * time soon, so this hack solves the problem.
         * 
         * Unless 'real' functionality is added to this structure, keep it read-only at run-time and
         * don't encourage usage beyond its current scope.
         */

        [SerializeField]
        private MaterialOverride[] m_Items;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="size">The number of accessories.</param>
        public MaterialOverrideGroup(int size)
        {
            m_Items = new MaterialOverride[Mathf.Max(0, size)];
        }

        /// <summary>
        /// The accessory at the specified index.
        /// </summary>
        /// <param name="index">
        /// Accessory index. [Limit: 0 &lt;= value &lt; <see cref="Count"/>]
        /// </param>
        /// <returns>The accessory at the specified index.</returns>
        public MaterialOverride this[int index]
        {
            get { return m_Items[index]; }
        }

        /// <summary>
        /// The number of accessories in the group. [Limit: >= 0]
        /// </summary>
        public int Count
        {
            get { return m_Items.Length; }
        }

        /// <summary>
        /// Accessory enumperator.
        /// </summary>
        /// <returns>Accessory enumperator.</returns>
        public IEnumerator<MaterialOverride> GetEnumerator()
        {
            foreach (var item in m_Items)
                yield return item;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}