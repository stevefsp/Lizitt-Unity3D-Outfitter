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
    /// Mount point types.
    /// </summary>
    public enum MountPointType
    {
        /* 
         * Design notes:
         * 
         * This class can be customized to meet the needs of the target project.  Just make sure you understand 
         * all notes before making changes. 
         * 
         * The meaning of the names can be repurposed with the exception of 'root'.  Do not remove 'root' or change 
         * its value.  All other names and values can be refactored and repurposed as desired. 
         * 
         * New name-value pairs can be added at any time. Unused pairs can be removed with no impact. Unused pairs 
         * other than 'root' can be removed with no impact. 
         * 
         * Be careful when removing pairs that have been used, or changing values that have been assigned to an 
         * asset or scene object.
         * 
         */

        // Central  ////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The outfit root.
        /// </summary>
        Root = 0,           // Required.  Do not alter the meaning, remove, or change the value.

        /// <summary>
        /// The head.
        /// </summary>
        Head = 1,

        /// <summary>
        /// The eyes.
        /// </summary>
        Eyes = 2,

        /// <summary>
        /// The chest. (Upper front torso.)
        /// </summary>
        Chest = 3,

        /// <summary>
        /// The abdomen. (Lower front torso.)
        /// </summary>
        Abdomen = 4,

        /// <summary>
        /// The upper back torso.
        /// </summary>
        UpperBack = 5,

        /// <summary>
        /// The lower back torso.
        /// </summary>
        LowerBack = 6,
        
        // Right ///////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The right shoulder.
        /// </summary>
        RightShoulder = 7,

        /// <summary>
        /// The right hand/wrist.
        /// </summary>
        RightHand = 8,

        /// <summary>
        /// The right hip.
        /// </summary>
        RightHip = 9,

        /// <summary>
        /// The right ankle/foot.
        /// </summary>
        RightFoot = 10,

        // Left ////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The left shoulder.
        /// </summary>
        LeftShoulder = 11,

        /// <summary>
        /// The left wrist/hand.
        /// </summary>
        LeftHand = 12,

        /// <summary>
        /// The left hip.
        /// </summary>
        LeftHip = 13,

        /// <summary>
        /// The left ankle/foot.
        /// </summary>
        LeftFoot = 14,

        // Custom //////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Custom location A.
        /// </summary>
        CustomA = 1001,

        /// <summary>
        /// Custom location B.
        /// </summary>
        CustomB = 1002,

        /// <summary>
        /// Custom location C.
        /// </summary>
        CustomC = 1003,

        /// <summary>
        /// Custom location D.
        /// </summary>
        CustomD = 1004,
    }
}