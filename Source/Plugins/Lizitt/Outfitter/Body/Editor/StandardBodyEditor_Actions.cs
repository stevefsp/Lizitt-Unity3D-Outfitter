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
            DrawOutfitActions();
            DrawAccessoryActions();
            DrawActionSettings();
        }

        #region Outfit Actions

        private void DrawOutfitActions()
        {
            EditorGUILayout.LabelField("Manage Outfit", EditorStyles.boldLabel);

            DrawOutfitApplyRemove();
            DrawOutfitSwap();
        }

        private static Outfit m_OutfitApplyRemoveChoice = null;

        private void DrawOutfitApplyRemove()
        {
            var body = Target;

            GUIContent label;
            bool isApplyMode;
            RemoveActionType removeType = RemoveActionType.Undefined;
            var btnStyle = GUI.skin.button;

            if (body.Outfit)
            {
                m_OutfitApplyRemoveChoice = body.Outfit;
                isApplyMode = false;
                label = new GUIContent("Remove", "Remove the outfit from the body.");

                if (OutfitterEditorUtil.IsNonDestructiveConfirmed)
                {
                    btnStyle = LizittEditorGUIUtil.YellowButton;
                    removeType = RemoveActionType.RemoveOnly;
                }
                else if (OutfitterEditorUtil.IsDestructiveConfirmed)
                {
                    btnStyle = LizittEditorGUIUtil.RedButton;
                    removeType = RemoveActionType.RemoveAndDestroy;
                }
            }
            else
            {
                isApplyMode = true;
                label = new GUIContent("Apply", "Apply the outfit to the body.");

                if (m_OutfitApplyRemoveChoice && m_OutfitApplyRemoveChoice.Owner)
                {
                    label.tooltip += " (Current outfit owner: " + m_OutfitApplyRemoveChoice.Owner.name + ")";
                    btnStyle = LizittEditorGUIUtil.YellowButton;
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

            GUI.enabled = m_OutfitApplyRemoveChoice;
            if (GUILayout.Button(label, btnStyle, GUILayout.MaxWidth(70)))
            {
                if (isApplyMode)
                    SetOutfit(body, m_OutfitApplyRemoveChoice, OutfitterEditorUtil.AutoOffset, removeType);
                else
                    SetOutfit(body, null, OutfitterEditorUtil.AutoOffset, removeType);
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
        }

        GUIContent m_SwapLabel = new GUIContent("Swap", "Swap this outfit with the body's current outfit.");

        private static Outfit m_OutfitSwapChoice = null;

        private void DrawOutfitSwap()
        {
            var body = Target;
            if (!body.Outfit)
            {
                // Nothing to do.
                m_OutfitSwapChoice = null;
                return;
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

            RemoveActionType removeType = RemoveActionType.Undefined;
            var btnStyle = GUI.skin.button;

            if (OutfitterEditorUtil.IsNonDestructiveConfirmed)
            {
                btnStyle = LizittEditorGUIUtil.YellowButton;
                removeType = RemoveActionType.RemoveOnly;
            }
            else if (OutfitterEditorUtil.IsDestructiveConfirmed)
            {
                btnStyle = LizittEditorGUIUtil.RedButton;
                removeType = RemoveActionType.RemoveAndDestroy;
            }

            GUI.enabled = m_OutfitSwapChoice;
            if (GUILayout.Button(m_SwapLabel, btnStyle, GUILayout.MaxWidth(70)))
            {
                if (SetOutfit(body, m_OutfitSwapChoice, OutfitterEditorUtil.AutoOffset, removeType, false))
                {
                    var item = m_OutfitSwapChoice;
                    m_OutfitSwapChoice = m_OutfitApplyRemoveChoice ? m_OutfitApplyRemoveChoice : null;
                    m_OutfitApplyRemoveChoice = item;
                }
                // else there should have been an error message for the body.
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region Accessory Actions

        public static Accessory m_AddAccessoryChoice;

        private bool DrawAccessoryActions()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Manage Accessories", EditorStyles.boldLabel);

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
                btnStyle = LizittEditorGUIUtil.YellowButton;
                removeType = RemoveActionType.RemoveOnly;
            }
            else if (OutfitterEditorUtil.IsDestructiveConfirmed)
            {
                btnStyle = LizittEditorGUIUtil.RedButton;
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
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

            LizittEditorGUIUtil.BeginLabelWidth(80);
            OutfitterEditorUtil.AutoOffset =
                EditorGUILayout.Slider(AutoOffsetLabel, OutfitterEditorUtil.AutoOffset, 0, 5);
            LizittEditorGUIUtil.EndLabelWidth();
        }

        #endregion

        #endregion
    }
}
