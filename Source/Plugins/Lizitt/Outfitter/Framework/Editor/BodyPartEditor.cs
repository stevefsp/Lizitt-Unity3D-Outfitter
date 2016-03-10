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
    /// <see cref="BodyPart"/> custom editor.
    /// </summary>
    [CustomEditor(typeof(BodyPart))]
    [CanEditMultipleObjects]
    public partial class BodyPartEditor
        : Editor
    {
        #region Main Editor

        private BodyPart Target
        {
            get { return target as BodyPart; }
        }

        void OnEnable()
        {
            if (!Target.Collider)
            {
                var col = Target.GetComponentInChildren<Collider>();
                if (col)
                {
                    Undo.IncrementCurrentGroup();
                    Undo.RecordObject(Target, "Initialize Body Part Collider");
                    Target.Collider = col;
                    Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
                }
            }
        }

        /// <summary>
        /// See Unity documentation.
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (serializedObject.isEditingMultipleObjects)
            {
                EditorGUILayout.HelpBox("Multi-object editing not supported for actions.", MessageType.None);
                return;
            }

            var bp = Target;
            if (bp.Rigidbody)
            {
                OutfitterEditorUtil.ShowInspectorActions =
                    EditorGUILayout.Foldout(OutfitterEditorUtil.ShowInspectorActions, "Actions");

                if (OutfitterEditorUtil.ShowInspectorActions)
                    DrawActions();
            }
            else
                EditorGUILayout.HelpBox("The collider does not have a rigidbody.", MessageType.Error);
            
            EditorGUILayout.Space();
        }

        #endregion

        #region Actions

        private readonly GUIContent m_StatusLabel = 
            new GUIContent("Collider Status", "Modifies the current collider status.");

        private void DrawActions()
        {
            var bp = Target;

            // Note: Rigidbody is guarenteed.

            var nstatus = LizittEditorGUI.ColliderBehaviorPopup(
                EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight), m_StatusLabel,
                bp.ColliderBehavior, ColliderBehaviorCategory.RigidBody);

            if (nstatus != bp.ColliderBehavior)
            {
                var rb = bp.Rigidbody;
                if (rb)
                    LizittEditorGUIUtil.SetRigidbodyBehavior(rb, (RigidbodyBehavior)nstatus);
            }
        }

        #endregion
    }
}
