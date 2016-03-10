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
using System.Collections.Generic;

namespace com.lizitt.outfitter.editor
{
    public class OffsetMounterObjectEditor
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

        private OffsetMounterObject Target
        {
            get { return target as OffsetMounterObject; }
        }

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

            EditorGUILayout.Space();
            OutfitterEditorUtil.ShowInspectorActions =
                EditorGUILayout.Foldout(OutfitterEditorUtil.ShowInspectorActions, "Actions");

            if (OutfitterEditorUtil.ShowInspectorActions)
            {
                OnActionGUI();

                GUILayout.Label("Copy Settings", EditorStyles.boldLabel);

                bool failedOnError;
                var item = EditorGUILayout.ObjectField("Other -> This", null, typeof(Object), true);
                if (item)
                {
                    Undo.IncrementCurrentGroup();
                    Undo.RecordObject(target, "Copy Other settings to mounter.");

                    if (item == target)
                        Debug.Log("Don't be silly!  I can't copy from myself.", this);  // Not error.  Keep it light.
                    else if (!CopyFrom(item, out failedOnError) && !failedOnError)
                    {
                        Debug.LogError("Uknown object type.  Can't copy settings from " + item.GetType().Name, this);
                    }

                    Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
                }

                item = EditorGUILayout.ObjectField("This -> Other", null, typeof(Object), true);
                if (item)
                {
                    var undoLabel = "Copy Mounter Settings to Other";

                    Undo.IncrementCurrentGroup();
                    Undo.RecordObject(item, undoLabel);

                    if (item == target)
                        Debug.Log("Don't be silly!  I can't copy to myself.", this);  // Not error.  Keep it light.
                    else if (!CopyTo(item, undoLabel, out failedOnError) && !failedOnError)
                    {
                        Debug.LogError("Uknown object type. Can't copy settings to " + item.GetType().Name, this);
                    }

                    Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
                }
            }
        }

        protected virtual void OnActionGUI()
        {
            // Do nothing.
        }

        private bool CopyTo(Object copyTarget, string undoLabel, out bool failedOnError)
        {
            SimpleOffsetAccessory acc = null;

            if (copyTarget is Transform)
            {
                acc = GetOffsetAccessory((copyTarget as Transform).gameObject, out failedOnError);
                if (!acc)
                    return false;
            }

            if (copyTarget is GameObject)
            {
                acc = GetOffsetAccessory(copyTarget as GameObject, out failedOnError);
                if (!acc)
                    return false;
            }

            failedOnError = false;

            if (acc || copyTarget is SimpleOffsetAccessory)
            {
                acc = acc ? acc : copyTarget as SimpleOffsetAccessory;

                Undo.RecordObject(acc, undoLabel);

                acc.PositionOffset = Target.PositionOffset;
                acc.RotationOffset = Target.RotationOffset;
                acc.MountedCoverage = Target.MountedCoverage;

                return true;
            }

            if (copyTarget is OffsetMounterObject)
            {
                CopyTo(copyTarget as OffsetMounterObject);
                return true;
            }

            return false;
        }

        protected virtual void CopyTo(OffsetMounterObject copyTarget)
        {
            copyTarget.PositionOffset = Target.PositionOffset;
            copyTarget.RotationOffset = Target.RotationOffset;
            copyTarget.MountedCoverage = Target.MountedCoverage;
        }

        private bool CopyFrom(Object source, out bool failedOnError)
        {
            SimpleOffsetAccessory acc = null;

            if (source is Transform)
            {
                var trans = source as Transform;
                acc = GetOffsetAccessory(trans.gameObject, out failedOnError);

                if (failedOnError)
                    return false;

                if (!acc)
                {
                    Target.PositionOffset = trans.localPosition;
                    Target.RotationOffset = trans.localEulerAngles;
                    return true;
                }
            }

            if (source is GameObject)
            {
                var go = source as GameObject;
                acc = GetOffsetAccessory(go, out failedOnError);

                if (failedOnError)
                    return false;

                if (!acc)
                {
                    Target.PositionOffset = go.transform.localPosition;
                    Target.RotationOffset = go.transform.localEulerAngles;
                    return true;
                }
            }

            failedOnError = false;

            if (acc || source is SimpleOffsetAccessory)
            {
                acc = acc ? acc : source as SimpleOffsetAccessory;

                Target.PositionOffset = acc.PositionOffset;
                Target.RotationOffset = acc.RotationOffset;
                Target.MountedCoverage = acc.MountedCoverage;

                return true;
            }

            if (source is OffsetMounterObject)
            {
                CopyFrom(source as OffsetMounterObject);
                return true;
            }

            return false;
        }

        protected virtual void CopyFrom(OffsetMounterObject copySource)
        {
            Target.PositionOffset = copySource.PositionOffset;
            Target.RotationOffset = copySource.RotationOffset;
            Target.MountedCoverage = copySource.MountedCoverage;
        }

        private SimpleOffsetAccessory GetOffsetAccessory(GameObject gameObject, out bool failedOnError)
        {
            failedOnError = false;
            var accs = gameObject.GetComponents<SimpleOffsetAccessory>();

            if (accs.Length == 1)
                return accs[0];

            if (accs.Length > 0)
            {
                Debug.LogError("More than one offset accessory on the object.  Don't know which to use.", this);
                failedOnError = true;
                return null;
            }

            return null;
        }
    }
}
