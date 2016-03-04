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
    /// Settings used to add an accessory to an accessory manager. (Such as a <see cref="Body"/>.)
    /// </summary>
    public struct AccessoryAddSettings 
    {
        /// <summary>
        /// The mount target for the accessory.
        /// </summary>
        public MountPointType LocationType { get; set; }

        /// <summary>
        /// If true, ignore 'limited accessory' and coverage restrictions.  (Other restictions may still apply.)
        /// </summary>
        public bool IgnoreRestrictions { get; set; }

        private Object m_Mounter;

        /// <summary>
        /// The mounter that will be used as the priority mounter for all mount operations. (Including automatic 
        /// transfer's between outfits.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// Only UnityEngine.Object's will be accepted.  Attempting to set a non-UnityEngine.Object will result in
        /// null being assigned. (Along with an error message.)  A destroyed object reference will never be returned.  
        /// A true null reference will be returned if the current object has been destroyed.
        /// </para>
        /// </remarks>
        public IAccessoryMounter Mounter
        {
            get { return m_Mounter ? m_Mounter as IAccessoryMounter : null; }
            set
            {
                if (value == null)
                    m_Mounter = null;
                else if (value is Object)
                {
                    var obj = value as Object;
                    m_Mounter = obj ? obj : null;
                }
                else
                {
                    Debug.LogError(value.GetType().Name + " is not a UnityEngine.Object.");
                    m_Mounter = null;
                }
            }
        }

        /// <summary>
        /// Coverage in addition to the mounter's coverage.
        /// </summary>
        public BodyCoverage AdditionalCoverage { get; set; }

        /// <summary>
        /// Applies the <see cref="AccessoryMountInfo"/> values to the settings.
        /// </summary>
        /// <param name="settings">The infomration values to apply.</param>
        public void Apply(AccessoryMountInfo info)
        {
            LocationType = info.LocationType;
            IgnoreRestrictions = info.IgnoreRestrictions;
            Mounter = info.Mounter;  // Use property.
            AdditionalCoverage = info.AdditionalCoverage;
        }
    }
}

