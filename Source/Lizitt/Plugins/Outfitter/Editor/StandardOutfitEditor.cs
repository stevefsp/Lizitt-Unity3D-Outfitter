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
using System.Collections.Generic;

namespace com.lizitt.outfitter.editor
{
    /// <summary>
    /// The <see cref="StandardOutfit"/> custom editor.
    /// </summary>
    [CustomEditor(typeof(StandardOutfit))]
    public class StandardOutfitEditor
        : Editor
    {
        /*
         * Design notes:
         * 
         * The design is meant to minimize the need to hard code field values in the editor
         * while maintaining the use of serialized properties to format each field properly.
         * 
         * With the exception of the core fields and the observers list, all sections are expected 
         * to start with a 'start' property field and end with the section's namesake 'complex'
         * field. 'Complex' in this context means a field with child fields.  They are used 
         * to control the foldout state for their associated section.  All other fields that 
         * belong to a section are expected to be between the start and end fields.  (The 
         * property iterator follows the field order in the class definition.)
         * 
         * All fields are expected to be in a section with sections layed out internally as follows: 
         * 
         * Section start field
         * ....
         * .... Other section fields
         * ....
         * Section end complex field
         * 
         * The order of the fields in each section is the same as defined in the source class.
         * The order of the sections is controlled by this editor code.
         * 
         * To allow reordering of sections, each section goes through separate load and draw
         * draw steps.  The load step gathers the persistant serialized properties from the
         * property iterator.  The draw step draws the properties.
         */

        #region Editor Members

        void OnEnable()
        {
            var outfit = target as StandardOutfit;

            // Assume auto-detect has already been performed if the motion root is assigned
            // Want to be conservative.
            if (!Application.isPlaying && outfit && !outfit.MotionRoot)
            {
                StandardOutfit.UnsafeRefreshAllSettings(outfit);
                EditorUtility.SetDirty(outfit);
            }
        }

        /// <summary>
        /// See Unity documentation.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var prop = serializedObject.GetIterator();
            prop.NextVisible(true);

            EditorGUILayout.PropertyField(prop);  // The script field.

            while (prop.NextVisible(false))
            {
                bool atEnd = false;

                if (prop.propertyPath == CoreStartPath)
                    atEnd = LoadCoreSection(prop);
                if (prop.propertyPath == ObserverListPath)
                    atEnd = LoadObserverSection(prop);
                else if (prop.propertyPath == MountPointStartPath)
                    atEnd = LoadMountSection(prop);
                else if (prop.propertyPath == BodyPartStartPath)
                    atEnd = LoadBodyPartSection(prop);
                else if (prop.propertyPath == MaterialStartPath)
                    atEnd = LoadMaterialSection(prop);

                if (atEnd)
                    break;  // Error message handled by load methods.
            }

            // The order of these draw calls can be re-ordered.

            DrawCoreSection();          // Best first.
            EditorGUILayout.Space();
            DrawMountSection();
            DrawBodyPartSection();
            DrawMaterialSection();
            DrawObserverSection();      // Best last.
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Core Section

        // Does not implement a foldout.

        private const string CoreStartPath = "m_MotionRoot";
        private const string CoreEndPath = "m_Owner";

        private List<SerializedProperty> m_CoreProperties = new List<SerializedProperty>();

        private bool LoadCoreSection(SerializedProperty prop)
        {
            return LoadStandardSection(
                "Core Properties Section", prop, CoreEndPath, m_CoreProperties);
        }

        private void DrawCoreSection()
        {
            // No foldout for this section.

            foreach (var prop in m_CoreProperties)
                EditorGUILayout.PropertyField(prop);

            m_CoreProperties.Clear();
        }

        #endregion

        #region Observer Section

        // Currently only supports one field.

        private const string ObserverListPath = "m_Observers";

        private SerializedProperty m_ObserverProperty;

        private bool LoadObserverSection(SerializedProperty prop)
        {
            m_ObserverProperty = GetPropertyInstance(prop);
            return false;
        }

        private void DrawObserverSection()
        {
            m_ObserverProperty.isExpanded = EditorGUILayout.Foldout(
                m_ObserverProperty.isExpanded, m_ObserverProperty.displayName);

            if (m_ObserverProperty.isExpanded)
                EditorGUILayout.PropertyField(m_ObserverProperty);

            m_ObserverProperty = null;
        }

        #endregion

        #region Mount Point Section

        private const string MountPointStartPath = "m_Limited";
        private const string MountPointEndListPath = "m_MountPoints";

        private List<SerializedProperty> m_MountProperties = new List<SerializedProperty>();

        private bool LoadMountSection(SerializedProperty prop)
        {
            return LoadStandardSection(
                "Mount Point Section", prop, MountPointEndListPath, m_MountProperties);
        }

        private void DrawMountSection()
        {
            DrawFoldoutSection(m_MountProperties, "Accessories & MountPoints");
        }

        #endregion

        #region Body Parts Section

        private const string BodyPartStartPath = "m_AutoLoadParts";
        private const string BodyPartEndListPath = "m_Parts";

        private List<SerializedProperty> m_PartProperties = new List<SerializedProperty>();

        private bool LoadBodyPartSection(SerializedProperty prop)
        {
            return LoadStandardSection(
                "Body Part Section", prop, BodyPartEndListPath, m_PartProperties);
        }

        private void DrawBodyPartSection()
        {
            DrawFoldoutSection(m_PartProperties, "Body Parts");
        }

        #endregion

        #region Materials Section

        private const string MaterialStartPath = "m_BlendRenderer";
        private const string MaterialEndListPath = "m_OutfitMaterials";

        private List<SerializedProperty> m_MaterialProperties = new List<SerializedProperty>();

        private bool LoadMaterialSection(SerializedProperty prop)
        {
            return LoadStandardSection(
                "Materials Section", prop, MaterialEndListPath, m_MaterialProperties);
        }

        private void DrawMaterialSection()
        {
            DrawFoldoutSection(m_MaterialProperties, "Renderers & Materials");
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Loads all properties into the list, from <paramref name="prop"/> to the property
        /// identified by <paramref name="endPath"/>
        /// </summary>
        /// <param name="sectionName">Used for error messages.</param>
        /// <returns>True if the end of the property iterator was reached. (Error).</returns>
        private static bool LoadStandardSection(
            string sectionName, SerializedProperty prop, string endPath, List<SerializedProperty> list)
        {
            list.Clear();

            list.Add(GetPropertyInstance(prop));

            while (prop.NextVisible(false))
            {
                list.Add(GetPropertyInstance(prop));
                if (prop.propertyPath == endPath)
                    return false;
            }

            Debug.LogError(
                "Internal error: Property section reached the end of the serialized object:"
                + sectionName);

            return true;
        }

        /// <summary>
        /// Draws a standard foldout section from the list of properties, with the last property
        /// expected to be a property with children. (So 'isExpanded' can be used for the foldout.)
        /// </summary>
        private void DrawFoldoutSection(List<SerializedProperty> list, string foldoutTitle)
        {
            var endListProp = list[list.Count - 1];
            endListProp.isExpanded = EditorGUILayout.Foldout(endListProp.isExpanded, foldoutTitle);
            if (!endListProp.isExpanded)
                return;

            foreach (var prop in list)
                EditorGUILayout.PropertyField(prop);

            EditorGUILayout.Space();

            list.Clear();
        }

        /// <summary>
        /// Converts the property interator to a static property for later use.
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        private static SerializedProperty GetPropertyInstance(SerializedProperty prop)
        {
            return prop.serializedObject.FindProperty(prop.propertyPath);
        }

        #endregion
    }
}
