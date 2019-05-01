﻿using BepInEx;
using UnityEngine;
using RoR2;
using AetherLib.Util.Reflection;
using System;
using UnityEngine.Networking;


/*
    CharacterDefault*** : Before Item are apllied
    ***Effect : Item specific hook
    ***Recalculate : Applyed after all item are applied
    PostRecalculate apply hook before the Health and Shield are updated

    Hook are additional,if you want to act multiplicative, do it in PostRecalculate
    Cooldown hook are MULTIPLICATIVE with other mod/BaseValue, if you want to make it Additional/Substractif, do it in PostRecalculate


    Example Application : 

    public class ExampleClass : BaseUnityPlugin
    {
       

       public float BonusHealth(CharacterBody character) // Exponential Health bonus
       {
           float MaxHealth = character.baseMaxHealth;
           float e = 1.3f
           return character.levelMaxHealth * (Mathf.Pow(character.level - 1, e) - character.level);//We reduce "character.level" to prevent this to stack with the actual base value
       }

       public float ShieldCalculate(CharacterBody character)//Make so Shield Give 5 additional shield per character level
       {
           float ShieldBoost = 5;
           return character.inventory.GetItemCount(ItemIndex.PersonalShield) * (ShieldBoost * (character.level - 1));
       }

       public void PostRecalculateFunc(CharacterBody character)
       {
           //Convert Half the Health as Damage with 0.1 ratio : 

           float BonusDamage = character.maxHealth*0.05f;
           character.SetPropertyValue("maxHealth", character.maxHealth*0.5f);
           character.SetPropertyValue("damage", BonusDamage);
       }

       public void Awake()
       {
           ModRecalculate.HealthRecalculation += delegate { return 5; }; //Simple +5 health after item are applied
           ModRecalculate.CharacterDefaultHealth += BonusHealth; // Apply BonusHealth function result to Health before item are applied
           ModRecalculate.ShieldItemEffect += ShieldCalculate;  // Apply Shield bonus function result to shield
           ModRecalculate.PostRecalculate += PostRecalculateFunc;  // Apply the post recalculate function before the Health and Shield is updated
       }
    }


       HOOK LIST (IN ORDER OF CALL) :
           CharacterDefaultHealth
           InfusionEffect
           KnurlMaxHpEffect
           ItemBoosHpEffect
           HealthRecalculation

           CharacterDefaultShield
           TranscendenceEffect
           ShieldItemEffect
           ShieldRecalculation

           CharacterDefaultRegen
           SlugEffect
           KnurlRegenEffect
           HealthDecayEffect
           RegenRecalculation

           CharacterDefaultSpeed
           RedWimpHoofEffect
           EnergyDrinkEffect
           BettleJuiceSpeedEffect
           MoveSpeedRecalculation

           JumpPower
           JumpCount

           CharacterDefaultDamage
           BettleJuiceDamageEffect
           DamageRecalculation

           CharacterDefaultAttackSpeed
           SyringueEffect
           BettleJuiceAttackSpeedEffect
           AttackSpeedRecalculation

           CharacterDefaultCrit
           GlassesEffect
           CritRecalculation

           CharacterDefaultArmor
           BucklerEffect
           ArmorRecalculation

           AlienHeadEffect
           CoolDownRecalculation

           PrimaryCoolDownMultiplier
           PrimaryStackCount

           SecondaryCoolDownMultiplier
           SecondaryStackCount

           UtilityCoolDownMultiplier
           UtilityStackCount

           PostRecalculate

       */


    

namespace PlexusUtils
{

    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Plexus.ModRecalculate", "ModRecalculate", "0.1.0")]

    public class PlexusUtils : BaseUnityPlugin
    {
        public void Awake()
        {
            ModRecalculate.Init();
        }
    }

    static public class ModRecalculate
    {

        #region Hook
        
        //Used to add/Multiply together Hooks
        private static float HookHandler(MulticastDelegate e ,CharacterBody character)
        {
            float value = 0;
            foreach (Delegate d in e.GetInvocationList())
            {
                value += (float)d.DynamicInvoke(character);
            }
            return value;
        }
        private static float HookHandlerMultiplier(MulticastDelegate e, CharacterBody character)
        {
            float value = (float)(e.GetInvocationList()[0].DynamicInvoke(character));
            foreach (Delegate d in e.GetInvocationList())
            {
                value *= (float)d.DynamicInvoke(character);
            }
            return value;
        }


