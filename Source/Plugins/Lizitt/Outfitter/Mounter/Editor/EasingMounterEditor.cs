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
    [CustomEditor(typeof(EasingMounter), true)]
    public class EasingMounterEditor
        : OffsetMounterObjectEditor
    {
        protected override void CopyFrom(OffsetMounterObject copySource)
        {
            base.CopyFrom(copySource);

            var source = copySource as EasingMounter;
            if (!source)
                return;

            var targ = target as EasingMounter;

            targ.From = source.From;
            targ.To = source.To;
            targ.MountBufferSize = source.MountBufferSize;
            targ.EaseDuration = source.EaseDuration;
            //targ.EaseSpace = source.EaseSpace;
            targ.UseSharedSpace = source.UseSharedSpace;
        }

        protected override void CopyTo(OffsetMounterObject copyTarget)
        {
            base.CopyTo(copyTarget);

            var copyTarg = copyTarget as EasingMounter;
            if (!copyTarg)
                return;

            var targ = target as EasingMounter;

            copyTarg.From = targ.From;
            copyTarg.To = targ.To;
            copyTarg.MountBufferSize = targ.MountBufferSize;
            copyTarg.EaseDuration = targ.EaseDuration;
            //copyTarg.EaseSpace = targ.EaseSpace;
            copyTarg.UseSharedSpace = targ.UseSharedSpace;
        }
    }
}
