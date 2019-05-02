using BepInEx;
using UnityEngine;
using RoR2;
using System.Reflection;
using System;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace PlexusUtils
{
    public enum StatIndex
    {
        MaxHealth,
        MaxShield,

        Regen,
        SafeRegen,

        MoveSpeed,
        RunningMoveSpeed,
        SafeMoveSpeed,
        SafeRunningMoveSpeed,

        JumpPower,
        JumpCount,

        Damage,
        AttackSpeed,

        Crit,
        Armor,
        RunningArmor,

        GlobalCoolDown,
        CoolDownPrimary,
        CoolDownSecondary,
        CoolDownUtility,
        CoolDownSpecial,
        CountPrimary,
        CountSecondary,
        CountUtility,
        CountSpecial,
    }

    public class ModItemStat
    {
        public StatIndex Stat;
        private float m_BaseBonus;
        private float m_StackBonus;
        private float m_BaseMultBonus;
        private float m_StackMultBonus;

        #region Properties
        public float FlatBonus { get { return m_BaseBonus; } }
        public float StackBonus { get { return m_StackBonus; } }
        public float MultBonus { get { return m_BaseMultBonus; } }
        public float MultStackBonus { get { return m_StackMultBonus; } }

        public float GetFlatBonusFromCount(int count)
        {
            if (count > 0)
                return (count - 1) * m_StackBonus + m_BaseBonus;
            return 0;
        }
        public float GetPercentBonusFromCount(int count)
        {
            if (count > 0)
                return (count - 1) * m_StackMultBonus + m_BaseMultBonus;
            return 0;
        }
        #endregion

        

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="FlatBonus">Flat bonus when player Have the item</param>
        /// <param name="FlatStackBonus">Flat bonus for each additional item the player own</param>
        /// <param name="MultBonus">Multiplicative bonus when player Have the item</param>
        /// <param name="MultStackBonus">Multiplicative bonus for each additional item the player own</param>
        /// <param name="Stat"></param>
        public ModItemStat(float FlatBonus, float FlatStackBonus, float MultBonus, float MultStackBonus, StatIndex Stat)
        {
            m_BaseBonus = FlatBonus;
            m_StackBonus = FlatStackBonus;
            m_BaseMultBonus = MultBonus;
            m_StackMultBonus = MultStackBonus;
            this.Stat = Stat;
        }
        /// <summary>
        /// Set Flat and Stack Bonusat the same time, if you want to set the separatly use ModItemStats(float FlatBonus, float StackBonus, StatIndex Stat)
        /// </summary>
        /// <param name="FlatBonus">Flat bonus for each item the player own</param>
        /// <param name="Stat"></param>
        public ModItemStat(float FlatBonus, StatIndex Stat)
        {
            m_BaseBonus = FlatBonus;
            m_StackBonus = FlatBonus;
            m_BaseMultBonus = 0;
            m_StackMultBonus = 0;
            this.Stat = Stat;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="FlatBonus">Flat bonus when player Have the item</param>
        /// <param name="FlatStackBonus">Flat bonus for each additional item the player own</param>
        /// <param name="MultBonus">Multiplicative bonus for each item the player own</param>
        public ModItemStat(float FlatBonus, float StackBonus, StatIndex Stat)
        {
            m_BaseBonus = FlatBonus;
            m_StackBonus = StackBonus;
            m_BaseMultBonus = 0;
            m_StackMultBonus = 0;
            this.Stat = Stat;
        }
        public ModItemStat(float FlatBonus, float StackBonus, float MultBonus, StatIndex Stat)
        {
            m_BaseBonus = FlatBonus;
            m_StackBonus = StackBonus;
            m_BaseMultBonus = MultBonus;
            m_StackMultBonus = MultBonus;
            this.Stat = Stat;
        }
        #endregion

        #region Operator
        public static ModItemStat operator +(ModItemStat a, ModItemStat b)
        {
            a.m_BaseBonus += b.m_BaseBonus;
            a.m_StackBonus += b.m_StackBonus;
            a.m_BaseMultBonus += b.m_BaseMultBonus;
            a.m_StackMultBonus += b.m_StackMultBonus;
            return a;
        }
        public static ModItemStat operator -(ModItemStat a, ModItemStat b)
        {
            a.m_BaseBonus -= b.m_BaseBonus;
            a.m_StackBonus -= b.m_StackBonus;
            a.m_BaseMultBonus -= b.m_BaseMultBonus;
            a.m_StackMultBonus -= b.m_StackMultBonus;
            return a;
        }
        public static ModItemStat operator *(ModItemStat a, float b)
        {
            a.m_BaseBonus *= b;
            a.m_StackBonus *= b;
            a.m_BaseMultBonus *= b;
            a.m_StackMultBonus *= b;
            return a;
        }
        public static ModItemStat operator /(ModItemStat a, float b)
        {
            a.m_BaseBonus /= b;
            a.m_StackBonus /= b;
            a.m_BaseMultBonus /= b;
            a.m_StackMultBonus /= b;
            return a;
        }
        #endregion

    }

    public class ModItem
    {
        private ItemIndex m_Index;
        

        private List<ModItemStat> m_StatList;


        #region properties
        public List<ModItemStat> GetStatsList { get { return m_StatList; } }
        public ItemIndex Index { get { return m_Index; } private set { m_Index = value; } }

        /// <summary>
        /// Flat bonus of the First Item
        /// </summary>
        /// <param name="Stat"></param>
        /// <returns></returns>
        public float FlatBonus(StatIndex Stat)
        {
            ModItemStat s = m_StatList.Find(x => x.Stat == Stat);
            if (s != null)
                return s.FlatBonus;
            return 0;
        }
        /// <summary>
        /// Flat bonus Per item after the first one
        /// </summary>
        /// <param name="Stat"></param>
        /// <returns></returns>
        public float StackBonus(StatIndex Stat)
        {
            ModItemStat s = m_StatList.Find(x => x.Stat == Stat);
            if (s != null)
                return s.StackBonus;
            return 0;
        }
        /// <summary>
        /// Multiplicative bonus of the First item
        /// </summary>
        /// <param name="Stat"></param>
        /// <returns></returns>
        public float MultBonus(StatIndex Stat)
        {
            ModItemStat s = m_StatList.Find(x => x.Stat == Stat);
            if (s != null)
                return s.MultBonus;
            return 0;
        }
        /// <summary>
        /// Multiplicative bonus Per item after the first one
        /// </summary>
        /// <param name="Stat"></param>
        /// <returns></returns>
        public float MultStackBonus(StatIndex Stat)
        {
            ModItemStat s = m_StatList.Find(x => x.Stat == Stat);
            if (s != null)
                return s.MultStackBonus;
            return 0;
        }
        /// <summary>
        /// Get FlatBonus From Count
        /// </summary>
        /// <param name="Stat"></param>
        /// <param name="Count"></param>
        /// <returns></returns>
        public float GetFlatBonusFromCount(StatIndex Stat, int Count)
        {
            ModItemStat s = m_StatList.Find(x => x.Stat == Stat);
            if (s != null)
                return s.GetFlatBonusFromCount(Count);
            return 0;
        }
        /// <summary>
        /// Get Multiplicative Bonus from Count
        /// </summary>
        /// <param name="Stat"></param>
        /// <param name="Count"></param>
        /// <returns></returns>
        public float GetMultStackBonusFromCount(StatIndex Stat, int Count)
        {
            ModItemStat s = m_StatList.Find(x => x.Stat == Stat);
            if (s != null)
                return s.GetPercentBonusFromCount(Count);
            return 0;
        }
        #endregion


        public ModItem(ItemIndex Index)
        {
            this.m_Index = Index;
            m_StatList = new List<ModItemStat>();
        }

        public ModItem(ItemIndex Index,List<ModItemStat> Stats)
        {
            this.m_Index = Index;
            m_StatList = Stats;
        }

        public ModItem(ItemIndex Index, ModItemStat Stat)
        {
            this.m_Index = Index;
            m_StatList = new List<ModItemStat>
            {
                Stat
            };
        }
        public ModItem(ItemIndex Index, ModItemStat Stat1, ModItemStat Stat2)
        {
            this.m_Index = Index;
            m_StatList = new List<ModItemStat>
            {
                Stat1,
                Stat2
            };
        }
        public ModItem(ItemIndex Index, ModItemStat Stat1, ModItemStat Stat2, ModItemStat Stat3)
        {
            this.m_Index = Index;
            m_StatList = new List<ModItemStat>
            {
                Stat1,
                Stat2,
                Stat3
            };
        }
        public ModItem(ItemIndex Index, ModItemStat Stat1, ModItemStat Stat2, ModItemStat Stat3, ModItemStat Stat4)
        {
            this.m_Index = Index;
            m_StatList = new List<ModItemStat>
            {
                Stat1,
                Stat2,
                Stat3,
                Stat4
            };
        }

        #region Operator
        public static ModItem operator +(ModItem Item, ModItemStat Stat)
        {
            if (Item.m_StatList.Exists(x => x.Stat == Stat.Stat))
            {
                Item.m_StatList[Item.m_StatList.FindIndex(x => x.Stat == Stat.Stat)] += Stat;
            }
            return Item;
        }
        public static ModItem operator +(ModItem Item, List<ModItemStat> Stats)
        {
            foreach(ModItemStat Stat in Stats)
                if (Item.m_StatList.Exists(x => x.Stat == Stat.Stat))
                {
                    Item.m_StatList[Item.m_StatList.FindIndex(x => x.Stat == Stat.Stat)] += Stat;
                }
            return Item;
        }
        public static ModItem operator -(ModItem Item, ModItemStat Stat)
        {
            if (Item.m_StatList.Exists(x => x.Stat == Stat.Stat))
            {
                Item.m_StatList[Item.m_StatList.FindIndex(x => x.Stat == Stat.Stat)] -= Stat;
            }
            return Item;
        }
        #endregion
    }

    static public class ModItemManager
    {
        static public Dictionary<ItemIndex, ModItem> ModItemDictionary;

        static public void AddModItem(ItemIndex index, ModItem ModItem)
        {
            if (!ModItemDictionary.ContainsKey(index))
            {
                ModItemDictionary.Add(index, ModItem);
            }
            else
            {
                ModItemDictionary[index] += ModItem.GetStatsList;
            }

        }
        static public void AddStatToItem(ItemIndex index, ModItemStat stat)
        {
            if (ModItemDictionary.ContainsKey(index))
            {
                ModItemDictionary[index] += stat;
            }
            else
            {
                throw new Exception("ModItemManager ERROR : ItemIndex does not exist in ModItemDictionary");
            }
        }
        static public void AddStatToItem(ItemIndex index, List<ModItemStat> stats)
        {
            if (ModItemDictionary.ContainsKey(index))
            {
                ModItemDictionary[index] += stats;
            }
            else
            {
                throw new Exception("ModItemManager ERROR : ItemIndex does not exist in ModItemDictionary");
            }
        }

        static public ModItem GetModItem(ItemIndex index)
        {
            if (ModItemDictionary.ContainsKey(index))
            {
                return ModItemDictionary[index];
            }
            else
            {
                throw new Exception("ModItemManager ERROR : ItemIndex does not exist in ModItemDictionary");
            }
        }

        static public void Update()
        {
            ModItemDictionary = new Dictionary<ItemIndex, ModItem>();
            foreach (ItemIndex itemIndex in (ItemIndex[])Enum.GetValues(typeof(ItemIndex)))
            {
                ModItemDictionary.Add(itemIndex, new ModItem(itemIndex));
            }
            //Fun Start here
            
            AddStatToItem(ItemIndex.PersonalShield, new ModItemStat(25, StatIndex.MaxShield));
            AddStatToItem(ItemIndex.AlienHead, new ModItemStat(0,0,0.75f, StatIndex.GlobalCoolDown));
            
        }

        static public float GetBonusForStat(CharacterBody c,StatIndex stat)
        {
            float value = 0;
            if (c.inventory)
            {
                foreach (ItemIndex itemIndex in (ItemIndex[])Enum.GetValues(typeof(ItemIndex)))
                {
                    if (c.inventory.GetItemCount(itemIndex) > 0)
                        value += ModItemDictionary[itemIndex].GetFlatBonusFromCount(stat, c.inventory.GetItemCount(itemIndex));
                }
            }
            return value;
        }
        static public float GetMultiplierForStat(CharacterBody c,StatIndex stat)
        {
            float value = 0;
            if (c.inventory)
            {
                foreach (ItemIndex itemIndex in (ItemIndex[])Enum.GetValues(typeof(ItemIndex)))
                {
                    if (c.inventory.GetItemCount(itemIndex) > 0)
                        value += ModItemDictionary[itemIndex].GetMultStackBonusFromCount(stat, c.inventory.GetItemCount(itemIndex));
                }
            }
            return value;
        }

        static public float GetMultiplierForStatCD(CharacterBody c, StatIndex stat)
        {
            float value = 1;
            if (c.inventory)
            {
                foreach (ItemIndex itemIndex in (ItemIndex[])Enum.GetValues(typeof(ItemIndex)))
                {
                    if (c.inventory.GetItemCount(itemIndex) > 0)
                        value *= ModItemDictionary[itemIndex].GetMultStackBonusFromCount(stat, c.inventory.GetItemCount(itemIndex));
                }
            }
            return value;
        }

    }
}
