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
    /// useful base from which to provide common utility and editor search features.
    /// </para>
    /// <para>
    /// It is important to support concurrent mount operations for mounters that are designed to be project assets.  
    /// This is easy for immediate-complete mounters, but care must be taken for mounters that take time to complete.
    /// </para>
    /// <para>
    /// Mounters based on this class can support both concurrent mount operations and full state serialization.
    /// Actual support dependeds on the concrete implemenation.
    /// </para>
    /// </remarks>
    public abstract class AccessoryMounterObject
        : ScriptableObject, IAccessoryMounter
    {
        public abstract BodyCoverage GetCoverageFor(MountPoint location);

        public abstract MountPointType DefaultLocation { get; }

        public abstract bool CanMount(Accessory accessory, MountPoint location);

        public abstract bool InitializeMount(Accessory accessory, MountPoint location);

        public abstract bool UpdateMount(Accessory accessory, MountPoint location, bool immediateComplete);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // HACK: Unity 5.3.1: Workaround for Mono's optional parameter key duplication bug.

        public bool UpdateMount(Accessory accessory, MountPoint location)
        {
            return UpdateMount(accessory, location, false);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public abstract bool UpdateMount(Accessory accessory, MountPoint location, float deltaTime);

        public virtual void CancelMount(Accessory accessory, MountPoint location)
        {
            // Do nothing.
            // Most mounters are expected to be immediate-complete, so don't make them all implement this method.
        }

        public virtual void OnAccessoryDestroy(Accessory accessory, DestroyType type)
        {
            // Do nothing.
            // For most scriptable object mounters the main purpose for this is state cleanup of in-progress mounts,
            // so likelihood of use is similar to CancelMount.
        }
    }
}
