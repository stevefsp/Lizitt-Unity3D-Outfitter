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
    /// <summary>
    /// The standard outfit types.
    /// </summary>
	public enum OutfitType
	{
        /* 
         * Design notes:
         * 
         * Make sure you understand all notes before refactoring this element. 
         * 
         * The meaning of any value can be repurposed except for the 'None' outfit.  
         * The 'None' outfit must always mean 'no outfit' / invisible.
         * 
         * It is ok to refactor all names as desired.
         * 
         * The values do not need to be sequential, though the category range restrictions 
         * must be followed.
         * 
         */

        /*******************************************************************************************
         * Standard Outfits
         * 
         * Generally considered long term, general use outfits.
         * 
         * There must be at least one standard outfit.
         * All values must be less than the first custom outfit.
         * Editors generally consider standard outfits to be  assignable at design time.
         * 
         * It is ok to add new types at any time.  Unused types can be removed with no impact.
         * But be careful when removing a type that has been assigned to a project or scene
         * object.  The assignment may become unusable at run-time or it may be forced to a default
         * value the next time it is accessed in the editor.
         * 
         ******************************************************************************************/

        /// <summary>
        /// Casual outfit.
        /// </summary>
        Casual = 0,  // This is the default value defined in OutfitterUtil.

        /// <summary>
        /// Formal outfit.
        /// </summary>
        Formal = 1,

        /// <summary>
        /// Job outfit.
        /// </summary>
        Job = 2,

        /// <summary>
        /// Underwear outfit.
        /// </summary>
        Skivvies = 3,

        /*******************************************************************************************
         * Custom Outfits
         * 
         * Custom outfits are short term outfits whose meaning and purpose are context senstive.
         * They exist so the standard outfits don't need to be temporarily overridden at run-time.
         *
         * There must be at least one custom outfit.
         * New custom types must have a value above the first custom type.
         * 
         * In general, custom outfits cannot be assign to the major components at design time.
         * (E.g. the Outfitter won't let you predefine prototypes for any of the custom
         * types via the editor, and it won't let you set the start outfit to a custom type.  
         * But it will allow you to add/remove custom types at run-time.)
         *
         * Add and remove custom types following the same guidelines as standard outfits.  
         * (E.g. Avoid removing types that are already in use by project and scene objects.) 
         * 
         ******************************************************************************************/

        /// <summary>
        /// A temporary outfit that can be changed at any time by whatever is controlling the
        /// agent.
        /// </summary>
        Custom = int.MaxValue - 100,  // Ok to refactor name.  Best not to remove or change value.

        /*******************************************************************************************
         * Special 'None' Outfit
         * 
         * Means 'no outfit'.  Must always be the highest value and it's meaning cannot change.
         ******************************************************************************************/

        /// <summary>
        /// No outfit at all.
        /// </summary>
        None = int.MaxValue
	}
}
