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
    /// A component that responds to <see cref="Outfit"/> events.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Most accessories require this interface to be implemented by a Unity Object for
    /// serialization purposes, so outfits are allowed to reject observers that are not 
    /// Unity Objects.
    /// </para>
    /// </remarks>
    public interface IOutfitObserver
    {
        /// <summary>
        /// Process an accessory mount event.
        /// </summary>
        /// <param name="sender">The outfit that mounted the accessory.</param>
        /// <param name="accessory">The accessory that was mounted.</param>
        void OnMountAccessory(Outfit sender, Accessory accessory);

        /// <summary>
        /// Process an accessory unmount event.
        /// </summary>
        /// <param name="sender">The outfit that unmounted the accessory.</param>
        /// <param name="accessory">The accessory that was unmounted.</param>
        void OnUnmountAccessory(Outfit sender, Accessory accessory);

        /// <summary>
        /// Perform outfit bake operations.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is called before the outfit performs any of its internal bake operations, 
        /// but whether or not an outfit feature is still useable depends on the call order
        /// of the observers. (E.g. Once an observer that is responsible for deleting all colliders
        /// has been called, the outfit's collider features will not be avaiable to later observers.)
        /// </para>
        /// <para>
        /// Observers must only interact with outfit accessories through normal outfit operations.
        /// (E.g. Remove them using the outfit's unmount method.)  Observers must not invalidate 
        /// accessories owned by the outfit.
        /// </para>
        /// <para>
        /// The outfit reference is useful for certain types of synchronization operations.
        /// (E.g. To bake the skinned mesh state from a source instance to its cloned instance.)
        /// </para>
        /// </remarks>
        /// <param name="outfit">The outfit being baked.</param>
        /// <param name="referenceOutfit">
        /// The outfit that the outfit being baked is derived from.  (E.g. Was instanced from.)
        /// Or null if the outfit has no applicable reference.
        /// </param>
        void OnBake(Outfit sender, Outfit referenceOutfit = null);

        /// <summary>
        /// Perform post bake operations.  (The outfit component is no longer available.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is called after the outfit is no longer valid.  Its purpose is to 
        /// allow components that depend on the outfit to clean themselves up.  (E.g. Self destroy.)
        /// </para>
        /// </remarks>
        /// <param name="outfit">The baked outfit's GameObject.</param>
        void OnBakePost(GameObject sender);
    }
}

