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
using System.Collections.Generic;

namespace com.lizitt.outfitter.editor
{
    public class OffsetMounterObjectEdtior
        : Editor
    {
        /*
         * Design notes:
         * 
         * Fun with h*cking.
         * 
         * This editor moves all non-coverage enumerations to the top, draws the coverage, then draws the rest
         * of the fields in their normal order.
         */

        private readonly List<SerializedProperty> m_Properties = new List<SerializedProperty>();

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var prop = serializedObject.GetIterator();
            prop.NextVisible(true);

            EditorGUILayout.PropertyField(prop);  // Script field.

            SerializedProperty coverageProp = null;

            m_Properties.Clear();
            while (prop.NextVisible(false))
            {
                if (prop.propertyType == SerializedPropertyType.Enum)
                {
                    if (coverageProp == null)
                        coverageProp = serializedObject.FindProperty(prop.propertyPath);
                    else
                        EditorGUILayout.PropertyField(prop);  // Location type(s);
                    continue;
                }

                m_Properties.Add(serializedObject.FindProperty(prop.propertyPath));
            }

            EditorGUILayout.PropertyField(coverageProp);

            foreach (var item in m_Properties)
                EditorGUILayout.PropertyField(item);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
