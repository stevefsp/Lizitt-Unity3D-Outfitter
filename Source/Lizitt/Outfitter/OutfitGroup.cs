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
using System.Collections.Generic;

namespace com.lizitt.outfitter
{
    /// <summary>
    /// Defines a group of related outfits.
    /// </summary>
    [System.Serializable]
    public class OutfitGroup
    {
        [System.Serializable]
        private struct GroupItem
        {
            public OutfitType typ;
            public BodyOutfit prototype;
        }

        [SerializeField]
        // Tooltip handled by custom editor.
        [OutfitType(OutfitFilterType.ExcludeCustom)]
        private OutfitType m_StartOutfit = OutfitterUtil.DefaultOutfit;
         
        [SerializeField]
        // Tooltip handled by custom editor.
        [OutfitType(OutfitFilterType.StandardOnly)]
        private OutfitType m_DefaultOutfit = OutfitterUtil.DefaultOutfit;

        [SerializeField]
        private List<GroupItem> m_Prototypes = new List<GroupItem>(0);

        /// <summary>
        /// The outfit type to use when an particular outfit type is not defined.
        /// </summary>
        public OutfitType DefaultOutfit
        {
            get { return m_DefaultOutfit; }
            set 
            { 
                if (!value.IsStandard())
                {
                    Debug.LogError("Default outfit can't be a custom or 'None' outfit: " + value);
                    return;
                }

                m_DefaultOutfit = value; 
            }
        }

        /// <summary>
        /// The outfit type that should be applied on component start.
        /// </summary>
        public OutfitType StartOutfit
        {
            get { return m_StartOutfit; }
            set 
            { 
                if (value.IsCustom())
                {
                    Debug.LogError("Start outfit can't be a custom outfit.");
                    return;
                }

                m_StartOutfit = value; 
            }
        }

        /// <summary>
        /// The outfit prototype by type.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is the 'raw' outfit value.  If doesn't take into account the value of 
        /// <see cref="DefaultOUtfit"/>.
        /// </para>
        /// <para>
        /// <see cref="OutfitType.None"/> is invalid and will always fail.
        /// </para>
        /// </remarks>
        /// <param name="typ">The type of outfit.</param>
        /// <returns>The prototype for the specified type.</returns>
        public BodyOutfit this[OutfitType typ]
        {
            get
            {
                if (typ == OutfitType.None)
                {
                    Debug.LogError("The 'None' type is invalid.");
                    return null;
                }

                int i = IndexOf(typ);

                return i == -1 ? null : m_Prototypes[i].prototype;
            }
            set
            {
                if (typ == OutfitType.None)
                {
                    Debug.LogError("Can't set the 'None' outfit.");
                    return;
                }

                var item = new GroupItem();
                item.typ = typ;
                item.prototype = value;

                int i = IndexOf(typ);

                if (i == -1)
                {
                    if (value)
                        m_Prototypes.Add(item);
                }
                else if (value)
                    m_Prototypes[i] = item;
                else
                    m_Prototypes.RemoveAt(i);
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
        public BodyOutfit GetOutfitOrDefault(OutfitType typ)
        {
            if (typ == OutfitType.None)
            {
                Debug.LogError("Can't get outfit of type 'None'.");
                return null;
            }

            int i = IndexOf(typ);

            if (i == -1 || !m_Prototypes[i].prototype)
            {
                i = IndexOf(m_DefaultOutfit);

                if (i == -1 || !m_Prototypes[i].prototype)
                {
                    Debug.LogError("Outfit group does not have a valid default outfit.");
                    return null;
                }

                return m_Prototypes[i].prototype;
            }

            return m_Prototypes[i].prototype;
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
        public OutfitType GetRealType(OutfitType typ)
        {
            if (typ == OutfitType.None)
                return OutfitType.None;

            var i = IndexOf(typ);

            if (i == -1 || !m_Prototypes[i].prototype)
                return m_DefaultOutfit;

            return typ;
        }

        public bool IsValid(bool silent = true)
        {
            // Expect editor to stop introduction of invalid values, such as duplicates.
            // Start outfit does not have to be defined.  It can be 'None' and if not defined it
            // will simply default.

            if (!m_DefaultOutfit.IsStandard())
            {
                if (!silent)
                {
                    Debug.LogError(
                        "Default outfit must be a normal outfit type: " + m_DefaultOutfit);
                }

                return false;
            }

            foreach (var item in m_Prototypes)
            {
                if (item.typ == m_DefaultOutfit)
                {
                    if (item.prototype)
                        return true;

                    if (!silent)
                        Debug.LogError("Default outfit not assigned a prototype.");

                    return false;
                }
            }

            if (!silent)
                Debug.LogError("The default outfit type not defined.");

            return false;
        }

        private int IndexOf(OutfitType typ)
        {
            for (int i = 0; i < m_Prototypes.Count; i++)
            {
                if (m_Prototypes[i].typ == typ)
                    return i;
            }

            return -1;
        }
    }
}