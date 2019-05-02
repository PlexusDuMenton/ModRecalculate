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

        Damage,
        AttackSpeed,

        Crit,
        Armor,
        RunningArmor,
    }

    public class ModItemStats
    {
        public StatIndex Stat;


        private float m_BaseBonus;
        private float m_StackBonus;
        private float m_BaseMultBonus;
        private float m_StackMultBonus;

        public static ModItemStats operator +(ModItemStats a, ModItemStats b)
        {
            a.m_BaseBonus += b.m_BaseBonus;
            a.m_StackBonus += b.m_StackBonus;
            a.m_BaseMultBonus += b.m_BaseMultBonus;
            a.m_StackMultBonus += b.m_StackMultBonus;
            return a;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="FlatBonus">Flat bonus when player Have the item</param>
        /// <param name="FlatStackBonus">Flat bonus for each additional item the player own</param>
        /// <param name="MultBonus">Multiplicative bonus when player Have the item</param>
        /// <param name="MultStackBonus">Multiplicative bonus for each additional item the player own</param>
        /// <param name="Stat"></param>
        public ModItemStats(float FlatBonus, float FlatStackBonus, float MultBonus, float MultStackBonus, StatIndex Stat)
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
        public ModItemStats(float FlatBonus, StatIndex Stat)
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
        public ModItemStats(float FlatBonus, float StackBonus, StatIndex Stat)
        {
            m_BaseBonus = FlatBonus;
            m_StackBonus = StackBonus;
            m_BaseMultBonus = 0;
            m_StackMultBonus = 0;
            this.Stat = Stat;
        }
        public ModItemStats(float FlatBonus, float StackBonus, float MultBonus, StatIndex Stat)
        {
            m_BaseBonus = FlatBonus;
            m_StackBonus = StackBonus;
            m_BaseMultBonus = MultBonus;
            m_StackMultBonus = MultBonus;
            this.Stat = Stat;
        }

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
    }

    public class ModItem
    {
        public ItemIndex Index;
        /// <summary>
        /// Flat bonus of the First Item
        /// </summary>
        /// <param name="Stat"></param>
        /// <returns></returns>
        public float FlatBonus(StatIndex Stat)
        {
            ModItemStats s = m_StatList.Find(x => x.Stat == Stat);
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
            ModItemStats s = m_StatList.Find(x => x.Stat == Stat);
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
            ModItemStats s = m_StatList.Find(x => x.Stat == Stat);
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
            ModItemStats s = m_StatList.Find(x => x.Stat == Stat);
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
            ModItemStats s = m_StatList.Find(x => x.Stat == Stat);
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
            ModItemStats s = m_StatList.Find(x => x.Stat == Stat);
            if (s != null)
                return s.GetPercentBonusFromCount(Count);
            return 0;
        }

        private List<ModItemStats> m_StatList;

        /*
        public static ModItem operator +(ModItem Item, ModItemStats Stat)
        {
            ModItemStats buffer = Item.m_StatList.Find(x => x.Stat == Stat.Stat);
            if (buffer != null)
            {
                buffer.fl
            }
            return Item;
        }
        */
    }
}
