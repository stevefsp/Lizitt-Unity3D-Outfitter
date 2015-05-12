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
namespace com.lizitt.outfitter
{
    /// <summary>
    /// Flags representing body coverage.
    /// </summary>
    [System.Flags]
    public enum BodyCoverage
    {
        /// <summary>
        /// The part of the head normally covered by hair.  (Top/back)
        /// </summary>
        HeadTop = 1 << 0,

        /// <summary>
        /// The face, except for the eyes.
        /// </summary>
        Face = 1 << 1,

        /// <summary>
        /// The eyes.
        /// </summary>
        Eyes = 1 << 2,

        /// <summary>
        /// The neck.
        /// </summary>
        Neck = 1 << 3,

        /// <summary>
        /// The fron of the chest (upper torso).
        /// </summary>
        ChestFront = 1 << 4,

        /// <summary>
        /// The back of the chest (upper torso).
        /// </summary>
        ChestBack = 1 << 5,

        /// <summary>
        /// The front of the waist (lower torso).
        /// </summary>
        WaistFront = 1 << 6,

        /// <summary>
        /// The back of the waist (lower torso).
        /// </summary>
        WaistBack = 1 << 7,

        /// <summary>
        /// The right shoulder.
        /// </summary>
        RightShoulder = 1 << 8,

        /// <summary>
        /// The left shoulder.
        /// </summary>
        LeftShoulder = 1 << 9,

        /// <summary>
        /// The right upper arm.
        /// </summary>
        RightUpperArm = 1 << 10,

        /// <summary>
        /// the left upper arm.
        /// </summary>
        LeftUpperArm = 1 << 11,

        /// <summary>
        /// The right fore arm.
        /// </summary>
        RightForearm = 1 << 12,

        /// <summary>
        /// The left fore arm.
        /// </summary>
        LeftForearm = 1 << 13,

        /// <summary>
        /// The right wrist.
        /// </summary>
        RightWrist = 1 << 14,

        /// <summary>
        /// The left wrist.
        /// </summary>
        LeftWrist = 1 << 15,

        /// <summary>
        /// The right hand.
        /// </summary>
        RightHand = 1 << 16,

        /// <summary>
        /// The left hand.
        /// </summary>
        LeftHand = 1 << 17,

        /// <summary>
        /// The right upper leg.
        /// </summary>
        RightUpperLeg = 1 << 18,

        /// <summary>
        /// The left upper leg.
        /// </summary>
        LeftUpperLeg = 1 << 19,

        /// <summary>
        /// The right calf.
        /// </summary>
        RightCalf = 1 << 20,

        /// <summary>
        /// The left calf.
        /// </summary>
        LeftCalf = 1 << 21,

        /// <summary>
        /// The right ankle.
        /// </summary>
        RightAnkle = 1 << 22,

        /// <summary>
        /// The left ankle.
        /// </summary>
        LeftAnkle = 1 << 23,

        // 24 - 27 available.

        /// <summary>
        /// Custom mount point A.
        /// </summary>
        CustomA = 1 << 28,

        /// <summary>
        /// Custom mount point B.
        /// </summary>
        CustomB = 1 << 29,

        /// <summary>
        /// Custom mount point C.
        /// </summary>
        CustomC = 1 << 30,

        /// <summary>
        /// Custom mount point C.
        /// </summary>
        CustomD = 1 << 31
    }
}
