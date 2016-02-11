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
using UnityEditor;
using UnityEngine;

namespace com.lizitt.outfitter.editor
{
    /// <summary>
    /// Editor configuation for an outfit material list.
    /// </summary>
    public struct OutfitMaterialListInfo
    {
        /// <summary>
        /// The path name of the outfit material type field.
        /// </summary>
        public string ItemTypePropName { get; set; }

        /// <summary>
        /// The path name of the outfit material data field.
        /// </summary>
        public string ItemDataPropName { get; set; }

        /// <summary>
        /// The header title for the list.
        /// </summary>
        public string ListHeaderLabel { get; set; }

        /// <summary>
        /// The draw height of each list element.
        /// </summary>
        public float ListElementHeight { get; set; }

        /// <summary>
        /// If true, allow multiple targets for each material type.
        /// </summary>
        public bool AllowMultipleTargets { get; set; }

        /// <summary>
        /// The validation method to use for each list entry.
        /// </summary>
        public ValidateProperty Validate { get; set; }

        /// <summary>
        /// The draw method to use for each list element.
        /// </summary>
        public System.Action<Rect, SerializedProperty, GUIContent, GUIStyle> 
            DrawElement { get; set; }
    }
}

