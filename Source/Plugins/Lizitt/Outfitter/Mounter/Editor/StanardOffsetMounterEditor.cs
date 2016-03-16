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

namespace com.lizitt.outfitter.editor
{
    [CustomEditor(typeof(StandardOffsetMounter))]
    public class StandardOffsetMounterEditor
        : OffsetMounterObjectEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("Accessory Mounter\n\nA mounter that immediately parents to its location"
                + " with optional position and rotation offsets. The mount operation will always succeed"
                + " if its location type matches the the mount point's location type.\n\nUpdate completes"
                + " immediately.\n\nSupports multiple concurrent mount operations.", 
                MessageType.None); 
        }

        protected override void CopyFrom(OffsetMounterObject copySource)
        {
            base.CopyFrom(copySource);

            var targ = target as StandardOffsetMounter;

            if (copySource is EasingMounter)
                targ.SetDefaultLocation((copySource as EasingMounter).To);
            else if (copySource is StandardOffsetMounter)
                targ.SetDefaultLocation((copySource as StandardOffsetMounter).DefaultLocation);
        }

        protected override void CopyTo(OffsetMounterObject copyTarget)
        {
            base.CopyTo(copyTarget);

            var targ = target as StandardOffsetMounter;

            if (copyTarget is EasingMounter)
                (copyTarget as EasingMounter).To = targ.DefaultLocation ;
            else if (copyTarget is StandardOffsetMounter)
                (copyTarget as StandardOffsetMounter).SetDefaultLocation(targ.DefaultLocation);
        }
    }
}