        //Hook to apply change before the Update of Health and Shield
        public delegate void Hook_PostRecalculateHook(CharacterBody character);
        public static event Hook_PostRecalculateHook PostRecalculate;


        //Health related Hook
        
        public delegate float Hook_HealthRecalculation(CharacterBody character);
        public static event Hook_HealthRecalculation HealthRecalculation;
        public delegate float Hook_CharacterDefaultHealth(CharacterBody character);
        public static event Hook_CharacterDefaultHealth CharacterDefaultHealth;
        public delegate float Hook_InfusionEffect(CharacterBody character);
        public static event Hook_InfusionEffect InfusionEffect;
        public delegate float Hook_KnurlMaxHpEffect(CharacterBody character);
        public static event Hook_KnurlMaxHpEffect KnurlMaxHpEffect;
        public delegate float Hook_ItemBoosHpEffect(CharacterBody character);
        public static event Hook_ItemBoosHpEffect ItemBoosHpEffect;

        //Shield Related Hook
        public delegate float Hook_ShieldRecalculation(CharacterBody character);
        public static event Hook_ShieldRecalculation ShieldRecalculation;
        public delegate float Hook_CharacterDefaultShield(CharacterBody character);
        public static event Hook_CharacterDefaultShield CharacterDefaultShield;
        public delegate float Hook_TranscendenceEffect(CharacterBody character);
        public static event Hook_ShieldRecalculation TranscendenceEffect;
        public delegate float Hook_ShieldItemEffect(CharacterBody character);
        public static event Hook_ShieldItemEffect ShieldItemEffect;

        //HealthRegen related Hook
        public delegate float Hook_RegenRecalculation(CharacterBody character);
        public static event Hook_RegenRecalculation RegenRecalculation;
        public delegate float Hook_CharacterDefaultRegen(CharacterBody character);
        public static event Hook_CharacterDefaultRegen CharacterDefaultRegen;
        public delegate float Hook_SlugEffect(CharacterBody character);
        public static event Hook_SlugEffect SlugEffect;
        public delegate float Hook_KnurlRegenEffect(CharacterBody character);
        public static event Hook_KnurlRegenEffect KnurlRegenEffect;
        public delegate float Hook_HealthDecayEffect(CharacterBody character);
        public static event Hook_HealthDecayEffect HealthDecayEffect;

        //MoveSpeed related Hook
        public delegate float Hook_MoveSpeedRecalculation(CharacterBody character);
        public static event Hook_MoveSpeedRecalculation MoveSpeedRecalculation;
        public delegate float Hook_CharacterDefaultSpeed(CharacterBody character);
        public static event Hook_CharacterDefaultSpeed CharacterDefaultSpeed;
        public delegate float Hook_RedWimpHoofEffect(CharacterBody character);
        public static event Hook_RedWimpHoofEffect RedWimpHoofEffect;
        public delegate float Hook_EnergyDrinkEffect(CharacterBody character);
        public static event Hook_EnergyDrinkEffect EnergyDrinkEffect;
        public delegate float Hook_BettleJuiceSpeedEffect(CharacterBody character);
        public static event Hook_BettleJuiceSpeedEffect BettleJuiceSpeedEffect;

        public delegate float Hook_JumpPower(CharacterBody character);
        public static event Hook_JumpPower JumpPower;
        public delegate float Hook_JumpCount(CharacterBody character);
        public static event Hook_JumpCount JumpCount;

        public delegate float Hook_DamageRecalculation(CharacterBody character);
        public static event Hook_DamageRecalculation DamageRecalculation;
        public delegate float Hook_CharacterDefaultDamage(CharacterBody character);
        public static event Hook_CharacterDefaultDamage CharacterDefaultDamage;
        public delegate float Hook_BettleJuiceDamageEffect(CharacterBody character);
        public static event Hook_BettleJuiceDamageEffect BettleJuiceDamageEffect;

