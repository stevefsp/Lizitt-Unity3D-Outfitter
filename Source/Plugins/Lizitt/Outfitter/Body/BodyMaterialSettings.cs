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
    /// Overrides or persists the outfit materials for a body.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Can be assigned as an observer to only a single body at a time while <see cref="PersistSettings"/> is true,
    /// otherwise can be an observer of any number of concurrent <see cref="Body"/> instances.
    /// </para>
    /// <para>
    /// There are two modes: Override and persist.  In override mode the component will apply the outfit 
    /// materials to all outfits as they are set.  Any changes to the outfits while they is assigned to the body are 
    /// ignored. In persist mode the changes to outfits while they are assigned to the body are fed back into the 
    /// component settings and applied to future outfits.
    /// </para>
    /// </remarks>
    [AddComponentMenu(LizittUtil.LizittMenu + "Body Material Settings", OutfitterUtil.BodyObserverMenuOrder)]
    public sealed class BodyMaterialSettings
        : BodyObserverBehaviour
    {
        #region Primary Settings

        [Space]

        [SerializeField]
        [Tooltip("Persist changes to outfit materials while the outfit is asigned to the body.  Otherwise simply"
            + " override incoming outfits with these fixed settings.")]
        private bool m_Persist = true;

        /// <summary>
        /// Persist material changes between outfits as they are swapped out, otherwise always overrides
        /// outfit materials with the current settings.
        /// </summary>
        /// <remarks>
        /// <para>
        /// With persistance enabled all outfit materials from the outgoing outfit will be synchronzied with
        /// the current settings in an additive manner.  Materials of exising types are matched to those
        /// in the outfit, and new types are added.  The result is that the settings always contains the last 
        /// know material for each material type.
        /// </para>
        /// <para>
        /// When persistance is disabled the setting will never change autoamtically. (Override mode)
        /// </para>
        /// </remarks>
        public bool PersistSettings
        {
            get { return m_Persist; }
            set { m_Persist = value; }
        }

        [Space]

        [SerializeField]
        [Tooltip("The materials to apply to body outfits.")]
        [OutfitMaterialGroup]
        private OutfitMaterialGroup m_Materials = new OutfitMaterialGroup();

        /// <summary>
        /// The materials to apply to body outfits.
        /// </summary>
        public OutfitMaterialGroup OutfitMaterials
        {
            get { return m_Materials; }
            // Only add a setter if it is truely needed.  It introduces reference sharing that is not serializable.
        }

        #endregion

        #region Body Observer

        protected override void OnOutfitChange(Body sender, Outfit previous, bool wasForced)
        {
            // Sync on force release.  Still appropriate to record available state.
            if (m_Persist && previous)
                m_Materials.SyncFrom(previous, true, false);

            if (sender.Outfit)
                m_Materials.ApplyTo(sender.Outfit);
        }

        #endregion

        #region Utility Members

        /// <summary>
        /// Apply the outfit materials to an arbitrary outfit.
        /// </summary>
        /// <param name="outfit">The outfit.</param>
        public int ApplyMaterials(Outfit outfit)
        {
            return m_Materials.ApplyTo(outfit);
        }

        #endregion
    }
}
