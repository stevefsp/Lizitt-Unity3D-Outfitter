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
    /// <see cref="StandardAccessory"/> custom editor.
    /// </summary>
    [CustomEditor(typeof(StandardAccessory))]
    public class StandardAccessoryEditor
        : Editor
    {
        private const string ObserversPath = "m_Observers";

        /// <summary>
        /// See Unity documentation.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var prop = serializedObject.GetIterator();
            prop.NextVisible(true);

            EditorGUILayout.PropertyField(prop);

            EditorGUILayout.Space();

            while (prop.NextVisible(false))
            {
                if (prop.propertyPath == ObserversPath)
                    DrawObservers(prop);
                else
                    EditorGUILayout.PropertyField(prop, true);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawObservers(SerializedProperty prop)
        {
            EditorGUILayout.Space();
            prop.isExpanded = EditorGUILayout.Foldout(prop.isExpanded, new GUIContent(prop.displayName, prop.tooltip));
            if (prop.isExpanded)
            {
                EditorGUILayout.HelpBox(
                    "Objects that implement the '" + typeof(IAccessoryObserver).Name + "' interface.",
                    MessageType.Info, true);
                EditorGUILayout.PropertyField(prop);
            }
            else
                EditorGUILayout.Space();
        }
    }
}
