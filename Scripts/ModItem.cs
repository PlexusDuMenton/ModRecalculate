using BepInEx;
using UnityEngine;
using RoR2;
using System.Reflection;
using System;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace PlexusUtils
{
    public enum HitEffectType
    {
        OnHitEnemy = 0,
        OnHitAll = 1
    }
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
        /// <param name="MultStackBonus">Multiplicative bonus for each additional item the player own, for Cooldowns values of 0 are ignored</param>
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
        private int m_Index;
        private List<ModHitEffect> m_EffectList;

        private List<ModItemStat> m_StatList;


        #region properties
        public List<ModItemStat> GetStatsList { get { return m_StatList; } }
        public List<ModHitEffect> GetHitEffectList { get { return m_EffectList; } }
        public int Index { get { return m_Index; } private set { m_Index = value; } }

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


        public ModItem(int Index)
        {
            this.m_Index = Index;
            m_StatList = new List<ModItemStat>();
            m_EffectList = new List<ModHitEffect>();
        }

        public ModItem(int Index, List<ModItemStat> Stats)
        {
            this.m_Index = Index;
            m_StatList = Stats;
            m_EffectList = new List<ModHitEffect>();
        }

        public ModItem(int Index, ModItemStat Stat)
        {
            this.m_Index = Index;
            m_StatList = new List<ModItemStat>
            {
                Stat
            };
            m_EffectList = new List<ModHitEffect>();
        }
        public ModItem(int Index, ModItemStat Stat1, ModItemStat Stat2)
        {
            this.m_Index = Index;
            m_StatList = new List<ModItemStat>
            {
                Stat1,
                Stat2
            };
            m_EffectList = new List<ModHitEffect>();
        }
        public ModItem(int Index, ModItemStat Stat1, ModItemStat Stat2, ModItemStat Stat3)
        {
            this.m_Index = Index;
            m_StatList = new List<ModItemStat>
            {
                Stat1,
                Stat2,
                Stat3
            };
            m_EffectList = new List<ModHitEffect>();
        }
        public ModItem(int Index, ModItemStat Stat1, ModItemStat Stat2, ModItemStat Stat3, ModItemStat Stat4)
        {
            this.m_Index = Index;
            m_StatList = new List<ModItemStat>
            {
                Stat1,
                Stat2,
                Stat3,
                Stat4
            };
            m_EffectList = new List<ModHitEffect>();
        }

        #region Operator

        public static ModItem operator +(ModItem Item, ModHitEffect Effect)
        {
            if (!Item.m_EffectList.Exists(x => x.GetType() == Effect.GetType()))
            {
                Item.m_EffectList.Add(Effect);
            }
            return Item;
        }
        public static ModItem operator +(ModItem Item, List<ModHitEffect> Effects)
        {
            foreach (ModHitEffect Effect in Effects)
                if (!Item.m_EffectList.Exists(x => x.GetType() == Effect.GetType()))
                {
                    Item.m_EffectList.Add(Effect);
                }
            return Item;
        }


        public static ModItem operator +(ModItem Item, ModItemStat Stat)
        {
            if (Item.m_StatList.Exists(x => x.Stat == Stat.Stat))
            {
                Item.m_StatList[Item.m_StatList.FindIndex(x => x.Stat == Stat.Stat)] += Stat;
            }
            else
            {
                Item.m_StatList.Add(Stat);
            }
            return Item;
        }
        public static ModItem operator +(ModItem Item, List<ModItemStat> Stats)
        {
            foreach (ModItemStat Stat in Stats)
                if (Item.m_StatList.Exists(x => x.Stat == Stat.Stat))
                {
                    Item.m_StatList[Item.m_StatList.FindIndex(x => x.Stat == Stat.Stat)] += Stat;
                }
                else
                {
                    Item.m_StatList.Add(Stat);
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

    public class ModHitEffect
    {

        public HitEffectType EffectType = HitEffectType.OnHitEnemy;
        /// <summary>
        /// Check if the effect is Proc or not
        /// </summary>
        /// <param name="globalEventManager"></param>
        /// <param name="damageInfo"></param>
        /// <param name="victim"></param>
        /// <param name="itemCount"></param>
        /// <returns></returns>
        virtual public bool Condition(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim, int itemCount)
        {
            return true;
        }
        virtual public void Effect(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim, int itemCount)
        {

        }
    }


    static public class ModItemManager
    {
        static public Dictionary<int, ModItem> ModItemDictionary;


        static private Dictionary<int, ModItem> m_DefaultModItemDictionary;

        static public Dictionary<int, ModItem> DefaultModItemDictionary { get { return m_DefaultModItemDictionary; } }

        static private void DefaultOnHitEffect(int index, ModHitEffect HitEffect)
        {
            if (m_DefaultModItemDictionary.ContainsKey(index))
            {
                m_DefaultModItemDictionary[index] += HitEffect;
            } else
            {
                throw new Exception("ModItemManager ERROR : int does not exist in m_DefaultModItemDictionary\nBtw you shouldn't mess with this boi !  Index :  " + index);
            }
        }

        static private void DefaultStatItem(int index, ModItemStat stat)
        {
            if (m_DefaultModItemDictionary.ContainsKey(index))
            {
                m_DefaultModItemDictionary[index] += stat;
            }
            else
            {
                throw new Exception("ModItemManager ERROR : int does not exist in m_DefaultModItemDictionary\nBtw you shouldn't mess with this boi !  Index :  " + index);
            }
        }

        public static void AddOnHitEffect(int index, ModHitEffect HitEffect)
        {
            if (ModItemDictionary.ContainsKey(index))
            {
                ModItemDictionary[index] += HitEffect;
            }
            else
            {
                throw new Exception("ModItemManager ERROR : int does not exist in ModItemDictionary");
            }
        }
        public static void AddOnHitEffect(int index, List<ModHitEffect> HitEffects)
        {
            if (ModItemDictionary.ContainsKey(index))
            {
                ModItemDictionary[index] += HitEffects;
            }
            else
            {
                throw new Exception("ModItemManager ERROR : int does not exist in ModItemDictionary");
            }
        }

        static public void AddModItem(int index, ModItem ModItem)
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
        static public void AddStatToItem(int index, ModItemStat stat)
        {
            if (ModItemDictionary.ContainsKey(index))
            {
                ModItemDictionary[index] += stat;
            }
            else
            {
                throw new Exception("ModItemManager ERROR : int does not exist in ModItemDictionary");
            }
        }
        static public void AddStatToItem(int index, List<ModItemStat> stats)
        {
            if (ModItemDictionary.ContainsKey(index))
            {
                ModItemDictionary[index] += stats;
            }
            else
            {
                throw new Exception("ModItemManager ERROR : int does not exist in ModItemDictionary");
            }
        }

        static public ModItem GetModItem(int index)
        {
            if (ModItemDictionary.ContainsKey(index))
            {
                return ModItemDictionary[index];
            }
            else
            {
                throw new Exception("ModItemManager ERROR : int does not exist in ModItemDictionary");
            }
        }

        static public void Init()
        {
            m_DefaultModItemDictionary = new Dictionary<int, ModItem>();

            foreach (ItemIndex itemIndex in (ItemIndex[])Enum.GetValues(typeof(ItemIndex)))
            {
                if (itemIndex != ItemIndex.Count && itemIndex != ItemIndex.None)
                {
                    m_DefaultModItemDictionary.Add((int)itemIndex, new ModItem((int)itemIndex));
                }
            }

            //Default On Hit Effect
            DefaultOnHitEffect((int)ItemIndex.HealOnCrit, new HealOnCritHitReplace());
            DefaultOnHitEffect((int)ItemIndex.CooldownOnCrit, new CoolDownOnCritHitReplace());
            DefaultOnHitEffect((int)ItemIndex.AttackSpeedOnCrit, new AttackSpeedOnCritHitReplace());
            DefaultOnHitEffect((int)ItemIndex.Seed, new HealOnHitReplace());
            DefaultOnHitEffect((int)ItemIndex.BleedOnHit, new BleedOnHitReplace());
            DefaultOnHitEffect((int)ItemIndex.SlowOnHit, new SlowOnHitReplace());
            DefaultOnHitEffect((int)ItemIndex.GoldOnHit, new GoldOnHitReplace());
            DefaultOnHitEffect((int)ItemIndex.Missile, new MissileOnHitReplace());
            DefaultOnHitEffect((int)ItemIndex.ChainLightning, new UkeleleOnHitReplace());
            DefaultOnHitEffect((int)ItemIndex.BounceNearby, new HookEffectReplace());
            DefaultOnHitEffect((int)ItemIndex.StickyBomb, new StickyBombOnHitReplace());
            DefaultOnHitEffect((int)ItemIndex.IceRing, new IceRingEffectReplace());
            DefaultOnHitEffect((int)ItemIndex.FireRing, new FireRingEffectReplace());
            DefaultOnHitEffect((int)ItemIndex.Behemoth, new BehemotEffectReplace());

            //Default Stats
            DefaultStatItem((int)ItemIndex.Knurl, new ModItemStat(40, StatIndex.MaxHealth));
            DefaultStatItem((int)ItemIndex.BoostHp, new ModItemStat(0, 0, 0.1f, StatIndex.MaxHealth));
            DefaultStatItem((int)ItemIndex.PersonalShield, new ModItemStat(25, StatIndex.MaxShield));
            DefaultStatItem((int)ItemIndex.HealWhileSafe, new ModItemStat(0, 0, 2.5f, 1.5f, StatIndex.SafeRegen));
            DefaultStatItem((int)ItemIndex.Knurl, new ModItemStat(1.6f, StatIndex.Regen));
            DefaultStatItem((int)ItemIndex.HealthDecay, new ModItemStat(0, 0, -0.1f, StatIndex.Regen));
            DefaultStatItem((int)ItemIndex.SprintOutOfCombat, new ModItemStat(0, 0, 0.3f, StatIndex.SafeRunningMoveSpeed));
            DefaultStatItem((int)ItemIndex.Hoof, new ModItemStat(0, 0, 0.14f, StatIndex.MoveSpeed));
            DefaultStatItem((int)ItemIndex.SprintBonus, new ModItemStat(0, 0, 0.3f, 0.2f, StatIndex.RunningMoveSpeed));
            DefaultStatItem((int)ItemIndex.Feather, new ModItemStat(1, StatIndex.JumpCount));
            DefaultStatItem((int)ItemIndex.BoostDamage, new ModItemStat(0, 0, 0.1f, StatIndex.Damage));
            DefaultStatItem((int)ItemIndex.Syringe, new ModItemStat(0, 0, 0.15f, StatIndex.AttackSpeed));
            DefaultStatItem((int)ItemIndex.CritGlasses, new ModItemStat(10, StatIndex.Crit));
            DefaultStatItem((int)ItemIndex.AttackSpeedOnCrit, new ModItemStat(5, 0, StatIndex.Crit));
            DefaultStatItem((int)ItemIndex.CritHeal, new ModItemStat(5, 0, StatIndex.Crit));
            DefaultStatItem((int)ItemIndex.HealOnCrit, new ModItemStat(5, 0, StatIndex.Crit));
            DefaultStatItem((int)ItemIndex.CooldownOnCrit, new ModItemStat(5, 0, StatIndex.Crit));
            DefaultStatItem((int)ItemIndex.SprintArmor, new ModItemStat(30, StatIndex.RunningArmor));
            DefaultStatItem((int)ItemIndex.DrizzlePlayerHelper, new ModItemStat(70, StatIndex.Armor));
            DefaultStatItem((int)ItemIndex.AlienHead, new ModItemStat(0, 0, 0.75f, StatIndex.GlobalCoolDown));
            DefaultStatItem((int)ItemIndex.UtilitySkillMagazine, new ModItemStat(0, 0, 2f / 3f, 1, StatIndex.CoolDownUtility));
            DefaultStatItem((int)ItemIndex.SecondarySkillMagazine, new ModItemStat(1, StatIndex.CountSecondary));
            DefaultStatItem((int)ItemIndex.UtilitySkillMagazine, new ModItemStat(2, StatIndex.CountUtility));

            Update();
        }

        static public void Update()
        {

            ModItemDictionary = new Dictionary<int, ModItem>();

            foreach (KeyValuePair<int, ModItem> kv in m_DefaultModItemDictionary)
            {
                ModItemDictionary.Add(kv.Key, kv.Value);
            }
        }

        static public float GetBonusForStat(CharacterBody c, StatIndex stat)
        {
            float value = 0;
            if (c.inventory)
            {
                foreach (KeyValuePair<int,ModItem> kv in ModItemDictionary)
                {
                    if (ModItemDictionary.ContainsKey(kv.Key) && c.inventory.GetItemCount(kv.Key) > 0)
                        value += kv.Value.GetFlatBonusFromCount(stat, c.inventory.GetItemCount(kv.Key));
                }
            }
            return value;
        }
        static public float GetMultiplierForStat(CharacterBody c, StatIndex stat)
        {
            float value = 0;
            if (c.inventory)
            {
                foreach (KeyValuePair<int, ModItem> kv in ModItemDictionary)
                {
                    if (ModItemDictionary.ContainsKey(kv.Key) && c.inventory.GetItemCount(kv.Key) > 0)
                        value += kv.Value.GetMultStackBonusFromCount(stat, c.inventory.GetItemCount(kv.Key));
                }
            }
            return value;
        }

        static public float GetMultiplierForStatCD(CharacterBody c, StatIndex stat)
        {
            float value = 1;
            if (c.inventory)
            {
                foreach (KeyValuePair<int, ModItem> kv in ModItemDictionary)
                {
                    if (ModItemDictionary.ContainsKey(kv.Key) && c.inventory.GetItemCount(kv.Key) > 0)
                        if (kv.Value.GetMultStackBonusFromCount(stat, c.inventory.GetItemCount(kv.Key)) != 0)
                            value *= kv.Value.GetMultStackBonusFromCount(stat, c.inventory.GetItemCount(kv.Key));
                }
            }
            return value;
        }

        static public void OnHitEnemyEffects(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim)
        {
            float procCoefficient = damageInfo.procCoefficient;
            CharacterBody body = damageInfo.attacker.GetComponent<CharacterBody>();
            CharacterMaster master = body.master;
            if (!(bool)body || procCoefficient <= 0.0 || !(bool)body || !(bool)master || !(bool)master.inventory)
                return;

            Inventory inventory = master.inventory;

            foreach (KeyValuePair<int, ModItem> Kv in ModItemDictionary)
            {
                int count = inventory.GetItemCount(Kv.Key);
                if (count > 0)
                {
                    foreach (ModHitEffect HitEffects in Kv.Value.GetHitEffectList)
                    {

                        if (HitEffects.EffectType == HitEffectType.OnHitEnemy && HitEffects.Condition(globalEventManager, damageInfo, victim, count))
                        {
                            HitEffects.Effect(globalEventManager, damageInfo, victim, count);
                        }
                    }
                }
            }
        }

        static public void OnHitAllEffects(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim)
        {
            float procCoefficient = damageInfo.procCoefficient;
            CharacterBody body = damageInfo.attacker.GetComponent<CharacterBody>();
            CharacterMaster master = body.master;
            if (!(bool)body || procCoefficient <= 0.0 || !(bool)body || !(bool)master || !(bool)master.inventory)
                return;

            Inventory inventory = master.inventory;

            foreach (KeyValuePair<int, ModItem> Kv in ModItemDictionary)
            {
                int count = inventory.GetItemCount(Kv.Key);
                if (count > 0)
                {
                    foreach (ModHitEffect HitEffects in Kv.Value.GetHitEffectList)
                    {

                        if (HitEffects.EffectType == HitEffectType.OnHitAll && HitEffects.Condition(globalEventManager, damageInfo, victim, count))
                        {
                            HitEffects.Effect(globalEventManager, damageInfo, victim, count);
                        }
                    }
                }
            }
        }

    }

    static class InventoryExtender
    {
        public static int GetItemCount(this Inventory inv, int ItemIndex)
        {
            return inv.GetFieldValue<int[]>("itemStacks")[ItemIndex];
        }
    }
}
