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
    /// Defines a group of related outfit materials. (Body, head, eyes, etc.)
    /// </summary>
    /// <remarks>
    /// <para>
    /// WARNING: This class can't be used in an array.  E.g. An array of <see cref="OutfitMaterialGroup"/> objects,  
    /// or an array of objects that contain <see cref="OutfitMaterialGroup"/> objects.  This is a limitation of
    /// the Unity's ReorderableList GUI control.
    /// </para>
    /// </remarks>
    [System.Serializable]
    public class OutfitMaterialGroup
    {
        [SerializeField]
        private List<OutfitMaterial> m_Items = new List<OutfitMaterial>(0);

        /// <summary>
        /// The number of items in the group.  (Some may be undefined.)
        /// </summary>
        public int Count
        {
            get { return m_Items.Count; }
        }

        /// <summary>
        /// The material for the specified type, of null if there is none.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Setting a type to null will remove the material's definition.  Setting the value of an undefined type
        /// to non-null will add the material definition.
        /// </para>
        /// </remarks>
        /// <param name="typ">The material type.</param>
        /// <returns>The material for the specified type, of null if there is none.</returns>
        public Material this[OutfitMaterialType typ]
        {
            get 
            {
                for (int i = 0; i < m_Items.Count; i++)
                {
                    var item = m_Items[i];
                    if (item.MaterialType == typ)
                        return item.Material ? item.Material : null;
                }

                return null;
            }
            set 
            {
                for (int i = 0; i < m_Items.Count; i++)
                {
                    var item = m_Items[i];

                    if (item.MaterialType == typ)
                    {
                        if (value)
                        {
                            item.Material = value;
                            m_Items[i] = item;
                        }
                        else
                            m_Items.RemoveAt(i);

                        return;
                    }
                }

                if (value)
                    m_Items.Add(new OutfitMaterial(typ, value)); 
                else
                    Debug.LogError("Can't add a null outfit material: Type: " + typ);
            }
        }

        /// <summary>
        /// The item at the specified index. (May be undefined.)
        /// </summary>
        /// <param name="index">The index. [0 &lt;= value &tl; <see cref="Count"/>]</param>
        /// <returns>The item at the specified index.</returns>
        public OutfitMaterial this[int index]
        {
            get 
            {
                var item = m_Items[index];

                if (!item.Material)
                    item.Material = null;

                return item;
            }
        }

        /// <summary>
        /// True if the material is fully defined.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A material is fully defined if it has both a definition and the the definition's material is valid.
        /// </para>
        /// </remarks>
        /// <param name="typ">The material type.</param>
        /// <returns>True if the material is fully defined.</returns>
        public bool IsDefined(OutfitMaterialType typ)
        {
            for (int i = 0; i < m_Items.Count; i++)
            {
                if (m_Items[i].MaterialType == typ && m_Items[i].Material)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Apply all fully defined materials to the outfit.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method can be called lazily. Missing materials, undefined materials, and undefined targets 
        /// will simply be skipped.
        /// </para>
        /// </remarks>
        /// <param name="outfit">The outfit to apply the materials to. (Required)</param>
        /// <returns>The number of outfit materials applied.  (Not the total targets applied.)</returns>
        public int ApplyTo(Outfit outfit)
        {
            int count = 0;

            if (outfit)
            {
                for (int i = 0; i < m_Items.Count; i++)
                {
                    var item = m_Items[i];

                    var mat = item.Material;  // Only non-null materials.
                    if (mat)
                        count += outfit.ApplySharedMaterial(item.MaterialType, mat) == 0 ? 0 : 1;
                }
            } 
            else
                Debug.LogError("Can't apply materials to a null outfit.");

            return count;
        }

        /// <summary>
        /// Synchronizes the materials in the outfit to the group.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="outfit">The outfit to synchronize from.</param>
        /// <param name="addNew">
        /// Add materials that exist in the outfit but not in the group, otherwise ignore new materials.
        /// </param>
        /// <param name="removeUndefined">Remove materials not found in the outfit from the group.</param>
        public void SyncFrom(Outfit outfit, bool addNew, bool removeUndefined)
        {
            if (!outfit)
            {
                Debug.LogError("Can't synchronized from a null outfit.");
                return;
            }

            for (int i = m_Items.Count - 1; i >= 0; i--)
            {
                var item = m_Items[i];

                var omat = outfit.GetSharedMaterial(item.MaterialType);
                if (omat)
                {
                    item.Material = omat;
                    m_Items[i] = item;
                }
                else if (removeUndefined)
                    m_Items.RemoveAt(i);
            }

            if (addNew)
            {
                // Technically the material is sync'd twice.  But this is still the most efficient method.
                for (int i = 0; i < outfit.OutfitMaterialCount; i++)
                {
                    var omat = outfit.GetSharedMaterial(i);
                    if (omat.Material)
                        this[omat.MaterialType] = omat.Material;
                }
            }
        }
    }
}