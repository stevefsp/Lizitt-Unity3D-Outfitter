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
using com.lizitt.u3d;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace com.lizitt.outfitter
{
    [CustomPropertyDrawer(typeof(MaterialOverride))]
    public class MaterialOverrideDrawer
        : PropertyDrawer
    {
        // TODO: This is NOT generalized.  It only works properly for the an object with a field
        // called m_BodyPrototype.  So either generalize it or convert to an attribute 
        // drawer.  (Maybe an attribute drawer that requires the property name that holds the search
        // target?)

        private const float Lines = 4;
        private const float LineHeight = 17;

        private static readonly GUIContent MaterialLabel = new GUIContent(
            "Override With", "Material that will override the target material.");

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
            public string[] rendererLabels;

            public readonly List<Material[]> materials = new List<Material[]>(5);
            public readonly List<string[]> materialLabels = new List<string[]>(5);

            public void Refresh(GameObject proto)
            {
                this.prototype = proto;

                renderers.Clear();
                materials.Clear();
                materialLabels.Clear();
                var sl = new List<string>(5);

                renderers.Add(null);
                materials.Add(null);
                materialLabels.Add(null);

                sl.Add("Select Renderer...");

                for (int i = 0; i < proto.transform.childCount; i++)
                {
                    var r = proto.transform.GetChild(i).GetComponent<Renderer>();

                    if (!r)
                        continue;

                    renderers.Add(r);
                    sl.Add(r.name);

                    var mats = r.sharedMaterials;

                    var labels = new string[mats.Length];

                    for (int j = 0; j < mats.Length; j++)
                    {
                        labels[j] = mats[j].name;
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

        private Dictionary<GameObject, ProtoInfo> m_Infos = new Dictionary<GameObject, ProtoInfo>();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return Lines * LineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (m_Infos.Count > 25)
            {
                Debug.LogWarning("Prototype information exceeded 25. Cleared information.");
                m_Infos.Clear();
            }

            var protoProp = property.serializedObject.FindProperty("m_BodyPrototype");

            var proto = protoProp.objectReferenceValue as GameObject;

            if (proto)
            {
                // Note: GetComponentsInChildren() does not work on the root of a model asset.
                // Have to inspect children.

                ProtoInfo info;
                
                if (!m_Infos.TryGetValue(proto, out info))
                {
                    info = new ProtoInfo();
                    info.Refresh(proto);
                    m_Infos.Add(proto, info);
                    //Debug.Log("Generated prototype information for " + proto.name);
                }

                if (info.renderers.Count > 0)
                {
                    PrototypeGUI(position, property, label, info);
                    return;
                }
            }
            
            StandardGUI(position, property, label);
        }

        private void PrototypeGUI(Rect position, SerializedProperty property, GUIContent label,
            ProtoInfo info)
        { 
            label = EditorGUI.BeginProperty(position, label, property);

            var rec = new Rect(position.x, position.y, position.width, position.height / Lines);
            EditorGUI.LabelField(rec, label);

            var props = new Props(property);

            // Renderer

            int iOrig = 0;

            if (props.renderer.objectReferenceValue)
            {
                iOrig = info.renderers.IndexOf(props.renderer.objectReferenceValue as Renderer);
                iOrig = iOrig == -1 ? 0 : iOrig;
            }

            rec = new Rect(rec.xMin, rec.yMax, rec.width, rec.height);
            int iSel = EditorGUI.Popup(rec, "Renderer", iOrig, info.rendererLabels);

            if (iSel != iOrig)
                props.renderer.objectReferenceValue = info.renderers[iSel];

            int iRen = iSel;

            // Target Index

            rec = new Rect(rec.xMin, rec.yMax, rec.width, rec.height);

            if (iRen == 0)
                EditorGUI.LabelField(rec, "...");
            else
            {
                string[] labels = info.materialLabels[iRen];

                iOrig = Mathf.Clamp(props.index.intValue, 0, labels.Length);

                // Don't bother.  Warning isn't helpful because clamping happens frequently as 
                // renderers are switched back and forth.
                //if (iOrig != props.index.intValue)
                //{
                //    if (props.index.intValue != -1)
                //        Debug.LogWarning("Clamped target material index for " + label);
                //}

                iSel = EditorGUI.Popup(rec, "Override Target", iOrig, labels);

                if (iSel != iOrig)
                    props.index.intValue = iSel;
            }

            // Override material.

            rec = new Rect(rec.xMin, rec.yMax, rec.width, rec.height);
            EditorGUI.PropertyField(rec, props.material, MaterialLabel);

            EditorGUI.EndProperty();
        }

        private void StandardGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var props = new Props(property);

            label = EditorGUI.BeginProperty(position, label, property);

            var rec = new Rect(position.x, position.y, position.width, position.height / Lines);
            EditorGUI.LabelField(rec, label);

            rec = new Rect(rec.xMin, rec.yMax, rec.width, rec.height);
            EditorGUI.PropertyField(rec, props.renderer);

            rec = new Rect(rec.xMin, rec.yMax, rec.width, rec.height);
            EditorGUI.PropertyField(rec, props.index);

            rec = new Rect(rec.xMin, rec.yMax, rec.width, rec.height);
            EditorGUI.PropertyField(rec, props.material);

            EditorGUI.EndProperty();
        }

        
    }
}
