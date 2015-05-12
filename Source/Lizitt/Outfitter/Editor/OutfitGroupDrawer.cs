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

namespace com.lizitt.outfitter.editor
{
    [CustomPropertyDrawer(typeof(OutfitGroup))]
    public class OutfitGroupDrawer
        : PropertyDrawer
    {
        // TODO: Need to find a better way of defining height.  
        // It doesn't handle changes to the number of outfits very well.

        private const float Lines = (int)OutfitType.None + 2.5f;
        private const float LineHeight = 17;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return Lines * LineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var defaultOutfitProp = property.FindPropertyRelative("m_DefaultOutfit");
            var startOutfitProp = property.FindPropertyRelative("m_StartOutfit");
            var prototypesProp = property.FindPropertyRelative("m_Prototypes");

            label = EditorGUI.BeginProperty(position, label, property);

            var rec = new Rect(position.x, position.y, position.width, position.height / Lines);

            EditorGUI.LabelField(rec, "Outfit Group");

            rec = new Rect(rec.xMin, rec.yMax, rec.width, rec.height);
            EditorGUI.PropertyField(rec, startOutfitProp);

            rec = new Rect(rec.xMin, rec.yMax, rec.width, rec.height);
            EditorGUI.PropertyField(rec, defaultOutfitProp);

            var expectedCount = (int)OutfitType.None;

            if (expectedCount != prototypesProp.arraySize)
                RepairPrototypes(prototypesProp, expectedCount, rec);
            else
                ProcessPrototypes(prototypesProp, rec);

            EditorGUI.EndProperty();
        }

        private void ProcessPrototypes(SerializedProperty prototypes, Rect rec)
        {
            var count = (int)OutfitterUtil.HighestVisibleOutfit + 1;

            for (int i = 0; i < count; i++)
            {
                rec = new Rect(rec.xMin, rec.yMax, rec.width, rec.height);

                var outfitProp = prototypes.GetArrayElementAtIndex(i);
                var lbl =  ((OutfitType)i).ToString();

                var choice = (BodyOutfit)EditorGUI.ObjectField(rec, lbl
                    , outfitProp.objectReferenceValue, typeof(BodyOutfit), false);

                if (GUI.changed)
                    outfitProp.objectReferenceValue = choice;
            }
        }

        private void RepairPrototypes(SerializedProperty prototypes, int expecedCount, Rect rec)
        {
            // Increase chance of catching user's attention.
            Debug.LogError("Invalid number of outfits in outfit group.  Expect: " + expecedCount 
                + ", Actual: " + prototypes.arraySize);

            rec = new Rect(rec.xMin, rec.yMax, rec.width, rec.height * 2);

            string message;

            if (expecedCount < prototypes.arraySize)
                message = "Outfit array is too large.  Data will be lost during the repair.";
            else
                message = "Outfit array is too small.  Repair will not destroy any data.";

            EditorGUI.HelpBox(rec, message, MessageType.Error);

            rec = new Rect(rec.xMin, rec.yMax, rec.width, rec.height);

            if (GUI.Button(rec, "Repair"))
                prototypes.arraySize = expecedCount;

            //int delta = expecedCount - prototypes.arraySize;

            //if (delta > 0)
            //{
            //    while (delta > 0)
            //    {
            //        prototypes.InsertArrayElementAtIndex(prototypes.arraySize - 1);
            //        delta--;
            //    }
            //}
            //else
            //{
            //    while (delta < 0)
            //    {
            //        prototypes.DeleteArrayElementAtIndex(prototypes.arraySize - 1);
            //        delta++;
            //    }
            //}
        }
    }
}
