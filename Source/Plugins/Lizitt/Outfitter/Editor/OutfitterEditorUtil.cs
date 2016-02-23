/*
 * Copyright (c) 2015-2016 Stephen A. Pratt
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
    /// Various editor utilities related to the Outfitter
    /// </summary>
    public static class OutfitterEditorUtil
    {
        #region Outfit Type

        private static GUIContent[] m_AllOutfitNames;
        private static int[] m_AllValues;

        private static GUIContent[] m_ExcludeNoneOutfitNames;
        private static int[] m_ExcludeNoneOutfitValues;

        private static GUIContent[] m_ExcludeCustomOutfitNames;
        private static int[] m_ExcludeCustomOutfitValues;

        #region Standard Outfit Arrays

        private static GUIContent[] m_StandardOutfitNames;
        private static int[] m_StandardOutfitValues;

        /// <summary>
        /// The <see cref="OutfitType"/> names for the standard outfits.  (Not custom, not None)
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is a reference to a global array.  Don't make changes to it.  The reason it is
        /// a reference is to reduce construction and garbage collection costs in editor GUI
        /// operations.
        /// </para>
        /// <para>
        /// Thhe order of the array is the same as <see cref="StandardOutfitValues"/>.
        /// </para>
        /// </remarks>
        public static GUIContent[] StandardOutfitNames
        {
            get
            {
                if (m_StandardOutfitNames == null)
                    BuildStandard();

                return m_StandardOutfitNames;
            }
        }

        /// <summary>
        /// The <see cref="OutfitType"/> values for the standard outfits.  (Not custom, not None)
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is a reference to a global array.  Don't make changes to it.  The reason it is
        /// a reference is to reduce construction and garbage collection costs in editor GUI
        /// operations.
        /// </para>
        /// <para>
        /// Thhe order of the array is the same as <see cref="StandardOutfitNames"/>.
        /// </para>
        /// </remarks>
        public static int[] StandardOutfitValues
        {
            get
            {
                if (m_StandardOutfitValues == null)
                    BuildStandard();

                return m_StandardOutfitValues;
            }
        }

        private static void BuildStandard()
        {
            var lin = new List<string>(System.Enum.GetNames(typeof(OutfitType)));
            var liv = new List<int>(System.Enum.GetValues(typeof(OutfitType)) as int[]);

            for (int i = lin.Count - 1; i >= 0; i--)
            {
                if (!((OutfitType)liv[i]).IsStandard())
                {
                    lin.RemoveAt(i);
                    liv.RemoveAt(i);
                }
            }

            m_StandardOutfitNames = CreateLabels(lin);
            m_StandardOutfitValues = liv.ToArray();
        }

        #endregion

        /// <summary>
        /// Draw the standard <see cref="OutfitType"/> GUI pop-up.
        /// </summary>
        /// <param name="position">The position to draw the GUI item.</param>
        /// <param name="label">The GUI label.</param>
        /// <param name="selectedValue">The currently selected outfit type value.</param>
        /// <param name="filterType">The filter to apply to the GUI item.</param>
        /// <returns>
        /// The outfit type value that is selected, or the value of 
        /// <see cref="OutfitterUtil.DefaultOutfit"/> if <paramref name="selectedValue"/> is invalid.
        /// </returns>
        public static int OutfitTypePopup(
            Rect position, GUIContent label, int selectedValue, OutfitFilterType filterType)
        {
            /*
             * Design notes:
             * 
             * The reason integers are used instead of the enum is to support 
             * SerializedProperties as the main use case.  SerializedProperties store enums
             * as integers.
             */

            GUIContent[] names;
            int[] values;

            switch (filterType)
            {
                case OutfitFilterType.StandardOnly:

                    names = StandardOutfitNames;
                    values = StandardOutfitValues;

                    break;

                case OutfitFilterType.ExcludeNone:

                    if (m_ExcludeNoneOutfitNames == null)
                    {
                        var lin = new List<string>(System.Enum.GetNames(typeof(OutfitType)));
                        var liv = new List<int>(System.Enum.GetValues(typeof(OutfitType)) as int[]);

                        for (int i = lin.Count - 1; i >= 0; i--)
                        {
                            if ((OutfitType)liv[i] == OutfitType.None)
                            {
                                lin.RemoveAt(i);
                                liv.RemoveAt(i);
                                break;
                            }
                        }

                        m_ExcludeNoneOutfitNames = CreateLabels(lin);
                        m_ExcludeNoneOutfitValues = liv.ToArray();
                    }

                    names = m_ExcludeNoneOutfitNames;
                    values = m_ExcludeNoneOutfitValues;

                    break;

                case OutfitFilterType.ExcludeCustom:

                    if (m_ExcludeCustomOutfitNames == null)
                    {
                        var lin = new List<string>(System.Enum.GetNames(typeof(OutfitType)));
                        var liv = new List<int>(System.Enum.GetValues(typeof(OutfitType)) as int[]);

                        for (int i = lin.Count - 1; i >= 0; i--)
                        {
                            if (((OutfitType)liv[i]).IsCustom())
                            {
                                lin.RemoveAt(i);
                                liv.RemoveAt(i);
                            }
                        }

                        m_ExcludeCustomOutfitNames = CreateLabels(lin);
                        m_ExcludeCustomOutfitValues = liv.ToArray();
                    }

                    names = m_ExcludeCustomOutfitNames;
                    values = m_ExcludeCustomOutfitValues;

                    break;

                default:

                    if (m_AllOutfitNames == null)
                    {
                        m_AllOutfitNames = CreateLabels(
                            new List<string>(System.Enum.GetNames(typeof(OutfitType))));
                        m_AllValues = System.Enum.GetValues(typeof(OutfitType)) as int[];
                    }

                    names = m_AllOutfitNames;
                    values = m_AllValues;

                    break;
            }

            int currentIdx = 0;

            for (int i = 0; i < values.Length; i++)
            {
                var typ = values[i];
                if (typ == selectedValue)
                {
                    currentIdx = i;
                    break;
                }
                else if (typ == (int)OutfitterUtil.DefaultOutfit)
                    currentIdx = i;  // Use default if current not found.
            }

            int selectedIdx = EditorGUI.Popup(position, label, currentIdx, names);

            return values[selectedIdx];
        }

        #endregion

        #region Mount Point Type

        private static GUIContent[] m_MountPointTypeNames;
        private static int[] m_MountPointTypeValues;

        /// <summary>
        /// The <see cref="OutfitType"/> names for the standard outfits.  (Not custom, not None)
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is a reference to a global array.  Don't make changes to it.  The reason it is
        /// a reference is to reduce construction and garbage collection costs in editor GUI
        /// operations.
        /// </para>
        /// <para>
        /// Thhe order of the array is the same as <see cref="StandardOutfitValues"/>.
        /// </para>
        /// </remarks>
        public static GUIContent[] MountPointTypeNames
        {
            get
            {
                if (m_MountPointTypeNames == null)
                    BuildMountTypes();

                return m_MountPointTypeNames;
            }
        }

        /// <summary>
        /// The <see cref="OutfitType"/> values for the standard outfits.  (Not custom, not None)
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is a reference to a global array.  Don't make changes to it.  The reason it is
        /// a reference is to reduce construction and garbage collection costs in editor GUI
        /// operations.
        /// </para>
        /// <para>
        /// Thhe order of the array is the same as <see cref="StandardOutfitNames"/>.
        /// </para>
        /// </remarks>
        public static int[] MountPointTypeValues
        {
            get
            {
                if (m_MountPointTypeValues == null)
                    BuildMountTypes();

                return m_MountPointTypeValues;
            }
        }

        private static void BuildMountTypes()
        {
            var lin = new List<string>(System.Enum.GetNames(typeof(MountPointType)));
            var liv = new List<int>(System.Enum.GetValues(typeof(MountPointType)) as int[]);

            m_MountPointTypeNames = CreateLabels(lin);
            m_MountPointTypeValues = liv.ToArray();
        }
        #endregion

        #region Outfit Material Type

        private static GUIContent[] m_OutfitMaterialTypeNames;
        private static int[] m_OutfitMaterialTypeValues;

        /// <summary>
        /// The <see cref="OutfitMaterialType"/> names.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is a reference to a global array.  Don't make changes to it.  The reason it is
        /// a reference is to reduce construction and garbage collection costs in editor GUI
        /// operations.
        /// </para>
        /// <para>
        /// Thhe order of the array is the same as <see cref="StandardOutfitValues"/>.
        /// </para>
        /// </remarks>
        public static GUIContent[] OutfitMaterialTypeNames
        {
            get
            {
                if (m_OutfitMaterialTypeNames == null)
                    BuildOutfitMaterialTypes();

                return m_OutfitMaterialTypeNames;
            }
        }

        /// <summary>
        /// The <see cref="OutfitType"/> values for the standard outfits.  (Not custom, not None)
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is a reference to a global array.  Don't make changes to it.  The reason it is
        /// a reference is to reduce construction and garbage collection costs in editor GUI
        /// operations.
        /// </para>
        /// <para>
        /// Thhe order of the array is the same as <see cref="StandardOutfitNames"/>.
        /// </para>
        /// </remarks>
        public static int[] OutfitMaterialTypeTypeValues
        {
            get
            {
                if (m_OutfitMaterialTypeValues == null)
                    BuildOutfitMaterialTypes();

                return m_OutfitMaterialTypeValues;
            }
        }

        private static void BuildOutfitMaterialTypes()
        {
            var lin = new List<string>(System.Enum.GetNames(typeof(OutfitMaterialType)));
            var liv = new List<int>(System.Enum.GetValues(typeof(OutfitMaterialType)) as int[]);

            m_OutfitMaterialTypeNames = CreateLabels(lin);
            m_OutfitMaterialTypeValues = liv.ToArray();
        }

        #endregion

        #region Private Utility Members

        private static GUIContent[] CreateLabels(List<string> labels)
        {
            var result = new GUIContent[labels.Count];

            for (int i = 0; i < labels.Count; i++)
                result[i] = new GUIContent(labels[i]);

            return result;
        }

        #endregion

        #region Event Members

        /// <summary>
        /// If true, the user has pre-authorized a 'destructive edit'.
        /// </summary>
        public static bool IsDestructiveConfirmed
        {
            get { return !IsNonDestructiveConfirmed && Event.current.control; }
        }

        /// <summary>
        /// If true, the user has pre-authorized a 'non-destructive edit'.
        /// </summary>
        public static bool IsNonDestructiveConfirmed
        {
            get { return Event.current.shift; }
        }

        #endregion

        #region Shared EditorPrefs

        private static bool m_IsInitialized = false;

        private static void CheckInitialized()
        {
            if (m_IsInitialized)
                return;

            m_IsInitialized = true;

            InitializeShowInspector();
            InitializeAutoOffset();
        }

        private const string ShowActionsKey = "com.lizitt.outfitter.editor.ShowInspectorActions";
        private static bool m_ShowInspectorActions;

        /// <summary>
        /// If true, the inspector actions foldouts should be displayed open.
        /// </summary>
        public static bool ShowInspectorActions
        {
            get 
            {
                CheckInitialized();
                return m_ShowInspectorActions; 
            }
            set 
            {
                CheckInitialized();

                if (m_ShowInspectorActions != value)
                    EditorPrefs.SetBool(ShowActionsKey, value);

                m_ShowInspectorActions = value; 
            }
        }

        private static void InitializeShowInspector()
        {
            if (!EditorPrefs.HasKey(ShowActionsKey))
            {
                m_ShowInspectorActions = false;
                EditorPrefs.SetBool(ShowActionsKey, m_ShowInspectorActions);
            }
            else
                m_ShowInspectorActions = EditorPrefs.GetBool(ShowActionsKey);
        }

        private const string AutoOffsetKey = "com.lizitt.outfitter.editor.AutoOffsetKey";
        private static float m_AutoOffset;

        /// <summary>
        /// When an item is removed from its owner, move it out of the way by this distance so that is doesn't obstruct the owner.
        /// </summary>
        public static float AutoOffset
        {
            get
            {
                CheckInitialized();
                return m_AutoOffset;
            }
            set
            {
                CheckInitialized();

                if (m_AutoOffset != value)
                    EditorPrefs.SetFloat(AutoOffsetKey, value);
                
                m_AutoOffset = value;
            }
        }

        private static void InitializeAutoOffset()
        {
            if (!EditorPrefs.HasKey(AutoOffsetKey))
            {
                m_AutoOffset = 0;
                EditorPrefs.SetFloat(AutoOffsetKey, m_AutoOffset);
            }
            else
                m_AutoOffset = EditorPrefs.GetFloat(AutoOffsetKey);
        }

        #endregion
    }
}
