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
    /// Defines the renderer target of a common (well known) outfit material.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This object is helpful in defining a common (well knwon) renderer material that can be accessed by clients
    /// without the need to know where the renderer or material is.  I.e. A client can access the material only 
    /// using its <see cref="OutfitMaterialType"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="OutfitMaterialType"/>
    [System.Serializable]
    public struct OutfitMaterialTarget
    {
        [SerializeField]
        [Tooltip("The material type.")]
        private OutfitMaterialType m_Type;

        /// <summary>
        /// The material type.
        /// </summary>
        public OutfitMaterialType MaterialType
        {
            get { return m_Type; }
            set { m_Type = value; }
        }

        [SerializeField]
        [Tooltip("The location of the material. (Required)")]
        private RendererMaterialPtr m_Target;

        /// <summary>
        /// The location of the material. (Required)
        /// </summary>
        public RendererMaterialPtr Target
        {
            get { return m_Target; }
            set { m_Target = value; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="typ">The material type.</param>
        /// <param name="target">The location of the material.</param>
        public OutfitMaterialTarget(OutfitMaterialType typ, RendererMaterialPtr target)
        {
            this.m_Type = typ;
            this.m_Target = target;
        }

        /// <summary>
        /// True if the target is fully defined.
        /// </summary>
        public bool IsDefined
        {
            get { return m_Target != null && m_Target.IsDefined; }
        }

        /// <summary>
        /// The target's material, or null if there is none.
        /// </summary>
        public Material SharedMaterial
        {
            get { return m_Target == null ? null : m_Target.SharedMaterial; }
        }

        /// <summary>
        /// The outfit material for the target.
        /// </summary>
        public OutfitMaterial SharedOutfitMaterial
        {
            get { return new OutfitMaterial(m_Type, SharedMaterial); }
        }

        /// <summary>
        /// Applies the specified material to the target.
        /// </summary>
        /// <param name="material">The material to apply.</param>
        /// <remarks>True if the material was applied, or false if no change was made.</remarks>
        public bool ApplySharedMaterial(Material material)
        {
            if (m_Target != null)
                return Target.ApplySharedMaterial(material);
            return false;
        }
    }
}