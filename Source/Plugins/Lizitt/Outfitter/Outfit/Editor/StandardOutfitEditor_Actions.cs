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
        // Action Related Members

        private static readonly GUIContent ContextLabel = new GUIContent(
            "Context", "The context to use when creating/updating mount points and body parts.");

        private GameObject m_ContextChoice;  // Unlikely two outfits will need to share the same context, so not static.

        private bool ShowActions
        {
            get { return !m_IsAsset && OutfitterEditorUtil.ShowInspectorActions; }
        }

        #region Action Section: Accessories and MountPoints

        private void DrawAcessoryAndMountActionSection()
        {
            // No space needed because of preceding list.
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            DrawAccessoryActions();
            EditorGUILayout.Space();
            DrawMountPointActions();
        }

        #region Accessory Actions

        private static Accessory m_AddAccessoryChoice;

        private void DrawAccessoryActions()
        {
            EditorGUILayout.LabelField("Manage Accessories");

            DrawAddAccessory();
            DrawCurrentAccessories();
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
                btnStyle = EditorGUIUtil.YellowButton;
                removeType = RemoveActionType.RemoveOnly;
            }
            else if (OutfitterEditorUtil.IsDestructiveConfirmed)
            {
                btnStyle = EditorGUIUtil.RedButton;
                removeType = RemoveActionType.RemoveAndDestroy;
            }

            if (GUILayout.Button("X", btnStyle, GUILayout.MaxWidth(20)))
                isDirty = ReleaseAccessory(Target, accessory, OutfitterEditorUtil.AutoOffset, removeType);

            EditorGUILayout.EndHorizontal();

            return isDirty;
        }

        #endregion

        #region MountPoint Actions

        private readonly GUIContent ApplyMountPointContextLabel
            = new GUIContent("Apply Context", "Apply the context to all mount points.");

        private readonly GUIContent RefreshMountPointsLabel = 
            new GUIContent("Refresh MPs", "Perform a local child search for mount points.");

        private readonly GUIContent ResetMountPointsLabel = new GUIContent("Reset MPs", "Clear the mount point list.");

        private Transform m_TransformChoice = null;  // Not static.
        private LocalComponentPopup m_TransformPopup = null;
        private static MountPointType m_MountTypeChoice;

        private void DrawMountPointActions()
        {
            EditorGUILayout.LabelField("Mount Points");

            var outfit = Target;

            EditorGUIUtil.BeginLabelWidth(70);

            m_ContextChoice =
                EditorGUILayout.ObjectField(ContextLabel, m_ContextChoice, typeof(GameObject), true) as GameObject;

            EditorGUILayout.BeginHorizontal();

            m_MountTypeChoice = (MountPointType)EditorGUILayout.EnumPopup(m_MountTypeChoice);

            if (m_TransformPopup == null)
                m_TransformPopup = new LocalComponentPopup(typeof(Transform), false);

            m_TransformChoice = m_TransformPopup.OnGUI(
                EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight * 1.1f),
                m_TransformChoice, GUIContent.none, outfit.gameObject) as Transform;

            EditorGUILayout.EndHorizontal();

            GUI.enabled = m_TransformChoice;
            if (GUILayout.Button("Create Mount Point"))
            {
                if (m_TransformChoice.gameObject.GetComponent<MountPoint>())
                    Debug.LogError(m_TransformChoice.name + " Already has a mount point attached.", outfit);
                else if (outfit.GetMountPoint(m_MountTypeChoice))
                    Debug.LogError("Outfit already has a mount point of type: " + m_MountTypeChoice, outfit);
                else
                {
                    // Note for prefabs: If there is a missing body part in the array before the action and the 
                    // user undoes the action, the mount point array may end up containing an invalid reference 
                    // to the prefab's asset. This appears to be some kind of prefab related bug.

                    const string undoLabel = "Create Mount Point";
                    Undo.IncrementCurrentGroup();

                    Undo.RecordObjects(Outfit.UnsafeGetUndoObjects(outfit).ToArray(), undoLabel);  // For the refresh.

                    var bp = Undo.AddComponent<MountPoint>(m_TransformChoice.gameObject);
                    Undo.RecordObject(bp, undoLabel);
                    bp.LocationType = m_MountTypeChoice;
                    bp.Context = m_ContextChoice;

                    StandardOutfit.UnsafeRefreshMountPoints(outfit);

                    Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
                    m_TransformChoice = null;
                }
            }
            GUI.enabled = true;

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(ApplyMountPointContextLabel))
            {
                string undoLabel = "Apply Mount Point Context";
                Undo.IncrementCurrentGroup();
                Undo.RecordObjects(Outfit.UnsafeGetUndoObjects(outfit).ToArray(), undoLabel);

                outfit.ApplyMountPointContext(m_ContextChoice);

                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            }

            if (GUILayout.Button(RefreshMountPointsLabel))
            {
                Undo.RecordObject(Target, "Refresh Mount Points");
                StandardOutfit.UnsafeRefreshMountPoints(Target, false);
            }

            if (GUILayout.Button(ResetMountPointsLabel))
            {
                Undo.RecordObject(Target, "Reset Mount Points");
                StandardOutfit.UnsafeClearMountPoints(Target);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUIUtil.EndLabelWidth();
        }

        #endregion

        #endregion

        #region Action Section: BodyPart & Collider

        public void DrawBodyPartActionSection()
        {
            // No space needed because of preceding list.
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            DrawColliderActions();
            EditorGUILayout.Space();
            DrawBodyPartActions();
        }

        #region Colliders

        private static RigidbodyBehavior m_RigidbodyBehaviorChoice = RigidbodyBehavior.Disabled;
        private static int m_ColliderLayer = UnityLayer.Default;

        private static readonly GUIContent BodyPartStatusLabel = 
            new GUIContent("Set BPs", "Apply the rigidbody behavior to all body parts.");

        private static readonly GUIContent PrimaryStatusLabel = 
            new GUIContent("Set Pri", "Apply the rigidbody behavior to the primary collider.");

        public void DrawColliderActions()
        {
            const float BtnWidth = 60;

            EditorGUILayout.LabelField("Manage Colliders");

            var outfit = Target;

            EditorGUIUtil.BeginLabelWidth(50);

            EditorGUILayout.LabelField("Behavior");
            EditorGUILayout.BeginHorizontal();

            m_RigidbodyBehaviorChoice = (RigidbodyBehavior)EditorGUILayout.EnumPopup(m_RigidbodyBehaviorChoice);

            GUI.enabled = outfit.BodyPartCount > 0;
            if (GUILayout.Button(BodyPartStatusLabel, GUILayout.MaxWidth(BtnWidth)))
                ApplyBodyPartColliderStatus(outfit, m_RigidbodyBehaviorChoice);
            GUI.enabled = true;

            GUI.enabled = outfit.PrimaryRigidbody;
            if (GUILayout.Button(PrimaryStatusLabel, GUILayout.MaxWidth(BtnWidth)))
                EditorGUIUtil.SetRigidbodyBehavior(outfit.PrimaryRigidbody, m_RigidbodyBehaviorChoice);
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Layer");
            EditorGUILayout.BeginHorizontal();
            m_ColliderLayer = EditorGUILayout.LayerField(m_ColliderLayer);

            GUI.enabled = outfit.BodyPartCount > 0;
            if (GUILayout.Button(BodyPartStatusLabel, GUILayout.MaxWidth(BtnWidth)))
                ApplyBodyPartLayer(outfit, m_ColliderLayer);
            GUI.enabled = true;

            GUI.enabled = outfit.PrimaryCollider;
            if (GUILayout.Button(PrimaryStatusLabel, GUILayout.MaxWidth(BtnWidth)))
                SetPrimaryColliderLayer(outfit, m_ColliderLayer);
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            EditorGUIUtil.EndLabelWidth();
        }

        #endregion

        #region Body Parts

        private readonly GUIContent ApplyBodyPartContextLabel
            = new GUIContent("Apply Context", "Apply the context to all body parts.");

        private readonly GUIContent RefreshBodyPartsLabel 
            = new GUIContent("Refresh BPs", "Perform a local child search for body parts.");

        private readonly GUIContent ResetBodyPartsLabel = new GUIContent("Reset BPs", "Clear the mount point list.");

        private Collider m_ColliderChoice = null;
        private LocalComponentPopup m_ColliderPopup = null;
        private BodyPartType m_PartTypeChoice;

        public void DrawBodyPartActions()
        {
            EditorGUILayout.LabelField("Body Parts");

            EditorGUIUtil.BeginLabelWidth(70);

            m_ContextChoice = 
                EditorGUILayout.ObjectField(ContextLabel, m_ContextChoice, typeof(GameObject), true) as GameObject;

            var outfit = Target;

            EditorGUILayout.BeginHorizontal();

            m_PartTypeChoice = (BodyPartType)EditorGUILayout.EnumPopup(m_PartTypeChoice);

            if (m_ColliderPopup == null)
                m_ColliderPopup = new LocalComponentPopup(typeof(Collider), false);

            m_ColliderChoice = m_ColliderPopup.OnGUI(
                EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight * 1.1f),
                m_ColliderChoice, GUIContent.none, outfit.gameObject) as Collider;

            EditorGUILayout.EndHorizontal();

            GUI.enabled = m_ColliderChoice;
            if (GUILayout.Button("Create Boby Part"))
            {
                if (m_ColliderChoice.gameObject.GetComponent<BodyPart>())
                    Debug.LogError(m_ColliderChoice.name + " Already has a body part attached.", outfit);
                else if (outfit.GetBodyPart(m_PartTypeChoice))
                    Debug.LogError("Outfit already has a body part of type: " + m_PartTypeChoice, outfit);
                else
                {
                    // Note for prefabs: If there is a missing body part in the array before the action and the 
                    // user undoes the action, the mount point array may end up containing an invalid reference 
                    // to the prefab's asset. This appears to be some kind of prefab related bug.

                    const string undoLabel = "Create Body Part";
                    Undo.IncrementCurrentGroup();

                    Undo.RecordObjects(Outfit.UnsafeGetUndoObjects(outfit).ToArray(), undoLabel);  // For refresh.

                    var bp = Undo.AddComponent<BodyPart>(m_ColliderChoice.gameObject);
                    Undo.RecordObject(bp, undoLabel);
                    bp.Collider = m_ColliderChoice;
                    bp.PartType = m_PartTypeChoice;
                    bp.Context = m_ContextChoice;

                    StandardOutfit.UnsafeRefreshBodyParts(outfit);

                    Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
                    m_ColliderChoice = null;
                }
            }
            GUI.enabled = true;

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(ApplyBodyPartContextLabel))
            {
                string undoLabel = "Apply Body Part Context";
                Undo.IncrementCurrentGroup();
                Undo.RecordObjects(Outfit.UnsafeGetUndoObjects(outfit).ToArray(), undoLabel);

                outfit.ApplyBodyPartContext(m_ContextChoice);

                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            }

            if (GUILayout.Button(RefreshBodyPartsLabel))
            {
                Undo.RecordObject(outfit, "Refresh Body Parts");
                StandardOutfit.UnsafeRefreshBodyParts(outfit, false);
            }

            if (GUILayout.Button(ResetBodyPartsLabel))
            {
                Undo.RecordObject(outfit, "Reset Body Parts");
                StandardOutfit.UnsafeClearBodyParts(outfit);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUIUtil.EndLabelWidth();
        }

        #endregion

        #endregion

        #region Action Section: Renderer & Materials

        private void DrawRendererAndMaterialActionSection()
        {
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Set Outfit Materials");

            var matTypes = Target.GetOutfitMaterialTypes();

            for (int i = 0; i < matTypes.Length; i++)
                DrawMaterialAction(matTypes[i]);
        }

        private void DrawMaterialAction(OutfitMaterialType matType)
        {
            var outfit = Target;

            EditorGUIUtil.BeginLabelWidth(70);

            var cmat = outfit.GetSharedMaterial(matType);
            var nmat = EditorGUILayout.ObjectField(matType.ToString(), cmat, typeof(Material), false) as Material;

            EditorGUIUtil.EndLabelWidth();

            if (nmat || nmat != cmat)
                ApplyOutfitMaterial(outfit, matType, nmat);
        }

        #endregion

        #region Action Section : Observers

        private readonly GUIContent RefreshObserversLabel = 
            new GUIContent("Refresh Observers", "Perform a local search for mount points.");

        public void DrawObserverActionSection()
        {
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            if (GUILayout.Button(RefreshObserversLabel, GUILayout.MaxWidth(130)))
            {
                Undo.RecordObject(Target, "Refresh Observers");
                StandardOutfit.RefreshObservers(Target);
                EditorGUILayout.Space();
            }
        }

        #endregion

        #region Action Section: Miscellaneous

        private static bool m_ShowMiscActions = false;

        private void DrawMiscellaneousActionsSection()
        {
            DrawSettingsActions();
        }

        #region Settings Actions

        private static readonly GUIContent AutoOffsetLabel =
            new GUIContent("Auto-offset", "When an item is removed, move it out of the way by this distance.");

        private void DrawSettingsActions()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Inspector Settings", EditorStyles.boldLabel);

            EditorGUIUtil.BeginLabelWidth(80);
            OutfitterEditorUtil.AutoOffset =
                EditorGUILayout.Slider(AutoOffsetLabel, OutfitterEditorUtil.AutoOffset, 0, 5);
            EditorGUIUtil.EndLabelWidth();
        }

        #endregion

        #endregion
    }
}
