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
    /// A utility class for managing a list of ordered <see cref="Outfit"/> observers.
    /// </summary>
    [System.Serializable]
    public sealed class OutfitObserverGroup
        : ObjectList<IOutfitObserver>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="initialCapacity">The initial capacity of the list.</param>
        public OutfitObserverGroup(int initialCapacity)
            : base(initialCapacity)
        {
        }

        /// <summary>
        /// Send the <see cref="IOutfitObserver.OnStateChange"/> event to all observers.
        /// </summary>
        /// <param name="sender">The outfit sending the event. (Required)</param>
        public void SendStateChange(Outfit sender)
        {
            bool hasNull = false;
            for (int i = 0; i < Count; i++)
            {
                if (this[i] == null)
                    hasNull = true;
                else
                    this[i].OnStateChange(sender);
            }
            if (hasNull)
                PurgeDestroyed();
        }

        /// <summary>
        /// Send the <see cref="IOutfitObserver.OnMountAccessory"/> event to all observers.
        /// </summary>
        /// <param name="sender">The outfit sending the event. (Required)</param>
        /// <param name="accessory">The accesosry that was mounted to the outfit. (Required)</param>
        public void SendAccessoryMount(Outfit sender, Accessory accessory)
        {
            bool hasNull = false;
            for (int i = 0; i < Count; i++)
            {
                if (this[i] == null)
                    hasNull = true;
                else
                    this[i].OnMountAccessory(sender, accessory);
            }
            if (hasNull)
                PurgeDestroyed();
        }

        /// <summary>
        /// Send the <see cref="IOutfitObserver.OnReleaseAccessory"/> event.
        /// </summary>
        /// <param name="sender">The outfit sending the event. (Required)</param>
        /// <param name="accessory">The accesosry that has been released from the outfit.</param>
        public void SendReleaseAccessory(Outfit sender, Accessory accessory)
        {
            bool hasNull = false;
            for (int i = 0; i < Count; i++)
            {
                if (this[i] == null)
                    hasNull = true;
                else
                    this[i].OnReleaseAccessory(sender, accessory);
            }
            if (hasNull)
                PurgeDestroyed();
        }

        /// <summary>
        /// Send the <see cref="IOutfitObserver.OnDestroy"/> event.
        /// </summary>
        /// <param name="sender">The outfit being destroyed.</param>
        /// <param name="referenceOutfit">
        /// The outfit that the sender is derived from.  (E.g. Was instanced from.) Or null if the sender has no
        /// applicable reference. (Only applicable to the 'bake' type.)
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