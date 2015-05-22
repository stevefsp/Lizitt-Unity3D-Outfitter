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
using com.lizitt.u3d;
using UnityEngine;

namespace com.lizitt.outfitter
{
    /// <summary>
    /// A outfitter body with common settings and behavior.
    /// </summary>
    public class StandardBody
        : OutfitterBody
    {
        /*
         * Design notes:
         * 
         * Coliders:
         * 
         * The standard outfit creation process assigns collider layers via the collider 
         * settings asset. This body always overrides those values.  I've gone back
         * and forth on this, but my current opinion is that it is appropriate that the 
         * body maintains the layer state and syncs its state to new outfits when they
         * are applied. I can't think of a use case where collider state should be 
         * outfit-centric rather than body-centric, and making the body overrides optional
         * adds complexity for no real benefit.  However, the body collider settings
         * should still contain layer information for purposes of completeness. (Collider
         * settings without layer settings doesn't seem appropriate.)  The downside
         * is that designers my get confused.  (Ahhh!!! Where are my outfit layers getting 
         * overriden!!!)
         * 
         */

        #region Unity Editor Fields

        [Header("General Settings")]

        [SerializeField]
        [Tooltip("Used as the motion root when there is no outfit, and for storage of inactive"
            + " accessories.  If not assigned, the body's transform will be used.")]
        private Transform m_DefaultMotionRoot = null;

        [SerializeField]
        [Tooltip("The layer of the surface collider. (Overrides outfit settings.)")]
        [UnityLayer]
        private int m_SurfaceColliderLayer = UnityLayer.Default;  // See design notes.

        [SerializeField]
        [Tooltip("The layer of the body colliders. (Overrides outfit settings.)")]
        [UnityLayer]
        private int m_BodyColliderLayer = UnityLayer.Default; // See design notes.

        [SerializeField]
        [Tooltip("The default status of the body colliders.")]
        private BodyColliderStatus m_BodyColliderStatus = BodyColliderStatus.Disabled;

        [Header("Overrides & Accessories")]

        [SerializeField]
        [Tooltip("The material overrides for the body.")]
        private BodyMaterialOverrides m_MaterialOverrides = new BodyMaterialOverrides();

        [SerializeField]
        [Tooltip("The accessories to automatically attach to compatible outfits.")]
        private BodyAccessory[] m_Accessories = new BodyAccessory[0];

        #endregion

        #region Initialization

        protected override OutfitterBody.Info GetInfo(bool instantiate)
        {
            var info = new OutfitterBody.Info();

            info.accessories = new BodyAccessory[m_Accessories.Length];

            for (int i = 0; i < m_Accessories.Length; i++)
            {
                if (!m_Accessories[i])
                    continue;

                if (instantiate)
                {
                    info.accessories[i] = m_Accessories[i].Instantiate();
                    info.accessories[i].StripCloneName();
                }
                else
                    info.accessories[i] = m_Accessories[i];
            }

            info.bodyColliderLayer = m_BodyColliderLayer;
            info.bodyColliderStatus = m_BodyColliderStatus;
            info.materialOverrides = m_MaterialOverrides;
            info.surfaceColliderLayer = m_SurfaceColliderLayer;

            // Note: The base class decides whether or not to accept a null root motion.
            info.defaultMotionRoot = m_DefaultMotionRoot;

            return info;
        }

        #endregion
    }
}

