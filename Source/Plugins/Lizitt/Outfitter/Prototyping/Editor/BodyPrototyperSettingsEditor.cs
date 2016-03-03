#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace com.lizitt.outfitter.proto.editor
{
    [CustomEditor(typeof(BodyPrototyperSettings))]
    public class BodyPrototyperSettingsEditor
        : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            var targ = target as BodyPrototyperSettings;

            if (Application.isPlaying)
                return;

            if (!targ.Body)
            {
                if (GUILayout.Button("Instantiate a Standard Body"))
                    targ.InitializeBody();
            }

            EditorGUILayout.Space();

            if (targ.gameObject.tag != DefaultTag.EditorOnly)
                EditorGUILayout.HelpBox("The GameObject tag must be '" + DefaultTag.EditorOnly + "'", MessageType.Error);
        }
    }
}

#endif
