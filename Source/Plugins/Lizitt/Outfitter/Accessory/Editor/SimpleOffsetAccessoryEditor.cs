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
using UnityEditor;
using UnityEngine;
using com.lizitt.editor;

namespace com.lizitt.outfitter.editor
{
    /// <summary>
    /// <see cref="SimpleOffsetAccessory"/> custom editor.
    /// </summary>
    [CustomEditor(typeof(SimpleOffsetAccessory))]
    [CanEditMultipleObjects]
    public class SimpleOffsetAccessoryEditor
        : Editor
    {
        private static BehaviourPropertyHelper<SimpleOffsetAccessory> m_Helper = null;

        void OnEnable()
        {
            if (m_Helper == null)
                m_Helper = new BehaviourPropertyHelper<SimpleOffsetAccessory>();
        }

        /// <summary>
        /// See Unity documentation.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();

            if (!serializedObject.isEditingMultipleObjects)
            {
                EditorGUILayout.BeginHorizontal();

                var acc = target as Accessory;
                EditorGUILayout.LabelField(
                    string.Format("Owner is '{0}' ({1})", (acc.Owner ? acc.Owner.name : "None"), acc.Status));

                GUI.enabled = acc.Owner;
                if (GUILayout.Button("->", GUILayout.MaxWidth(30)))
                    Selection.activeObject = acc.Owner;
                GUI.enabled = true;

                EditorGUILayout.EndHorizontal();
            }

            m_Helper.LoadProperties(serializedObject, false);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(m_Helper.ExtractProperty<MountPointType>());
            EditorGUILayout.PropertyField(m_Helper.ExtractProperty<BodyCoverage>());
            EditorGUILayout.Space();

            var observerProp = m_Helper.ExtractProperty<AccessoryObserverGroup>();

            m_Helper.DrawProperties();

            EditorGUILayout.Space();

            DrawObservers(observerProp);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawObservers(SerializedProperty prop)
        {
            EditorGUILayout.Space();

            if (serializedObject.isEditingMultipleObjects)
            {
                EditorGUILayout.HelpBox("Multi-object editing not supported for Observers.", MessageType.None);
                return;
            }

            prop.isExpanded = EditorGUILayout.Foldout(prop.isExpanded, new GUIContent(prop.displayName, prop.tooltip));
            if (prop.isExpanded)
                EditorGUILayout.PropertyField(prop);
            else
                EditorGUILayout.Space();
        }
    }
}