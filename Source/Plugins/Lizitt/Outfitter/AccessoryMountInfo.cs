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
    /// Information about an accessory's current mount state and behavior.
    /// </summary>
    [System.Serializable]
    public struct AccessoryMountInfo
    {
        [SerializeField]
        [Tooltip("The accessory the information applies to.")]
        private Accessory m_Accessory;

        /// <summary>
        /// The accessory the information applies to.
        /// </summary>
        public Accessory Accessory
        {
            get { return m_Accessory; }
            set { m_Accessory = value; }
        }

        [SerializeField]
        [Tooltip("The current/desired location of the accessory accessory.")]
        private MountPointType m_LocationType;

        /// <summary>
        /// The current/desired location of the accessory accessory.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value does not necessarily indicate where the accessory is currently mounted.
        /// </para>
        /// </remarks>
        public MountPointType LocationType
        {
            get { return m_LocationType; }
            set { m_LocationType = value; }
        }

        [SerializeField]
        [Tooltip("The outfit the accessory is currently mounted to, or null if not mounted.")]
        private Outfit m_Outfit;

        /// <summary>
        /// The outfit the accessory is currently mounted to, or null if not mounted.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is the outfit the manager (e.g. the body) considers this to be the current owner 
        /// of the accessory.  A divergence between this value and the current accessory state 
        /// indicates a problem.
        /// </para>
        /// </remarks>
        public Outfit Outfit
        {
            get { return m_Outfit; }
            set { m_Outfit = value; }
        }

        [SerializeField]
        [Tooltip("The mounter that will be used as the priority mounter for all mount operations."
            + " (Including automatic transfer's between outfits.) (Optional)")]
        [RequireObjectType(typeof(IAccessoryMounter))]
        private Object m_Mounter;

        /// <summary>
        /// The mounter that will be used as the priority mounter for all mount operations.
        /// (Including automatic transfer's between outfits.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// Only Unity Objects will be accepted.  Attempting to set a non-Unity Object will result in null being
        /// assigned. (Along with an error message.)  A destroyed object reference will never be returned.  If
        /// the current object is destroyed, a true null reference will be returned.
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
                    Debug.LogError(value.GetType().Name + " is not a Unity Object.");
                    m_Mounter = null;
                }
            }
        }

        [SerializeField]
        [Tooltip("Additional coverage that will be applied when the accessory is mounted.")]
        private BodyCoverage m_AdditionalCoverage;

        /// <summary>
        /// Additional coverage that will be applied when the accessory is mounted.
        /// </summary>
        public BodyCoverage AdditionalCoverage
        {
            get { return m_AdditionalCoverage; }
            set { m_AdditionalCoverage = value; }
        }

        [SerializeField]
        [Tooltip("The restrictions flag to use when mounting the accessory.")]
        private bool m_IgnoreRestrictions;

        /// <summary>
        /// The restrictions flag to use when mounting to an outfit.
        /// </summary>
        public bool IgnoreRestrictions
        {
            get { return m_IgnoreRestrictions; }
            set { m_IgnoreRestrictions = value; }
        }

        /// <summary>
        /// Applies the specified settings.
        /// </summary>
        /// <param name="settings">The settings to apply.</param>
        public void Apply(AccessoryAddSettings settings)
        {
            m_IgnoreRestrictions = settings.IgnoreRestrictions;
            m_LocationType = settings.LocationType;
            Mounter = settings.Mounter;
            m_AdditionalCoverage = settings.AdditionalCoverage;
        }
    }
}