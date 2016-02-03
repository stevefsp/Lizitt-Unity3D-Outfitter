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
    /// A utility class for managing a list of outfit observers.
    /// </summary>
    [System.Serializable]
    public class OutfitObserverGroup
        : ObjectList<IOutfitObserver>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="initialSize">The initial buffer size.</param>
        public OutfitObserverGroup(int initialSize)
            : base(initialSize)
        {
        }

        /// <summary>
        /// Send the <see cref="IOutfitObserver.OnMountAccessory"/> event.
        /// </summary>
        /// <param name="sender">The outfit sending the event.</param>
        /// <param name="accessory">The accesosry that has been mounted to the outfit.</param>
        public void SendMount(Outfit sender, Accessory accessory)
        {
            bool hasNull = false;
            for (int i = 0; i < Count; i++)
            {
                if (this[i] == null)
                    hasNull = true;

                this[i].OnMountAccessory(sender, accessory);
            }
            if (hasNull)
                PurgeNulls();
        }

        /// <summary>
        /// Send the <see cref="IOutfitObserver.OnReleaseAccessory"/> event.
        /// </summary>
        /// <param name="sender">The outfit sending the event.</param>
        /// <param name="accessory">The accesosry that has been unmounted from the outfit.</param>
        public void SendUnmount(Outfit sender, Accessory accessory)
        {
            bool hasNull = false;
            for (int i = 0; i < Count; i++)
            {
                if (this[i] == null)
                    hasNull = true;

                this[i].OnReleaseAccessory(sender, accessory);
            }
            if (hasNull)
                PurgeNulls();
        }

        /// <summary>
        /// Send the <see cref="IOutfitObserver.OnDestroy"/> event.
        /// </summary>
        /// <param name="sender">The outfit being destroyed.</param>
        /// <param name="referenceOutfit">
        /// The outfit that the sender is derived from.  (E.g. Was instanced from.)
        /// Or null if the sender has no applicable reference. (Only applicable to the
        /// 'bake' type.)
        /// </param>
        public void SendDestroy(Outfit sender, DestroyType typ, Outfit referenceOutfit)
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i] != null)
                    this[i].OnDestroy(sender, typ, referenceOutfit);
            }
        }
    }
}