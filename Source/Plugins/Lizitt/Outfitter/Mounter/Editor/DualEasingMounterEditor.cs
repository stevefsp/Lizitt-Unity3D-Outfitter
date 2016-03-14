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

namespace com.lizitt.outfitter.editor
{
    [CustomEditor(typeof(DualCurveEasingMounter), true)]
    public class DualCurveEasingMounterEditor
        : EasingMounterEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space(); 
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("Accessory Mounter\n\nTransfers an accessory from one mount point to"
                + " another, easing its position and rotation based on separate curves.\n\nOne animation curve"
                + " is used for all position axes while second animation curve is used for all rotation axes.\n\n"
                + " Will complete immediately if used outside of play mode. (No animation.)\n\nSupports multiple"
                + " concurrent mount operations. ",
                MessageType.None); 

        }

        protected override void CopyFrom(OffsetMounterObject copySource)
        {
            base.CopyFrom(copySource);

            var source = copySource as DualCurveEasingMounter;
            if (!source)
                return;

            var targ = target as DualCurveEasingMounter;
            targ.PositionCurve = new AnimationCurve(source.PositionCurve.keys);
            targ.RotationCurve = new AnimationCurve(source.RotationCurve.keys);
        }

        protected override void CopyTo(OffsetMounterObject copyTarget)
        {
            base.CopyTo(copyTarget);

            var copyTarg = copyTarget as DualCurveEasingMounter;
            if (!copyTarg)
                return;

            var targ = target as DualCurveEasingMounter;
            copyTarg.PositionCurve = new AnimationCurve(targ.PositionCurve.keys);
            copyTarg.RotationCurve = new AnimationCurve(targ.RotationCurve.keys);
        }
    }
}
