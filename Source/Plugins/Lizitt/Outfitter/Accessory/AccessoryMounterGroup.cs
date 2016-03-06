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
    /// Defines a group of ordered accessory mounters.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Designed for use as a field in a Unity component.  It it provides a better editor experience when used with 
    /// <see cref="ObjectListAttribute"/>.
    /// </para>
    /// </remarks>
    [System.Serializable]
    public sealed class AccessoryMounterGroup
        : ObjectList<IAccessoryMounter>
    {
        /// <summary>
        /// Replace the items in the buffer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// To use this method safely, either set <paramref name="asReference"/> to false, or
        /// discard all external references to the <paramref name="items"/> array.
        /// </para>
        /// </remarks>
        /// <param name="mounters">The object to udpate. (Required)</param>
        /// <param name="asReference">
        /// If true, the inernal buffer will be replaced by a referecne to <paramref name="items"/>.  
        /// Otherwise <paramref name="items"/> will be copied.
        /// </param>
        /// <param name="items">The items to put into the buffer.</param>
        public static void UnsafeReplaceItems(
            Object refObj, AccessoryMounterGroup mounters, params IAccessoryMounter[] items)
        {
            mounters.AddItems(refObj, items);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="initialCapacity">The initial capacity of the group.</param>
        public AccessoryMounterGroup(int initialCapacity)
            : base(initialCapacity)
        {
        }

        /// <summary>
        /// Performs a <see cref="IAccessoryMounter.CanMount"/> check on the mounters and returns the index of the
        /// first one that returns true, or -1 if none was found.
        /// </summary>
        /// <param name="accessory">The accessory. (Required)</param>
        /// <param name="location">The location.</param>
        /// <param name="restrictions">The body converage restrictions.</param>
        /// <returns>The index of the mounter than can mount the accessory, or -1 if none was found.</returns>
        public int CanMount(Accessory accessory, MountPoint location, BodyCoverage restrictions)
        {
            for (int i = 0; i < Count; i++)
            {
                if (Accessory.CanMount(accessory, this[i], location, restrictions))
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Return the first mounter found that can mount the accessory, initialized and ready to update.  Or null if 
        /// none is avaiable.
        /// </summary>
        /// <param name="accessory">The accessory. (Required)</param>
        /// <param name="location">The mount location. (Required)</param>
        /// <returns></returns>
        public IAccessoryMounter GetInitializedMounter(Accessory accessory, MountPoint location)
        {
            for (int i = 0; i < Count; i++)
            {
                var mounter = this[i];
                if (mounter != null && mounter.InitializeMount(accessory, location))
                    return mounter;
            }

            return null;
        }

        /// <summary>
        /// Send the <see cref="IAccessoryMounter.OnAccessoryDestroy"/> event to all mounters.
        /// </summary>
        /// <param name="sender">The accessory to be destroyed.</param>
        /// <param name="typ">The accessory destroy type.</param>
        public void SendAccessoryDestroy(Accessory sender, DestroyType typ)
        {
            for (int i = 0; i < Count; i++)
            {
                var mounter = this[i];
                if (mounter != null)
                   mounter.OnAccessoryDestroy(sender, typ);
            }
        }
    }
}