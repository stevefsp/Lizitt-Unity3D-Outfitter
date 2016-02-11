/*
 * Copyright (c) 2015-2016 Stephen A. Pratt
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
namespace com.lizitt.outfitter
{
    /// <summary>
    /// Accessory status types.
    /// </summary>
    public enum AccessoryStatus
    {
        /*
         * Design notes:
         * 
         * There are five potential states:
         * 
         * Mounted(Mounting)/Managed
         * Stored/Managed
         * Not-mounted/Unmanaged
         * Not-mounted/Managed
         * Destroyed
         * 
         * Where: Owned <-> Managed
         * 
         * Not-mounted/Managed is excluded as a type because it adds complexity with minimal
         * benefit.  The only use case I can think of is a component that wants to move the 
         * accessory around the scene while it is in an not-mounted visual state.  But this
         * minor use case can be handled in other ways, such as allowing the accessory's visual
         * state to be controlled separately from its mount state.
         * 
         * Destroyed is not included because it is very brief and could cause confusion as to
         * where to respond to it.  The proper place to respond is in the OnDestroy event, not
         * the OnStateChange event. 
         */

        /// <summary>
        /// The accessory is unmanaged.  (Not mounted, not managed.)
        /// </summary>
        Unmanaged = 0,

        /// <summary>
        /// The accessory is attached to a mount point. (Mounted and managed.)
        /// </summary>
        Mounted,

        /// <summary>
        /// The accessory is transitioning to 'mounted'. (Mount in progress, managed.)
        /// </summary>
        Mounting,

        /// <summary>
        /// The accessory is inactive.  (Stored and managed.)
        /// </summary>
        Stored,
    }
}
