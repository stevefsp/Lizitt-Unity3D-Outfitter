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
using UnityEngine;

namespace com.lizitt.outfitter
{
    /// <summary>
    /// Defines a group of related outfits.
    /// </summary>
    [System.Serializable]
    public class OutfitGroup
    {
        [SerializeField]
        [Tooltip("The outfit that should be applied on start.")]
        private OutfitType m_StartOutfit = OutfitterUtil.DefaultOutfit;
         
        [SerializeField]
        [Tooltip("The outfit type to use when an particular outfit type is not assigned.")]
        private OutfitType m_DefaultOutfit = OutfitterUtil.DefaultOutfit;

        // Note: 'None' never has a prototype.
        [SerializeField]
        private BodyOutfit[] m_Prototypes = new BodyOutfit[(int)OutfitType.None];

        /// <summary>
        /// The type of the default outfit.
        /// </summary>
        public OutfitType DefaultOutfit
        {
            get { return m_DefaultOutfit; }
            set { m_DefaultOutfit = value; }
        }

        /// <summary>
        /// The type of the start outfit.
        /// </summary>
        public OutfitType StartOutfit
        {
            get { return m_StartOutfit; }
            set { m_StartOutfit = value; }
        }

        /// <summary>
        /// The outfit prototype by type.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is the 'raw' outfit vaoue.  If doesn't take into account the value of 
        /// <see cref="DefaultOUtfit"/>.
        /// </para>
        /// <para>
        /// <see cref="OutfitType.None"/> will always return null.
        /// </para>
        /// </remarks>
        /// <param name="type">The type of outfit.</param>
        /// <returns>The prototype for the specified type.</returns>
        public BodyOutfit this[OutfitType type]
        {
            get 
            {
                if (type == OutfitType.None)
                    return null;
                return m_Prototypes[(int)type]; 
            }
            set 
            {
                if (type == OutfitType.None)
                    return;
                m_Prototypes[(int)type] = value; 
            }
        }

        /// <summary>
        /// Gets the prototype for the specified outfit type, or the default proprotype if 
        /// the type is not defined.
        /// </summary>
        /// <param name="type">The type of the outfit to retrieve.</param>
        /// <returns>
        /// The prototype for the specified outfit type, or the default proprotype if 
        /// the type is not defined.
        /// </returns>
        public BodyOutfit GetOutfitOrDefault(OutfitType type)
        {
            if (type == OutfitType.None)
            {
                Debug.LogError("Can't get outfit for 'None' outfity type.");
                return null;
            }

            var result = this[type];
            if (!result)
                result = this[m_DefaultOutfit];

            if (!result)
                Debug.LogError("Outfit group does not have a valid default outfit.");

            return result;
        }

        /// <summary>
        /// Returns the type of the prototype that will be used if <see cref="GetOutfitOrDefault"/>
        /// is called. (Or <see cref="OutfitType.None"/> if type is <see cref="OutfitType.None"/>).
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is useful for determining if an outfit type uses the default
        /// prototype.
        /// </para>
        /// <para>
        /// If <paramref name="type"/> is <see cref="OutfitType.None"/>, will return 
        /// <see cref="OutfitType.None"/>.  If <paramref name="type"/> has no assigned 
        /// prototype, or it is the default, then the default outfit type will be returned.  
        /// Otherwise it will return the type unaltered.
        /// </para>
        /// </remarks>
        /// <param name="type">The outfit type.</param>
        /// <returns>The type of the prototype that will be used if <see cref="GetOutfitOrDefault"/>
        /// is called.</returns>
        public OutfitType GetRealType(OutfitType type)
        {
            if (type == OutfitType.None)
                return OutfitType.None;

            if (!this[type])
                return m_DefaultOutfit;

            return type;
        }
    }
}