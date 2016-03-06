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
using com.lizitt.editor;

namespace com.lizitt.outfitter.editor
{
    [CustomEditor(typeof(BodyMaterialSettings))]
    public sealed class BodyMaterialSettingsEditor
        : Editor
    {
        private ReorderableListControl m_ListControl;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var prop = serializedObject.GetIterator();

            prop.NextVisible(true);  // Ignore the script field.
            while (prop.NextVisible(false))
            {
                if (prop.isArray)
                    DrawIgnores(prop);
                else
                    EditorGUILayout.PropertyField(prop);
            }

            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawIgnores(SerializedProperty property)
        {
            if (m_ListControl == null)
            {
                m_ListControl = new ReorderableListControl(
                    property, "Do not persist", ReorderableListControl.SingleElementHeight);
            }

            var position = EditorGUILayout.GetControlRect(false, m_ListControl.GetPropertyHeight(property));
            m_ListControl.OnGUI(position, property, GUIContent.none); 
        }
    }
}