        public delegate float Hook_AttackSpeedRecalculation(CharacterBody character);
        public static event Hook_AttackSpeedRecalculation AttackSpeedRecalculation;
        public delegate float Hook_CharacterDefaultAttackSpeed(CharacterBody character);
        public static event Hook_CharacterDefaultAttackSpeed CharacterDefaultAttackSpeed;
        public delegate float Hook_SyringueEffect(CharacterBody character);
        public static event Hook_SyringueEffect SyringueEffect;
        public delegate float Hook_BettleJuiceAttackSpeedEffect(CharacterBody character);
        public static event Hook_BettleJuiceAttackSpeedEffect BettleJuiceAttackSpeedEffect;

        public delegate float Hook_CritRecalculation(CharacterBody character);
        public static event Hook_CritRecalculation CritRecalculation;
        public delegate float Hook_CharacterDefaultCrit(CharacterBody character);
        public static event Hook_CharacterDefaultCrit CharacterDefaultCrit;
        public delegate float Hook_GlassesEffect(CharacterBody character);
        public static event Hook_GlassesEffect GlassesEffect;

        public delegate float Hook_ArmorRecalculation(CharacterBody character);
        public static event Hook_ArmorRecalculation ArmorRecalculation;
        public delegate float Hook_CharacterDefaultArmor(CharacterBody character);
        public static event Hook_CharacterDefaultArmor CharacterDefaultArmor;
        public delegate float Hook_BucklerEffect(CharacterBody character);
        public static event Hook_BucklerEffect BucklerEffect;

        public delegate float Hook_CoolDownRecalculation(CharacterBody character);
        public static event Hook_CoolDownRecalculation CoolDownRecalculation;
        public delegate float Hook_AlienHeadEffect(CharacterBody character);
        public static event Hook_AlienHeadEffect AlienHeadEffect;

        public delegate float Hook_PrimaryCoolDownMultiplier(CharacterBody character);
        public static event Hook_PrimaryCoolDownMultiplier PrimaryCoolDownMultiplier;
        public delegate float Hook_SecondaryCoolDownMultiplier(CharacterBody character);
        public static event Hook_SecondaryCoolDownMultiplier SecondaryCoolDownMultiplier;
        public delegate float Hook_UtilityCoolDownMultiplier(CharacterBody character);
        public static event Hook_UtilityCoolDownMultiplier UtilityCoolDownMultiplier;

        public delegate float Hook_PrimaryStackCount(CharacterBody character);
        public static event Hook_PrimaryStackCount PrimaryStackCount;
        public delegate float Hook_SecondaryStackCount(CharacterBody character);
        public static event Hook_SecondaryStackCount SecondaryStackCount;
        public delegate float Hook_UtilityStackCount(CharacterBody character);
        public static event Hook_UtilityStackCount UtilityStackCount;
        #endregion


        

        //Used to add the defaults Hook
        static public void Init()
        {

            //Health
            CharacterDefaultHealth = Base_CharacterDefaultHealth;
            InfusionEffect = Base_InfusionEffect;
            KnurlMaxHpEffect = Base_KnurlMaxHpEffect;
            ItemBoosHpEffect = Base_ItemBoosHpEffect;
            HealthRecalculation = Base_RecalculateHealth;

            //Shield
            CharacterDefaultShield = Base_CharacterDefaultShield;
            TranscendenceEffect = Base_TranscendenceEffect;
            ShieldItemEffect = Base_ShieldItemEffect;
            ShieldRecalculation = Base_ShieldRecalculation;

            //Health regen
            CharacterDefaultRegen = Base_CharacterDefaultRegen;
            SlugEffect = Base_SlugEffect;
            KnurlRegenEffect = Base_KnurlRegenEffect;
            HealthDecayEffect = Base_HealthDecayEffect;
            RegenRecalculation = Base_RegenRecalculation;

            //MoveSpeed
            CharacterDefaultSpeed = Base_CharacterDefaultSpeed;
            RedWimpHoofEffect = Base_RedWimpHoofEffect;
            EnergyDrinkEffect = Base_EnergyDrinkEffect;
            BettleJuiceSpeedEffect = Base_BettleJuiceSpeedEffect;
            MoveSpeedRecalculation = Base_MoveSpeedRecalculation;

            //Mobility misc
            JumpPower = Base_JumpPower;
            JumpCount = Base_JumpCount;

            //Damage
            CharacterDefaultDamage = Base_CharacterDefaultDamage;
            BettleJuiceDamageEffect = Base_BettleJuiceDamageEffect;
            DamageRecalculation = Base_DamageRecalculation;

            //AttackSpeed
            AttackSpeedRecalculation = Base_AttackSpeedRecalculation;
            CharacterDefaultAttackSpeed = Base_CharacterDefaultAttackSpeed;
            SyringueEffect = Base_SyringueEffect;
            BettleJuiceAttackSpeedEffect = Base_BettleJuiceAttackSpeedEffect;

            //Critical Chance
            CritRecalculation = Base_CritRecalculation;
            CharacterDefaultCrit = Base_CharacterDefaultCrit;
            GlassesEffect = Base_GlassesEffect;

            //Armor
            CharacterDefaultArmor = Base_CharacterDefaultArmor;
            BucklerEffect = Base_BucklerEffect;
            ArmorRecalculation = Base_ArmorRecalculation;

            //Cooldown
            CoolDownRecalculation = Base_CoolDownRecalculation;
            AlienHeadEffect = Base_AlienHeadEffect;

            //ability bonus cooldown
            PrimaryCoolDownMultiplier = Base_PrimaryCoolDownMultiplier;
            SecondaryCoolDownMultiplier = Base_SecondaryCoolDownMultiplier;
            UtilityCoolDownMultiplier = Base_UtilityCoolDownMultiplier;

            //Ability Stack Count
            PrimaryStackCount = Base_PrimaryStackCount;
            SecondaryStackCount = Base_SecondaryStackCount;
            UtilityStackCount = Base_UtilityStackCount;

            Debug.Log(HealthRecalculation.GetType());
            On.RoR2.CharacterBody.RecalculateStats += ModdedRecalculate;
        }


