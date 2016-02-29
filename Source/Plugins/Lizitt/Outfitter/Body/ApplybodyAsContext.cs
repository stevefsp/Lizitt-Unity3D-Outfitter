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
    /// Set the context of all of the outfit's components to the body's GameObject, and clears them when the outfit is 
    /// released.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Can be an observer of any number of concurrent <see cref="Body"/> instances.
    /// </para>
    /// </remarks>
    [CreateAssetMenu(menuName = LizittUtil.LizittMenu + "Apply Body As Context",
        order = OutfitterUtil.BodyObserverMenuOrder + 2)]
    public class ApplyBodyAsContext
        : ApplyBodyOutfitContext
    {
        #region Settings

        [SerializeField]
        [Tooltip("Include the context of body parts.")]
        private bool m_IncludeBodyParts = true;

        /// <summary>
        /// Include the context of body parts.
        /// </summary>
        public bool IncludeBodyParts
        {
            get { return m_IncludeBodyParts; }
            set { m_IncludeBodyParts = value; }
        }

        [SerializeField]
        [Tooltip("Include the context of mount points.")]
        private bool m_IncludeMountPoints = true;

        /// <summary>
        /// Include the context of mount points.
        /// </summary>
        public bool IncludeMountPoints
        {
            get { return m_IncludeMountPoints; }
            set { m_IncludeMountPoints = value; }
        }

        #endregion

        #region Overrides

        public override GameObject GetContext(Body sender, Outfit previous)
        {
            return sender.gameObject;
        }

        public override void ApplyContext(Outfit outfit, GameObject context)
        {
            ProcessBodyParts(outfit, context);
            ProcessMountPoints(outfit, context);
        }

        public override void ClearContext(Outfit outfit, bool wasForced)
        {
            // Always clear, even if forced.
            ProcessBodyParts(outfit, null);  // Null is correct.  All context's have been set to the body.
            ProcessMountPoints(outfit, null);
        }

        #endregion

        #region Behavior

        private void ProcessBodyParts(Outfit outfit, GameObject context)
        {
            if (m_IncludeBodyParts)
            {
                for (int i = 0; i < outfit.BodyPartCount; i++)
                {
                    var item = outfit.GetBodyPart(i);
                    if (item)
                        item.Context = context;
                }
            }
        }

        private void ProcessMountPoints(Outfit outfit, GameObject context)
        {
            if (m_IncludeMountPoints)
            {
                for (int i = 0; i < outfit.MountPointCount; i++)
                {
                    var item = outfit.GetMountPoint(i);
                    if (item)
                        item.Context = context;
                }
            }
        }

        #endregion
    }
}
