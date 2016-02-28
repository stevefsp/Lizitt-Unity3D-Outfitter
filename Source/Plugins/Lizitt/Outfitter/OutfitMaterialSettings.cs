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
    /// Defines a group of related, unique outfit materials as a serializable reference.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Can't contain more than one material of each type.  (One material for 'body',  one for 'team', etc.)  
    /// This makes the class useful for defining a group of materials that are to be applied to an outfit.
    /// </para>
    /// </remarks>
    [CreateAssetMenu(menuName = LizittUtil.LizittMenu + "Outfit Material Settings", 
        order = OutfitterUtil.BaseMenuOrder + 5)]
    public sealed class OutfitMaterialSettings
        : ScriptableObject
    {
        [Space]

        [SerializeField]
        [OutfitMaterialGroup]
        private OutfitMaterialGroup m_Materials = new OutfitMaterialGroup();

        /// <summary>
        /// The materials to apply to body outfits.
        /// </summary>
        public OutfitMaterialGroup OutfitMaterials
        {
            get { return m_Materials; }
            // Only add a setter if it is truely needed.  It introduces reference sharing that is not serializable.
        }

        #region Utility Members

        /// <summary>
        /// Apply the outfit materials to anoutfit.
        /// </summary>
        /// <param name="outfit">The outfit.</param>
        public int ApplyMaterials(Outfit outfit)
        {
            return m_Materials.ApplyTo(outfit);
        }

        #endregion
    }
}