        //HEALTH FUNCTIONS
        static public float Base_CharacterDefaultHealth(CharacterBody character)
        {
            return character.baseMaxHealth + (character.level - 1) * character.levelMaxHealth;
        }
        static public float Base_InfusionEffect(CharacterBody character)
        {
            if (character.inventory.GetItemCount(ItemIndex.Infusion) > 0)
                return character.inventory.infusionBonus;
            return 0;
        }
        static public float Base_KnurlMaxHpEffect(CharacterBody character)
        {
            return character.inventory.GetItemCount(ItemIndex.Knurl) * 40;
        }
        static public float Base_ItemBoosHpEffect(CharacterBody character)
        {
            return 1 + character.inventory.GetItemCount(ItemIndex.BoostHp) * 0.1f;
        }
        static public float Base_RecalculateHealth(CharacterBody character)
        {
            //CharacterLinked Health Stats
            float MaxHealth = HookHandler(CharacterDefaultHealth,character);
            float HealthBonusItem = 0;
            float hpbooster = 0;

            //Item Linked Bonus
            if ((bool)character.inventory) { 
                //Item Flat Bonus
                HealthBonusItem += HookHandler(InfusionEffect, character);
                HealthBonusItem += HookHandler(KnurlMaxHpEffect, character);

                //Item MultiplierBonus
                hpbooster = HookHandler(ItemBoosHpEffect,character);
            }

            //Applying flat bonus and Level up bonus
            MaxHealth = MaxHealth + HealthBonusItem;

            //Applying Shaped Glass and mult bonus effects
            if ((bool)character.inventory)
            {
                MaxHealth *= hpbooster / character.CalcLunarDaggerPower();
            }

            return MaxHealth;
        }

        //SHIELD FUNCTIONS
        static public float Base_CharacterDefaultShield(CharacterBody character)
        {
            return (character.baseMaxShield + character.levelMaxShield * (character.level - 1));
        }
        static public float Base_TranscendenceEffect(CharacterBody character)
        {
            float shield = 0;
            if (character.inventory.GetItemCount(ItemIndex.ShieldOnly) > 0)
            {

                shield = character.maxHealth * (1.5f + (character.inventory.GetItemCount(ItemIndex.ShieldOnly) - 1) * 0.25f);
                character.SetPropertyValue("maxHealth", 1);
            }
            return shield;
        }
        static public float Base_ShieldItemEffect(CharacterBody character)
        {
            return character.inventory.GetItemCount(ItemIndex.PersonalShield) * 25;
        }
        static public float Base_ShieldRecalculation(CharacterBody character)
        {
            //CharacterLinked Shield Stats
            float MaxShield = HookHandler(CharacterDefaultShield,character);

            //ShieldItem Calculation (There is hook for it)
            if ((bool)character.inventory)
            {
                MaxShield += HookHandler(TranscendenceEffect,character);
                MaxShield += HookHandler(ShieldItemEffect,character);
            }

            //Default Game Buff
            if (character.HasBuff(BuffIndex.EngiShield))
                MaxShield += character.maxHealth * 1f;
            if (character.HasBuff(BuffIndex.EngiTeamShield))
                MaxShield += character.maxHealth * 0.5f;

            
            //NPC elite buff ?
            if (character.GetFieldValue<BuffMask>("buffMask").HasBuff(BuffIndex.AffixBlue))
            {
                character.SetPropertyValue("maxHealth", character.maxHealth * 0.5f);
                MaxShield += character.maxHealth;
            }

            return MaxShield;
        }

