using BepInEx;
using UnityEngine;
using RoR2;
using System.Reflection;
using System;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace PlexusUtils
{
   enum Priority:short
   {
        Last = short.MaxValue,
        Multiplicative = 16000,
        Additive = 8000,
        High = 1000,
        VeryHigh = 400,
        Critical = 200,
        Maximum = 1,
   }
    [Flags]
    enum FunctionTag:int
    {
        None = 0x0,
        Health = 0x1,
        Shield = 0x2,
        Regen = 0x4,
        MoveSpeed = 0x8,
        JumpPower = 0x10,
        JumpCount = 0x20,
        Damage = 0x40,
        AttackSpeed = 0x80,
        Crit = 0x100,
        Armor = 0x200,
        GeneralCoolDown = 0x400,
        PrimaryCoolDown = 0x800,
        SecondaryCoolDown = 0x1000,
        UtilityCoolDown = 0x2000,
        SpecialCoolDown = 0x4000,
        PrimaryCount = 0x8000,
        SecondaryCount = 0x10000,
        UtilityCount = 0x20000,
        SpecialCount = 0x40000,
        All = 0x7ffff,
    }


    class ModRecalculateCustom
    {
        public short RecalculatePriority = (short)Priority.Additive;
        public FunctionTag FlagOverWrite = FunctionTag.None;

        public virtual float RecalculateHealth(float baseValue, CharacterBody character) => baseValue;
        public virtual float RecalculateShield(float baseValue, CharacterBody character) => baseValue;
        public virtual float RecalculateRegen(float baseValue, CharacterBody character) => baseValue;

        public virtual float RecalculateMoveSpeed(float baseValue, CharacterBody character) => baseValue;
        public virtual float RecalculateJumpPower(float baseValue, CharacterBody character) => baseValue;
        public virtual float RecalculateJumpCount(float baseValue, CharacterBody character) => baseValue;

        public virtual float RecalculateDamage(float baseValue, CharacterBody character) => baseValue;
        public virtual float RecalculateAttackSpeed(float baseValue, CharacterBody character) => baseValue;
        public virtual float RecalculateCrit(float baseValue, CharacterBody character) => baseValue;
        public virtual float RecalculateArmor(float baseValue, CharacterBody character) => baseValue;

        public virtual float RecalculateGeneralCooldown(float baseValue, CharacterBody character) => baseValue;
        public virtual float RecalculatePrimaryCoolDown(float baseValue, CharacterBody character) => baseValue;
        public virtual float RecalculateSecondaryCooldown(float baseValue, CharacterBody character) => baseValue;
        public virtual float RecalculateUtilityCooldown(float baseValue, CharacterBody character) => baseValue;
        public virtual float RecalculateSpecialCooldown(float baseValue, CharacterBody character) => baseValue;

        public virtual float RecalculatePrimaryCount(float baseValue, CharacterBody character) => baseValue;
        public virtual float RecalculateSecondaryCount(float baseValue, CharacterBody character) => baseValue;
        public virtual float RecalculateUtilityCount(float baseValue, CharacterBody character) => baseValue;
        public virtual float RecalculateSpecialCount(float baseValue, CharacterBody character) => baseValue;
    }

    class DefaultRecalculate : ModRecalculateCustom
    {
        new public short RecalculatePriority = 0;

        public override float RecalculateHealth(float baseValue, CharacterBody character)
        {
            float MaxHealth = character.baseMaxHealth + (character.level - 1) * character.levelMaxHealth;
            float HealthBonusItem = 0;
            float hpbooster = 1;
            float healthDivider = 1;
            if ((bool)character.inventory)
            {
                HealthBonusItem += ModItemManager.GetBonusForStat(character, StatIndex.MaxHealth);

                if (character.inventory.GetItemCount(ItemIndex.Infusion) > 0)
                    HealthBonusItem += character.inventory.infusionBonus;
                hpbooster += ModItemManager.GetMultiplierForStat(character, StatIndex.MaxHealth);
                healthDivider = character.CalcLunarDaggerPower();
            }
            MaxHealth += HealthBonusItem;
            MaxHealth *= hpbooster / healthDivider;
            return MaxHealth;
        }

        public override float RecalculateShield(float baseValue, CharacterBody character)
        {
            float MaxShield = character.baseMaxShield + character.levelMaxShield * (character.level - 1);

            if (character.inventory) { 
                if (character.inventory.GetItemCount(ItemIndex.ShieldOnly) > 0)
                {

                    MaxShield += character.maxHealth * (1.25f + (character.inventory.GetItemCount(ItemIndex.ShieldOnly) - 1) * 0.5f);
                    character.SetPropertyValue("maxHealth", 1);
                }
            }
            //Buff
            if (character.HasBuff(BuffIndex.EngiShield))
                MaxShield += character.maxHealth * 1f;
            if (character.HasBuff(BuffIndex.EngiTeamShield))
                MaxShield += character.maxHealth * 0.5f;


            //NPC Overload Buff
            if (character.GetFieldValue<BuffMask>("buffMask").HasBuff(BuffIndex.AffixBlue))
            {
                character.SetPropertyValue("maxHealth", character.maxHealth * 0.5f);
                MaxShield += character.maxHealth;
            }
            if (character.inventory)
            {
                MaxShield += ModItemManager.GetBonusForStat(character, StatIndex.MaxShield);

                MaxShield *= (1 + ModItemManager.GetMultiplierForStat(character, StatIndex.MaxShield));
            }
            return MaxShield;
        }

        public override float RecalculateRegen(float baseValue, CharacterBody character)
        {
            float BaseRegen = (character.baseRegen + character.levelRegen * (character.level - 1)) * 2.5f;

            float RegenBonus = 0;
            float regenmult = 1;
            //Item Related
            if ((bool)character.inventory)
            {
                RegenBonus += ModItemManager.GetBonusForStat(character, StatIndex.Regen);
                if (character.outOfDanger)
                    RegenBonus += ModItemManager.GetBonusForStat(character, StatIndex.SafeRegen);
                if (character.inventory.GetItemCount(ItemIndex.HealthDecay) > 0 )
                    RegenBonus-= character.maxHealth / character.inventory.GetItemCount(ItemIndex.HealthDecay);
                regenmult += ModItemManager.GetMultiplierForStat(character, StatIndex.Regen);
                if (character.outOfDanger)
                    regenmult += ModItemManager.GetMultiplierForStat(character, StatIndex.SafeRegen);
            }

            float totalRegen = (BaseRegen * regenmult + RegenBonus);

            return totalRegen;

        }

        public override float RecalculateMoveSpeed(float baseValue, CharacterBody character)
        {
            float BaseMoveSpeed = character.baseMoveSpeed + character.levelMoveSpeed * (character.level - 1);

            float SpeedBonus = 1;


            //More weird stuff
            if ((bool)character.inventory)
                if (character.inventory.currentEquipmentIndex == EquipmentIndex.AffixYellow)
                    BaseMoveSpeed += 2;

            if (character.isSprinting)
                BaseMoveSpeed *= character.GetFieldValue<float>("sprintingSpeedMultiplier");


            //SpeedBonus
            if (character.HasBuff(BuffIndex.BugWings))
                SpeedBonus += 0.2f;
            if (character.HasBuff(BuffIndex.Warbanner))
                SpeedBonus += 0.3f;
            if (character.HasBuff(BuffIndex.EnrageAncientWisp))
                SpeedBonus += 0.4f;
            if (character.HasBuff(BuffIndex.CloakSpeed))
                SpeedBonus += 0.4f;
            if (character.HasBuff(BuffIndex.TempestSpeed))
                SpeedBonus += 1;
            if (character.HasBuff(BuffIndex.WarCryBuff))
                SpeedBonus += .5f;
            if (character.HasBuff(BuffIndex.EngiTeamShield))
                SpeedBonus += 0.3f;

            SpeedBonus += ModItemManager.GetMultiplierForStat(character, StatIndex.MoveSpeed);
            if (character.isSprinting)
                SpeedBonus += ModItemManager.GetMultiplierForStat(character, StatIndex.RunningMoveSpeed);
            if (character.outOfCombat && character.outOfDanger)
            {
                SpeedBonus += ModItemManager.GetMultiplierForStat(character, StatIndex.SafeMoveSpeed);
                if (character.isSprinting)
                    SpeedBonus += ModItemManager.GetMultiplierForStat(character, StatIndex.SafeRunningMoveSpeed);
            }

            //Debuff Speed
            float SpeedMalus = 1f;
            if (character.HasBuff(BuffIndex.Slow50))
                SpeedMalus += 0.5f;
            if (character.HasBuff(BuffIndex.Slow60))
                SpeedMalus += 0.6f;
            if (character.HasBuff(BuffIndex.Slow80))
                SpeedMalus += 0.8f;
            if (character.HasBuff(BuffIndex.ClayGoo))
                SpeedMalus += 0.5f;
            if (character.HasBuff(BuffIndex.Slow30))
                SpeedMalus += 0.3f;
            if (character.HasBuff(BuffIndex.Cripple))
                ++SpeedMalus;

            BaseMoveSpeed += ModItemManager.GetBonusForStat(character, StatIndex.MoveSpeed);
            if (character.isSprinting)
                BaseMoveSpeed += ModItemManager.GetBonusForStat(character, StatIndex.RunningMoveSpeed);
            if (character.outOfCombat && character.outOfDanger)
            {
                BaseMoveSpeed += ModItemManager.GetBonusForStat(character, StatIndex.SafeMoveSpeed);
                if (character.isSprinting)
                    BaseMoveSpeed += ModItemManager.GetBonusForStat(character, StatIndex.SafeRunningMoveSpeed);
            }

            float MoveSpeed = BaseMoveSpeed * (SpeedBonus / SpeedMalus);
            if ((bool)character.inventory)
            {
                MoveSpeed *= 1.0f - 0.05f * character.GetBuffCount(BuffIndex.BeetleJuice);
            }

            return MoveSpeed;
        }


        public override float RecalculateJumpPower(float baseValue, CharacterBody character)
        {
            float JumpPower = character.baseJumpPower + character.levelJumpPower * (character.level - 1) + ModItemManager.GetBonusForStat(character, StatIndex.JumpPower);
            JumpPower *= 1 + ModItemManager.GetMultiplierForStat(character, StatIndex.JumpPower);
            return JumpPower;
        }

        public override float RecalculateJumpCount(float baseValue, CharacterBody character)
        {
            float JumpCount = character.baseJumpCount + ModItemManager.GetBonusForStat(character, StatIndex.JumpCount);
            JumpCount *= 1 + ModItemManager.GetMultiplierForStat(character, StatIndex.JumpCount);
            return JumpCount;
        }

        public override float RecalculateDamage(float baseValue, CharacterBody character)
        {
            float BaseDamage = character.baseDamage + character.levelDamage * (character.level - 1);
            BaseDamage += ModItemManager.GetBonusForStat(character, StatIndex.Damage);

            float DamageBoost = 0;
            int DamageBoostCount = character.inventory ? character.inventory.GetItemCount(ItemIndex.BoostDamage) : 0;
            if (DamageBoostCount > 0)
                DamageBoost += DamageBoostCount * DamageBoost;
            DamageBoost -= 0.05f * character.GetBuffCount(BuffIndex.BeetleJuice);

            if (character.HasBuff(BuffIndex.GoldEmpowered))
                DamageBoost += 1;

            float DamageMult = DamageBoost + (character.CalcLunarDaggerPower());
            DamageMult += ModItemManager.GetMultiplierForStat(character, StatIndex.Damage);
            return BaseDamage * DamageMult;
        }

        public override float RecalculateAttackSpeed(float baseValue, CharacterBody character)
        {
            float BaseAttackSpeed = character.baseAttackSpeed + character.levelAttackSpeed * (character.level - 1);

            //Item efect
            float AttackSpeedBonus = 1f;
            if (character.inventory)
            {
                if (character.inventory.currentEquipmentIndex == EquipmentIndex.AffixYellow)
                    AttackSpeedBonus += 0.5f;
            }

            //Buffs
            float AttackSpeedMult = AttackSpeedBonus + character.GetFieldValue<int[]>("buffs")[2] * 0.12f;
            if (character.HasBuff(BuffIndex.Warbanner))
                AttackSpeedMult += 0.3f;
            if (character.HasBuff(BuffIndex.EnrageAncientWisp))
                AttackSpeedMult += 2f;
            if (character.HasBuff(BuffIndex.WarCryBuff))
                AttackSpeedMult += 1f;


            BaseAttackSpeed += ModItemManager.GetBonusForStat(character, StatIndex.AttackSpeed);
            AttackSpeedMult += ModItemManager.GetMultiplierForStat(character, StatIndex.AttackSpeed);
            float AttackSpeed = BaseAttackSpeed * AttackSpeedMult;
            //Debuff
            AttackSpeed *= 1 - (0.05f * character.GetBuffCount(BuffIndex.BeetleJuice));

            return AttackSpeed;
        }

        public override float RecalculateCrit(float baseValue, CharacterBody character)
        {
            float CriticalChance = character.baseCrit + character.levelCrit * (character.level - 1);


            CriticalChance += ModItemManager.GetBonusForStat(character, StatIndex.Crit);
            CriticalChance *= 1 + ModItemManager.GetMultiplierForStat(character, StatIndex.AttackSpeed);

            if (character.HasBuff(BuffIndex.FullCrit))
                CriticalChance += 100;


            return CriticalChance;
        }

        public override float RecalculateArmor(float baseValue, CharacterBody character)
        {
            float BaseArmor = character.baseArmor + character.levelArmor * (character.level - 1);
            float BonusArmor = 0;

            if (character.HasBuff(BuffIndex.ArmorBoost))
                BonusArmor += 200;
            if (character.HasBuff(BuffIndex.Cripple))
                BonusArmor -= 20;
            float TotalArmor = BaseArmor + BonusArmor;
            TotalArmor += ModItemManager.GetBonusForStat(character, StatIndex.Armor);
            if (character.isSprinting)
                TotalArmor += ModItemManager.GetBonusForStat(character, StatIndex.RunningArmor);
            TotalArmor *= 1 + ModItemManager.GetMultiplierForStat(character, StatIndex.Armor);
            if (character.isSprinting)
                TotalArmor *= 1 + ModItemManager.GetMultiplierForStat(character, StatIndex.RunningArmor);
            return TotalArmor;
        }

        public override float RecalculateGeneralCooldown(float baseValue, CharacterBody character)
        {
            float CoolDownMultiplier = 1f;
            CoolDownMultiplier += ModItemManager.GetBonusForStat(character, StatIndex.GlobalCoolDown);

            CoolDownMultiplier *= ModItemManager.GetMultiplierForStatCD(character, StatIndex.GlobalCoolDown);

            if (character.HasBuff(BuffIndex.GoldEmpowered))
                CoolDownMultiplier *= 0.25f;
            if (character.HasBuff(BuffIndex.NoCooldowns))
                CoolDownMultiplier = 0.0f;


            return CoolDownMultiplier;
        }

        public override float RecalculatePrimaryCoolDown(float baseValue, CharacterBody character)
        {
            float CoolDownMultiplier = 1f;
            CoolDownMultiplier += ModItemManager.GetBonusForStat(character, StatIndex.CoolDownPrimary);

            CoolDownMultiplier *= ModItemManager.GetMultiplierForStatCD(character, StatIndex.CoolDownPrimary);
            return CoolDownMultiplier;
        }
        public override float RecalculatePrimaryCount(float baseValue, CharacterBody character)
        {
            float count = 0;
            count += ModItemManager.GetBonusForStat(character, StatIndex.CountPrimary);

            count *= 1 + ModItemManager.GetMultiplierForStat(character, StatIndex.CountPrimary);
            return count;
        }
        public override float RecalculateSecondaryCooldown(float baseValue, CharacterBody character)
        {
            float CoolDownMultiplier = 1f;
            CoolDownMultiplier += ModItemManager.GetBonusForStat(character, StatIndex.CoolDownSecondary);

            CoolDownMultiplier *= ModItemManager.GetMultiplierForStatCD(character, StatIndex.CoolDownSecondary);
            return CoolDownMultiplier;
        }
        public override float RecalculateSecondaryCount(float baseValue, CharacterBody character)
        {
            float count = 0;
            count += ModItemManager.GetBonusForStat(character, StatIndex.CountSecondary);
            count *= 1 + ModItemManager.GetMultiplierForStat(character, StatIndex.CountSecondary);
            return count;
        }
        public override float RecalculateSpecialCooldown(float baseValue, CharacterBody character)
        {
            float CoolDownMultiplier = 1f;
            CoolDownMultiplier += ModItemManager.GetBonusForStat(character, StatIndex.CoolDownUtility);

            CoolDownMultiplier *= ModItemManager.GetMultiplierForStatCD(character, StatIndex.CoolDownUtility);
            return CoolDownMultiplier;
        }
        public override float RecalculateSpecialCount(float baseValue, CharacterBody character)
        {
            float count = 0;
            count += ModItemManager.GetBonusForStat(character, StatIndex.CountUtility);
            count *= 1 + ModItemManager.GetMultiplierForStat(character, StatIndex.CountUtility);
            return count;
        }
        public override float RecalculateUtilityCooldown(float baseValue, CharacterBody character)
        {
            float CoolDownMultiplier = 1f;
            CoolDownMultiplier += ModItemManager.GetBonusForStat(character, StatIndex.CoolDownSpecial);

            CoolDownMultiplier *= ModItemManager.GetMultiplierForStatCD(character, StatIndex.CoolDownSpecial);
            return CoolDownMultiplier;
        }
        public override float RecalculateUtilityCount(float baseValue, CharacterBody character)
        {
            float count = 0;
            count += ModItemManager.GetBonusForStat(character, StatIndex.CountSpecial);

            count *= 1 + ModItemManager.GetMultiplierForStat(character, StatIndex.CountSpecial);
            return count;
        }

    }


    static class SurvivorStatsAPI
    {
        public const string Dependency = "com.Plexus.ModRecalculate";
        public const string Version = "1.0.0";

        static List<ModRecalculateCustom> m_RecalulateList;
        static Dictionary<int,ModRecalculateCustom> m__temp_RecalulateDic;

        static public void Init()
        {
            m_RecalulateList = new List<ModRecalculateCustom>
            {
                new DefaultRecalculate()
            };
            ModItemManager.Init();
            ModEffects.Init();
            On.RoR2.CharacterBody.RecalculateStats += ModdedRecalculate;
        }

        static MethodInfo GetMethodInfo(Func<float, CharacterBody, float> f)
        {
            return f.Method;
        }

        static float StatHandler(MethodInfo method,CharacterBody character)
        {
            float value = 0;
            foreach (ModRecalculateCustom recal in m_RecalulateList)
            {
                value = (float)method.Invoke(recal,new object[2] { value, character });
            }
            return value;
        }

        

        static public void AddOrder(this Dictionary<int, ModRecalculateCustom>dic ,int pos, ModRecalculateCustom obj,bool warn = false)
        {
            try { 
                if (dic.ContainsKey(pos))
                {
                    AddOrder(dic, pos + 1, obj,true);
                }
                dic.Add(pos, obj);
                if (warn)
                    Debug.Log("Character Stat API warning : The loading priority for "+obj.ToString() + " priority : " + obj.RecalculatePriority + " is allready used, priotity : " + pos + " given");
            }
            catch (OverflowException)
            {
                throw new Exception("Error, the Minimum priority is allready used by : "+ dic[short.MaxValue].ToString() +", only one recalculate can be at the Minimum priority");
            }
        }

        static public void ReorderRecalculateList()
        {
            m__temp_RecalulateDic = new Dictionary<int, ModRecalculateCustom>();
            foreach(ModRecalculateCustom obj in m_RecalulateList)
            {
                m__temp_RecalulateDic.AddOrder(obj.RecalculatePriority, obj);
            }
            m_RecalulateList = new List<ModRecalculateCustom>();
            foreach(KeyValuePair<int, ModRecalculateCustom> kv in m__temp_RecalulateDic)
            {
                m_RecalulateList.Add(kv.Value);
            }
        }

        static public void AddCustomRecalculate(ModRecalculateCustom customRecalculate)
        {
            m_RecalulateList.Add(customRecalculate);
            ReorderRecalculateList();
        }
        static public void ModdedRecalculate(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody character)
        {
            if (character == null)
                return;

            //ModifyItem(character); //Will ahve to tinker if we leave the modder completly free to update item whenever they want or add a hook for that :shrug:

            character.SetPropertyValue("experience", (float)TeamManager.instance.GetTeamExperience(character.teamComponent.teamIndex));
            float l = TeamManager.instance.GetTeamLevel(character.teamComponent.teamIndex);
            if (character.inventory)
            {
                l += character.inventory.GetItemCount(ItemIndex.LevelBonus);

            }
            character.SetPropertyValue("level", l);
            float Level = character.level - 1f;

            character.SetPropertyValue("isElite", character.GetFieldValue<BuffMask>("buffMask").containsEliteBuff);

            float preHealth = character.maxHealth;
            float preShield = character.maxShield;

            character.SetPropertyValue("maxHealth"      , StatHandler(GetMethodInfo(new DefaultRecalculate().RecalculateHealth)         , character));
            character.SetPropertyValue("maxShield"      , StatHandler(GetMethodInfo(new DefaultRecalculate().RecalculateShield)         , character));

            character.SetPropertyValue("regen"          , StatHandler(GetMethodInfo(new DefaultRecalculate().RecalculateRegen)          , character));

            character.SetPropertyValue("moveSpeed"      , StatHandler(GetMethodInfo(new DefaultRecalculate().RecalculateMoveSpeed)      , character));
            character.SetPropertyValue("acceleration"   , character.moveSpeed / character.baseMoveSpeed * character.baseAcceleration);

            character.SetPropertyValue("jumpPower"      , StatHandler(GetMethodInfo(new DefaultRecalculate().RecalculateJumpPower)      , character));
            character.SetPropertyValue("maxJumpHeight"  , Trajectory.CalculateApex(character.jumpPower)); 
            character.SetPropertyValue("maxJumpCount"   , (int)StatHandler(GetMethodInfo(new DefaultRecalculate().RecalculateJumpCount) , character));

            character.SetPropertyValue("damage"         , StatHandler(GetMethodInfo(new DefaultRecalculate().RecalculateDamage)         , character));

            character.SetPropertyValue("attackSpeed"    , StatHandler(GetMethodInfo(new DefaultRecalculate().RecalculateAttackSpeed)    , character));

            character.SetPropertyValue("crit"           , StatHandler(GetMethodInfo(new DefaultRecalculate().RecalculateCrit)           , character));
            character.SetPropertyValue("armor"          , StatHandler(GetMethodInfo(new DefaultRecalculate().RecalculateArmor)          , character));
            
            //CoolDown 
            float CoolDownMultiplier = StatHandler(GetMethodInfo(new DefaultRecalculate().RecalculateGeneralCooldown), character);
            if ((bool)character.GetFieldValue<SkillLocator>("skillLocator").primary)
            {
                character.GetFieldValue<SkillLocator>("skillLocator").primary.cooldownScale = StatHandler(GetMethodInfo(new DefaultRecalculate().RecalculatePrimaryCoolDown), character) * CoolDownMultiplier;
                if (character.GetFieldValue<SkillLocator>("skillLocator").primary.baseMaxStock > 1)
                    character.GetFieldValue<SkillLocator>("skillLocator").primary.SetBonusStockFromBody((int)StatHandler(GetMethodInfo(new DefaultRecalculate().RecalculatePrimaryCount), character));
            }
            if ((bool)character.GetFieldValue<SkillLocator>("skillLocator").secondary)
            {
                character.GetFieldValue<SkillLocator>("skillLocator").secondary.cooldownScale = StatHandler(GetMethodInfo(new DefaultRecalculate().RecalculateSecondaryCooldown), character) * CoolDownMultiplier;
                character.GetFieldValue<SkillLocator>("skillLocator").secondary.SetBonusStockFromBody((int)StatHandler(GetMethodInfo(new DefaultRecalculate().RecalculateSecondaryCount), character));
            }
            if ((bool)character.GetFieldValue<SkillLocator>("skillLocator").utility)
            {
                character.GetFieldValue<SkillLocator>("skillLocator").utility.cooldownScale = StatHandler(GetMethodInfo(new DefaultRecalculate().RecalculateUtilityCooldown), character) * CoolDownMultiplier;
                character.GetFieldValue<SkillLocator>("skillLocator").utility.SetBonusStockFromBody((int)StatHandler(GetMethodInfo(new DefaultRecalculate().RecalculateUtilityCount), character));
            }
            if ((bool)character.GetFieldValue<SkillLocator>("skillLocator").special)
            {
                character.GetFieldValue<SkillLocator>("skillLocator").special.cooldownScale = StatHandler(GetMethodInfo(new DefaultRecalculate().RecalculateSpecialCooldown), character) * CoolDownMultiplier;
                if (character.GetFieldValue<SkillLocator>("skillLocator").special.baseMaxStock > 1)
                    character.GetFieldValue<SkillLocator>("skillLocator").special.SetBonusStockFromBody((int)StatHandler(GetMethodInfo(new DefaultRecalculate().RecalculateSpecialCount), character));
            }

            //Since it's not yet used in game, I leave that here for now
            character.SetPropertyValue("critHeal", 0.0f);
            if (character.inventory)
            {
                if (character.inventory.GetItemCount(ItemIndex.CritHeal) > 0)
                {
                    float crit = character.crit;
                    character.SetPropertyValue("crit", character.crit / (character.inventory.GetItemCount(ItemIndex.CritHeal) + 1));
                    character.SetPropertyValue("critHeal", crit - character.crit);
                }
            }

            
            //Health and Shield update
            if (NetworkServer.active)
            {
                float HealthOffset = character.maxHealth - preHealth;
                float ShieldOffset = character.maxShield - preShield;
                if (HealthOffset > 0)
                {
                    double num47 = character.healthComponent.Heal(HealthOffset, new ProcChainMask(), false);
                }
                else if (character.healthComponent.health > character.maxHealth)
                    character.healthComponent.Networkhealth = character.maxHealth;
                if (ShieldOffset > 0)
                    character.healthComponent.RechargeShield(ShieldOffset);
            }

            character.SetFieldValue("statsDirty", false);
        }

    }


    public static class ReflectionHelper
    {

        private static FieldInfo GetFieldInfo(Type type, string fieldName)
        {
            FieldInfo fieldInfo;
            do
            {
                fieldInfo = type.GetField(fieldName,
                       BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                type = type.BaseType;
            }
            while (fieldInfo == null && type != null);
            return fieldInfo;
        }

        private static PropertyInfo GetPropertyInfo(Type type, string fieldName)
        {
            PropertyInfo PropertyInfo;
            do
            {
                PropertyInfo = type.GetProperty(fieldName,
                       BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                type = type.BaseType;
            }
            while (PropertyInfo == null && type != null);
            return PropertyInfo;
        }

        public static T GetFieldValue<T>(this object obj, string fieldName)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            Type objType = obj.GetType();
            FieldInfo fieldInfo = GetFieldInfo(objType, fieldName);
            if (fieldInfo == null)
                throw new ArgumentOutOfRangeException("fieldName",
                  string.Format("Couldn't find field {0} in type {1}", fieldName, objType.FullName));
            return (T)fieldInfo.GetValue(obj);
        }

        public static void SetFieldValue(this object obj, string fieldName, object val)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            Type objType = obj.GetType();
            FieldInfo fieldInfo = GetFieldInfo(objType, fieldName);
            if (fieldInfo == null)
                throw new ArgumentOutOfRangeException("fieldName",
                  string.Format("Couldn't find field {0} in type {1}", fieldName, objType.FullName));
            fieldInfo.SetValue(obj, val);
        }

        public static T GetPropertyValue<T>(this object obj, string fieldName)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            Type objType = obj.GetType();
            PropertyInfo propertyInfo = GetPropertyInfo(objType, fieldName);
            if (propertyInfo == null)
                throw new ArgumentOutOfRangeException("propertyName",
                  string.Format("Couldn't find property {0} in type {1}", fieldName, objType.FullName));
            return (T)propertyInfo.GetValue(obj);
        }

        public static void SetPropertyValue(this object obj, string fieldName, object val)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            Type objType = obj.GetType();
            PropertyInfo propertyInfo = GetPropertyInfo(objType, fieldName);
            if (propertyInfo == null)
                throw new ArgumentOutOfRangeException("propertyName",
                  string.Format("Couldn't find property {0} in type {1}", fieldName, objType.FullName));
            propertyInfo.SetValue(obj, val);
        }
    }

}
