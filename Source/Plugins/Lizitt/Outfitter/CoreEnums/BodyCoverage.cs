/*
 * Copyright (c) 2015-2016 Stephen A. Pratt
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
        /* 
         * Design notes:
         * 
         * This class can be customized to meet the needs of the target project.  Just make sure you understand 
         * all notes before making changes. 
         * 
         * New name-value pairs can be added at any time. (Up to 32 total.)  Unused pairs can be removed with no impact. 
         * 
         * The meaning of names can be repurposed and refactored as desired.
         * 
         * Be careful when removing pairs that have been used, or changing values that have been assigned to an 
         * asset or scene object.
         * 
         */

        /// <summary>
        /// The top and back of the head.
        /// </summary>
        Hair             = 1 << 0,

        /// <summary>
        /// The face, except for the eyes.
        /// </summary>
        Face                = 1 << 1,

        /// <summary>
        /// The right eye.
        /// </summary>
        RightEye            = 1 << 2,

        /// <summary>
        /// The left eye.
        /// </summary>
        LeftEye             = 1 << 3,

        /// <summary>
        /// The neck.
        /// </summary>
        Neck                = 1 << 4,

        /// <summary>
        /// The chest. (Upper front torso.)
        /// </summary>
        Chest               = 1 << 5,

        /// <summary>
        /// The abdomen.  (Lower front torso.)
        /// </summary>
        Abdomen             = 1 << 6,

        /// <summary>
        /// The upper back torso.
        /// </summary>
        UpperBack           = 1 << 7,

        /// <summary>
        /// The lower back torso.
        /// </summary>
        LowerBack           = 1 << 8,

        /// <summary>
        /// The right shoulder.
        /// </summary>
        RightShoulder       = 1 << 9,

        /// <summary>
        /// The left shoulder.
        /// </summary>
        LeftShoulder        = 1 << 10,

        /// <summary>
        /// The right upper arm.
        /// </summary>
        RightUpperArm       = 1 << 11,

        /// <summary>
        /// The left upper arm.
        /// </summary>
        LeftUpperArm        = 1 << 12,

        /// <summary>
        /// The right forearm.
        /// </summary>
        RightLowerArm       = 1 << 13,

        /// <summary>
        /// The left forearm.
        /// </summary>
        LeftLowerArm        = 1 << 14,

        /// <summary>
        /// The right wrist.
        /// </summary>
        RightWrist          = 1 << 15,

        /// <summary>
        /// The left wrist.
        /// </summary>
        LeftWrist           = 1 << 16,

        /// <summary>
        /// The right hand.
        /// </summary>
        RightHand           = 1 << 17,

        /// <summary>
        /// The left hand.
        /// </summary>
        LeftHand            = 1 << 18,

        /// <summary>
        /// The right upper leg.
        /// </summary>
        RightUpperLeg       = 1 << 19,

        /// <summary>
        /// The left upper leg.
        /// </summary>
        LeftUpperLeg        = 1 << 20,

        /// <summary>
        /// The right calf.
        /// </summary>
        RightLowerLeg       = 1 << 21,

        /// <summary>
        /// The left calf.
        /// </summary>
        LeftLowerLeg        = 1 << 22,

        /// <summary>
        /// The right ankle.
        /// </summary>
        RightAnkle          = 1 << 23,

        /// <summary>
        /// The left ankle.
        /// </summary>
        LeftAnkle           = 1 << 24,

        /// <summary>
        /// The right foot.
        /// </summary>
        RightFoot           = 1 << 25,

        /// <summary>
        /// The left foot.
        /// </summary>
        LeftFoot            = 1 << 26,

        // Values avaialbe: 27 /////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Custom coverage A.
        /// </summary>
        CustomA             = 1 << 28,

        /// <summary>
        /// Custom coverage B.
        /// </summary>
        CustomB             = 1 << 29,

        /// <summary>
        /// Custom coverage C.
        /// </summary>
        CustomC             = 1 << 30,

        /// <summary>
        /// Custom coverage D.
        /// </summary>
        CustomD             = 1 << 31
    }
}
