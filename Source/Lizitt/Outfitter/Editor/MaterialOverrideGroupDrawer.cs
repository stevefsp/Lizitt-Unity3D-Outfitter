/*
 * Copyright (c) 2015 Stephen A. Pratt
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
using UnityEditorInternal;
using UnityEngine;
using System.Collections.Generic;

namespace com.lizitt.outfitter.editor
{
    [CustomPropertyDrawer(typeof(MaterialOverrideGroupAttribute))]
    public class MaterialOverrideGroupDrawer
        : PropertyDrawer
    {
        private struct Props
        {
            public SerializedProperty renderer;
            public SerializedProperty index;
            public SerializedProperty material;

            public Props(SerializedProperty property)
            {
                material = property.FindPropertyRelative("m_Material");

                var ptrProp = property.FindPropertyRelative("m_Target");

                renderer = ptrProp.FindPropertyRelative("m_Renderer");
                index = ptrProp.FindPropertyRelative("m_MaterialIndex");
            }
        }

        private class ProtoInfo
        {
            public GameObject prototype = null;

            public readonly List<Renderer> renderers = new List<Renderer>(5);
            public GUIContent[] rendererLabels;

            public readonly List<Material[]> materials = new List<Material[]>(5);
            public readonly List<GUIContent[]> materialLabels = new List<GUIContent[]>(5);

            public void Refresh(GameObject proto)
            {
                this.prototype = proto;

                renderers.Clear();
                materials.Clear();
                materialLabels.Clear();
                var sl = new List<GUIContent>(5);

                renderers.Add(null);
                materials.Add(null);
                materialLabels.Add(null);

                sl.Add(new GUIContent("Select Renderer..."));

                for (int i = 0; i < proto.transform.childCount; i++)
                {
                    var r = proto.transform.GetChild(i).GetComponent<Renderer>();

                    if (!r)
                        continue;

                    renderers.Add(r);
                    sl.Add(new GUIContent(r.name));

                    var mats = r.sharedMaterials;

                    var labels = new GUIContent[mats.Length];

                    for (int j = 0; j < mats.Length; j++)
                    {
                        labels[j] = new GUIContent(mats[j].name);
                    }

                    materials.Add(mats);
                    materialLabels.Add(labels);
                }

                if (renderers.Count == 1)
                {
                    renderers.Clear();
                    materials.Clear();
                    materialLabels.Clear();
                    sl.Clear();
                }

                rendererLabels = sl.ToArray();  // << Must be an array.
            }
        }

        private const string ItemPropName = "m_Items";

        private static readonly float HeaderHeight = 
            EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        private static readonly float FooterHeight =
            EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        private static readonly float ElementHeight = EditorGUIUtility.singleLineHeight * 3 
            + EditorGUIUtility.standardVerticalSpacing * 4 
            + 8;  // Improves separation of elements.  Looks better.

        private static readonly GUIContent RendererLabel = new GUIContent(
            "Renderer", "The renderer that contains the material to replace.");

        private static readonly GUIContent TargetLabel = new GUIContent(
            "Target", "The material that will be replaced.");

        private static readonly GUIContent MaterialLabel = new GUIContent(
            "Override", "Material that will override the target material.");

        private ReorderableList m_List;
        private ProtoInfo m_ProtoInfo;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (m_List == null)
                CreateReorderableList(property);

            int itemCount = m_List.serializedProperty.arraySize;

            float result = HeaderHeight + FooterHeight + EditorGUIUtility.singleLineHeight;

            if (itemCount == 0)
            {
                m_List.elementHeight = EditorGUIUtility.singleLineHeight;
                result += EditorGUIUtility.singleLineHeight * 1.1f;
            }
            else
            {
                m_List.elementHeight = ElementHeight;
                result += ElementHeight * itemCount;
            }

            return result;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = attribute as MaterialOverrideGroupAttribute;
            var protoProp = property.serializedObject.FindProperty(attr.prototypeField);

            if (protoProp == null)
            {
                Debug.LogError("Could not find prototype reference field: " + attr.prototypeField);
                m_ProtoInfo = null;
            }
            else
            {
                var proto = protoProp.objectReferenceValue as GameObject;

                if (proto)
                {
                    if (m_ProtoInfo == null || m_ProtoInfo.prototype != proto)
                    {
                        m_ProtoInfo = new ProtoInfo();
                        m_ProtoInfo.Refresh(proto);
                    }
                }
                else
                    m_ProtoInfo = null; 
            }

            protoProp = null;

            label = EditorGUI.BeginProperty(position, label, property);

            m_List.DoList(position);

            EditorGUI.EndProperty();
        }

        private void CreateReorderableList(SerializedProperty property)
        {
            var list = new ReorderableList(property.serializedObject
                , property.FindPropertyRelative(ItemPropName)
                , true, true, true, true);

            list.headerHeight = HeaderHeight;
            list.footerHeight = FooterHeight;

            list.drawHeaderCallback = delegate(Rect rect)
                {
                    EditorGUI.LabelField(rect, "Material Overrides");
                };

            list.drawElementCallback =
                delegate(Rect position, int index, bool isActive, bool isFocused)
                {
                    var element = list.serializedProperty.GetArrayElementAtIndex(index);

                    if (m_ProtoInfo == null || m_ProtoInfo.renderers.Count == 0)
                        StandardGUI(position, element);
                    else
                        PrototypeGUI(position, element);
                };

            list.onAddCallback = delegate(ReorderableList roList)
            {
                roList.index = AddItem(roList.serializedProperty);
            };

            m_List = list;
        }

        private int AddItem(SerializedProperty listProp)
        {
            int nidx = listProp.arraySize;

            listProp.arraySize++;

            var element = listProp.GetArrayElementAtIndex(nidx);
            // Override default behavior.  Rarely want to duplicate.

            var props = new Props(element);

            props.index.intValue = 0;
            props.renderer.objectReferenceValue = null;
            props.material.objectReferenceValue = null;

            listProp.serializedObject.ApplyModifiedProperties();

            return nidx;
        }

        private void PrototypeGUI(Rect position, SerializedProperty property)
        {
            var origLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 80;

            var props = new Props(property);
            var info = m_ProtoInfo;

            // Renderer

            int iOrig = 0;

            if (props.renderer.objectReferenceValue)
            {
                iOrig = info.renderers.IndexOf(props.renderer.objectReferenceValue as Renderer);
                iOrig = iOrig == -1 ? 0 : iOrig;
            }

            var space = EditorGUIUtility.standardVerticalSpacing;
            var height = EditorGUIUtility.singleLineHeight;

            var rect = new Rect(position.x, position.y + space, position.width, height);
            int iSel = EditorGUI.Popup(rect, RendererLabel, iOrig, info.rendererLabels);

            if (iSel != iOrig)
                props.renderer.objectReferenceValue = info.renderers[iSel];

            int iRen = iSel;

            // Target Index

            rect = new Rect(rect.xMin, rect.yMax + space, rect.width, rect.height);

            if (iRen == 0)
                EditorGUI.LabelField(rect, TargetLabel, new GUIContent("None"));
            else
            {
                GUIContent[] labels = info.materialLabels[iRen];

                iOrig = Mathf.Clamp(props.index.intValue, 0, labels.Length);

                // Don't bother.  Warning isn't helpful because clamping happens frequently as 
                // renderers are switched back and forth.
                //if (iOrig != props.index.intValue)
                //{
                //    if (props.index.intValue != -1)
                //        Debug.LogWarning("Clamped target material index for " + label);
                //}

                iSel = EditorGUI.Popup(rect, TargetLabel, iOrig, labels);

                if (iSel != iOrig)
                    props.index.intValue = iSel;
            }

            // Override material.

            rect = new Rect(rect.xMin, rect.yMax + space, rect.width, rect.height);
            EditorGUI.PropertyField(rect, props.material, MaterialLabel);

            EditorGUIUtility.labelWidth = origLabelWidth;
        }

        private void StandardGUI(Rect position, SerializedProperty property)
        {
            var props = new Props(property);

            var space = EditorGUIUtility.standardVerticalSpacing;
            var height = EditorGUIUtility.singleLineHeight;

            var rect = new Rect(position.x, position.y + space, position.width, height);
            EditorGUI.PropertyField(rect, props.renderer);

            rect = new Rect(rect.xMin, rect.yMax + space, rect.width, rect.height);
            EditorGUI.PropertyField(rect, props.index);

            rect = new Rect(rect.xMin, rect.yMax + space, rect.width, rect.height);
            EditorGUI.PropertyField(rect, props.material);
        }
    }
}