        //REGEN FUNCTIONS
        static public float Base_CharacterDefaultRegen(CharacterBody character)
        {
            return (character.baseRegen + character.levelRegen * (character.level - 1)) * 2.5f;
        }
        static public float Base_SlugEffect(CharacterBody character)
        {
            if (character.outOfDanger && character.inventory.GetItemCount(ItemIndex.HealWhileSafe) > 0)
                return (2.5f + (character.inventory.GetItemCount(ItemIndex.HealWhileSafe) - 1) * 1.5f);
            return 0;
        }
        static public float Base_KnurlRegenEffect(CharacterBody character)
        {
            return character.inventory.GetItemCount(ItemIndex.Knurl) * 1.6f;
        }
        static public float Base_HealthDecayEffect(CharacterBody character) //I guess that just for debuff ??
        {
            if (character.inventory.GetItemCount(ItemIndex.HealthDecay) > 0)
                return character.maxHealth / character.inventory.GetItemCount(ItemIndex.HealthDecay);
            return 0;
        }
        static public float Base_RegenRecalculation(CharacterBody character)
        {

            float BaseRegen = HookHandler(CharacterDefaultRegen,character);
            float RegenBonus = 0;
            //Item Related
            if ((bool)character.inventory)
            {
                RegenBonus *= HookHandler(SlugEffect,character);
                RegenBonus += HookHandler(KnurlRegenEffect,character);
                RegenBonus -= HookHandler(HealthDecayEffect,character);
            }
            return  BaseRegen + RegenBonus;
            
        }

        //MoveSpeed
        static public float Base_CharacterDefaultSpeed(CharacterBody character)
        {
            return character.baseMoveSpeed + character.levelMoveSpeed * (character.level - 1);
        }
        static public float Base_RedWimpHoofEffect(CharacterBody character) //SinceRedWimp and hoof are linked, also I'm too lazy to rewrite this ¯\_(ツ)_/¯
        {
            float RedWinpBonus = 1f;

            //Couldn't realy pinpoints what this thing realy is, guess it's unimplemented stuff
            if (Run.instance.enabledArtifacts.HasArtifact(ArtifactIndex.Spirit))
            {
                float num33 = 1f;
                if ((bool)character.healthComponent)
                    num33 = character.healthComponent.combinedHealthFraction;
                RedWinpBonus += 1f - num33;
            }

            if ((bool)character.inventory)
            {
                if (character.outOfCombat && character.outOfDanger && character.inventory.GetItemCount(ItemIndex.SprintOutOfCombat) > 0)
                    RedWinpBonus += (float)character.inventory.GetItemCount(ItemIndex.SprintOutOfCombat) * 0.3f;

                return RedWinpBonus + (float)character.inventory.GetItemCount(ItemIndex.Hoof) * 0.14f;
            }
            return RedWinpBonus;
        }
        static public float Base_EnergyDrinkEffect(CharacterBody character)
        {
            if (character.isSprinting && character.inventory.GetItemCount(ItemIndex.SprintBonus) > 0)
                return (0.1f + 0.2f * character.inventory.GetItemCount(ItemIndex.SprintBonus)) / character.GetFieldValue<float>("sprintingSpeedMultiplier");
            return 0;
        }
        static public float Base_BettleJuiceSpeedEffect(CharacterBody character) //My guess is that it's just a slow debuff
        {
            
            return 1.0f - 0.05f * character.GetBuffCount(BuffIndex.BeetleJuice);
        }
        static public float Base_MoveSpeedRecalculation(CharacterBody character)
        {
            float BaseMoveSpeed = HookHandler(CharacterDefaultSpeed,character);

            float SpeedBonus = HookHandler(RedWimpHoofEffect,character);


            //More weird stuff
            if ((bool)character.inventory)
                if (character.inventory.currentEquipmentIndex == EquipmentIndex.AffixYellow)
                    BaseMoveSpeed += 2f;

            if (character.isSprinting)
                BaseMoveSpeed *= character.GetFieldValue<float>("sprintingSpeedMultiplier");


            //SpeedBonus
            if ((bool)character.inventory)
            {
                SpeedBonus += HookHandler(EnergyDrinkEffect,character);
            }
            if (character.HasBuff(BuffIndex.BugWings))
                SpeedBonus += 0.2f;
            if (character.HasBuff(BuffIndex.Warbanner))
                SpeedBonus += 0.3f;
            if (character.HasBuff(BuffIndex.EnrageAncientWisp))
                SpeedBonus += 0.4f;
            if (character.HasBuff(BuffIndex.CloakSpeed))
                SpeedBonus += 0.4f;
            if (character.HasBuff(BuffIndex.TempestSpeed))
                ++SpeedBonus;
            if (character.HasBuff(BuffIndex.WarCryBuff))
                SpeedBonus += 0.5f;
            if (character.HasBuff(BuffIndex.EngiTeamShield))
                SpeedBonus += 0.3f;


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

            float MoveSpeed = BaseMoveSpeed * (SpeedBonus / SpeedMalus);
            if ((bool)character.inventory)
            {
                MoveSpeed *= HookHandler(BettleJuiceSpeedEffect,character);
            }

            return MoveSpeed;
        }

