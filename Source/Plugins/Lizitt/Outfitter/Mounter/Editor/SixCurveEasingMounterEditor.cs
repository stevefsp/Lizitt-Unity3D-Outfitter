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
    [CustomEditor(typeof(SixCurveEasingMounter), true)]
    public class SixCurveEasingMounterEditor
        : EasingMounterEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space(); 
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("Accessory Mounter\n\nTransfers an accessory from one mount point to"
                + " another, easing its position and rotation based on an animation curve for each axis.\n\n"
                + " Each position and rotation axis has its own animation curve.\n\nWill complete immediately"
                + " if used outside of play mode. (No animation.)\n\nSupports multiple concurrent mount operations.",
                MessageType.None); 
        }

        protected override void CopyFrom(OffsetMounterObject copySource)
        {
            base.CopyFrom(copySource);

            var source = copySource as SixCurveEasingMounter;
            if (!source)
                return;

            var targ = target as SixCurveEasingMounter;

            targ.PositionCurveX = new AnimationCurve(source.PositionCurveX.keys);
            targ.PositionCurveY = new AnimationCurve(source.PositionCurveY.keys);
            targ.PositionCurveZ = new AnimationCurve(source.PositionCurveZ.keys);

            targ.RotationCurveX = new AnimationCurve(source.RotationCurveX.keys);
            targ.RotationCurveY = new AnimationCurve(source.RotationCurveY.keys);
            targ.RotationCurveZ = new AnimationCurve(source.RotationCurveZ.keys);
        }

        protected override void CopyTo(OffsetMounterObject copyTarget)
        {
            base.CopyTo(copyTarget);

            var copyTarg = copyTarget as SixCurveEasingMounter;
            if (!copyTarg)
                return;

            var targ = target as SixCurveEasingMounter;

            copyTarg.PositionCurveX = new AnimationCurve(targ.PositionCurveX.keys);
            copyTarg.PositionCurveY = new AnimationCurve(targ.PositionCurveY.keys);
            copyTarg.PositionCurveZ = new AnimationCurve(targ.PositionCurveZ.keys);

            copyTarg.RotationCurveX = new AnimationCurve(targ.RotationCurveX.keys);
            copyTarg.RotationCurveY = new AnimationCurve(targ.RotationCurveY.keys);
            copyTarg.RotationCurveZ = new AnimationCurve(targ.RotationCurveZ.keys);
        }
    }
}
