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
using UnityEngine;
using UnityEngine.Serialization;

namespace com.lizitt.outfitter
{
    /*
     * Design notes:
     * 
     * There is an inefficiency in the current design.  If both an outfit and a body have
     * overrides, each override is applied in a serial manner resulting in wasted effort and
     * resources at the outfit level.  E.g. The prototype's body material is overriden by the 
     * outfit, then the body overrides the outfit's body material.
     * 
     * But I'm not fixing this inefficiency at this time because I expect in the large majority
     * of use cases the designer will only use one of the override points, either the outfit
     * or the body.  So it likely isn't worth the added code complexity to get rid of this
     * inefficiency.
     */

    /// <summary>
    /// Standard material overrides for a body.
    /// </summary>
    [System.Serializable]
    public class BodyMaterialOverrides
    {
        [SerializeField]
        [Tooltip("The replacement body material. (If applicable.)")]
        [FormerlySerializedAs("m_BodyMaterial")]
        private Material m_Body = null;

        [SerializeField]
        [Tooltip("The replacement head material. (If applicable.)")]
        [FormerlySerializedAs("m_HeadMaterial")]
        private Material m_Head = null;

        [SerializeField]
        [Tooltip("The replacement eye material.  (If applicable.)")]
        [FormerlySerializedAs("m_EyeMaterial")]
        private Material m_Eye = null;

        /// <summary>
        /// The replacement head material. (If applicable.)
        /// </summary>
        public Material HeadMaterial
        {
            get { return m_Head; }
        }

        /// <summary>
        /// The replacement body material. (If applicable.)
        /// </summary>
        public Material BodyMaterial
        {
            get { return m_Body; }
        }

        /// <summary>
        /// The replacement eye material. (If applicable.)
        /// </summary>
        public Material EyeMaterial
        {
            get { return m_Eye; }
        }
    }
}