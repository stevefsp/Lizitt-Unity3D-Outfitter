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
    /// based on an animation curve for each axis.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each position and rotation axis has its own animation curve.
    /// </para>
    /// <para>
    /// Supports multiple concurrent mount operations. Will complete immediately if used outside of play mode. 
    /// (No animation.) 
    /// </para>
    /// </remarks>
    [CreateAssetMenu(
        menuName = OutfitterMenu.AssetMenu + "Six Curve Mounter", order = OutfitterMenu.MounterAssetMenuOrder + 3)]
    public sealed class SixCurveEasingMounter
        : EasingMounter
    {
        [Header("Position Curves")]

        [SerializeField]
        [Tooltip("The normalized animation curve applied to the position x-axis."
            + " (Both time and value axes are normalized.)")]
        private AnimationCurve m_PositionX = new AnimationCurve();

        /// <summary>
        /// The normalized animation curve applied to the position x-axis (Both time and value axes are normalized.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// Both the time and value axes of the animation curve are normalized.  I.e. 0.5 on the time axis 
        /// represents 50% of the <see cref="EaseDuration"/>.  0.8 on the value axis represents 80% completion 
        /// of the x-axis tranlation.
        /// </para>
        /// </remarks>
        public AnimationCurve PositionCurveX
        {
            get { return m_PositionX; }
            set { m_PositionX = value; }
        }

        [SerializeField]
        [Tooltip(
            "The normalized animation curve applied to the position y-axis. (Both time and value axes are normalized.)")]
        private AnimationCurve m_PositionY = new AnimationCurve();

        /// <summary>
        /// The normalized animation curve applied to the position y-axis (Both time and value axes are normalized.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// Both the time and value axes of the animation curve are normalized.  I.e. 0.5 on the time axis 
        /// represents 50% of the <see cref="EaseDuration"/>.  0.8 on the value axis represents 80% completion of
        /// the y-axis tranlation.
        /// </para>
        /// </remarks>
        public AnimationCurve PositionCurveY
        {
            get { return m_PositionY; }
            set { m_PositionY = value; }
        }

        [SerializeField]
        [Tooltip(
            "The normalized animation curve applied to the position y-axis. (Both time and value axes are normalized.)")]
        private AnimationCurve m_PositionZ = new AnimationCurve();

        /// <summary>
        /// The normalized animation curve applied to the position z-axis (Both time and value axes are normalized.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// Both the time and value axes of the animation curve are normalized.  I.e. 0.5 on the time axis 
        /// represents 50% of the <see cref="EaseDuration"/>.  0.8 on the value axis represents 80% completion 
        /// of the z-axis tranlation.
        /// </para>
        /// </remarks>
        public AnimationCurve PositionCurveZ
        {
            get { return m_PositionZ; }
            set { m_PositionZ = value; }
        }

        [Header("Rotation Curves")]

        [SerializeField]
        [Tooltip(
            "The normalized animation curve applied to the rotation x-axis. (Both time and value axes are normalized.)")]
        private AnimationCurve m_RotationX = new AnimationCurve();

        /// <summary>
        /// The normalized animation curve applied to the rotation x-axis (Both time and value axes are normalized.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// Both the time and value axes of the animation curve are normalized.  I.e. 0.5 on he time axis 
        /// represents 50% of the <see cref="EaseDuration"/>.  0.8 on the value axis represents 80% completion 
        /// of the x-axis rotation.
        /// </para>
        /// </remarks>
        public AnimationCurve RotationCurveX
        {
            get { return m_RotationX; }
            set { m_RotationX = value; }
        }

        [SerializeField]
        [Tooltip(
            "The normalized animation curve applied to the rotation y-axis. (Both time and value axes are normalized.)")]
        private AnimationCurve m_RotationY = new AnimationCurve();

        /// <summary>
        /// The normalized animation curve applied to the rotation y-axis (Both time and value axes are normalized.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// Both the time and value axes of the animation curve are normalized.  I.e. 0.5 on the time axis 
        /// represents 50% of the <see cref="EaseDuration"/>.  0.8 on the value axis represents 80% completion
        /// of the y-axis rotation.
        /// </para>
        /// </remarks>
        public AnimationCurve RotationCurveY
        {
            get { return m_RotationY; }
            set { m_RotationY = value; }
        }

        [SerializeField]
        [Tooltip(
            "The normalized animation curve applied to the rotation z-axis. (Both time and value axes are normalized.)")]
        private AnimationCurve m_RotationZ = new AnimationCurve();

        /// <summary>
        /// The normalized animation curve applied to the rotation z-axis (Both time and value axes are normalized.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// Both the time and value axes of the animation curve are normalized.  I.e. 0.5 on the time axis 
        /// represents 50% of the <see cref="EaseDuration"/>.  0.8 on the value axis represents 80% completion
        /// of the z-axis rotation.
        /// </para>
        /// </remarks>
        public AnimationCurve RotationCurveZ
        {
            get { return m_RotationZ; }
            set { m_RotationZ = value; }
        }

        public override Vector3 GetPosition(Vector3 start, Vector3 end, float normalizedTime)
        {
            return new Vector3(
                end.x + (start.x - end.x) * (1 - m_PositionX.Evaluate(normalizedTime)),
                end.y + (start.y - end.y) * (1 - m_PositionY.Evaluate(normalizedTime)),
                end.z + (start.z - end.z) * (1 - m_PositionZ.Evaluate(normalizedTime)));
        }

        public override Vector3 GetEulerAngles(Vector3 start, Vector3 end, float normalizedTime)
        {
            return new Vector3(
                Easing.Clerp(start.x, end.x, m_RotationX.Evaluate(normalizedTime)),
                Easing.Clerp(start.y, end.y, m_RotationY.Evaluate(normalizedTime)),
                Easing.Clerp(start.z, end.z, m_RotationZ.Evaluate(normalizedTime)));
        }
    }
}
