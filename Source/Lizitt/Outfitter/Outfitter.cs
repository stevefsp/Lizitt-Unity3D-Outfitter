﻿/*
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
using System.Collections.Generic;
using UnityEngine;

namespace com.lizitt.outfitter
{
    /// <summary>
    /// A component that is used to manage outfit changes for a <see cref="OutfitterBody"/>.
    /// </summary>
    [SelectionBase]
    public class Outfitter
        : MonoBehaviour
    {
        #region Constant and Static Members

        /// <summary>
        /// The suffix used to identify baked bodys & meshes.
        /// </summary>
        public const string BakeSuffix = "_BAKED";

        /// <summary>
        /// The suffix used to identify the root of the outfit.
        /// </summary>
        public const string BodySuffix = "_Body";

        /// <summary>
        /// Creates an outfit of the specified type for external use.
        /// </summary>
        /// <remarks>
        /// <para>
        /// All current body settings are applied, with new accessories.  The outfit is 
        /// positioned and oriented to match the current outfit.
        /// </para>
        /// </remarks>
        /// <param name="outfitter">The outfitter containing the outfit configuration.</param>
        /// <param name="type">The type of outfit to create. (All allowed except 'None'.)</param>
        /// <returns>The newly created outfit.</returns>
        public static BodyOutfit CreateOutfit(Outfitter outfitter, OutfitType type)
        {
            return CreateOutfit(outfitter, type, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// <para>Does not do any positioning.  So the outfit will likely be at the origin.</para>
        /// </remarks>
        /// <param name="outfitter"></param>
        /// <param name="type"></param>
        /// <param name="isExternal">Is the new outfit meant for external use.  (A duplicate.)</param>
        /// <returns></returns>
        private static BodyOutfit CreateOutfit(Outfitter outfitter, OutfitType type
            , bool isExternal)
        {
            if (type == OutfitType.None)
                throw new System.ArgumentException("Can't create a 'None' type outfit.");

            var outfit = outfitter.m_Outfits.GetOutfitOrDefault(type).Instantiate();
            outfit.name = outfitter.name + BodySuffix;
            outfit.Initialize();

            if (isExternal)
            {
                var body = outfitter.m_Body
                    ? outfitter.m_Body
                    : outfitter.GetComponentInChildren<OutfitterBody>();

                if (body)
                    body.ApplyTo(outfit, true);
                else
                {
                    Debug.LogWarning("Outfitter does not have a body.  No body overrides to apply.",
                        outfitter);
                }
            }

            return outfit;
        }

        #endregion

        #region Fields

        [Header("Outfit Settings")]

        [SerializeField]
        private OutfitGroup m_Outfits = null;

        [Header("Body Settings")]

        [SerializeField]
        [Tooltip("The body to apply the outfit changes to. (If null, will default to the"
            + "first body found in a child search.")]
        private OutfitterBody m_Body = null;

        private OutfitType m_CurrentOutfitType = OutfitType.None;

        /// <summary>
        /// The source of the currently applied outfit has changed. 
        /// (So if the same type if re-applied, it will result in an outfit change.)
        /// </summary>
        private bool m_CurrentOutfitChanged = false;

        private bool m_IsInitialized = false;

        #endregion

        #region Initialization

        void Awake()
        {
            m_Body = m_Body ? m_Body : GetComponentInChildren<OutfitterBody>();

            if (!m_Body)
            {
                Debug.LogError("Body is not assigned.", this);
                Destroy(this);
            }
        }

        void Start()
        {
            CheckInitialized();
        }

        private void CheckInitialized()
        {
            if (!m_IsInitialized)
            {
                m_IsInitialized = true;  // Failure should prevent future initializations.
                DeletePlaceholder();
                ApplyOutfit(m_Outfits.StartOutfit);
            }
        }

        #endregion

        #region Properties & Indexer

        /// <summary>
        /// Gets the prototype for the specified outfit, or null if the outfit is not defined.
        /// </summary>
        /// <param name="type">The outfit type.</param>
        /// <returns>The prototype for the specified outfit, or null if the outfit is not defined.</returns>
        public BodyOutfit this[OutfitType type]
        {
            get { return m_Outfits[type]; }
        }

        /// <summary>
        /// The body the outfit changes is applied to.
        /// </summary>
        public OutfitterBody Body
        {
            get { return m_Body; }
        }

        /// <summary>
        /// The current outfit type.
        /// </summary>
        public OutfitType CurrentOutfit
        {
            get { return m_CurrentOutfitType; }
        }

        /// <summary>
        /// The default outfit type.
        /// </summary>
        public OutfitType DefaultOutfit
        {
            get { return m_Outfits.DefaultOutfit; }
        }

        /// <summary>
        /// The outfit applied at component start.
        /// </summary>
        public OutfitType StartOutfit
        {
            get { return m_Outfits.StartOutfit; }
        }

        #endregion

        #region Outfit Management

        /// <summary>
        /// Gets the prototype for the specified outfit type, or the default prototype 
        /// if the specified outfit is not assigned.
        /// </summary>
        /// <param name="type">The outfit type.</param>
        /// <returns>
        /// The prototype for the specified outfit type, or the default prototype if the 
        /// specified outfit is not assigned.
        /// </returns>
        public BodyOutfit GetOutfitOrDefault(OutfitType type)
        {
            return m_Outfits.GetOutfitOrDefault(type);
        }

        /// <summary>
        /// Apply the outfit. (The old outfit is destroyed.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// The default behavior is for no action to be taken if the current outfit type
        /// is the same as the requested outfit type.
        /// </para>
        /// <para>
        /// <paramref name="changeCallback"/> can be used to perform extra preparations such 
        /// as animator synchronziation before the new outfit is applied to the body.  It will
        /// only be called if a change is actually triggered.  Also, depending on the type of 
        /// transition, one of the its parameters may be null.
        /// </para>
        /// </remarks>
        /// <param name="type">The outfit type to apply.</param>
        /// <param name="force">Force the outfit to be applied, even if the current outfit type
        /// is the same as the requested outfit type.</param>
        /// <param name="changeCallback">
        /// A method to be called just before the outfit change is applied to the body.
        /// </param>
        public void ApplyOutfit(OutfitType type, bool force = false, OutfitChange changeCallback = null)
        {
            CheckInitialized();

            force = force || m_CurrentOutfitChanged;

            if (type == OutfitType.None && !m_Body.HasOutfit)
            {
                // Already in the 'None' state.  Force doesn't apply to this case.
                return;
            }

            // These next checks are important. Want clients to be able to call apply as often as 
            // desired without incurring unnecessary costs. 

            if (!force && m_Outfits.GetRealType(type)
                == m_Outfits.GetRealType(m_CurrentOutfitType))
            {
                // Even though the outfit doesn't need to change, the type may have changed.
                m_CurrentOutfitType = type;
                return;
            }

            BodyOutfit oldOutfit;

            // Design note: The callback is happening before the change is applied to the
            // body due to event timing issues.  The callback may change things the
            // body needs to detect.  E.g. Animator change events.

            if (type == OutfitType.None)
            {
                if (changeCallback != null)
                    changeCallback(m_Body.Outfit, null);  // See design note above.

                OnApplyOutfitPre(m_Body.Outfit, null);
                oldOutfit = m_Body.ClearOutfit();
            }
            else
            {
                var noutfit = CreateOutfit(this, type, false);

                // Parent and snap the outfit root to the outfitter for consistancy.  The body will
                // take care of altering this behavior as appropriate.
                noutfit.transform.parent = transform;
                noutfit.transform.position = transform.position;
                noutfit.transform.rotation = transform.rotation;

                if (changeCallback != null)
                    changeCallback(m_Body.Outfit, noutfit);  // See design note above.

                OnApplyOutfitPre(m_Body.Outfit, noutfit);
                oldOutfit = m_Body.SetOutfit(noutfit);
            }

            if (oldOutfit)
                oldOutfit.gameObject.SafeDestroy();

            m_CurrentOutfitType = type;
            m_CurrentOutfitChanged = false;
        }

        /// <summary>
        /// Called just before a new outfit is applied to the body.
        /// </summary>
        /// <param name="oldOutfit">The body's current outfit. (May be null.)</param>
        /// <param name="newOutfit">The new outfit to be applied. (May be null.)</param>
        protected virtual void OnApplyOutfitPre(BodyOutfit oldOutfit, BodyOutfit newOutfit)
        {
            return;
        }

        /// <summary>
        /// Assigns the prototype to the specified outfit slot.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the prototype of the currently applied outfit is being set, the change will not
        /// be applied. Use <see cref="ApplyOutfit"/> to apply the change.
        /// </para>
        /// <para>
        /// The outfitter tracks changes to the prototypes via this method.  So
        /// <see cref="ApplyOutfit"/>'s force parameter is not needed.
        /// </para>
        /// </remarks>
        /// <param name="type">
        /// The outfit type to set. [Limits: Any except <see cref="OutfitType.None"/>.
        /// </param>
        /// <param name="prototype">The new outfit prototype. (Null allowed.)</param>
        public void SetPrototype(OutfitType type, BodyOutfit prototype)
        {
            if (type == OutfitType.None)
            {
                Debug.LogWarning("Can't set the 'None' outfit type.", this);
                return;
            }

            m_Outfits[type] = prototype;

            if (type == m_CurrentOutfitType)
                m_CurrentOutfitChanged = true;
        }

        #endregion

        #region Baking and Placeholders

        /// <summary>
        /// Bakes the outfit, purging all outfit related components.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If in editor mode, the default outfit will be created in its bind pose, parented to
        /// the outfitter.  This is safe since the outfitter will automatically delete all 
        /// child object's with the baked suffix when it starts in play mode.
        /// </para>
        /// <para>
        /// If in play mode, the current outfit will be duplicated in its current animated pose.
        /// </para>
        /// </remarks>
        /// <returns>A baked version of the outfitter's outfit.</returns>
        public GameObject BakePlaceholder(bool removeColliders)
        {
            GameObject bakedGo;

            if (Application.isPlaying)
            {
                if (!m_Body.HasOutfit)
                {
                    Debug.LogWarning("Body does not have an outfit to bake.");
                    return null;
                }

                bakedGo = LocalBakePlaceholder(m_Body.Outfit.Instantiate(), m_Body.Outfit);
            }
            else
            {
                if (StartOutfit == OutfitType.None)
                {
                    Debug.LogWarning("Can't bake the None outfit.");
                    return null;
                }

                // Note: Using the public version of CreateOutfit since it correctly positions the object.
                bakedGo = LocalBakePlaceholder(CreateOutfit(this, StartOutfit), null);
                bakedGo.transform.parent = transform;
            }

            if (removeColliders)
            {
                foreach (var col in bakedGo.GetComponentsInChildren<Collider>())
                {
                    var rb = col.GetComponent<Rigidbody>();
                    if (rb)
                        rb.SafeDestroy();

                    col.SafeDestroy();
                }
            }

            return bakedGo;
        }

        /// <summary>
        /// Delete any child placeholder objects. (Objects created by 
        /// <see cref="BakePlaceholder"/>.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// The search is performed based on <see cref="BakeSuffix"/>.
        /// </para>
        /// </remarks>
        public void DeletePlaceholder()
        {
            // Get rid of placeholder body if it exists.
            foreach (var trans in GetComponentsInChildren<Transform>(true))
            {
                if (trans && trans.name.EndsWith(BakeSuffix))
                {
                    foreach (var item in trans.GetComponentsInChildren<MeshFilter>())
                    {
                        var mesh = item.sharedMesh;
                        if (mesh && mesh.name.EndsWith(BakeSuffix))
                        {
                            // Mesh was baked from a skinned mesh.  Must destory of they will
                            // leak.
                            if (Application.isPlaying)
                                Destroy(mesh);
                            else
                                DestroyImmediate(mesh);
                        }
                    }

                    trans.gameObject.SafeDestroy();
                }
            }
        }

        private static GameObject LocalBakePlaceholder(BodyOutfit outfit, BodyOutfit source)
        {
            // Do not make this an overload of BakePlaceholder.  Outfit can be interpreted as a bool.
            // So this would become BakePalceholder(bool, bool) which can mess things up.  You can
            // get recusion in the public method during refactoring.

            GameObject go = outfit.gameObject;

            outfit.PurgeOutfitComponents();
            outfit.SafeDestroy();

            Dictionary<string, GameObject> outfitRenderObjects;

            if (source)
            {
                // There are two main issues that need to be addressed when an outfit has a source.
                // A newly instantiated outfit starts in its bind pose.  So have to bake the
                // skinned meshes from the source outfit to the target outfit.
                // Also, render enabled state may be lost, especially for some accessories.  So
                // need to copy that state from source to target as well.
                // See notes in BodyAccessory.  Need to come up with a better solutio for
                // copying state.

                go.name = source.name;  // Important since the root may have a skinned mesh.

                outfitRenderObjects = new Dictionary<string, GameObject>();
                foreach (var renderer in go.GetComponentsInChildren<Renderer>())
                {
                    outfitRenderObjects.Add(renderer.name, renderer.gameObject);

                    if (renderer is SkinnedMeshRenderer)
                        renderer.SafeDestroy();
                }

                foreach (var renderer in source.GetComponentsInChildren<Renderer>())
                {
                    var outfitRenderObject = outfitRenderObjects[renderer.name];

                    var smr = renderer as SkinnedMeshRenderer;

                    if (smr)
                    {
                        // Bake from source.
                        var mesh = new Mesh();
                        mesh.name = smr.sharedMesh.name + BakeSuffix;
                        smr.BakeMesh(mesh);

                        // Transfer to outfit.

                        outfitRenderObject.AddComponent<MeshFilter>().mesh = mesh;

                        var outfitRenderer = outfitRenderObject.AddComponent<MeshRenderer>();
                        outfitRenderer.sharedMaterials = renderer.sharedMaterials;
                        outfitRenderer.enabled = smr.enabled;

                    }
                    else
                    {
                        var outfitRenderer = outfitRenderObject.GetComponent<Renderer>();
                        outfitRenderer.enabled = renderer.enabled;
                    }
                }
            }
            else
            {
                // There is no source, so will bake bind poses.
                foreach (var renderer in go.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    var mesh = new Mesh();
                    mesh.name = renderer.sharedMesh.name + BakeSuffix;
                    renderer.BakeMesh(mesh);

                    var rgo = renderer.gameObject;
                    rgo.AddComponent<MeshFilter>().mesh = mesh;
                    rgo.AddComponent<MeshRenderer>().sharedMaterials = renderer.sharedMaterials;
                    renderer.SafeDestroy();
                }
            }

            foreach (var anim in go.GetComponentsInChildren<Animator>())
                anim.SafeDestroy();

            go.StripCloneName();
            go.name += BakeSuffix;

            return go;
        }

        #endregion
    }
}