        //Mobility
        static public float Base_JumpPower(CharacterBody character)
        {
            return character.baseJumpPower + character.levelJumpPower * (character.level-1);
        }
        static public float Base_JumpCount(CharacterBody character)
        {
            if (character.inventory)
                return character.baseJumpCount + character.inventory.GetItemCount(ItemIndex.Feather);
            return character.baseJumpCount;
        }

        //Damage
        static public float Base_CharacterDefaultDamage(CharacterBody character)
        {
            return character.baseDamage + character.levelDamage * (character.level - 1);
        }
        static public float Base_BettleJuiceDamageEffect(CharacterBody character) //My guess is that it's just a slow debuff
        {
            if (character.GetBuffCount(BuffIndex.BeetleJuice) > 0)
                return 0.05f * character.GetBuffCount(BuffIndex.BeetleJuice);
            return 0;
        }
        static public float Base_DamageRecalculation(CharacterBody character)
        {
            float BaseDamage = HookHandler(CharacterDefaultDamage,character);

            //DamageBoost
            float DamageBoost = 1f;
            int DamageBoostCount = character.inventory ? character.inventory.GetItemCount(ItemIndex.BoostDamage) : 0;
            if (DamageBoostCount > 0)
                DamageBoost += DamageBoostCount * 0.1f;
            DamageBoost -= HookHandler(BettleJuiceDamageEffect,character);

            if (character.HasBuff(BuffIndex.GoldEmpowered))
                ++DamageBoost;


            float DamageMult = DamageBoost + (character.CalcLunarDaggerPower() - 1f);

            return BaseDamage*DamageMult;
        }

