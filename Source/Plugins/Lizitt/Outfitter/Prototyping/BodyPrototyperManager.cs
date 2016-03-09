#if UNITY_EDITOR

using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace com.lizitt.outfitter.proto
{
    /// <summary>
    /// A component useful for prototyping body and its features.  (Editor only.)
    /// </summary>
    [AddComponentMenu(
        OutfitterUtil.Menu + "Body Prototyper Manager (Editor Only)", OutfitterUtil.EditorComponentMenuOrder + 0)]
    [RequireComponent(typeof(BodyPrototyperSettings))]
    public class BodyPrototyperManager
        : MonoBehaviour
    {
        // TODO: v0.3: Add proper documentation.

        #region Settings

        private BodyPrototyperSettings m_Manager;
        private int m_OutfitIndex;

        [Header("Mouse Settings")]

        [SerializeField]
        [Range(0.1f, 2)]
        private float m_MouseSensitivity = 0.6f;

        [SerializeField]
        [Range(-2, 2)]
        private float m_WheelSensitivity = 0.6f;

        [SerializeField]
        private bool m_InvertAxisX = false;

        [SerializeField]    
        private bool m_InvertAxisY = false;

        [SerializeField]
        private bool m_InvertButtons = false;
        public bool InvertButtons
        {
            get { return m_InvertButtons; }
            set { m_InvertButtons = value; }
        }

        [Header("Camera Settings")]

        [SerializeField]
        [Range(0, 3)]
        private float m_CameraHeight = 1;

        [SerializeField]
        [Range(0.5f, 5)]
        private float m_CameraDistance = 2.25f;

        [Header("Display Settings")]

        [SerializeField]
        private bool m_RotationDisc = true;

        [SerializeField]
        [Range(100, 500)]
        private float m_RightWidth = 150;

        [SerializeField]
        [Range(100, 500)]
        private float m_LeftWidth = 150;

        [Header("Miscellaneous Settings")]

        [SerializeField]
        [Tooltip(
            "If the current outfit has an animator, wait for transitions to complete before setting a new outfit.")]
        private bool m_MonitorAnimator = true;

        [SerializeField]
        [Tooltip("The maximum length of time to wait for the animator before setting a new outfit.")]
        [Range(0, 2)]
        private float m_AnimatorTimeout = 1;

        #endregion

        #region Utility Members

        private void SetOutfit(int index)
        {
            var outfit = m_Manager.Outfits[m_OutfitIndex];

            var orig = m_Manager.Body.SetOutfit(outfit);
            if (orig)
                orig.transform.position = m_Manager.GetStoragePosition(orig);
        }

        #endregion

        #region Styles

        #region Accessory

        private static GUIStyle m_StoredAccessoryStyle;
        private static GUIStyle StoredAccessoryStyle
        {
            get
            {
                if (m_StoredAccessoryStyle == null)
                {
                    m_StoredAccessoryStyle = new GUIStyle(GUI.skin.button);
                    m_StoredAccessoryStyle.normal.textColor = Color.cyan;
                    m_StoredAccessoryStyle.focused.textColor = Color.cyan;
                    m_StoredAccessoryStyle.hover.textColor = Color.cyan;
                }

                return m_StoredAccessoryStyle;
            }
        }

        private static GUIStyle m_MountingAccessoryStyle;
        private static GUIStyle MountingAccessoryStyle
        {
            get
            {
                if (m_MountingAccessoryStyle == null)
                {
                    m_MountingAccessoryStyle = new GUIStyle(GUI.skin.button);
                    m_MountingAccessoryStyle.normal.textColor = Color.yellow;
                    m_MountingAccessoryStyle.focused.textColor = Color.yellow;
                    m_MountingAccessoryStyle.hover.textColor = Color.yellow;
                }

                return m_MountingAccessoryStyle;
            }
        }

        private static GUIStyle m_MountedAccessoryStyle;
        private static GUIStyle MountedAccessoryStyle
        {
            get
            {
                if (m_MountedAccessoryStyle == null)
                {
                    m_MountedAccessoryStyle = new GUIStyle(GUI.skin.button);
                    m_MountedAccessoryStyle.normal.textColor = Color.green;
                    m_MountedAccessoryStyle.focused.textColor = Color.green;
                    m_MountedAccessoryStyle.hover.textColor = Color.green;
                }

                return m_MountedAccessoryStyle;
            }
        }

        private GUIStyle GetStyle(Accessory accessory)
        {
            switch (accessory.Status)
            {
                case AccessoryStatus.Mounted:

                    return MountedAccessoryStyle;

                case AccessoryStatus.Stored:

                    return StoredAccessoryStyle;

                case AccessoryStatus.Mounting:

                    return MountingAccessoryStyle;
            }

            return GUI.skin.button;
        }

        #endregion

        private static GUIStyle m_WarningLabel;

        private static GUIStyle WarningLabel
        {
            get
            {
                if (m_WarningLabel == null)
                {
                    m_WarningLabel = new GUIStyle(GUI.skin.box);
                    // m_WarningLabel.fontStyle = FontStyle.Bold;  Overkill.
                    m_WarningLabel.normal.textColor = Color.yellow;
                }

                return m_WarningLabel;
            }
        }

        #endregion

        #region Initialization

        void Awake()
        {
            m_Manager = GetComponent<BodyPrototyperSettings>();
        }

        void Start()
        {
            if (!Camera.main)
            {
                Debug.LogError("Scene does not have a main camera.", this);
                enabled = false;
                return;
            }

            InitializeSettings();
            InitializeCamera();
        }

        private void InitializeSettings()
        {
            m_Manager.InstantiateAssets();

            m_Manager.InitializeBody();

            if (m_Manager.Outfits.Count == 0 || m_Manager.Outfits[0])
                m_Manager.Outfits.Insert(0, null);

            foreach (var item in m_Manager.Outfits)
            {
                if (item && !item.IsManaged)
                    item.transform.position = m_Manager.GetStoragePosition(item);
            }

            m_OutfitIndex = m_Manager.Outfits.Count == 1 ? 0 : 1;
            SetOutfit(m_OutfitIndex);

            foreach (var item in m_Manager.Accessories)
            {
                if (item && !item.IsManaged)
                    item.transform.position = m_Manager.GetStoragePosition(item);
            }
        }

        #endregion

        #region Camera

        private float m_BodyRotationY;

        private Transform m_CameraPivot;
        private Camera m_Camera;
        private Vector3 m_CameraPivotRotation; 

        private void InitializeCamera()
        {
            var body = m_Manager.Body;
            m_BodyRotationY = body.transform.eulerAngles.y;

            m_Camera = Camera.main;
            m_CameraPivot = new GameObject(m_Camera.name + "_Pivot").transform;
            m_Camera.transform.parent = m_CameraPivot;

            m_CameraPivotRotation = new Vector3(0, body.transform.eulerAngles.y + 180, 0);
        }

        private void ProcessEvents()
        {
            // Ummm.  Why did I do this with Event rather Input?  Not worth changing now.

            int bodyBtn = 0;
            int camBtn = 1;
            if (m_InvertButtons)
            {
                bodyBtn = 1;
                camBtn = 0;
            }

            var e = Event.current;
            if (e.isMouse)
            {
                if (e.type == EventType.MouseDrag)
                {
                    if (e.button == bodyBtn)
                        m_BodyRotationY += e.delta.x * -m_MouseSensitivity;  // Not effected by inverted axis.
                    else if (e.button == camBtn)
                    {
                        m_CameraPivotRotation.y += e.delta.x * m_MouseSensitivity * (m_InvertAxisX ? -1 : 1);
                        m_CameraPivotRotation.x += e.delta.y * m_MouseSensitivity * (m_InvertAxisY ? 1 : -1);
                    }
                    else if (e.button == 2)
                        m_CameraHeight += e.delta.y * m_MouseSensitivity / 180;
                }
            }

            if (e.type == EventType.ScrollWheel)
                m_CameraDistance += e.delta.y * m_WheelSensitivity * 0.1f;
        }

        #endregion

        #region Update

        void LateUpdate()
        {
            var body = m_Manager.Body;
            body.transform.eulerAngles = new Vector3(0, m_BodyRotationY, 0);

            m_CameraDistance = Mathf.Max(0.5f, m_CameraDistance);
            m_CameraPivotRotation.x = Mathf.Clamp(m_CameraPivotRotation.x, -85, 85);

            m_CameraPivot.position = body.transform.position + Vector3.up * m_CameraHeight;
            m_CameraPivot.transform.eulerAngles = new Vector3(-m_CameraPivotRotation.x, m_CameraPivotRotation.y, 0);

            m_Camera.transform.localPosition = new Vector3(0, 0, -m_CameraDistance);
            m_Camera.transform.LookAt(m_CameraPivot.position);

            m_OutfitBlockTime -= Time.deltaTime;
        }

        #endregion

        #region GUI Members

        void OnRenderObject()
        {
            if (m_RotationDisc)
            {
                var color = ColorUtil.Chocolate;
                var trans = m_Manager.Body.transform;
                var dist = 0.5f;
                var pos = trans.position + Vector3.up * 0.005f;
                DebugDraw.Circle(pos, dist, color);
                DebugDraw.Arrow(pos, pos + trans.forward * dist, 0, 0.1f, color);
            }
        }

        private float m_OutfitBlockTime = 0;

        void OnGUI()
        {
            ProcessEvents();
            DrawTopArea();
            DrawLeftArea();
            DrawRightArea();
        }

        private void DrawTopArea()
        {
            var body = m_Manager.Body;

            GUILayout.BeginArea(new Rect(m_LeftWidth + 5, 5, Screen.width - m_LeftWidth - m_RightWidth - 10, 90));

            var outfitName = (body.Outfit ? m_Manager.Body.Outfit.name : "No Outfit");
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(outfitName, GUI.skin.box, GUILayout.MinWidth(100)) && body.Outfit)
            {
                Selection.activeObject = body.Outfit;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (m_OutfitBlockTime > 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Waiting for Animator", WarningLabel);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            GUILayout.EndArea();
        }

        private void DrawLeftArea()
        {
            var body = m_Manager.Body;

            GUILayout.BeginArea(new Rect(5, 5, m_LeftWidth, Screen.height));

            if (m_Manager.Outfits.Count > 0)
            {
                GUILayout.BeginVertical(GUI.skin.box);

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Body Outfits", GUI.skin.label))
                {
                    Selection.activeObject = body;
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                var labels = m_Manager.GetOutfitNames();

                var idx = GUILayout.SelectionGrid(m_OutfitIndex, labels, 1);
                if (idx != m_OutfitIndex)
                {
                    m_OutfitIndex = idx;

                    if (m_OutfitBlockTime <= 0)   // Less than or equal is necessary.
                    {
                        if (IsAnimatorInTransition())
                            StartCoroutine(DoSetOutfitDelayed());
                        else
                            SetOutfit(m_OutfitIndex);
                    }
                    // else the coroutine will catch the change.
                }

                GUILayout.EndVertical();
            }

            GUILayout.Space(10);

            if (m_Manager.OutfitMaterialGroups.Count > 0)
            {
                GUILayout.BeginVertical(GUI.skin.box);

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Outfit Materials");
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                var labels = m_Manager.GetMaterialGroupNames();

                GUI.enabled = body.Outfit;
                for (int i = 0; i < m_Manager.OutfitMaterialGroups.Count; i++)
                {
                    var item = m_Manager.OutfitMaterialGroups[i];
                    if (item)
                    {
                        if (GUILayout.Button(labels[i]))
                            item.ApplyMaterials(body.Outfit);

                    }
                }
                GUI.enabled = true;

                GUILayout.EndVertical();
            }

            GUILayout.EndArea();
        }

        private bool IsAnimatorInTransition()
        {
            bool isInTransition = false;

            if (m_MonitorAnimator && m_Manager.Body.Outfit)
            {
                var animator = m_Manager.Body.Outfit.GetAnimator();
                if (animator && animator.runtimeAnimatorController)
                {
                    for (int i = 0; i < animator.layerCount; i++)
                    {
                        if (animator.IsInTransition(i))
                        {
                            isInTransition = true;
                            break;
                        }
                    }
                }
            }
            // else don't care.

            return isInTransition;
        }

        private System.Collections.IEnumerator DoSetOutfitDelayed()
        {
            m_OutfitBlockTime = m_AnimatorTimeout;

            bool isInTransition;
            do
            {
                yield return null;
                // m_OutfitBlockTime -= Time.deltaTime;  // Handled in late update.
                isInTransition = IsAnimatorInTransition();
            }
            while (isInTransition && m_OutfitBlockTime > 0 && m_Manager.Body.Outfit);

            if (isInTransition)
            {
                Debug.LogWarning(
                    "Timed out or lost outfit while waiting for animator to complete a transition. Setting outfit.",
                    this);
            }

            SetOutfit(m_OutfitIndex);
            m_OutfitBlockTime = 0;
        }

        private void DrawRightArea()
        {
            var body = m_Manager.Body;

            GUILayout.BeginArea(new Rect(Screen.width - 5 - m_RightWidth, 5, m_RightWidth, Screen.height));

            if (m_Manager.Accessories.Count > 0)
            {
                for (int i = 0; i < m_Manager.Accessories.Count; i++)
                {
                    var item = m_Manager.Accessories[i];
                    if (item)
                    {
                        GUILayout.BeginVertical(GUI.skin.box);

                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(item.name, GUI.skin.label))
                        {
                            Selection.activeObject = item;
                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(item.Status.ToString(), GUI.skin.label))  // More space for the click.
                        {
                            Selection.activeObject = item;
                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();

                        if (GUILayout.Button(
                            item.Status == AccessoryStatus.Unmanaged ? "Add" : "Remove", GetStyle(item)))
                        {
                            if (item.Status == AccessoryStatus.Unmanaged)
                            {
                                var result = body.Accessories.Add(item);
                                if (result.IsFailed())
                                {
                                    Debug.LogErrorFormat(
                                        item, "Could not add accessory to body: {0}, Status: {1}", item.name, result);
                                }
                            }
                            else
                            {
                                body.Accessories.Remove(item);
                                item.transform.position = m_Manager.GetStoragePosition(item);
                            }
                        }

                        GUILayout.EndVertical();

                        GUILayout.Space(5);
                    }
                }

                if (GUILayout.Button("Retry Mounts"))
                {
                    body.Accessories.TryMountStored();
                }
            }

            GUILayout.EndArea();
        }

        #endregion
    }
}

#endif
