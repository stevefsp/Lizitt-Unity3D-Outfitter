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
using System.Collections.Generic;
using UnityEngine;

namespace com.lizitt.outfitter
{
    /// <summary>
    /// A utility class that keeps track of body coverage as accessories are added and removed.
    /// </summary>
    /// <remarks>
    /// The coverage of an accessory is determined when the accessory is added to the group and 
    /// stays constant until the accessory is removed.  Dynamic coverage behavior is ignored 
    /// while attached.
    /// </remarks>
    public class AccessoryCoverageGroup
    {
        private readonly List<BodyAccessory> m_Accessories;
        private readonly List<BodyCoverage> m_Coverages;
        private BodyCoverage m_Coverage = 0;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="initialSize">The initial size of the buffers.</param>
        public AccessoryCoverageGroup(int initialSize = 1)
        {
            m_Accessories = new List<BodyAccessory>(initialSize);
            m_Coverages = new List<BodyCoverage>(initialSize);
        }

        /// <summary>
        /// The current coverage.
        /// </summary>
        public BodyCoverage Coverage
        {
            get { return m_Coverage; }
        }

        /// <summary>
        /// True if the group contains the accessory.
        /// </summary>
        /// <param name="accessory">The accessory to check.</param>
        /// <returns>True if the group contains the accessory.</returns>
        public bool Contains(BodyAccessory accessory)
        {
            return m_Accessories.Contains(accessory);
        }

        /// <summary>
        /// Add the accessory.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The operation will fail if the accessory is null or already added.
        /// </para>
        /// </remarks>
        /// <param name="accessory">The accessory to add.</param>
        /// <returns>True if the add was successful.</returns>
        public bool Add(BodyAccessory accessory)
        {
            if (!accessory)
            {
                Debug.LogError("Accessory is null.");
                return false;
            }

            if (m_Accessories.Contains(accessory))
            {
                Debug.LogError("Accessory has already been added: " + accessory.name);
                return false;
            }

            m_Accessories.Add(accessory);

            var coverage = accessory.Coverage;

            m_Coverage |= coverage;
            m_Coverages.Add(coverage);

            if (m_StatusChangeHandler == null)
                m_StatusChangeHandler = new BodyAccessory.StatusChange(HandleStatusChange);

            accessory.OnStatusChange += m_StatusChangeHandler;

            return true;
        }

        /// <summary>
        /// Remove the accessory.
        /// </summary>
        /// <param name="accessory">The accessory to remove.</param>
        /// <returns>
        /// True if the accessory was removed.  (Exists and removed.)
        /// </returns>
        public bool Remove(BodyAccessory accessory)
        {
            if (!accessory)
            {
                Debug.LogError("Accessory is null.");
                return false;
            }

            var i = m_Accessories.IndexOf(accessory);

            if (i == -1)
                // Support lazy removal.
                return false;

            m_Coverage &= ~m_Coverages[i];

            m_Accessories.RemoveAt(i);
            m_Coverages.RemoveAt(i);

            accessory.OnStatusChange -= m_StatusChangeHandler;

            return true;
        }

        private BodyAccessory.StatusChange m_StatusChangeHandler;

        private void HandleStatusChange(BodyAccessory accessory, AccessoryStatus status)
        {
            Remove(accessory);
        }
    }
}

