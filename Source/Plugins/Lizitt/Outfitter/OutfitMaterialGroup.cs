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
using System.Collections.Generic;

namespace com.lizitt.outfitter
{
    /// <summary>
    ///  A group of outfit materials. (Body, head, eyes, etc.)
    /// </summary>
    [System.Serializable]
    public class OutfitMaterialGroup
    {
        [SerializeField]
        private List<OutfitMaterial> m_Items = new List<OutfitMaterial>(0);

        /// <summary>
        /// The number of materials defined.
        /// </summary>
        public int Count
        {
            get { return m_Items.Count; }
        }

        /// <summary>
        /// The outfit material at the specified index. (May not be fully defined.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// There is no guarentee that the returned outfit material is fully defined.  
        /// (See: <see cref="IsDefined"/>)
        /// </para>
        /// </remarks>
        /// <param name="index">The index.</param>
        /// <returns>
        /// The outfit material at the specified index.  (May not be fully defined.)
        /// </returns>
        public OutfitMaterial this[int index]
        {
            get { return m_Items[index]; }
        }

        public void RemoveAt(int index)
        {
            m_Items.RemoveAt(index);
        }

        /// <summary>
        /// Adds the target for the specified type.
        /// </summary>
        /// <remarks>
        /// <para>
        /// In order to support the most use cases, the following is allowed:  More than one 
        /// target per material type, the same target for multiple material types, and undefined 
        /// targets.
        /// </para>
        /// </remarks>
        /// <param name="typ">The material type.</param>
        /// <param name="target">The target.</param>
        public void AddTarget(OutfitMaterialType typ, RendererMaterialPtr target)
        {
            // Note: Duplicate entries for the same type doesn't matter.  Just extra processing.
            // Defining the same target for more than one type is odd, but can be a valid use case.
            // Adding undefined target it odd, but doesn't hurt anything.  So it is not worth 
            // adding, testing, and maintaining a bunch of error/warning checks.

            m_Items.Add(new OutfitMaterial(typ, target));
        }

        /// <summary>
        /// True if the outfit material exists and has at lease one fully defined target.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Just because an outfit material exists does not mean that it is fully defined.
        /// It may not have it's renderer assigned, or its material index may be set to an
        /// undefined value.
        /// </para>
        /// <para>
        /// The group may have more than one target for each material type.  This method just
        /// makes sure that there is atleast one that is fully defined.
        /// </para>
        /// </remarks>
        /// <param name="typ">The material type.</param>
        /// <returns>True if the outfit material exists and has been fully defined.</returns>
        public bool IsDefined(OutfitMaterialType typ)
        {
            for (int i = 0; i < m_Items.Count; i++)
            {
                if (m_Items[i].Type == typ && m_Items[i].Target != null && m_Items[i].Target.IsDefined)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Get the shared material for the specified type, or null there is none.
        /// </summary>
        /// <remarks>
        /// <para>
        /// There may be more than one target for a material type.  This method will return
        /// the first found since it is expected all material targets will have the same material.
        /// </para>
        /// </remarks>
        /// <param name="typ">The material type.</param>
        /// <returns>The shared material for the specified type, or null there is none.</returns>
        public Material GetSharedMaterial(OutfitMaterialType typ)
        {
            for (int i = 0; i < m_Items.Count; i++)
            {
                if (m_Items[i].Type == typ && m_Items[i].Target != null)
                    return m_Items[i].Target.SharedMaterial;
            }

            return null;
        }

        /// <summary>
        /// Apply the material to the outfit material target.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The material will be applied if the outfit material exists, is fully defined, and 
        /// <paramref name="material"/> is not null.  (This method can't be used to apply
        /// a null material.)
        /// </para>
        /// </remarks>
        /// <param name="typ">The material type.</param>
        /// <param name="material">The material to apply. (Required)</param>
        /// <returns>The number of targets the material was applied to.</returns>
        public int ApplySharedMaterial(OutfitMaterialType typ, Material material)
        {
            int count = 0;

            if (material)
            {
                for (int i = 0; i < m_Items.Count; i++)
                {
                    if (m_Items[i].Type == typ)
                    {
                        if (m_Items[i].Target.ApplySharedMaterial(material))
                            count++;
                    }
                }
            }
        
            return count;
        }

        /// <summary>
        /// Removeds all outfit matierials from the group.
        /// </summary>
        public void Clear()
        {
            m_Items.Clear();
        }
    }
}