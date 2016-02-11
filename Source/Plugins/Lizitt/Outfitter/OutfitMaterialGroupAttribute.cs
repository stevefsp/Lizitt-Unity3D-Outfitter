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
    /// Provides a user friendly re-orderable list of local outfit materials when added to
    /// a <see cref="OutfitMaterialGroup"/> field.
    /// </summary>
    public class OutfitMaterialGroupAttribute
        : PropertyAttribute
    {
        /// <summary>
        /// The path to the reference object to search for local components, relative to 
        /// SerializedProperty.serializedObject, or null if the reference object is 
        /// SerializedProperty.serializedObject.targetObject.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If non-null, the path must refer to a serialized property of type 'ObjectReference'.
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
        public OutfitMaterialGroupAttribute(bool allowMultipleTargets = false, string searchPropertyPath = null)
        {
            SearchPropertyPath = searchPropertyPath;
            AllowMultipleTargets = allowMultipleTargets;
        }
    }
}