        //Attack Speed
        static public float Base_CharacterDefaultAttackSpeed(CharacterBody character)
        {
            return character.baseAttackSpeed + character.levelAttackSpeed * (character.level - 1);
        }
        static public float Base_SyringueEffect(CharacterBody character) //My guess is that it's just a slow debuff
        {
            return character.inventory.GetItemCount(ItemIndex.Syringe) * 0.15f;
        }
        static public float Base_BettleJuiceAttackSpeedEffect(CharacterBody character) //My guess is that it's just a slow debuff
        {
            if (character.GetBuffCount(BuffIndex.BeetleJuice) > 0)
                return 1 - 0.05f * character.GetBuffCount(BuffIndex.BeetleJuice);
            return 1;
        }
        static public float Base_AttackSpeedRecalculation(CharacterBody character)
        {
            float BaseAttackSpeed = HookHandler(CharacterDefaultAttackSpeed,character);

            //Item efect
            float AttackSpeedBonus = 1f;
            if (character.inventory) { 
                AttackSpeedBonus += HookHandler(SyringueEffect,character);
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
                ++AttackSpeedMult;

            float AttackSpeed = BaseAttackSpeed * AttackSpeedMult;
            //Debuff
            AttackSpeed *= HookHandler(BettleJuiceAttackSpeedEffect,character);

            return AttackSpeed;
        }

        //CritChance
        static public float Base_CharacterDefaultCrit(CharacterBody character)
        {
            return character.baseCrit + character.levelCrit * (character.level - 1);
        }
        static public float Base_GlassesEffect(CharacterBody character)
        {
            return character.inventory.GetItemCount(ItemIndex.CritGlasses) * 10f;
        }
        static public float Base_CritRecalculation(CharacterBody character)
        {
            float CriticalChance = HookHandler(CharacterDefaultCrit,character);

            if (character.inventory) {
                CriticalChance += HookHandler(GlassesEffect,character);
                if (character.inventory.GetItemCount(ItemIndex.AttackSpeedOnCrit) > 0)
                    CriticalChance += 5f;
                if (character.inventory.GetItemCount(ItemIndex.CooldownOnCrit) > 0)
                    CriticalChance += 5f;
                if (character.inventory.GetItemCount(ItemIndex.HealOnCrit) > 0)
                    CriticalChance += 5f;
                if (character.inventory.GetItemCount(ItemIndex.CritHeal) > 0)
                    CriticalChance += 5f;
            }
            if (character.HasBuff(BuffIndex.FullCrit))
                CriticalChance += 100f;
            return CriticalChance;
        }

        //Armor
        static public float Base_CharacterDefaultArmor(CharacterBody character)
        {
            return character.baseArmor + character.levelArmor * (character.level - 1);
        }
        static public float Base_BucklerEffect(CharacterBody character)
        {
            if (character.isSprinting && character.inventory.GetItemCount(ItemIndex.SprintArmor) > 0)
                return character.inventory.GetItemCount(ItemIndex.SprintArmor) * 30;
            return 0;
        }
        static public float Base_ArmorRecalculation(CharacterBody character)
        {
            float BaseArmor = HookHandler(CharacterDefaultArmor,character);
            float BonusArmor = 0;

            if (character.HasBuff(BuffIndex.ArmorBoost))
                BonusArmor += 200;
            if (character.HasBuff(BuffIndex.Cripple))
                BonusArmor -= 20;

            if (character.inventory)
            {
                BonusArmor = character.inventory.GetItemCount(ItemIndex.DrizzlePlayerHelper) * 70f;
                BonusArmor += HookHandler(BucklerEffect,character);
            }
                
            return BaseArmor + BonusArmor;
        }

        //CoolDown
        static public float Base_AlienHeadEffect(CharacterBody character)  
        {
            float CDMult = 1;
            for (int index = 0; index < character.inventory.GetItemCount(ItemIndex.AlienHead); ++index)
                CDMult *= 0.75f;
            return CDMult;
        }
        static public float Base_CoolDownRecalculation(CharacterBody character)
        {
            float CoolDownMultiplier = 1f;
            if (character.HasBuff(BuffIndex.GoldEmpowered))
                CoolDownMultiplier *= 0.25f;
            if (character.inventory)
                CoolDownMultiplier *= HookHandler(AlienHeadEffect,character);
            if (character.HasBuff(BuffIndex.NoCooldowns))
                CoolDownMultiplier = 0.0f;

            return CoolDownMultiplier;
        }

        static public float Base_PrimaryCoolDownMultiplier(CharacterBody character)
        {
            return 1;
        }
        static public float Base_SecondaryCoolDownMultiplier(CharacterBody character)
        {
            return 1;
        }
        static public float Base_UtilityCoolDownMultiplier(CharacterBody character)
        {
            if (character.inventory.GetItemCount(ItemIndex.UtilitySkillMagazine) > 0)
                return 0.6666667f;
            return 1;
        }

        static public float Base_PrimaryStackCount(CharacterBody character)
        {
            return 0;
        }
        static public float Base_SecondaryStackCount(CharacterBody character)
        {
            return character.inventory.GetItemCount(ItemIndex.SecondarySkillMagazine);
        }
        static public float Base_UtilityStackCount(CharacterBody character)
        {
            return character.inventory.GetItemCount(ItemIndex.UtilitySkillMagazine) * 2;
        }

        static public void ModdedRecalculate(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody character)
        {
            character.SetPropertyValue("experience",(float) TeamManager.instance.GetTeamExperience(character.teamComponent.teamIndex));
            float l = (float)TeamManager.instance.GetTeamLevel(character.teamComponent.teamIndex);
            if (character.inventory)
            {
                l += character.inventory.GetItemCount(ItemIndex.LevelBonus);

            }
            character.SetPropertyValue("level", l);

            
            float Level = character.level - 1f;
            character.SetPropertyValue("isElite", character.GetFieldValue<BuffMask>("buffMask").containsEliteBuff);

            float preHealth = character.maxHealth;
            float preShield = character.maxShield;


            
            character.SetPropertyValue("maxHealth", HookHandler(HealthRecalculation,character));

            character.SetPropertyValue("maxShield", HookHandler(ShieldRecalculation,character));

            character.SetPropertyValue("regen", HookHandler(RegenRecalculation,character));

            character.SetPropertyValue("moveSpeed" , HookHandler(MoveSpeedRecalculation,character));

            character.SetPropertyValue("acceleration" , character.moveSpeed / character.baseMoveSpeed* character.baseAcceleration); // No real need to add hook, well, if people come to ask me for it I guess I'll do

            character.SetPropertyValue("jumpPower" , HookHandler(JumpPower,character));

            character.SetPropertyValue("maxJumpHeight" , Trajectory.CalculateApex(character.jumpPower)); // No real need to add hook, well, if people come to ask me for it I guess I'll do

            character.SetPropertyValue("maxJumpCount" , (int)HookHandler(JumpCount,character));
            character.SetPropertyValue("damage", HookHandler(DamageRecalculation,character));

            character.SetPropertyValue("attackSpeed", HookHandler(AttackSpeedRecalculation,character));
            
            character.SetPropertyValue("crit", HookHandler(CritRecalculation,character));

            character.SetPropertyValue("armor", HookHandler(ArmorRecalculation,character));

            //CoolDown
            float CoolDownMultiplier = HookHandler(CoolDownRecalculation,character);

            if (character.inventory) { 
                if ((bool) character.GetFieldValue<SkillLocator>("skillLocator").primary)
                    character.GetFieldValue<SkillLocator>("skillLocator").primary.cooldownScale = HookHandlerMultiplier(PrimaryCoolDownMultiplier, character) * CoolDownMultiplier;
                    character.GetFieldValue<SkillLocator>("skillLocator").primary.SetBonusStockFromBody((int)HookHandler(PrimaryStackCount,character));
                if ((bool) character.GetFieldValue<SkillLocator>("skillLocator").secondary)
                {
                    character.GetFieldValue<SkillLocator>("skillLocator").secondary.cooldownScale = HookHandlerMultiplier(SecondaryCoolDownMultiplier,character) * CoolDownMultiplier;
                    character.GetFieldValue<SkillLocator>("skillLocator").secondary.SetBonusStockFromBody((int)HookHandler(SecondaryStackCount,character));
                }
                if ((bool) character.GetFieldValue<SkillLocator>("skillLocator").utility)
                {
                    character.GetFieldValue<SkillLocator>("skillLocator").utility.cooldownScale = HookHandlerMultiplier(UtilityCoolDownMultiplier,character)* CoolDownMultiplier;
                    character.GetFieldValue<SkillLocator>("skillLocator").utility.SetBonusStockFromBody((int)HookHandler(UtilityStackCount,character));
                }
                if ((bool) character.GetFieldValue<SkillLocator>("skillLocator").special)
                    character.GetFieldValue<SkillLocator>("skillLocator").special.cooldownScale = CoolDownMultiplier;
            }

            //CriticalHeal, it don't seam used for now, so ... no hook 
            //yea I'm lazy, but making those is boring !
            character.SetPropertyValue("critHeal", 0.0f);
            if (character.inventory) { 
                if (character.inventory.GetItemCount(ItemIndex.CritHeal) > 0)
                {
                    float crit = character.crit;
                    character.SetPropertyValue("crit", character.crit/ (character.inventory.GetItemCount(ItemIndex.CritHeal) + 1));
                    character.SetPropertyValue("critHeal", crit - character.crit);
                }
            }

            PostRecalculate(character);
            //UpdateHealth/Shield Value
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
            character.SetFieldValue("statsDirty",false);
        }



    }
}
