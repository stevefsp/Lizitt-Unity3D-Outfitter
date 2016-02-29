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
    /// Associates a material with an outfit material type.
    /// </summary>
    [System.Serializable]
    public struct OutfitMaterial
    {
        [SerializeField]  // Tooltip isn't useful.
        [SortedEnumPopup(typeof(OutfitMaterialType))]
        private OutfitMaterialType m_Type;

        /// <summary>
        /// The type of the material.
        /// </summary>
        public OutfitMaterialType MaterialType
        {
            get { return m_Type; }
            set { m_Type = value; }
        }

        [SerializeField]  // Tooltip isn't useful.
        private Material m_Material;

        /// <summary>
        /// The material.
        /// </summary>
        public Material Material
        {
            get { return m_Material; }
            set { m_Material = value; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="typ">The material type.</param>
        /// <param name="material">The material.</param>
        public OutfitMaterial(OutfitMaterialType typ, Material material)
        {
            m_Type = typ;
            m_Material = material;
        }
    }
}