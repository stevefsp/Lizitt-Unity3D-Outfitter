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
    /// based on separate position and rotation curves.
    /// </summary>
    /// <remarks>
    /// <para>
    /// One animation curve is used for all position axes while second animation curve is
    /// used for all rotation axes.
    /// </para>
    /// <para>
    /// This mounter can only be used for one mount operation at a time, so it must either be 
    /// assigned to a single accessory or shared using a pooling system in a manner that prevents
    /// concurrent use.
    /// </para>
    /// <para>
    /// Will complete immediately if used outside of play mode. (No animation.)
    /// </para>
    /// <para>
    /// Does not support unmounting.
    /// </para>
    /// </remarks>
    [CreateAssetMenu(menuName = LizittUtil.LizittMenu + "Dual Curve Mounter",
        order = OutfitterUtil.MounterMenuOrder + 3)]
    public class DualCurveMounter
        : EasingMounter
    {
        [Header("Curves")]

        [SerializeField]
        [Tooltip("The normalized animation curve applied to all position axes."
            + " (Both time and value axes are normalized.)")]
        private AnimationCurve m_Position = new AnimationCurve();

        /// <summary>
        /// The normalized animation curve applied to all position axes. (Both time 
        /// and value axes are normalized.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// Both the time and value axes of the animation curve are normalized.  I.e. 0.5 on 
        /// the time axis represents 50% of the <see cref="EaseDuration"/>.  0.8 on the value axis
        /// represents 80% completion of the translation.
        /// </para>
        /// </remarks>
        public AnimationCurve PositionCurve
        {
            get { return m_Position; }
            set { m_Position = value; }
        }

        [SerializeField]
        [Tooltip("The normalized animation curve applied to all rotation axes."
            + " (Both time and value axes are normalized.)")]
        private AnimationCurve m_Rotation = new AnimationCurve();

        /// <summary>
        /// The normalized animation curve applied to all rotation axes. (Both time 
        /// and value axes are normalized.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// Both the time and value axes of the animation curve are normalized.  I.e. 0.5 on 
        /// the time axis represents 50% of the <see cref="EaseDuration"/>.  0.8 on the value axis
        /// represents 80% completion of the rotation.
        /// </para>
        /// </remarks>
        public AnimationCurve RotationCurve
        {
            get { return m_Rotation; }
            set { m_Rotation = value; }
        }

        public override Vector3 GetLocalPosition(Vector3 start, Vector3 end, float normalizedTime)
        {
            var eval = (1 - m_Position.Evaluate(normalizedTime));

            return new Vector3(
                end.x + (start.x - end.x) * eval,
                end.y + (start.y - end.y) * eval,
                end.z + (start.z - end.z) * eval);
        }

        public override Vector3 GetLocalEulerAngles(Vector3 start, Vector3 end, float normalizedTime)
        {
            var eval = m_Rotation.Evaluate(normalizedTime);

            return new Vector3(
                Easing.Clerp(start.x, end.x, eval),
                Easing.Clerp(start.y, end.y, eval),
                Easing.Clerp(start.z, end.z, eval));
        }
    }
}
