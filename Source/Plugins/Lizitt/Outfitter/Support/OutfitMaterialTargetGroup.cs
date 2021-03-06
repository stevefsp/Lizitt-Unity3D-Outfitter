﻿/*
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
    /// Provides a user friendly re-orderable list of local outfit materials when added to a 
    /// <see cref="OutfitMaterialTargetGroup"/> field.
    /// </summary>
    public sealed class OutfitMaterialTargetGroupAttribute
        : PropertyAttribute
    {
        // TODO: v0.3: Add support both absolute and relative paths.

        /// <summary>
        /// The path to the reference object to search for local components.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The value is relative to SerializedProperty.serializedObject, or null if the reference object is 
        /// SerializedProperty.serializedObject.targetObject. If non-null, the path must refer to a serialized 
        /// property of type 'ObjectReference'.
        /// </para>
        /// </remarks>
        public string SearchPropertyPath { get; private set; }

        /// <summary>
        /// If true, then the list is allowed to contain multiple targets for each material type.
        /// </summary>
        public bool AllowMultipleTargets { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// <para>
        /// If <paramref name="searchPropertyPath"/> is non-null, it must refer to a serialized 
        /// property of type 'ObjectReference'.
        /// </para>
        /// </remarks>
        /// <param name="searchPropertyPath">
        /// The path to the reference object to search for local components, relative to 
        /// SerializedProperty.serializedObject, or null if the reference object is 
        /// SerializedProperty.serializedObject.targetObject.
        /// </param>
        public OutfitMaterialTargetGroupAttribute(bool allowMultipleTargets = false, string searchPropertyPath = null)
        {
            SearchPropertyPath = searchPropertyPath;
            AllowMultipleTargets = allowMultipleTargets;
        }
    }

    /// <summary>
    ///  A group of ordered outfit material targets. (Body, head, eyes, etc.)
    /// </summary>
    [System.Serializable]
    public sealed class OutfitMaterialTargetGroup
    {
        [SerializeField]
        private List<OutfitMaterialTarget> m_Items = new List<OutfitMaterialTarget>(0);

        /// <summary>
        /// The maximum number of materials currently defined.  (Some entries may be undefined.)
        /// </summary>
        public int Count
        {
            get { return m_Items.Count; }
        }

        /// <summary>
        /// The outfit material target at the specified index. (May be undefined.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// There is no guarentee that the returned target is fully defined.  (See: <see cref="IsDefined"/>)
        /// </para>
        /// </remarks>
        /// <param name="index">The index.</param>
        /// <returns>The outfit material target at the specified index.  (May be undefined.)</returns>
        public OutfitMaterialTarget this[int index]
        {
            get { return m_Items[index]; }
        }

        /// <summary>
        /// Removes the outfit material target at the specified index.
        /// </summary>
        /// <param name="index">The index. [0 &lt;= value &lt; <see cref="Count"/>]</param>
        public void RemoveAt(int index)
        {
            m_Items.RemoveAt(index);
        }

        /// <summary>
        /// Adds the target for the specified type.
        /// </summary>
        /// <remarks>
        /// <para>
        /// In order to support the most use cases, the following is allowed:  More than one target per material
        /// type, the same target for multiple material types, and undefined targets.
        /// </para>
        /// </remarks>
        /// <param name="typ">The material type.</param>
        /// <param name="target">The target.</param>
        public void AddTarget(OutfitMaterialType typ, RendererMaterialPtr target)
        {
            // Adding undefined target it odd, but doesn't hurt anything, so it is not worth  adding, testing, and
            // maintaining a bunch of error/warning checks.

            m_Items.Add(new OutfitMaterialTarget(typ, target));
        }

        /// <summary>
        /// True if the outfit material exists and has at lease one fully defined target.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Just because a material type is in the group not mean that it is fully defined. It may not have it's
        /// renderer assigned, or its material index may be set to an undefined value.
        /// </para>
        /// <para>
        /// The group may have more than one target for each material type.  This method just makes sure that there
        /// is atleast one that is fully defined.
        /// </para>
        /// </remarks>
        /// <param name="typ">The material type.</param>
        /// <returns>True if the outfit material exists and has been fully defined.</returns>
        public bool IsDefined(OutfitMaterialType typ)
        {
            for (int i = 0; i < m_Items.Count; i++)
            {
                if (m_Items[i].MaterialType == typ && m_Items[i].IsDefined)
                    return true;
            }

            return false;
        }

        private static readonly List<OutfitMaterialType> m_MatTypeBuffer = new List<OutfitMaterialType>();

        /// <summary>
        /// Get the material types that are defined and can be set.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is not thread-safe.
        /// </para>
        /// </remarks>
        /// <returns>The material types that are defined and can be set.</returns>
        public OutfitMaterialType[] GetMaterialTypes()
        {
            for (int i = 0; i < m_Items.Count; i++)
            {
                if (m_Items[i].IsDefined && !m_MatTypeBuffer.Contains(m_Items[i].MaterialType))
                    m_MatTypeBuffer.Add(m_Items[i].MaterialType);
            }

            var result = m_MatTypeBuffer.ToArray();
            m_MatTypeBuffer.Clear();

            return result;
        }

        // TODO: v0.3: Add accessores for non-shared materials.

        /// <summary>
        /// Get the shared material for the specified type, or null there is none.
        /// </summary>
        /// <remarks>
        /// <para>
        /// There may be more than one target for a material type.  This method will return the first found since
        /// it is expected all material targets will have the same material.
        /// </para>
        /// </remarks>
        /// <param name="typ">The material type.</param>
        /// <returns>The shared material for the specified type, or null there is none.</returns>
        public Material GetSharedMaterial(OutfitMaterialType typ)
        {
            for (int i = 0; i < m_Items.Count; i++)
            {
                if (m_Items[i].MaterialType == typ && m_Items[i].Target != null)
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
        /// <paramref name="material"/> is not null.  (This method can't be used to apply a null material.)
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
                    if (m_Items[i].MaterialType == typ)
                    {
                        if (m_Items[i].Target.ApplySharedMaterial(material))
                            count++;
                    }
                }
            }
        
            return count;
        }

        /// <summary>
        /// Removes all outfit matierials from the group.
        /// </summary>
        public void Clear()
        {
            m_Items.Clear();
        }
    }
}