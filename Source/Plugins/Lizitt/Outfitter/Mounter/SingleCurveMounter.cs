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
using UnityEngine;

namespace com.lizitt.outfitter
{
    /// <summary>
    /// Transfers an accessory from one mount point to another, easing its position and rotation
    /// based on a single animation curve.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The same animation curve is used for all position and rotation axes.
    /// </para>
    /// <para>
    /// Supports multiple concurrent mount operations. Will complete immediately if used outside of play mode. 
    /// (No animation.) 
    /// </para>
    /// </remarks>
    [CreateAssetMenu(
        menuName = OutfitterUtil.AssetMenu + "Single Curve Mounter", order = OutfitterUtil.MounterMenuOrder + 1)]
    public sealed class SingleCurveMounter
        : EasingMounter
    {
        [Space]

        [SerializeField]
        [Tooltip("The normalized animation curve applied to all position and rotation axes. (Both time and value"
            + " axes are normalized.)")]
        private AnimationCurve m_EaseCurve = new AnimationCurve();

        /// <summary>
        /// The normalized animation curve applied to all position and rotation axes. (Both time and value axes
        /// are normalized.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// Both the time and value axes of the animation curve are normalized.  I.e. 0.5 on the time axis 
        /// represents 50% of <see cref="EaseDuration"/>.  0.8 on the value axis represents 80% completion
        /// of the translation and rotation.
        /// </para>
        /// </remarks>
        public AnimationCurve EaseCurve
        {
            get { return m_EaseCurve; }
            set { m_EaseCurve = value; }
        }

        public override Vector3 GetLocalPosition(Vector3 start, Vector3 end, float normalizedTime)
        {
            return end + (start - end) * (1 - m_EaseCurve.Evaluate(normalizedTime));
        }

        public override Vector3 GetLocalEulerAngles(Vector3 start, Vector3 end, float normalizedTime)
        {
            var eval = m_EaseCurve.Evaluate(normalizedTime);

            return new Vector3(
                Easing.Clerp(start.x, end.x, eval),
                Easing.Clerp(start.y, end.y, eval),
                Easing.Clerp(start.z, end.z, eval));

        }
    }
}
