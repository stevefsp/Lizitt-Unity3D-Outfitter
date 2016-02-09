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
    /// A base object for all acessory mounter project assets.
    /// </summary>
    /// <remarks>
    /// <para>
    /// There is no requirement that all ScriptableObject based mounters inherit from this class.  It is simply a
    /// base from which to provide common utility features.
    /// </para>
    /// </remarks>
    public abstract class AccessoryMounter
        : ScriptableObject, IAccessoryMounter
    {
        public abstract BodyCoverage GetCoverageFor(MountPointType locationType);

        public abstract MountPointType DefaultLocationType { get; }

        public abstract bool CanMount(Accessory accessory, MountPointType locationType);

        public abstract bool InitializeMount(Accessory accessory, MountPoint location);

        public abstract bool UpdateMount(Accessory accessory, MountPoint location, bool immediateComplete);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // HACK: Unity 5.3.1: Workaround for Mono's optional parameter key duplication bug.

        public bool UpdateMount(Accessory accessory, MountPoint location)
        {
            return UpdateMount(accessory, location, false);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public abstract void CancelMount(Accessory accessory, MountPoint location);

        public virtual void OnAccessoryDestroy(Accessory accessory, DestroyType type)
        {
            // Do nothing.
        }
    }
}
