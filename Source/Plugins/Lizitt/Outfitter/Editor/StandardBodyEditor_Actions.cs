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
    public partial class StandardBodyEditor
        : BodyEditor
    {
        #region Actions

        private void DrawActions()
        {
            bool isDirty =  DrawOutfitActions();
            isDirty = DrawAccessoryActions() || isDirty;

            DrawActionSettings();

            // Technically this should not be needed.  But just to be safe...
            if (isDirty)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            }
        }

        #region Outfit Actions

        private bool DrawOutfitActions()
        {
            EditorGUILayout.LabelField("Manage Outfit", EditorGUIUtil.BoldLabel);

            bool isDirty = DrawOutfitApplyRemove();
            isDirty = DrawOutfitSwap() || isDirty;

            return isDirty;
        }

        private static Outfit m_OutfitApplyRemoveChoice = null;

        private bool DrawOutfitApplyRemove()
        {
            var body = Target;

            GUIContent label;
            bool isApplyMode;
            RemoveActionType actionId = RemoveActionType.Undefined;
            var btnStyle = GUI.skin.button;

            if (body.Outfit)
            {
                m_OutfitApplyRemoveChoice = body.Outfit;
                isApplyMode = false;
                label = new GUIContent("Remove", "Remove the outfit from the body.");

                if (OutfitterEditorUtil.IsNonDestructiveConfirmed)
                {
                    btnStyle = EditorGUIUtil.YellowButton;
                    actionId = RemoveActionType.RemoveOnly;
                }
                else if (OutfitterEditorUtil.IsDestructiveConfirmed)
                {
                    btnStyle = EditorGUIUtil.RedButton;
                    actionId = RemoveActionType.RemoveAndDestroy;
                }
            }
            else
            {
                isApplyMode = true;
                label = new GUIContent("Apply", "Apply the outfit to the body.");

                if (m_OutfitApplyRemoveChoice && m_OutfitApplyRemoveChoice.Owner)
                {
                    label.tooltip += " (Current outfit owner: " + m_OutfitApplyRemoveChoice.Owner.name + ")";
                    btnStyle = EditorGUIUtil.YellowButton;
                }
            }

            EditorGUILayout.BeginHorizontal();

            GUI.enabled = isApplyMode;

            var noutfit = EditorGUILayout.ObjectField(
                m_OutfitApplyRemoveChoice, typeof(Outfit), true, GUILayout.ExpandWidth(true)) as Outfit;

            if (noutfit != m_OutfitApplyRemoveChoice)
            {
                // Can only happen in apply mode.
                if (noutfit && noutfit.IsManaged)
                {
                    // Undo will become too complex if this is allowed.
                    Debug.LogErrorFormat(body,
                        "Can't take control of an outfit that is already managed by another object."
                        + " Outfit: {0}, Owner: {1}", noutfit.name, noutfit.Owner.name);
                }
                else
                    m_OutfitApplyRemoveChoice = noutfit;
            }

            bool isDirty = false;

            GUI.enabled = m_OutfitApplyRemoveChoice;
            if (GUILayout.Button(label, btnStyle, GUILayout.MaxWidth(70)))
            {
                if (isApplyMode)
                    isDirty = SetOutfit(body, m_OutfitApplyRemoveChoice, true);
                else
                    isDirty = RemoveOutfit(body, OutfitterEditorUtil.AutoOffset, actionId);
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            return isDirty;
        }

        private Outfit m_OutfitSwapChoice = null;

        private bool DrawOutfitSwap()
        {
            var body = Target;

            if (!body.Outfit)
            {
                // Nothing to do.
                m_OutfitSwapChoice = null;
                return false;
            }

            EditorGUILayout.BeginHorizontal();

            var noutfit = EditorGUILayout.ObjectField(
                m_OutfitSwapChoice, typeof(Outfit), true, GUILayout.ExpandWidth(true)) as Outfit;

            if (noutfit != m_OutfitSwapChoice)
            {
                if (noutfit == body.Outfit)
                    Debug.LogWarning("Outfit is already assigned to the body: " + noutfit.name, body);
                else if (noutfit && noutfit.IsManaged)
                {
                    // Undo will become too complex if this is allowed.
                    Debug.LogErrorFormat(body,
                        "Can't take control of an outfit that is already managed by another object."
                        + " Outfit: {0}, Owner: {1}", noutfit.name, noutfit.Owner.name);
                }
                else
                    m_OutfitSwapChoice = noutfit;
            }

            GUIContent label = new GUIContent("Swap", "Swap this outfit with the body's current outfit.");
            RemoveActionType removeType = RemoveActionType.Undefined;
            var btnStyle = GUI.skin.button;

            if (OutfitterEditorUtil.IsNonDestructiveConfirmed)
            {
                btnStyle = EditorGUIUtil.YellowButton;
                removeType = RemoveActionType.RemoveOnly;
            }
            else if (OutfitterEditorUtil.IsDestructiveConfirmed)
            {
                btnStyle = EditorGUIUtil.RedButton;
                removeType = RemoveActionType.RemoveAndDestroy;
            }

            bool isDirty = false;

            GUI.enabled = m_OutfitSwapChoice;
            if (GUILayout.Button(label, btnStyle, GUILayout.MaxWidth(70)))
            {
                Undo.IncrementCurrentGroup();

                if (RemoveOutfit(body, OutfitterEditorUtil.AutoOffset, removeType, false))
                {
                    isDirty = true;
                    SetOutfit(body, m_OutfitSwapChoice, false);

                    var item = m_OutfitSwapChoice;
                    m_OutfitSwapChoice = m_OutfitApplyRemoveChoice ? m_OutfitApplyRemoveChoice : null;
                    m_OutfitApplyRemoveChoice = item;
                }
                // else there should have been an error message for the body.

                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            return isDirty;
        }

        #endregion

        #region Accessory Actions

        public static Accessory m_AddAccessoryChoice;

        private bool DrawAccessoryActions()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Manage Accessories", EditorGUIUtil.BoldLabel);

            bool isDirty = DrawAddAccessory();
            isDirty = DrawCurrentAccessories() || isDirty;

            return isDirty;
        }

        private bool DrawAddAccessory()
        {
            bool isDirty = false;

            EditorGUILayout.BeginHorizontal();

            var body = Target;

            var nchoice = EditorGUILayout.ObjectField(m_AddAccessoryChoice, typeof(Accessory), true) as Accessory;
            if (nchoice != m_AddAccessoryChoice)
            {
                if (nchoice && nchoice.IsManaged)
                {
                    // Undo will become too complex if this is allowed.
                    Debug.LogErrorFormat(body,
                        "Can't take control of an accessory that is already managed by another object."
                        + " Accessory: {0}, Owner: {1}", nchoice.name, nchoice.Owner.name);
                }
                else
                    m_AddAccessoryChoice = nchoice;
            }
             
            GUI.enabled = m_AddAccessoryChoice;
            if (GUILayout.Button("Add", GUILayout.MaxWidth(70)))
                isDirty = AddAccessory(body, m_AddAccessoryChoice);
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            if (isDirty)
                m_AddAccessoryChoice = null;

            return isDirty;
        }

        private bool DrawCurrentAccessories()
        {
            EditorGUILayout.Space();

            var body = Target;

            int count = body.Accessories.Count;

            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    var info = body.Accessories[i];
                    if (info.Accessory && DrawAccessory(info.Accessory))
                        return true;
                }
            }

            return false;
        }

        private bool DrawAccessory(Accessory accessory)
        {
            bool isDirty = false;

            EditorGUILayout.BeginHorizontal();

            GUI.enabled = false;
            EditorGUILayout.ObjectField(accessory, typeof(Accessory), false);
            GUI.enabled = true;

            GUIStyle btnStyle = GUI.skin.button;
            var removeType = RemoveActionType.Undefined;

            if (OutfitterEditorUtil.IsNonDestructiveConfirmed)
            {
                btnStyle = EditorGUIUtil.YellowButton;
                removeType = RemoveActionType.RemoveOnly;
            }
            else if (OutfitterEditorUtil.IsDestructiveConfirmed)
            {
                btnStyle = EditorGUIUtil.RedButton;
                removeType = RemoveActionType.RemoveAndDestroy;
            }

            var body = Target;

            if (GUILayout.Button("X", btnStyle, GUILayout.MaxWidth(20)))
                isDirty = RemoveAccessory(body, accessory, OutfitterEditorUtil.AutoOffset, removeType);

            EditorGUILayout.EndHorizontal();

            return isDirty;
        }

        #endregion

        #region Settings Actions

        private static readonly GUIContent AutoOffsetLabel =
            new GUIContent("Auto-offset", "When an item is removed, move it out of the way by this distance.");

        private void DrawActionSettings()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Settings", EditorGUIUtil.BoldLabel);

            var origWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 80;
            OutfitterEditorUtil.AutoOffset =
                EditorGUILayout.Slider(AutoOffsetLabel, OutfitterEditorUtil.AutoOffset, 0, 5);
            EditorGUIUtility.labelWidth = origWidth;
        }

        #endregion

        #endregion

        #region Outfit Members

        /// <summary>
        /// Properly sets the body's outfit while in editor mode.  (Includes undo.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// <paramref name="singleUndo"/> will create a single undo action.  If calling this method is part of
        /// a larger operation that needs a single undo, then set <paramref name="singleUndo"/> to false. Behavior 
        /// is undefined if <paramref name="singleUndo"/> is false and the caller does not properly group this method's
        /// actions.
        /// </para>
        /// </remarks>
        /// <param name="body">The body. (Required. Must be a scene object.)</param>
        /// <param name="outfit">The outfit. (Required)</param>
        /// <param name="singleUndo">
        /// If true, will group all actions into a single undo.  Otherwise the caller must properly group the undos.
        /// </param>
        /// <param name="undoLabel">The label to use for the undo, or null to use the default label.</param>
        /// <returns>True if the outfit was successfully set.</returns>
        public static bool SetOutfit(
            StandardBody body, Outfit outfit, bool singleUndo = true, string undoLabel = null)
        {
            // Design note: Can't generalize this to the Body because Body doesn't implement the needed observer.

            if (AssetDatabase.Contains(body))
            {
                Debug.LogError("Can't modify a body asset.  Body must be in the scene.", body);
                return false;
            }

            if (body.Outfit)
            {
                Debug.LogError("Can't set outfit.  Must remove the body's current outfit first.", body);
                return false;
            }

            if (!outfit)
            {
                return RemoveOutfit(
                    body, OutfitterEditorUtil.AutoOffset, RemoveActionType.Undefined, singleUndo, undoLabel);
            }

            if (outfit.IsManaged)
            {
                Debug.LogError("Can't set outfit.  The outfit is already managed by: " + outfit.Owner.name, body);
                return false;
            }

            bool success = false;
            undoLabel = string.IsNullOrEmpty(undoLabel) ? "Apply Outfit" : undoLabel;

            if (singleUndo)
                Undo.IncrementCurrentGroup();

            Undo.RecordObjects(Body.UnsafeGetUndoObjects(body).ToArray(), undoLabel);

            bool isNew = AssetDatabase.Contains(outfit);
            if (isNew)
            {
                var name = outfit.name;
                outfit = outfit.Instantiate();

                outfit.name = name;

                // Register with undo later.
            }
            else
                Undo.RecordObjects(Outfit.UnsafeGetUndoObjects(outfit).ToArray(), undoLabel);

            var origParent = outfit.transform.parent;

            if (body.SetOutfit(outfit) == outfit)
            {
                if (isNew)
                    outfit.gameObject.SafeDestroy();

                outfit = null;
            }
            else
            {
                if (isNew)
                    Undo.RegisterCreatedObjectUndo(outfit.gameObject, undoLabel);
                var parent = outfit.transform.parent;
                outfit.transform.parent = origParent;
                Undo.SetTransformParent(outfit.transform, parent, undoLabel);

                // Hack: Addition of body as outfit observer is not being recorded for serialization.  
                // This fixes it until the cause and proper fix can be determined.
                StandardOutfitEditor.AddObserverWithUndo(outfit, body);

                success = true;
            }

            if (singleUndo)
                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

            return success;
        }

        #endregion
    }
}
