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
    /// <see cref="SyncMountPointState"/> custom editor.
    /// </summary>
    [CustomEditor(typeof(SyncMountPointState))]
    public partial class SyncMountPointStateEditor
        : Editor
    {
        private SyncMountPointState Target
        {
            get { return target as SyncMountPointState; }
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

            EditorGUILayout.HelpBox("Body Observer\n\nSynchronize the state of the previous outfit's mount points to"
                + " the current outfit.\n\nLimitation: This observer is designed for memory efficiency.  It only"
                + " synchronizes between two known outfits, so the last known state is lost if the body's outfit is"
                + " set to null.\n\nCan observe multiple body instances.\n\nActive at design-time.",
                MessageType.None); 
        }

        private static Outfit m_To;
        private static Outfit m_From;

        private void DrawActions()
        {
            EditorGUILayout.LabelField("Synchronize");

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();
            LizEditorGUIUtil.BeginLabelWidth(50);
            m_From = EditorGUILayout.ObjectField("From", m_From, typeof(Outfit), true) as Outfit;
            m_To = EditorGUILayout.ObjectField("To", m_To, typeof(Outfit), true) as Outfit;
            LizEditorGUIUtil.EndLabelWidth();
            EditorGUILayout.EndVertical();

            GUI.enabled = (m_From && m_To);
            var settings = Target;
            if (GUILayout.Button("Apply", GUILayout.MaxWidth(70)))
            {
                SynchronizeState(
                    m_To, m_From, settings.IncludeBlockedStatus, settings.IncludeContext, settings.IncludeContext);
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Synchronize the mount part state of all common body parts.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The caller must properly collapse the undo records if <paramref name="singleUndo"/> is false.  Otherwise
        /// performing an undo will result in an invalid outfit state.
        /// </para>
        /// </remarks>
        /// <param name="to">The outfit being synchonized to. (Required)</param>
        /// <param name="from">The outfit state is syncronzied from. (Required)</param>
        /// <param name="includeBlocked">Synchronize the mount point 'is blocked' state.</param>
        /// <param name="includeContext">Persist the context unless it is the previous outfit's GameObject.</param>
        /// <param name="singleUndo">
        /// If true, collapse all undo records into a single undo, otherwise the caller will perform the collapse.
        /// </param>
        /// <param name="undoLabel">The label to use for all undo records. (Required)</param>
        /// <returns>True if the outfit state was altered.</returns>
        public static bool SynchronizeState(Outfit to, Outfit from, bool includeBlocked = true, 
            bool includeContext = true, bool singleUndo = true, string undoLabel = "Sync MountPoint State")
        {
            if (!(to && from && to.MountPointCount > 0 && from.MountPointCount > 0))
                return false;
            
            if (singleUndo)
                Undo.IncrementCurrentGroup();

            // Need more than just the body part components...
            Undo.RecordObjects(Outfit.UnsafeGetUndoObjects(to).ToArray(), undoLabel);

            bool changed = false;
            for (int i = 0; i < from.MountPointCount; i++)
            {
                var prevPart = from.GetMountPoint(i);
                if (prevPart)
                {
                    var part = to.GetMountPoint(prevPart.LocationType);
                    if (part)
                    {
                        changed = true;  // This is close enough.
                        MountPoint.Synchronize(part, prevPart, includeBlocked, includeContext, from.gameObject);
                    }
                }
            }

            if (singleUndo)
                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

            return changed;
        }
    }
}
