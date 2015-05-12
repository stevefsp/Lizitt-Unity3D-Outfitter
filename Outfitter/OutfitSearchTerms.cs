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
using System.Collections.Generic;
using UnityEngine;

namespace com.lizitt.outfitter
{
    /// <summary>
    /// Terms used when searching an object for standard outfit components
    /// </summary>
    public class OutfitSearchTerms
        : ScriptableObject
    {
        [System.Serializable]
        private class BodyMountPointTerm
        {
            [SerializeField]
            [Tooltip("The mount point the transform represents.")]
            private MountPointType m_MountType = MountPointType.Root;

            [SerializeField]
            [Tooltip("The term to use when searching for the mount point transform.)")]
            private string m_Term = "";

            public MountPointType MountType
            {
                get { return m_MountType; }
            }

            public bool IsMatch(Transform trans)
            {
                return (trans && trans.name.ToLower().Contains(m_Term.ToLower()));
            }
        }

        [Header("Mount Points")]

        [SerializeField]
        private BodyMountPointTerm[] m_MountTerms = new BodyMountPointTerm[0];

        [Header("Material Terms")]

        [SerializeField]
        [Tooltip("The search term used to locate the head material. (If applicable.)")]
        private string m_HeadTerm = null;

        [SerializeField]
        [Tooltip("The search term used to locate the eyes material. (If applicable.)")]
        private string m_EyeTerm = null;

        [SerializeField]
        [Tooltip("The search term used to locate the body material. (If applicable.)")]
        private string m_BodyTerm = null;

        [Header("Other Terms")]

        [SerializeField]
        [Tooltip("The search term used to locate the renderer that contains the head blendshapes."
            + " (If applicable.)")]
        private string m_BlendHeadTerm = null;

        private bool IsBlendHead(Renderer renderer)
        {
            if (renderer.name.ToLower().Contains(m_BlendHeadTerm.ToLower())
                && (renderer is SkinnedMeshRenderer)
                && ((SkinnedMeshRenderer)renderer).sharedMesh.blendShapeCount > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Attemps to derive outfit settings for the specified object.
        /// </summary>
        /// <param name="rootObject">The root of the component to search.</param>
        /// <returns>The settings derived from <see cref="rootObject"/></returns>
        public StandardOutfit.CoreOutfitInfo DeriveStandardSettings(GameObject rootObject)
        {
            // TODO: EVAL: Sloppy and not generalized.  Worth upgrading?

            var info = new StandardOutfit.CoreOutfitInfo();

            info.mountPoints = FindMountPoints(rootObject);

            foreach (var renderer in rootObject.GetComponentsInChildren<Renderer>())
            {
                if (!info.blendHead && IsBlendHead(renderer))
                    info.blendHead = (SkinnedMeshRenderer)renderer;

                var mats = renderer.sharedMaterials;

                for (int i = 0; i < mats.Length; i++)
                {
                    var matName = mats[i].name.ToLower();
                    if (info.headMaterial == null && matName.Contains(m_HeadTerm.ToLower()))
                        info.headMaterial = new RendererMaterialPtr(renderer, i);
                    else if (info.eyeMaterial == null && matName.Contains(m_EyeTerm.ToLower()))
                        info.eyeMaterial = new RendererMaterialPtr(renderer, i);
                    else if (info.bodyMaterial == null && matName.Contains(m_BodyTerm.ToLower()))
                        info.bodyMaterial = new RendererMaterialPtr(renderer, i);
                }

                if (!(info.headMaterial == null || info.eyeMaterial == null
                    || info.bodyMaterial == null))
                {
                    break;
                }
            }

            if (info.headMaterial == null)
                info.headMaterial = new RendererMaterialPtr();

            if (info.eyeMaterial == null)
                info.eyeMaterial = new RendererMaterialPtr();

            if (info.bodyMaterial == null)
                info.bodyMaterial = new RendererMaterialPtr();

            return info;
        }

        /// <summary>
        /// Finds all the mountpoints at or below the provided root, based on the mount point search
        /// terms.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The process allows for a single transform to be assigned to multiple mount types. Behavior 
        /// is undefined if a single transform matches multiple terms.
        /// </para>
        /// <para>
        /// Always includes a root mount point.  If no term is found and matched then the root object
        /// is assigned as the root mount point.
        /// </para>
        /// </remarks>
        /// <param name="rootObject">The component to search.</param>
        /// <returns>The discovered mount points, or a zero length array if none were found.</returns>
        private MountPoint[] FindMountPoints(GameObject rootObject)
        {
            var mountPoints = new List<MountPoint>();

            bool foundRoot = false;
            foreach (var trans in rootObject.GetComponentsInChildren<Transform>())
            {
                foreach (var term in m_MountTerms)
                {
                    if (term.IsMatch(trans))
                    {
                        mountPoints.Add(new MountPoint(term.MountType, trans));
                        if (term.MountType == MountPointType.Root)
                            foundRoot = true;
                    }
                }
            }

            if (!foundRoot)
                mountPoints.Add(new MountPoint(MountPointType.Root, rootObject.transform));

            return mountPoints.ToArray();
        }
    }
}
