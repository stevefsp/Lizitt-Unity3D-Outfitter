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
    /// A utility class for managing a list of ordered <see cref="Body"/> observers.
    /// </summary>
    /// <seealso cref="IBodyObserver"/>
    [System.Serializable]
    public sealed class BodyObserverGroup
        : ObjectList<IBodyObserver>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="initialCapacity">The initial capacity of the group.</param>
        public BodyObserverGroup(int initialCapacity)
            : base(initialCapacity)
        {
        }

        /// <summary>
        /// Send the <see cref="IBodyObserver.OnOutfitChange"/> event to all observers.
        /// </summary>
        /// <param name="sender">The body sending the event. (Required)</param>
        /// <param name="previous">The body's previous outfit, or null if there is none.</param>
        /// <param name="wasForced">
        /// True if the body was forced to release its previous outfit.
        /// </param>
        public void SendOutfitChange(Body sender, Outfit previous, bool wasForced)
        {
            bool hasNull = false;
            for (int i = 0; i < Count; i++)
            {
                if (this[i] == null)
                    hasNull = true;
                else
                    this[i].OnOutfitChange(sender, previous, wasForced);
            }
            if (hasNull)
                PurgeDestroyed();
        }
    }
}