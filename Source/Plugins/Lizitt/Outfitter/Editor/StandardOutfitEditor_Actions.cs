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
    public partial class StandardOutfitEditor
    {
        private void DrawActions()
        {
            bool isDirty = DrawAccessoryActions();
            isDirty = DrawBodyPartActions() || isDirty;
            isDirty = DrawMaterialActions() || isDirty;
            DrawUtilityActions();
            DrawSettingsActions();

            // Technically this should not be needed.  But just to be safe...
            if (isDirty)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            }
        }

        #region Accessory Actions

        private static Accessory m_AddAccessoryChoice;

        private bool DrawAccessoryActions()
        {
            EditorGUILayout.LabelField("Manage Accessories", EditorGUIUtil.BoldLabel);

            bool isDirty = DrawAddAccessory();
            isDirty = DrawCurrentAccessories() || isDirty;

            return isDirty;
        }

        private bool DrawAddAccessory()
        {
            bool isDirty = false;

            EditorGUILayout.BeginHorizontal();

            var outfit = Target;

            var nchoice = EditorGUILayout.ObjectField(m_AddAccessoryChoice, typeof(Accessory), true) as Accessory;
            if (nchoice != m_AddAccessoryChoice)
            {
                if (nchoice && nchoice.IsManaged)
                {
                    // Undo will become too complex if this is allowed.
                    Debug.LogErrorFormat(outfit,
                        "Can't take control of an accessory that is already managed by another object."
                        + " Accessory: {0}, Owner: {1}", nchoice.name, nchoice.Owner.name);
                }
                else
                    m_AddAccessoryChoice = nchoice;
            }

            GUI.enabled = m_AddAccessoryChoice;
            if (GUILayout.Button("Add", GUILayout.MaxWidth(70)))
                isDirty = !MountAccessory(outfit, m_AddAccessoryChoice).IsFailed();
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            if (isDirty)
                m_AddAccessoryChoice = null;

            return isDirty;
        }

        private bool DrawCurrentAccessories()
        {
            EditorGUILayout.Space();

            var outfit = Target;

            int count = outfit.AccessoryCount;

            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    var acc = outfit.GetAccessory(i);
                    if (acc && DrawAccessory(acc))
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
                //btnStyle = EditorGUIUtil.YellowButton;
                removeType = RemoveActionType.RemoveOnly;
            }
            else if (OutfitterEditorUtil.IsDestructiveConfirmed)
            {
                //btnStyle = EditorGUIUtil.RedButton;
                removeType = RemoveActionType.RemoveAndDestroy;
            }

            if (GUILayout.Button("X", btnStyle, GUILayout.MaxWidth(20)))
                isDirty = ReleaseAccessory(Target, accessory, OutfitterEditorUtil.AutoOffset, removeType);

            EditorGUILayout.EndHorizontal();

            return isDirty;
        }

        #endregion

        #region Collider Actions

        private static ColliderStatus m_ColliderStatusChoice = ColliderStatus.Disabled;
        private static int m_ColliderLayer = UnityLayer.Default;

        private static readonly GUIContent BodyPartStatusLabel =
            new GUIContent("Set BPs", "Apply to all body parts.");

        private static readonly GUIContent PrimaryStatusLabel = 
            new GUIContent("Set Pri", "Apply to the primary collider.");

        public bool DrawBodyPartActions()
        {
            const float BtnWidth = 60;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Manage Colliders", EditorGUIUtil.BoldLabel);

            var outfit = Target;
            bool isDirty = false;

            var origWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 50;

            EditorGUILayout.LabelField("Status");
            EditorGUILayout.BeginHorizontal();

            m_ColliderStatusChoice = 
                (ColliderStatus)EditorGUILayout.EnumPopup(m_ColliderStatusChoice);

            GUI.enabled = outfit.BodyPartCount > 0;
            if (GUILayout.Button(BodyPartStatusLabel, GUILayout.MaxWidth(BtnWidth)))
                isDirty = ApplyBodyPartColliderStatus(outfit, m_ColliderStatusChoice) || isDirty;
            GUI.enabled = true;

            GUI.enabled = outfit.PrimaryCollider;
            if (GUILayout.Button(PrimaryStatusLabel, GUILayout.MaxWidth(BtnWidth)))
                isDirty = SetPrimaryColliderStatus(outfit, m_ColliderStatusChoice) || isDirty;
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Layer");
            EditorGUILayout.BeginHorizontal();
            m_ColliderLayer = EditorGUILayout.LayerField(m_ColliderLayer);

            GUI.enabled = outfit.BodyPartCount > 0;
            if (GUILayout.Button(BodyPartStatusLabel, GUILayout.MaxWidth(BtnWidth)))
                isDirty = ApplyBodyPartLayer(outfit, m_ColliderLayer) || isDirty;
            GUI.enabled = true;

            GUI.enabled = outfit.PrimaryCollider;
            if (GUILayout.Button(PrimaryStatusLabel, GUILayout.MaxWidth(BtnWidth)))
                isDirty = SetPrimaryColliderLayer(outfit, m_ColliderLayer) || isDirty;
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = origWidth;

            return isDirty;
        }

        #endregion

        #region Material Actions

        private bool DrawMaterialActions()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Manage Outfit Materials", EditorGUIUtil.BoldLabel);

            var matTypes = Target.GetOutfitMaterialTypes();

            bool isDirty = false;
            for (int i = 0; i < matTypes.Length; i++)
                isDirty = DrawMaterial(matTypes[i]) || isDirty;

            return isDirty;
        }

        private bool DrawMaterial(OutfitMaterialType matType)
        {
            var outfit = Target;

            var origWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 70;

            var cmat = outfit.GetSharedMaterial(matType);
            var nmat = EditorGUILayout.ObjectField(matType.ToString(), cmat, typeof(Material), false) as Material;

            EditorGUIUtility.labelWidth = origWidth;

            if (!nmat || nmat == cmat)
                return false;

            return ApplyOutfitMaterial(outfit, matType, nmat) > 0;
        }

        #endregion

        #region Settings Actions

        private static readonly GUIContent AutoOffsetLabel =
            new GUIContent("Auto-offset", "When an item is removed, move it out of the way by this distance.");

        private void DrawSettingsActions()
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

        #region Utility Actions

        private void DrawUtilityActions()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Utilities", EditorGUIUtil.BoldLabel);

            DrawRestrictedUtilityActions();
        }

        private readonly GUIContent RefreshAllLabel = new GUIContent("All", "Refresh all settings.");
        private readonly GUIContent RefreshMountPointsLabel = new GUIContent("MPs", "Refresh mount points.");
        private readonly GUIContent ResetMountPointsLabel = new GUIContent("MPs", "Reset mount points.");
        private readonly GUIContent RefreshBodyPartsLabel = new GUIContent("BPs", "Refresh body parts.");
        private readonly GUIContent ResetBodyPartsLabel = new GUIContent("BPs", "Reset body parts.");
        private readonly GUIContent RefreshObserversLabel = new GUIContent("Obs", "Refresh observers.");

        private void DrawRestrictedUtilityActions()
        {
            if (Application.isPlaying)
                return;

            EditorGUILayout.LabelField("Refresh Settings");

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(RefreshAllLabel, GUILayout.MaxWidth(50)))
            {
                Undo.RecordObject(Target, "Refresh Outfit Settings");
                StandardOutfit.UnsafeRefreshAllSettings(Target);
            }

            if (GUILayout.Button(RefreshMountPointsLabel, GUILayout.MaxWidth(50)))
            {
                Undo.RecordObject(Target, "Refresh Mount Points");
                StandardOutfit.UnsafeRefreshMountPoints(Target, false);
            }

            if (GUILayout.Button(RefreshBodyPartsLabel, GUILayout.MaxWidth(50)))
            {
                Undo.RecordObject(Target, "Refresh Body Parts");
                StandardOutfit.UnsafeRefreshBodyParts(Target, false);
            }

            if (GUILayout.Button(RefreshObserversLabel, GUILayout.MaxWidth(50)))
            {
                Undo.RecordObject(Target, "Refresh Observers");
                StandardOutfit.RefreshObservers(Target);
            }

            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.LabelField("Reset Settings");

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(ResetMountPointsLabel, GUILayout.MaxWidth(50)))
            {
                Undo.RecordObject(Target, "Clear Mount Points");
                StandardOutfit.UnsafeClearMountPoints(Target);
            }

            if (GUILayout.Button(ResetBodyPartsLabel, GUILayout.MaxWidth(50)))
            {
                Undo.RecordObject(Target, "Clear Body Parts");
                StandardOutfit.UnsafeClearBodyParts(Target);
            }

            EditorGUILayout.EndHorizontal();
        }

        #endregion
    }
}
