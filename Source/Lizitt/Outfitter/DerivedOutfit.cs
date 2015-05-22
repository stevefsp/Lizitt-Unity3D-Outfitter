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
using UnityEngine.Serialization;
using System.Collections.Generic;

namespace com.lizitt.outfitter
{
    /// <summary>
    /// An outfit whose configuration is based on a prefab with various settings derived
    /// and applied after the prefab is instantiated.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The prototype is instantiated then the various settings are used to finalize the
    /// construction of the outfit.
    /// </para>
    /// </remarks>
    [SelectionBase]
    public class DerivedOutfit
        : StandardOutfit
    {
        #region Unity Editor Fields

        [Header("Main Settings")]

        [SerializeField]
        [Tooltip("The outfit prototype.")]
        [FormerlySerializedAs("m_BodyPrototype")]
        private GameObject m_Prototype = null;

        [SerializeField]
        [Tooltip("The terms used to identify the standard outfit components after instantiation.")]
        private OutfitSearchTerms m_OutfitTerms = null;

        [SerializeField]
        [Tooltip("The body collider settings.")]
        private BodyColliderSettings m_ColliderSettings = null;

        [Header("Variant Settings")]

        [SerializeField]
        [Tooltip("The material overrides for the outfit.  (If applicable.)")]
        private MaterialOverride[] m_MaterialOverrides = new MaterialOverride[0];

        [Space(8)]

        [SerializeField]
        [Tooltip("Accessories to add to the base outfit. (Exempt from coverage restrictions.)")]
        private AccessoryGroup m_Acessories = new AccessoryGroup(0);

        [SerializeField]
        [Tooltip("The body parts that are considered permanently blocked. (No new accessories.)")]
        [EnumFlags(typeof(BodyCoverage), OutfitterUtil.SortBodyCoverage)]
        private BodyCoverage m_CoverageBlocks = 0;

        [SerializeField]
        [Tooltip("If true, only accessories marked to ignore this flag will attach."
            + " (The accessories listed above are exempt from this flag.)")]
        private bool m_AccessoriesLimited = false;

        #endregion

        #region Mount Points & Accessories

        public sealed override bool AccessoriesLimited
        {
            get { return m_AccessoriesLimited; }
        }

        public sealed override BodyCoverage CoverageBlocks
        {
            get { return m_CoverageBlocks; }
        }

        #endregion

        #region Initialization

        protected sealed override GameObject GetOutfitInfo(out StandardOutfit.CoreOutfitInfo info)
        {
            if (!m_Prototype)
            {
                Debug.LogError("Body prototype has not been assigned.", this);
                info = new CoreOutfitInfo();
                return null;
            }

            var result = (GameObject)Instantiate(m_Prototype);

            info = m_OutfitTerms
                ? m_OutfitTerms.DeriveStandardSettings(result)
                : new StandardOutfit.CoreOutfitInfo();

            if (m_ColliderSettings == null)
                info.colliders = new BodyCollider[0];
            else
                info.colliders = m_ColliderSettings.ApplyTo(info.mountPoints);

            if (m_MaterialOverrides.Length > 0)
            {
                var renderers = result.GetComponentsInChildren<Renderer>();

                foreach (var renderer in renderers)
                {
                    foreach (var matOverride in m_MaterialOverrides)
                    {
                        if (matOverride.Target.Renderer && matOverride.Target.Renderer.name == renderer.name)
                        {
                            RendererMaterialPtr.Apply(
                                matOverride.Material, renderer, matOverride.Target.MaterialIndex);
                            continue;  // << Might be more than one override for this renderer.
                        }
                    }
                }
            }

            var accessories = new List<BodyAccessory>(m_Acessories.Count);

            foreach (var prototype in m_Acessories)
            {
                if (!prototype)
                    // Empty slot.
                    continue;

                BodyAccessory accessory = null;
                foreach (var mountPoint in info.mountPoints)
                {
                    if (mountPoint.MountType == prototype.MountPoint)
                    {
                        accessory = prototype.Instantiate<BodyAccessory>();
                        accessory.StripCloneName();

                        var controller = accessory.TakeOwnership(this, null);

                        if (controller == null || !controller.Attach(mountPoint.Transform))
                        {
                            // Will happen on technical errors only.  So accessory should have
                            // sent a debug message.

                            accessory.SafeDestroy();
                            accessory = null;

                            continue;
                        }

                        accessories.Add(accessory);

                        // Even though the accessory will only ever self release, can't lock
                        // ownership.  Doing so would prevent proper baking of the outfit.  
                        // (I.e. Purging.)
                        accessory.ReleaseOwnership(this, true);

                        break;
                    }
                }

                if (!accessory)
                {
                    // This is an error because this is outfit construction.
                    Debug.LogError(
                        "No mount point found for accessory: " + prototype.gameObject.name, this);
                }
            }

            info.accessories = accessories;

            return result;
        }

        protected sealed override Collider CreateSurfaceCollider(Transform motionTransform)
        {
            if (m_ColliderSettings)
                return m_ColliderSettings.CreateSurfaceCollider(motionTransform);
            return null;
        }

        #endregion
    }
}
