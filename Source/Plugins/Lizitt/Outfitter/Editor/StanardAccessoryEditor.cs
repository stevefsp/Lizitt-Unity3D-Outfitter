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
    /// <summary>
    /// <see cref="StandardAccessory"/> custom editor.
    /// </summary>
    [CustomEditor(typeof(StandardAccessory))]
    public class StandardAccessoryEditor
        : Editor
    {
        private static BehaviourPropertyHelper<StandardAccessory> m_Helper = null;

        void OnEnable()
        {
            if (m_Helper == null)
                m_Helper = new BehaviourPropertyHelper<StandardAccessory>();
        }

        /// <summary>
        /// See Unity documentation.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            m_Helper.LoadProperties(serializedObject, true);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(m_Helper.ExtractProperty<MountPointType>());
            EditorGUILayout.Space();

            var observerProp = m_Helper.ExtractProperty<AccessoryObserverGroup>();
            var mounterProp = m_Helper.ExtractProperty<AccessoryMounterGroup>();

            m_Helper.DrawProperties();

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(mounterProp);

            DrawObservers(observerProp);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawObservers(SerializedProperty prop)
        {
            EditorGUILayout.Space();
            prop.isExpanded = EditorGUILayout.Foldout(prop.isExpanded, new GUIContent(prop.displayName, prop.tooltip));
            if (prop.isExpanded)
                EditorGUILayout.PropertyField(prop);
            else
                EditorGUILayout.Space();
        }
    }
}
