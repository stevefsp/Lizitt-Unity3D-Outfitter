#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using com.lizitt.editor;

namespace com.lizitt.outfitter.proto.editor
{
    [CustomEditor(typeof(BodyPrototyperManager))]
    public class BodyPrototyperManagerEditor
        : Editor
    {
        public override void OnInspectorGUI()
        {
            LizittEditorGUIUtil.BeginLabelWidth(115);
            base.OnInspectorGUI();
            LizittEditorGUIUtil.EndLabelWidth();

            EditorGUILayout.Space();

            var targ = target as BodyPrototyperManager;

            var prefix = targ.InvertButtons
                ? "Mouse0: Rotate camera.\nMouse1: Rotate body."
                : "Mouse0: Rotate body.\nMouse1: Rotate camera.";

            EditorGUILayout.HelpBox(prefix + "\nMouse2: Camera height.\nWheel: Camera distance.", MessageType.None);
        }
    }
}

#endif
