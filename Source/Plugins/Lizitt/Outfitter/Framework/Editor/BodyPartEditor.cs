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
    /// <see cref="BodyPart"/> custom editor.
    /// </summary>
    [CustomEditor(typeof(BodyPart))]
    public partial class BodyPartEditor
        : Editor
    {
        #region Main Editor

        private BodyPart Target
        {
            get { return target as BodyPart; }
        }

        /// <summary>
        /// See Unity documentation.
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            OutfitterEditorUtil.ShowInspectorActions = 
                EditorGUILayout.Foldout(OutfitterEditorUtil.ShowInspectorActions, "Actions");

            if (OutfitterEditorUtil.ShowInspectorActions)
                DrawActions();
            
            EditorGUILayout.Space();
        }

        public void DrawActions()
        {
            var bp = Target;

            var nstatus = (ColliderStatus)EditorGUILayout.EnumPopup("Collider Status", bp.ColliderStatus);

            if (nstatus != bp.ColliderStatus)
                SetColliderStatus(bp, nstatus);
        }

        public static bool SetColliderStatus(
            BodyPart bodyPart, ColliderStatus status, bool singleUndo = true, string undoLabel = null)
        {
            if (bodyPart.ColliderStatus == status)
                return false;

            if (!bodyPart.Collider)
            {
                Debug.Log("Can't set status on a null collider.", bodyPart);
                return false;
            }

            undoLabel = string.IsNullOrEmpty(undoLabel) ? "Set Body Part Status" : undoLabel;

            if (singleUndo)
                Undo.IncrementCurrentGroup();

            Undo.RecordObject(bodyPart, undoLabel);
            Undo.RecordObject(bodyPart.Collider, undoLabel);
            if (bodyPart.Rigidbody)
                Undo.RecordObject(bodyPart.Rigidbody, undoLabel);

            bodyPart.ColliderStatus = status;

            if (singleUndo)
                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

            return true;
        }

        #endregion
    }
}
