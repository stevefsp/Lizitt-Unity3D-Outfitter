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
using UnityEngine;
using System.Collections.Generic;

namespace com.lizitt.outfitter.editor
{
    public static class OutfitterEditorUtil
    {
        private static GUIContent[] m_AllNames;
        private static int[] m_AllValues;

        private static GUIContent[] m_ExcludeNoneNames;
        private static int[] m_ExcludeNoneValues;

        private static GUIContent[] m_ExcludeCustomNames;
        private static int[] m_ExcludeCustomValues;

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

                    if (m_ExcludeNoneNames == null)
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

                        m_ExcludeNoneNames = CreateLabels(lin);
                        m_ExcludeNoneValues = liv.ToArray();
                    }

                    names = m_ExcludeNoneNames;
                    values = m_ExcludeNoneValues;

                    break;

                case OutfitFilterType.ExcludeCustom:

                    if (m_ExcludeCustomNames == null)
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

                        m_ExcludeCustomNames = CreateLabels(lin);
                        m_ExcludeCustomValues = liv.ToArray();
                    }

                    names = m_ExcludeCustomNames;
                    values = m_ExcludeCustomValues;

                    break;

                default:

                    if (m_AllNames == null)
                    {
                        m_AllNames = CreateLabels(
                            new List<string>(System.Enum.GetNames(typeof(OutfitType))));
                        m_AllValues = System.Enum.GetValues(typeof(OutfitType)) as int[];
                    }

                    names = m_AllNames;
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

        private static GUIContent[] CreateLabels(List<string> labels)
        {
            var result = new GUIContent[labels.Count];

            for (int i = 0; i < labels.Count; i++)
                result[i] = new GUIContent(labels[i]);

            return result;
        }
    }
}
