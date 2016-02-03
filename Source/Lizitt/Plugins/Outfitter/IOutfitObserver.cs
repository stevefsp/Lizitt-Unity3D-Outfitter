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
        void OnReleaseAccessory(Outfit sender, Accessory accessory);

        /// <summary>
        /// Prepare for the destruction of the outfit.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This event is sent before the outfit performs any internal operations.  So the
        /// outfit starts out as valid.  Whether it stays they way depends on the actions
        /// of the observers.
        /// </para>
        /// <para>
        /// Destruction of the outfit is inevitable.  There is nothing an observer can do to
        /// stop it.
        /// </para>
        /// <para>
        /// Observers must only interact with outfit accessories through normal outfit operations.
        /// (E.g. Remove them using the outfit's unmount method.)  Observers must not invalidate 
        /// accessories owned by the outfit.
        /// </para>
        /// <para>
        /// The outfit reference is useful for certain types of bake related synchronization 
        /// operations. (E.g. To bake the skinned mesh state from a source instance to its 
        /// cloned instance.)
        /// </para>
        /// </remarks>
        /// <param name="sender">The outfit that is to be destroyed.</param>
        /// <param name="typ">The destruction type.</param>
        /// <param name="referenceOutfit">
        /// The outfit that the outfit being baked is derived from.
        /// E.g. Was instanced from. Or null if the outfit has no applicable reference.
        /// (Only applies to the 'bake' type.)
        /// </param>s
        void OnDestroy(Outfit sender, DestroyType typ, Outfit referenceOutfit);
    }
}

