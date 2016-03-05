#if UNITY_EDITOR

using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace com.lizitt.outfitter.proto
{
    /// <summary>
    /// A component useful for prototyping body and its features.  (Editor only.)
    /// </summary>
    [AddComponentMenu(OutfitterUtil.Menu + "Body Prototyper Settings (Editor Only)", OutfitterUtil.EditorOnlyMenuOrder)]
    public class BodyPrototyperSettings
        : MonoBehaviour
    {
        // TODO: Add proper documentation.

        #region Body

        [SerializeField]
        [RequiredValue(typeof(Body), true)]
        private Body m_Body = null;
        public Body Body
        {
            get { return m_Body; }
            set { m_Body = value; }
        }

        public void InitializeBody()
        {
            if (!m_Body)
            {
                var stdBody = new GameObject("Body").AddComponent<StandardBody>();
                stdBody.DefaultMotionRoot = stdBody.transform;
                m_Body = stdBody;
            }
            else if (AssetDatabase.Contains(m_Body))
            {
                var name = m_Body.name;
                m_Body = m_Body.Instantiate();
                m_Body.name = name;
            }
        }

        #endregion

        #region Outfits

        [SerializeField]
        private List<Outfit> m_Outfits = new List<Outfit>();
        public List<Outfit> Outfits
        {
            get { return m_Outfits; }
        }

        private void InstantiateOutfits()
        {
            for (int i = 0; i < m_Outfits.Count; i++)
            {
                if (m_Outfits[i] && AssetDatabase.Contains(m_Outfits[i]))
                {
                    var name = m_Outfits[i].name;
                    m_Outfits[i] = m_Outfits[i].Instantiate();
                    m_Outfits[i].name = name;
                }
            }
        }

        private static Vector3 m_OutfitStorageRoot = Vector3.up * 500;
        public static Vector3 OutfitStorageRoot
        {
            get { return m_OutfitStorageRoot; }
            set { m_OutfitStorageRoot = value; }
        }

        public Vector3 GetStoragePosition(Outfit outfit)
        {
            if (outfit)
            {
                for (int i = 0; i < m_Outfits.Count; i++)
                {
                    if (m_Outfits[i] == outfit)
                    {
                        return OutfitStorageRoot + Vector3.right * m_Outfits.Count
                            + Vector3.forward * (i % 2);
                    }
                }
            }

            return OutfitStorageRoot;
        }

        public string[] GetOutfitNames()
        {
            var result = new string[m_Outfits.Count];

            for (int i = 0; i < m_Outfits.Count; i++)
                result[i] = m_Outfits[i] ? m_Outfits[i].name : "None";

            return result;
        }

        #endregion

        #region Accessories

        [SerializeField]
        private List<Accessory> m_Accessories = new List<Accessory>();

        public List<Accessory> Accessories
        {
            get { return m_Accessories; }
        }

        private void InstantiateAccessories()
        {
            for (int i = 0; i < m_Accessories.Count; i++)
            {
                if (m_Accessories[i] && AssetDatabase.Contains(m_Accessories[i]))
                {
                    var name = m_Accessories[i].name;
                    m_Accessories[i] = m_Accessories[i].Instantiate();
                    m_Accessories[i].name = name;
                }
            }
        }

        private static Vector3 m_AccessoryStorageRoot = Vector3.up * 500;
        public static Vector3 AccessoryStorageRoot
        {
            get { return m_AccessoryStorageRoot; }
            set { m_AccessoryStorageRoot = value; }
        }

        public Vector3 GetStoragePosition(Accessory accessory)
        {
            if (accessory)
            {
                for (int i = 0; i < m_Accessories.Count; i++)
                {
                    if (m_Accessories[i] == accessory)
                    {
                        return AccessoryStorageRoot + Vector3.left * m_Accessories.Count
                            + Vector3.forward * (i % 2);
                    }
                }
            }

            return OutfitStorageRoot;
        }

        #endregion

        #region Material Groups

        [SerializeField]
        private List<OutfitMaterialSettings> m_OutfitMaterialGroups = new List<OutfitMaterialSettings>();
        public List<OutfitMaterialSettings> OutfitMaterialGroups
        {
            get { return m_OutfitMaterialGroups; }
        }

        public string[] GetMaterialGroupNames()
        {
            var result = new string[m_OutfitMaterialGroups.Count];

            for (int i = 0; i < m_OutfitMaterialGroups.Count; i++)
                result[i] = m_OutfitMaterialGroups[i] ? m_OutfitMaterialGroups[i].name : null;

            return result;
        }

        #endregion

        #region Initialization

        public void InstantiateAssets()
        {
            InstantiateOutfits();
            InstantiateAccessories();
        }

        #endregion
    }
}

#endif
