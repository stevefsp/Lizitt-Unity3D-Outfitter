/*
 * Copyright (c) 2015 Stephen A. Pratt
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
	public enum OutfitType
	{
        /* 
         * Design notes:
         * 
         * The meaning of any value can be repurposed except for None.  None must always mean 
         * 'no outfit'.
         * 
         * These values are used as array indices.  So they must start with zero and remain 
         * sequential. 
         * 
         * Values can be added and removed below None without concern, up to the
         * point where outfitter assets and scene objects have been created.  After that point,
         * changing the values, especially the value of None, will require those objects be 
         * repaired.
         * 
         * Names should be changed through refactoring since some of them are in use in code.
         * 
         * Whatever value is assigned to OutfitterUtil.HighestVisibleOutfit will be the
         * highest Unity Editor assignable outfit.  Any outfit higher than 
         * OutfitterUtil.HighestVisibleOutfit can be assigned only at run-time via 
         * code. (Except None, which can never be assigned.)
         * 
         * Note: 'Assigned' means assigning an outfit prototype to a configuration.  All values 
         * are also used for other purposes.
         * 
         * The reason for this design is that the lower outfits are generally semi-permanent.  
         * They are assigned at design-time and changed only occasionally, with their meaning 
         * being fixed.  E.g. Casual means it is the character's default casual outfit.  
         * Values higher than OutfitterUtil.HighestVisibleOutfit are used for transient 
         * special purpose outfits that have meaning only in the context they are used. 
         * E.g. A special scene requires the character to be a bouncing teapot.  So the AI sets 
         * the custom outfit to a teapot, uses it, then sets the outfit back to null (default) 
         * after it is done.
         * 
         */

        /// <summary>
        /// Casual outfit.
        /// </summary>
        Casual = 0,  // This is the default value of OutfitterUtil.DefaultOutfit

        /// <summary>
        /// Formal outfit.
        /// </summary>
        Formal = 1,

        /// <summary>
        /// Job outfit.
        /// </summary>
        Job = 2,

        /// <summary>
        /// Skivvies outfit.
        /// </summary>
        Skivvies = 3,

        /// <summary>
        /// Nude outfit.
        /// </summary>
        Nude = 4,

        /// <summary>
        /// Body only outfit.  (Alternate <see cref="Nude"/>)
        /// </summary>
        BodyOnly = 5,   // This is the default value of OutfitterUtil.HighestVisibleOutfit.

        // By default, outfits beyond this point cannot be assigned values in the Unity Editor.  
        // These are special purpose slots for use in code.

        /// <summary>
        /// A temporary outfit that can be changed at any time by whatever is controlling the
        /// agent.
        /// </summary>
        Custom = 6,

        /// <summary>
        /// No body at all.
        /// </summary>
        None = 7    // Must always be the highest value.  It's meaning cannot change.
	}
}
