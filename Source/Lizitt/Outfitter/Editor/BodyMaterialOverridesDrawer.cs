﻿/*
 * Copyright (c) 2015 Stephen A. Pratt
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
    [CustomPropertyDrawer(typeof(BodyMaterialOverrides))]
    [CanEditMultipleObjects]
    public sealed class BodyMaterialOverridesDrawer
        : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 4
                + EditorGUIUtility.standardVerticalSpacing * 5;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // This flat layout looks better than the default foldout layout.
            // If this ends up not fitting a significant number of use-cases, consider switching 
            // to an attribute based design.

            label = EditorGUI.BeginProperty(position, label, property);

            float space = EditorGUIUtility.standardVerticalSpacing;

            var rect = new Rect(
                position.x, position.y + space, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(rect, label);

            rect = new Rect(rect.x, rect.yMax + space, rect.width, rect.height);
            EditorGUI.PropertyField(rect, property.FindPropertyRelative("m_Body"));

            rect = new Rect(rect.x, rect.yMax + space, rect.width, rect.height);
            EditorGUI.PropertyField(rect, property.FindPropertyRelative("m_Head"));

            rect = new Rect(rect.x, rect.yMax + space, rect.width, rect.height);
            EditorGUI.PropertyField(rect, property.FindPropertyRelative("m_Eye"));

            EditorGUI.EndProperty();
        }
    }
}