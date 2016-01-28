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
namespace com.lizitt.outfitter
{
    /// <summary>
    /// Body part types.
    /// </summary>
    public enum BodyPartType
    {
        /* 
         * Design notes:
         * 
         * Make sure you understand all notes before refactoring this element. 
         * 
         * New types can be added at any time.
         * 
         * The names and values have been synchronized with the equivalent bones defined in 
         * UnityEngine.HumanBodyBones for convenience.  None of the core outfitter code depends 
         * on this.
         * 
         * The meaning of the types can be repurposed and the names refactored as desired.
         * 
         * Unused/Unassigned types can be removed with no impact. 
         * 
         * Be careful when removing or changing the value of a type that has 
         * been assigned to an asset or scene object.  The assignment may become unusable at 
         * run-time or it may be forced to a default value the next time it is accessed in the 
         * editor.
         * 
         */

        /// <summary>
        /// The hips/waist.  (Lower torso.)
        /// </summary>
        Hips             = 0,

        /// <summary>
        /// The left upper leg.
        /// </summary>
        LeftUpperLeg     = 1,

        /// <summary>
        /// The right upper leg.
        /// </summary>
        RightUpperLeg    = 2,

        /// <summary>
        /// The left lower leg.
        /// </summary>
        LeftLowerLeg     = 3,

        /// <summary>
        /// The right lower leg.
        /// </summary>
        RightLowerLeg    = 4,

        /// <summary>
        /// The left foot.
        /// </summary>
        LeftFoot         = 5,

        /// <summary>
        /// The right foot.
        /// </summary>
        RightFoot        = 6,

        /// <summary>
        /// The spine.  (Between the hips and chest.  Mid-torso.)
        /// </summary>
        Spine            = 7,

        /// <summary>
        /// The chest.  (Upper torso.)
        /// </summary>
        Chest            = 8,

        /// <summary>
        /// The neck.
        /// </summary>
        Neck             = 9,

        /// <summary>
        /// The head.
        /// </summary>
        Head             = 10,

        /// <summary>
        /// The left shoulder.
        /// </summary>
        LeftShoulder     = 11,

        /// <summary>
        /// The right shoulder.
        /// </summary>
        RightShoulder    = 12,

        /// <summary>
        /// The left upper arm.
        /// </summary>
        LeftUpperArm     = 13,

        /// <summary>
        /// The right upper arm.
        /// </summary>
        RightUpperArm    = 14,

        /// <summary>
        /// The left lower arm.
        /// </summary>
        LeftLowerArm     = 15,

        /// <summary>
        /// The right lower arm.
        /// </summary>
        RightLowerArm    = 16,

        /// <summary>
        /// The left hand.
        /// </summary>
        LeftHand         = 17,

        /// <summary>
        /// The right hand.
        /// </summary>
        RightHand        = 18,
    }
}