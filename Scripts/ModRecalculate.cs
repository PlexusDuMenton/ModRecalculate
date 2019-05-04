using BepInEx;
using UnityEngine;
using RoR2;
using System.Reflection;
using System;
using UnityEngine.Networking;
using System.Collections.Generic;
using Frogtown;
using UnityEngine.Events;
using UnityEngine.UI;
/*
    CharacterDefault*** : Before Item are apllied
    ***Effect : Item specific hook
    ***Recalculate : Applyed after all item are applied
    PostRecalculate apply hook before the Health and Shield are updated

    Hook are additional,if you want to act multiplicative, do it in PostRecalculate
    Cooldown hook are MULTIPLICATIVE with other mod/BaseValue, if you want to make it Additional/Substractif, do it in PostRecalculate

    To modify Items stats on character, use the ModifyHook with ModItemManager.AddStatToItem()

    To Create effect, Create a child class of ModHitEffect and in Modify item do : 
    AddOnHitEffect(YOURITEMINDEX, new YOURCHILDCLASS());
    for example use "OnHitEnemyReplacement.cs" file, it contain all default rewriten effects


    Example Application : 
    public class ExampleClass : BaseUnityPlugin
    {
       

       public float BonusHealth(CharacterBody character) // Exponential Health bonus
       {
           float MaxHealth = character.baseMaxHealth;
            float e = 1.3f;
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
       public void ModifyItem(CharacterBody character)
        {
            //Syringe now give damage but give only give 0.1 attack speed instead of 0.15
            ModItemManager.AddStatToItem(ItemIndex.Syringe, new ModItemStat(2, StatIndex.Damage));
            ModItemManager.AddStatToItem(ItemIndex.Syringe, new ModItemStat(0,0,-0.05f, StatIndex.AttackSpeed));
        }

        float OverWriterHook(CharacterBody character)
        {
            return 200 + 100 * (character.level-1);
        }

        void OverWriteHooker()
        {
            ModRecalculate.ResetHook("HealthRecalculation", true); //This line is used to Reset hook, if the TotalReset is true, it'll delete all hook, else it'll only delete de Base_hook
            ModRecalculate.HealthRecalculation += OverWriterHook; //You have to implement your method after the Reset !
        }

        public void Awake()
        {
            OverWriteHooker(); //Look at OverWriteHooker()
            ModRecalculate.HealthRecalculation += delegate { return 5; }; //Simple +5 health after item are applied
            ModRecalculate.CharacterDefaultHealth += BonusHealth; // Apply BonusHealth function result to Health before item are applied
            ModRecalculate.ShieldItemEffect += ShieldCalculate;  // Apply Shield bonus function result to shield
            ModRecalculate.PostRecalculate += PostRecalculateFunc;  // Apply the post recalculate function before the Health and Shield is updated
            ModRecalculate.ModifyItem += ModifyItem;
        }
    }

       HOOK LIST (IN ORDER OF CALL) :
           ModifyItem

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
    [BepInPlugin(ModRecalculate.Dependency, "ModRecalculate", ModRecalculate.Version)]

    public class PlexusUtils : BaseUnityPlugin
    {

       
        public void Awake()
        {
            ModRecalculate.Init();

        }
    }

    public enum OverideState
    {
        Free = 0, //Overide still didn't happen and can be used
        Open = 1, //Overide as been used to change the Base formula
        Closed = 2, //Overide prevent all other hook to happen
        Warned = 3, //Closed and Error has been droped
    }

    

    static public class ModRecalculate
    {

        public const string Dependency = "com.Plexus.ModRecalculate";
        public const string Version = "0.5.1";

        #region Hook

        static event Hook_floatHook BufferHook;


        /// <summary>
        /// Reset Hook is used to overwrite Hook
        /// </summary>
        /// <param name="HookName"> Name of the hook you want to overwrite</param>
        /// <param name="TotalReset"> if false, simply delete the Base_Hook, if true prevent all other hooks </param>
        public static void ResetHook(string HookName,bool TotalReset)
        {
            if ((OverideState)typeof(ModRecalculate).GetField(HookName + "_Overide", BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public).GetValue(null) != OverideState.Free) { 
                FieldInfo f = typeof(ModRecalculate).GetField(HookName, BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public);
                Delegate[] DelegateList = ((MulticastDelegate)f.GetValue(null)).GetInvocationList();
                if (!TotalReset)
                {
                    if (DelegateList.Length > 1)
                    {
                        BufferHook = Delegate.CreateDelegate(typeof(Hook_floatHook), DelegateList[1].Target, DelegateList[1].Method.Name) as Hook_floatHook;
                    }


                    for (int i = 2; i < DelegateList.Length; i++)
                    {
                        BufferHook += Delegate.CreateDelegate(typeof(Hook_floatHook), DelegateList[i].Target, DelegateList[i].Method.Name) as Hook_floatHook;
                    }

                    f.SetValue(null, BufferHook);
                    typeof(ModRecalculate).GetField(HookName + "_Overide", BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public).SetValue(null, OverideState.Open);
                }
                else
                {
                    f.SetValue(null, null);
                    typeof(ModRecalculate).GetField(HookName + "_Overide", BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public).SetValue(null, OverideState.Closed);
                }

                
            }else
            {
                throw new Exception("Error While trying to overwrite " + HookName + " Hook, Another Mod has allready overwrited it");
            }


        }

        //Used to add/Multiply together Hooks
        private static float HookHandler(string c ,CharacterBody character)
        {
            float value = 0;
            MulticastDelegate e = (MulticastDelegate)typeof(ModRecalculate).GetField(c, BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public).GetValue(null);

            if ((int)typeof(ModRecalculate).GetField(c + "_Overide", BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public).GetValue(null) >= (int)OverideState.Closed)
            {
                if (e.GetInvocationList().Length > 1 && (int)typeof(ModRecalculate).GetField(c + "_Overide", BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public).GetValue(null) != (int)OverideState.Closed)
                {
                    typeof(ModRecalculate).GetField(c + "_Overide", BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public).SetValue(null, (int)OverideState.Warned);
                    Debug.LogError("Warning, There is Hook added but ignored since a mod decided to overide ALL HOOK on Hook : "+c);
                }
                return (float)e.GetInvocationList()[0].DynamicInvoke(character);
            }
            else{
                foreach (Delegate d in e.GetInvocationList())
                {

                    value += (float)d.DynamicInvoke(character);
                }
                return value;
            }

            
        }
        private static float HookHandlerMultiplier(string c, CharacterBody character)
        {
            

            MulticastDelegate e = (MulticastDelegate)typeof(ModRecalculate).GetField(c, BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public).GetValue(null);
            float value = (float)(e.GetInvocationList()[0].DynamicInvoke(character));
            if ((int)typeof(ModRecalculate).GetField(c + "_Overide", BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public).GetValue(null) >= (int)OverideState.Closed)
            {
                if (e.GetInvocationList().Length > 1 && (int)typeof(ModRecalculate).GetField(c + "_Overide", BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public).GetValue(null) != (int)OverideState.Closed)
                {
                    typeof(ModRecalculate).GetField(c + "_Overide", BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public).SetValue(null, (int)OverideState.Warned);
                    Debug.LogError("Warning, There is Hook added but ignored since a mod decided to overide ALL HOOK on Hook : " + c);
                }
                return value;
            }
            
            foreach (Delegate d in e.GetInvocationList())
            {
                value *= (float)d.DynamicInvoke(character);
            }
            return value;
        }



        public delegate float Hook_floatHook(CharacterBody character);
        public delegate void Hook_voidHook(CharacterBody character);

        //Hook to apply change before the Update of Health and Shield

        public static event Hook_voidHook PostRecalculate;

        public static event Hook_voidHook ModifyItem;

        //Health related Hook
        #region health
        public static event Hook_floatHook HealthRecalculation;
        public static OverideState HealthRecalculation_Overide = 0;
        public static event Hook_floatHook CharacterDefaultHealth;
        public static OverideState CharacterDefaultHealth_Overide = 0;
        public static event Hook_floatHook InfusionEffect;
        public static OverideState InfusionEffect_Overide = 0;
        public static event Hook_floatHook KnurlMaxHpEffect;
        public static OverideState KnurlMaxHpEffect_Overide = 0;
        public static event Hook_floatHook ItemBoosHpEffect;
        public static OverideState ItemBoosHpEffect_Overide = 0;
        #endregion

        #region shield
        //Shield Related Hook
        public static event Hook_floatHook ShieldRecalculation;
        public static OverideState ShieldRecalculation_Overide = 0;
        public static event Hook_floatHook CharacterDefaultShield;
        public static OverideState CharacterDefaultShield_Overide = 0;
        public static event Hook_floatHook TranscendenceEffect;
        public static OverideState TranscendenceEffect_Overide = 0;
        public static event Hook_floatHook ShieldItemEffect;
        public static OverideState ShieldItemEffect_Overide = 0;
        #endregion
        //HealthRegen related Hook
        public static event Hook_floatHook RegenRecalculation;
        public static OverideState RegenRecalculation_Overide = 0;
        public static event Hook_floatHook CharacterDefaultRegen;
        public static OverideState CharacterDefaultRegen_Overide = 0;
        public static event Hook_floatHook SlugEffect;
        public static OverideState SlugEffect_Overide = 0;
        public static event Hook_floatHook KnurlRegenEffect;
        public static OverideState KnurlRegenEffect_Overide = 0;
        public static event Hook_floatHook HealthDecayEffect;
        public static OverideState HealthDecayEffect_Overide = 0;

        //MoveSpeed related Hook
        public static event Hook_floatHook MoveSpeedRecalculation;
        public static OverideState MoveSpeedRecalculation_Overide = 0;
        public static event Hook_floatHook CharacterDefaultSpeed;
        public static OverideState CharacterDefaultSpeed_Overide = 0;
        public static event Hook_floatHook RedWimpHoofEffect;
        public static OverideState RedWimpHoofEffect_Overide = 0;
        public static event Hook_floatHook EnergyDrinkEffect;
        public static OverideState EnergyDrinkEffect_Overide = 0;
        public static event Hook_floatHook BettleJuiceSpeedEffect;
        public static OverideState BettleJuiceSpeedEffect_Overide = 0;

        public static event Hook_floatHook JumpPower;
        public static OverideState JumpPower_Overide = 0;
        public static event Hook_floatHook JumpCount;
        public static OverideState JumpCount_Overide = 0;

        public static event Hook_floatHook DamageRecalculation;
        public static OverideState DamageRecalculation_Overide = 0;
        public static event Hook_floatHook CharacterDefaultDamage;
        public static OverideState CharacterDefaultDamage_Overide = 0;
        public static event Hook_floatHook BettleJuiceDamageEffect;
        public static OverideState BettleJuiceDamageEffect_Overide = 0;
        public static event Hook_floatHook DamageBoostEffect;
        public static OverideState DamageBoostEffect_Overide = 0;

        public static event Hook_floatHook AttackSpeedRecalculation;
        public static OverideState AttackSpeedRecalculation_Overide = 0;
        public static event Hook_floatHook CharacterDefaultAttackSpeed;
        public static OverideState CharacterDefaultAttackSpeed_Overide = 0;
        public static event Hook_floatHook SyringueEffect;
        public static OverideState SyringueEffect_Overide = 0;
        public static event Hook_floatHook BettleJuiceAttackSpeedEffect;
        public static OverideState BettleJuiceAttackSpeedEffect_Overide = 0;

        public static event Hook_floatHook CritRecalculation;
        public static OverideState CritRecalculation_Overide = 0;
        public static event Hook_floatHook CharacterDefaultCrit;
        public static OverideState CharacterDefaultCrit_Overide = 0;
        public static event Hook_floatHook GlassesEffect;
        public static OverideState GlassesEffect_Overide = 0;

        public static event Hook_floatHook ArmorRecalculation;
        public static OverideState ArmorRecalculation_Overide = 0;
        public static event Hook_floatHook CharacterDefaultArmor;
        public static OverideState CharacterDefaultArmor_Overide = 0;
        public static event Hook_floatHook BucklerEffect;
        public static OverideState BucklerEffect_Overide = 0;

        public static event Hook_floatHook CoolDownRecalculation;
        public static OverideState CoolDownRecalculation_Overide = 0;
        public static event Hook_floatHook AlienHeadEffect;
        public static OverideState AlienHeadEffect_Overide = 0;

        public static event Hook_floatHook PrimaryCoolDownMultiplier;
        public static OverideState PrimaryCoolDownMultiplier_Overide = 0;
        public static event Hook_floatHook SecondaryCoolDownMultiplier;
        public static OverideState SecondaryCoolDownMultiplier_Overide = 0;
        public static event Hook_floatHook UtilityCoolDownMultiplier;
        public static OverideState UtilityCoolDownMultiplier_Overide = 0;
        public static event Hook_floatHook SpecialCoolDownMultiplier;
        public static OverideState SpecialCoolDownMultiplier_Overide = 0;

        public static event Hook_floatHook PrimaryStackCount;
        public static OverideState PrimaryStackCount_Overide = 0;
        public static event Hook_floatHook SecondaryStackCount;
        public static OverideState SecondaryStackCount_Overide = 0;
        public static event Hook_floatHook UtilityStackCount;
        public static OverideState UtilityStackCount_Overide = 0;
        public static event Hook_floatHook SpecialStackCount;
        public static OverideState SpecialStackCount_Overide = 0;
        #endregion

        #region VariableHell

        #region HealthShieldAndRegen
        //Max Health
        private static float m_base_InfusionMult = 1;
        public static float InfusionMult;
        private static float m_base_KnurlHealth = 0;
        [Obsolete("obselete, Please use ModItemManager.AddStatToItem() to add stats to item, this will no longer be supported on 1.0.0 release")]
        public static float KnurlHealth;
        private static float m_base_ItemBoostEffectMult = 0;
        [Obsolete("obselete, Please use ModItemManager.AddStatToItem() to add stats to item, this will no longer be supported on 1.0.0 release")]
        public static float ItemBoostEffectMult;
        private static float m_base_LunarDaggerHealthMalusMult = 1;
        public static float LunarDaggerHealthMalusMult;
        private static float m_base_CustomBonusHealthMult = 0;
        public static float CustomBonusHealthMult;

        //Max Shield
        private static float m_base_ShieldGen = 0;
        [Obsolete("obselete, Please use ModItemManager.AddStatToItem() to add stats to item, this will no longer be supported on 1.0.0 release")]
        public static float ShieldGen;
        private static float m_base_TranscendenceBonus = 0.5f;
        public static float TranscendenceBonus;
        private static float m_base_TranscendenceStack = 0.25f;
        public static float TranscendenceStack;
        private static float m_base_CustomBonusShieldMult =0;
        public static float CustomBonusShieldMult; 

        //Regen
        private static float m_base_SlugBonus = 0;
        [Obsolete("obselete, Please use ModItemManager.AddStatToItem() to add stats to item, this will no longer be supported on 1.0.0 release")]
        public static float SlugBonus;
        private static float m_base_SlugStack = 0;
        [Obsolete("obselete, Please use ModItemManager.AddStatToItem() to add stats to item, this will no longer be supported on 1.0.0 release")]
        public static float SlugStack;
        private static float m_base_KnurlRegen = 0;
        [Obsolete("obselete, Please use ModItemManager.AddStatToItem() to add stats to item, this will no longer be supported on 1.0.0 release")]
        public static float KnurlRegen;
        private static float m_base_HealthDecayMult = 1;
        public static float HealthDecayMult;
        #endregion
        #region SpeedAndMobility
        //Speed Item
        private static float m_base_RedWimp = 0;
        [Obsolete("obselete, Please use ModItemManager.AddStatToItem() to add stats to item, this will no longer be supported on 1.0.0 release")]
        public static float RedWimp;
        private static float m_base_Hoof = 0;
        [Obsolete("obselete, Please use ModItemManager.AddStatToItem() to add stats to item, this will no longer be supported on 1.0.0 release")]
        public static float Hoof;
        private static float m_base_EnergyDrinkBonus = 0;
        [Obsolete("obselete, Please use ModItemManager.AddStatToItem() to add stats to item, this will no longer be supported on 1.0.0 release")]
        public static float EnergyDrinkBonus;
        private static float m_base_EnergyDrinkStack = 0;
        [Obsolete("obselete, Please use ModItemManager.AddStatToItem() to add stats to item, this will no longer be supported on 1.0.0 release")]
        public static float EnergyDrinkStack;
        private static float m_base_BettleJuiceSpeedMalus = 0.05f;
        public static float BettleJuiceSpeedMalus;

        //Speed Buff
        private static float m_base_AffixYellowMoveSpeed = 2;
        public static float AffixYellowMoveSpeed;
        private static float m_base_BugWingBuff = 0.2f;
        public static float BugWingBuff;
        private static float m_base_WarbannerSpeed = 0.3f;
        public static float WarbannerSpeed;
        private static float m_base_EnrageAncientWispSpeed = 0.4f;
        public static float EnrageAncientWispSpeed;
        private static float m_base_CloakSpeed = 0.4f;
        public static float CloakSpeed;
        private static float m_base_TempestSpeed = 1;
        public static float TempestSpeed;
        private static float m_base_WarCryBuffSpeed = 0.5f;
        public static float WarCryBuffSpeed;
        private static float m_base_EngiTeamShieldSpeed = 0.3f;
        public static float EngiTeamShieldSpeed;


        //Mobility
        private static int m_base_BonusJumpPower = 0;
        [Obsolete("obselete, Please use ModItemManager.AddStatToItem() to add stats to item, this will no longer be supported on 1.0.0 release")]
        public static int BonusJumpPower;
        private static int m_base_BonusJumpCount = 0;
        [Obsolete("obselete, Please use ModItemManager.AddStatToItem() to add stats to item, this will no longer be supported on 1.0.0 release")]
        public static int BonusJumpCount;
        #endregion
        #region DamageAndAttackSpeed
        //Damage
        private static float m_base_BettleJuiceDamageMalus = 0.05f;
        public static float BettleJuiceDamageMalus;
        private static float m_base_DamageBoost = 0;
        [Obsolete("obselete, Please use ModItemManager.AddStatToItem() to add stats to item, this will no longer be supported on 1.0.0 release")]
        public static float DamageBoost;
        private static float m_base_GoldEmpoweredDamage = 1;
        public static float GoldEmpoweredDamage;
        private static float m_base_LunarDaggerDamageMult = 1;
        public static float LunarDaggerDamageMult;

        //AttackSpeed
        private static float m_base_SyringueSpeed = 0;
        [Obsolete("obselete, Please use ModItemManager.AddStatToItem() to add stats to item, this will no longer be supported on 1.0.0 release")]
        public static float SyringueSpeed;
        private static float m_base_BettleJuiceAttackSpeedMalus = 0.05f;
        public static float BettleJuiceAttackSpeedMalus;
        private static float m_base_AffixYellowAttackSpeed = 0.5f;
        public static float AffixYellowAttackSpeed;
        private static float m_base_WarbannerAttackSpeed = 0.3f;
        public static float WarbannerAttackSpeed;
        private static float m_base_EnrageAncientWispAttackSpeed = 2f;
        public static float EnrageAncientWispAttackSpeed;
        private static float m_base_WarCryAttackSpeed = 1;
        public static float WarCryAttackSpeed;
        #endregion
        #region CritAndArmor
        //CriticalChance
        private static float m_base_GlassCrit = 0;
        [Obsolete("obselete, Please use ModItemManager.AddStatToItem() to add stats to item, this will no longer be supported on 1.0.0 release")]
        public static float GlassCrit;
        private static float m_base_GlassCritStack = 0;
        [Obsolete("obselete, Please use ModItemManager.AddStatToItem() to add stats to item, this will no longer be supported on 1.0.0 release")]
        public static float GlassCritStack;
        private static float m_base_PredatoryInstincCrit = 0;
        [Obsolete("obselete, Please use ModItemManager.AddStatToItem() to add stats to item, this will no longer be supported on 1.0.0 release")]
        public static float PredatoryInstincCrit;
        private static float m_base_PredatoryInstincCritStack = 0;
        [Obsolete("obselete, Please use ModItemManager.AddStatToItem() to add stats to item, this will no longer be supported on 1.0.0 release")]
        public static float PredatoryInstincCritStack;
        private static float m_base_WickedRingCrit = 0;
        [Obsolete("obselete, Please use ModItemManager.AddStatToItem() to add stats to item, this will no longer be supported on 1.0.0 release")]
        public static float WickedRingCrit;
        private static float m_base_WickedRingCritStack = 0;
        [Obsolete("obselete, Please use ModItemManager.AddStatToItem() to add stats to item, this will no longer be supported on 1.0.0 release")]
        public static float WickedRingCritStack;
        private static float m_base_ScytheCrit = 0;
        [Obsolete("obselete, Please use ModItemManager.AddStatToItem() to add stats to item, this will no longer be supported on 1.0.0 release")]
        public static float ScytheCrit;
        private static float m_base_ScytheCritStack = 0;
        [Obsolete("obselete, Please use ModItemManager.AddStatToItem() to add stats to item, this will no longer be supported on 1.0.0 release")]
        public static float ScytheCritStack;
        private static float m_base_CritHealCrit = 0;
        [Obsolete("obselete, Please use ModItemManager.AddStatToItem() to add stats to item, this will no longer be supported on 1.0.0 release")]
        public static float CritHealCrit;
        private static float m_base_CritHealCritStack = 0;
        [Obsolete("obselete, Please use ModItemManager.AddStatToItem() to add stats to item, this will no longer be supported on 1.0.0 release")]
        public static float CritHealCritStack;
        private static float m_base_HUDCrit = 100;
        public static float HUDCrit;

        //Armor
        private static float m_base_BucklerArmor = 0;
        [Obsolete("obselete, Please use ModItemManager.AddStatToItem() to add stats to item, this will no longer be supported on 1.0.0 release")]
        public static float BucklerArmor;
        private static float m_base_ArmorBoostBuff = 200;
        public static float ArmorBoostBuff;
        private static float m_base_CrippleDebuff = 20;
        public static float CrippleDebuff;
        public static float m_base_DrizzlePlayerHelper = 0;
        [Obsolete("obselete, Please use ModItemManager.AddStatToItem() to add stats to item, this will no longer be supported on 1.0.0 release")]
        public static float DrizzlePlayerHelper;
        #endregion
        #region CD
        //CoolDown
        public static float m_base_AlienHeadCDMult = 1;
        [Obsolete("obselete, Please use ModItemManager.AddStatToItem() to add stats to item, this will no longer be supported on 1.0.0 release")]
        public static float AlienHeadCDMult;
        public static float m_base_GoldEmpoweredCD = 0.25f;
        public static float GoldEmpoweredCD;

        public static float m_base_SecondarySkillMagazineCD = 1;
        [Obsolete("obselete, Please use ModItemManager.AddStatToItem() to add stats to item, this will no longer be supported on 1.0.0 release")]
        public static float SecondarySkillMagazineCD;
        public static float m_base_UtilitySkillMagazineCD = 1;
        [Obsolete("obselete, Please use ModItemManager.AddStatToItem() to add stats to item, this will no longer be supported on 1.0.0 release")]
        public static float UtilitySkillMagazineCD;

        public static float m_base_SecondarySkillMagazineCount = 0;
        [Obsolete("obselete, Please use ModItemManager.AddStatToItem() to add stats to item, this will no longer be supported on 1.0.0 release")]
        public static float SecondarySkillMagazineCount;
        public static float m_base_UtilitySkillMagazineCount = 0;
        [Obsolete("obselete, Please use ModItemManager.AddStatToItem() to add stats to item, this will no longer be supported on 1.0.0 release")]
        public static float UtilitySkillMagazineCount;
        #endregion
        #endregion


#pragma warning disable CS0618 // Le type ou le membre est obsolète
        private static void Base_ModifyItem(CharacterBody character) //Yes it was fun making all those !
        {
            InfusionMult = m_base_InfusionMult;
            KnurlHealth = m_base_KnurlHealth;
            CustomBonusHealthMult = m_base_CustomBonusHealthMult;
            ItemBoostEffectMult = m_base_ItemBoostEffectMult;
            LunarDaggerHealthMalusMult = m_base_LunarDaggerHealthMalusMult;

            ShieldGen = m_base_ShieldGen;
            TranscendenceBonus = m_base_TranscendenceBonus;
            TranscendenceStack = m_base_TranscendenceStack;
            CustomBonusShieldMult = m_base_CustomBonusShieldMult;

            SlugBonus = m_base_SlugBonus;
            SlugStack = m_base_SlugStack;
            KnurlRegen = m_base_KnurlRegen;
            HealthDecayMult = m_base_HealthDecayMult;

            RedWimp = m_base_RedWimp;
            Hoof = m_base_Hoof;
            EnergyDrinkBonus = m_base_EnergyDrinkBonus;
            EnergyDrinkStack = m_base_EnergyDrinkStack;
            BettleJuiceSpeedMalus = m_base_BettleJuiceSpeedMalus;

            AffixYellowMoveSpeed = m_base_AffixYellowMoveSpeed;
            BugWingBuff = m_base_BugWingBuff;
            WarbannerSpeed = m_base_WarbannerSpeed;
            EnrageAncientWispSpeed = m_base_EnrageAncientWispSpeed;
            CloakSpeed = m_base_CloakSpeed;
            TempestSpeed = m_base_TempestSpeed;
            WarCryBuffSpeed = m_base_WarCryBuffSpeed;
            EngiTeamShieldSpeed = m_base_EngiTeamShieldSpeed;

            BonusJumpPower = m_base_BonusJumpPower;
            BonusJumpCount = m_base_BonusJumpCount;

            BettleJuiceDamageMalus = m_base_BettleJuiceDamageMalus;
            DamageBoost = m_base_DamageBoost;
            GoldEmpoweredDamage = m_base_GoldEmpoweredDamage;
            LunarDaggerDamageMult = m_base_LunarDaggerDamageMult;

            SyringueSpeed = m_base_SyringueSpeed;
            BettleJuiceAttackSpeedMalus = m_base_BettleJuiceAttackSpeedMalus;
            AffixYellowAttackSpeed = m_base_AffixYellowAttackSpeed;
            WarbannerAttackSpeed = m_base_WarbannerAttackSpeed;
            EnrageAncientWispAttackSpeed = m_base_EnrageAncientWispAttackSpeed;
            WarCryAttackSpeed = m_base_WarCryAttackSpeed;

            WickedRingCrit = m_base_WickedRingCrit;
            WickedRingCritStack = m_base_WickedRingCritStack;
            ScytheCrit = m_base_ScytheCrit;
            ScytheCritStack = m_base_ScytheCritStack;
            CritHealCrit = m_base_CritHealCrit;
            CritHealCritStack = m_base_CritHealCritStack;
            HUDCrit = m_base_HUDCrit;
            GlassCrit = m_base_GlassCrit;
            GlassCritStack = m_base_GlassCritStack;
            PredatoryInstincCrit = m_base_PredatoryInstincCrit;
            PredatoryInstincCritStack = m_base_PredatoryInstincCritStack;

            DrizzlePlayerHelper = m_base_DrizzlePlayerHelper;
            CrippleDebuff = m_base_CrippleDebuff;
            ArmorBoostBuff = m_base_ArmorBoostBuff;
            BucklerArmor = m_base_BucklerArmor;

            UtilitySkillMagazineCount = m_base_UtilitySkillMagazineCount;
            SecondarySkillMagazineCount = m_base_SecondarySkillMagazineCount;
            UtilitySkillMagazineCD = m_base_UtilitySkillMagazineCD;
            SecondarySkillMagazineCD = m_base_SecondarySkillMagazineCD;
            GoldEmpoweredCD = m_base_GoldEmpoweredCD;
            AlienHeadCDMult = m_base_AlienHeadCDMult;

            ModItemManager.Update();

        }

        //Used to add the defaults Hook
        static public void Init()
        {
            ModItemManager.Init();
            ModOnHitEnemy.Init();
            ModifyItem = Base_ModifyItem;

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
            DamageBoostEffect = Base_DamageBoostEffect;
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
            SpecialCoolDownMultiplier = Base_SpecialCoolDownMultiplier;

            //Ability Stack Count
            PrimaryStackCount = Base_PrimaryStackCount;
            SecondaryStackCount = Base_SecondaryStackCount;
            UtilityStackCount = Base_UtilityStackCount;
            SpecialStackCount = Base_SpecialStackCount;

            PostRecalculate = delegate (CharacterBody character) { /*To aboid Bug*/ };
            On.RoR2.CharacterBody.RecalculateStats += ModdedRecalculate;
        }

        #region BaseHook
        //HEALTH FUNCTIONS
        static public float Base_CharacterDefaultHealth(CharacterBody character)
        {
            return character.baseMaxHealth + (character.level - 1) * character.levelMaxHealth;
        }
        static public float Base_InfusionEffect(CharacterBody character)
        {
            if (character.inventory.GetItemCount(ItemIndex.Infusion) > 0)
                return character.inventory.infusionBonus * InfusionMult;
            return 0;
        }
        static public float Base_KnurlMaxHpEffect(CharacterBody character)
        {
            return character.inventory.GetItemCount(ItemIndex.Knurl) * KnurlHealth;
        }
        static public float Base_ItemBoosHpEffect(CharacterBody character)
        {
            return 1 + character.inventory.GetItemCount(ItemIndex.BoostHp) * ItemBoostEffectMult;
        }
        static public float Base_RecalculateHealth(CharacterBody character)
        {
            //CharacterLinked Health Stats

            float MaxHealth = HookHandler("CharacterDefaultHealth", character);
                float HealthBonusItem = 0;
                float hpbooster = 0;

                //Item Linked Bonus
                if ((bool)character.inventory) { 
                    //Item Flat Bonus
                    HealthBonusItem += HookHandler("InfusionEffect", character);
                    HealthBonusItem += HookHandler("KnurlMaxHpEffect", character);
                    HealthBonusItem += ModItemManager.GetBonusForStat(character, StatIndex.MaxHealth);

                    //Item MultiplierBonus
                    hpbooster = HookHandler("ItemBoosHpEffect",character);
                    hpbooster += CustomBonusHealthMult;
                    hpbooster += ModItemManager.GetMultiplierForStat(character, StatIndex.MaxHealth);
                }
                //Applying flat bonus and Level up bonus
                MaxHealth = MaxHealth + HealthBonusItem;
                //Applying Shaped Glass and mult bonus effects
                if ((bool)character.inventory)
                {
                    MaxHealth *= hpbooster / (character.CalcLunarDaggerPower()*LunarDaggerHealthMalusMult);
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

                shield = character.maxHealth * (1+ TranscendenceBonus + (character.inventory.GetItemCount(ItemIndex.ShieldOnly) - 1) * TranscendenceStack);
                character.SetPropertyValue("maxHealth", 1);
            }
            return shield;
        }
        static public float Base_ShieldItemEffect(CharacterBody character)
        {
            return character.inventory.GetItemCount(ItemIndex.PersonalShield) * ShieldGen;
        }
        static public float Base_ShieldRecalculation(CharacterBody character)
        {
            //CharacterLinked Shield Stats
            float MaxShield = HookHandler("CharacterDefaultShield",character);

            //ShieldItem Calculation (There is hook for it)
            if ((bool)character.inventory)
            {
                MaxShield += HookHandler("TranscendenceEffect",character);
                MaxShield += HookHandler("ShieldItemEffect",character);
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
            MaxShield+= ModItemManager.GetBonusForStat(character,StatIndex.MaxShield);

            MaxShield *= (1+CustomBonusShieldMult+ ModItemManager.GetMultiplierForStat(character, StatIndex.MaxShield) );
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
                return (SlugBonus + (character.inventory.GetItemCount(ItemIndex.HealWhileSafe) - 1) * SlugStack);
            return 0;
        }
        static public float Base_KnurlRegenEffect(CharacterBody character)
        {
            return character.inventory.GetItemCount(ItemIndex.Knurl) * KnurlRegen;
        }
        static public float Base_HealthDecayEffect(CharacterBody character) //I guess that just for debuff ??
        {
            if (character.inventory.GetItemCount(ItemIndex.HealthDecay) > 0 && HealthDecayMult != 0)
                return character.maxHealth / (character.inventory.GetItemCount(ItemIndex.HealthDecay)* HealthDecayMult);
            return 0;
        }
        static public float Base_RegenRecalculation(CharacterBody character)
        {

            float BaseRegen = HookHandler("CharacterDefaultRegen",character);
            float RegenBonus = 0;
            //Item Related
            if ((bool)character.inventory)
            {
                if (character.inventory.GetItemCount(ItemIndex.HealWhileSafe) > 0)
                    RegenBonus += (BaseRegen * HookHandler("SlugEffect",character)) - BaseRegen;
                RegenBonus += ModItemManager.GetBonusForStat(character, StatIndex.Regen);
                if (character.outOfDanger)
                    RegenBonus += ModItemManager.GetBonusForStat(character, StatIndex.SafeRegen);
                RegenBonus += HookHandler("KnurlRegenEffect",character);
                RegenBonus -= HookHandler("HealthDecayEffect",character);
            }

            float regenmult = 1 + ModItemManager.GetMultiplierForStat(character, StatIndex.Regen);
            if (character.outOfDanger)
                regenmult += ModItemManager.GetMultiplierForStat(character, StatIndex.SafeRegen);

            float totalRegen = (BaseRegen* regenmult + RegenBonus);

            return totalRegen;
            
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
                    RedWinpBonus += (float)character.inventory.GetItemCount(ItemIndex.SprintOutOfCombat) * RedWimp;

                return RedWinpBonus + (float)character.inventory.GetItemCount(ItemIndex.Hoof) * Hoof;
            }
            return RedWinpBonus;
        }
        static public float Base_EnergyDrinkEffect(CharacterBody character)
        {
            if (character.isSprinting && character.inventory.GetItemCount(ItemIndex.SprintBonus) > 0)
                return (m_base_EnergyDrinkBonus + m_base_EnergyDrinkStack * (character.inventory.GetItemCount(ItemIndex.SprintBonus)-1)) / character.GetFieldValue<float>("sprintingSpeedMultiplier");
            return 0;
        }
        static public float Base_BettleJuiceSpeedEffect(CharacterBody character) //My guess is that it's just a slow debuff
        {
            
            return 1.0f - BettleJuiceSpeedMalus * character.GetBuffCount(BuffIndex.BeetleJuice);
        }
        static public float Base_MoveSpeedRecalculation(CharacterBody character)
        {
            float BaseMoveSpeed = HookHandler("CharacterDefaultSpeed",character);

            float SpeedBonus = HookHandler("RedWimpHoofEffect",character);


            //More weird stuff
            if ((bool)character.inventory)
                if (character.inventory.currentEquipmentIndex == EquipmentIndex.AffixYellow)
                    BaseMoveSpeed += AffixYellowMoveSpeed;

            if (character.isSprinting)
                BaseMoveSpeed *= character.GetFieldValue<float>("sprintingSpeedMultiplier");


            //SpeedBonus
            if ((bool)character.inventory)
            {
                SpeedBonus += HookHandler("EnergyDrinkEffect",character);
            }
            if (character.HasBuff(BuffIndex.BugWings))
                SpeedBonus += BugWingBuff;
            if (character.HasBuff(BuffIndex.Warbanner))
                SpeedBonus += WarbannerSpeed;
            if (character.HasBuff(BuffIndex.EnrageAncientWisp))
                SpeedBonus += EnrageAncientWispSpeed;
            if (character.HasBuff(BuffIndex.CloakSpeed))
                SpeedBonus += CloakSpeed;
            if (character.HasBuff(BuffIndex.TempestSpeed))
                SpeedBonus+= TempestSpeed;
            if (character.HasBuff(BuffIndex.WarCryBuff))
                SpeedBonus += WarCryBuffSpeed;
            if (character.HasBuff(BuffIndex.EngiTeamShield))
                SpeedBonus += EngiTeamShieldSpeed;

            SpeedBonus += ModItemManager.GetMultiplierForStat(character, StatIndex.MoveSpeed);
            if (character.isSprinting)
                SpeedBonus += ModItemManager.GetMultiplierForStat(character, StatIndex.RunningMoveSpeed);
            if (character.outOfCombat && character.outOfDanger) { 
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
                MoveSpeed *= HookHandler("BettleJuiceSpeedEffect",character);
            }

            return MoveSpeed;
        }

        //Mobility
        static public float Base_JumpPower(CharacterBody character)
        {
            float JumpPower = character.baseJumpPower + character.levelJumpPower * (character.level - 1) + BonusJumpPower + ModItemManager.GetBonusForStat(character, StatIndex.JumpPower);
            JumpPower *= 1 + ModItemManager.GetMultiplierForStat(character, StatIndex.JumpPower);
            return JumpPower;
        }
        static public float Base_JumpCount(CharacterBody character)
        {
            float JumpCount = character.baseJumpCount + BonusJumpCount + ModItemManager.GetBonusForStat(character, StatIndex.JumpCount);
            JumpCount *= 1 + ModItemManager.GetMultiplierForStat(character, StatIndex.JumpCount);
            return JumpCount;
        }

        //Damage
        static public float Base_CharacterDefaultDamage(CharacterBody character)
        {
            return character.baseDamage + character.levelDamage * (character.level - 1);
        }
        static public float Base_BettleJuiceDamageEffect(CharacterBody character) //My guess is that it's just a slow debuff
        {
            if (character.GetBuffCount(BuffIndex.BeetleJuice) > 0)
                return BettleJuiceDamageMalus * character.GetBuffCount(BuffIndex.BeetleJuice);
            return 0;
        }
        static public float Base_DamageBoostEffect(CharacterBody character)
        {
            float DamageBoost = 1f;
            int DamageBoostCount = character.inventory ? character.inventory.GetItemCount(ItemIndex.BoostDamage) : 0;
            if (DamageBoostCount > 0)
                DamageBoost += DamageBoostCount * DamageBoost;
            DamageBoost -= HookHandler("BettleJuiceDamageEffect", character);

            if (character.HasBuff(BuffIndex.GoldEmpowered))

                DamageBoost += GoldEmpoweredDamage;

            return DamageBoost;
        }
        static public float Base_DamageRecalculation(CharacterBody character)
        {
            float BaseDamage = HookHandler("CharacterDefaultDamage",character);
            BaseDamage += ModItemManager.GetBonusForStat(character, StatIndex.Damage);

            float DamageMult = HookHandler("DamageBoostEffect", character) + (character.CalcLunarDaggerPower() - 1f)* LunarDaggerDamageMult;
            DamageMult += ModItemManager.GetMultiplierForStat(character, StatIndex.Damage);
            return BaseDamage*DamageMult;
        }

        //Attack Speed
        static public float Base_CharacterDefaultAttackSpeed(CharacterBody character)
        {
            return character.baseAttackSpeed + character.levelAttackSpeed * (character.level - 1);
        }
        static public float Base_SyringueEffect(CharacterBody character) //My guess is that it's just a slow debuff
        {
            return character.inventory.GetItemCount(ItemIndex.Syringe) * SyringueSpeed;
        }
        static public float Base_BettleJuiceAttackSpeedEffect(CharacterBody character) //My guess is that it's just a slow debuff
        {
            if (character.GetBuffCount(BuffIndex.BeetleJuice) > 0)
                return 1 - BettleJuiceAttackSpeedMalus * character.GetBuffCount(BuffIndex.BeetleJuice);
            return 1;
        }
        static public float Base_AttackSpeedRecalculation(CharacterBody character)
        {
            float BaseAttackSpeed = HookHandler("CharacterDefaultAttackSpeed",character);

            //Item efect
            float AttackSpeedBonus = 1f;
            if (character.inventory) { 
                AttackSpeedBonus += HookHandler("SyringueEffect",character);
                if (character.inventory.currentEquipmentIndex == EquipmentIndex.AffixYellow)
                    AttackSpeedBonus += AffixYellowAttackSpeed;
            }

            //Buffs
            float AttackSpeedMult = AttackSpeedBonus + character.GetFieldValue<int[]>("buffs")[2] * 0.12f;
            if (character.HasBuff(BuffIndex.Warbanner))
                AttackSpeedMult += WarbannerAttackSpeed;
            if (character.HasBuff(BuffIndex.EnrageAncientWisp))
                AttackSpeedMult += EnrageAncientWispAttackSpeed;
            if (character.HasBuff(BuffIndex.WarCryBuff))
                AttackSpeedMult += EnrageAncientWispAttackSpeed;


            BaseAttackSpeed +=  ModItemManager.GetBonusForStat(character, StatIndex.AttackSpeed);
            AttackSpeedMult += ModItemManager.GetMultiplierForStat(character, StatIndex.AttackSpeed);
            float AttackSpeed = BaseAttackSpeed * AttackSpeedMult;
            //Debuff
            AttackSpeed *= HookHandler("BettleJuiceAttackSpeedEffect",character);

            return AttackSpeed;
        }

        //CritChance
        static public float Base_CharacterDefaultCrit(CharacterBody character)
        {
            return character.baseCrit + character.levelCrit * (character.level - 1);
        }
        static public float Base_GlassesEffect(CharacterBody character)
        {
            if (character.inventory.GetItemCount(ItemIndex.CritGlasses) > 0)
                return (character.inventory.GetItemCount(ItemIndex.CritGlasses)-1) * GlassCritStack + GlassCrit;
            return 0;
        }
        static public float Base_CritRecalculation(CharacterBody character)
        {
            float CriticalChance = HookHandler("CharacterDefaultCrit",character);

            if (character.inventory) {
                CriticalChance += HookHandler("GlassesEffect",character);
                if (character.inventory.GetItemCount(ItemIndex.AttackSpeedOnCrit) > 0)
                    CriticalChance += PredatoryInstincCrit + PredatoryInstincCritStack*(character.inventory.GetItemCount(ItemIndex.AttackSpeedOnCrit)-1);
                if (character.inventory.GetItemCount(ItemIndex.CooldownOnCrit) > 0)
                    CriticalChance += WickedRingCrit + WickedRingCritStack * (character.inventory.GetItemCount(ItemIndex.AttackSpeedOnCrit) - 1);
                if (character.inventory.GetItemCount(ItemIndex.HealOnCrit) > 0)
                    CriticalChance += ScytheCrit + ScytheCritStack * (character.inventory.GetItemCount(ItemIndex.AttackSpeedOnCrit) - 1);
                if (character.inventory.GetItemCount(ItemIndex.CritHeal) > 0)
                    CriticalChance += CritHealCrit + CritHealCritStack * (character.inventory.GetItemCount(ItemIndex.AttackSpeedOnCrit) - 1);
            }
            if (character.HasBuff(BuffIndex.FullCrit))
                CriticalChance += HUDCrit;

            CriticalChance += ModItemManager.GetBonusForStat(character, StatIndex.Crit);
            CriticalChance *= 1 + ModItemManager.GetMultiplierForStat(character, StatIndex.AttackSpeed);
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
                return character.inventory.GetItemCount(ItemIndex.SprintArmor) * BucklerArmor;
            return 0;
        }
        static public float Base_ArmorRecalculation(CharacterBody character)
        {
            float BaseArmor = HookHandler("CharacterDefaultArmor",character);
            float BonusArmor = 0;

            if (character.HasBuff(BuffIndex.ArmorBoost))
                BonusArmor += ArmorBoostBuff;
            if (character.HasBuff(BuffIndex.Cripple))
                BonusArmor -= CrippleDebuff;

            if (character.inventory)
            {
                BonusArmor = character.inventory.GetItemCount(ItemIndex.DrizzlePlayerHelper) * DrizzlePlayerHelper;
                BonusArmor += HookHandler("BucklerEffect",character);
            }
            float TotalArmor = BaseArmor + BonusArmor;
            TotalArmor += ModItemManager.GetBonusForStat(character, StatIndex.Armor);
            if (character.isSprinting)
                TotalArmor += ModItemManager.GetBonusForStat(character, StatIndex.RunningArmor);
            TotalArmor *= 1+ModItemManager.GetMultiplierForStat(character, StatIndex.Armor);
            if (character.isSprinting)
                TotalArmor *= 1+ ModItemManager.GetMultiplierForStat(character, StatIndex.RunningArmor);
            return TotalArmor;
        }

        //CoolDown
        static public float Base_AlienHeadEffect(CharacterBody character)  
        {
            float CDMult = 1;
            for (int index = 0; index < character.inventory.GetItemCount(ItemIndex.AlienHead); ++index)
                CDMult *= AlienHeadCDMult;
            return CDMult;
        }
        static public float Base_CoolDownRecalculation(CharacterBody character)
        {
            float CoolDownMultiplier = 1f;
            CoolDownMultiplier += ModItemManager.GetBonusForStat(character, StatIndex.GlobalCoolDown);

            CoolDownMultiplier *= ModItemManager.GetMultiplierForStatCD(character, StatIndex.GlobalCoolDown);

            if (character.HasBuff(BuffIndex.GoldEmpowered))
                CoolDownMultiplier *= GoldEmpoweredCD;
            if (character.inventory)
                CoolDownMultiplier *= HookHandler("AlienHeadEffect",character);
            if (character.HasBuff(BuffIndex.NoCooldowns))
                CoolDownMultiplier = 0.0f;


            return CoolDownMultiplier;
        }

        static public float Base_PrimaryCoolDownMultiplier(CharacterBody character)
        {
            float CoolDownMultiplier = 1f;
            CoolDownMultiplier += ModItemManager.GetBonusForStat(character, StatIndex.CoolDownPrimary);

            CoolDownMultiplier *= ModItemManager.GetMultiplierForStatCD(character, StatIndex.CoolDownPrimary);
            return 1;
        }
        static public float Base_SecondaryCoolDownMultiplier(CharacterBody character)
        {
            float CoolDownMultiplier = 1f;
            CoolDownMultiplier += ModItemManager.GetBonusForStat(character, StatIndex.CoolDownSecondary);

            CoolDownMultiplier *= ModItemManager.GetMultiplierForStatCD(character, StatIndex.CoolDownSecondary);

            if (character.inventory.GetItemCount(ItemIndex.SecondarySkillMagazine) > 0)
                CoolDownMultiplier *= SecondarySkillMagazineCD;
            return CoolDownMultiplier;
        }
        static public float Base_UtilityCoolDownMultiplier(CharacterBody character)
        {
            float CoolDownMultiplier = 1f;
            CoolDownMultiplier += ModItemManager.GetBonusForStat(character, StatIndex.CoolDownUtility);

            CoolDownMultiplier *= ModItemManager.GetMultiplierForStatCD(character, StatIndex.CoolDownUtility);

            if (character.inventory.GetItemCount(ItemIndex.UtilitySkillMagazine) > 0)
                CoolDownMultiplier *= UtilitySkillMagazineCD;
            return CoolDownMultiplier;
        }
        static public float Base_SpecialCoolDownMultiplier(CharacterBody character)
        {
            float CoolDownMultiplier = 1f;
            CoolDownMultiplier += ModItemManager.GetBonusForStat(character, StatIndex.CoolDownSpecial);

            CoolDownMultiplier *= ModItemManager.GetMultiplierForStatCD(character, StatIndex.CoolDownSpecial);
            return 1;
        }

        static public float Base_PrimaryStackCount(CharacterBody character)
        {
            float count = 0;
            count += ModItemManager.GetBonusForStat(character, StatIndex.CountPrimary);

            count *= 1 + ModItemManager.GetMultiplierForStat(character, StatIndex.CountPrimary);
            return count;
        }
        static public float Base_SecondaryStackCount(CharacterBody character)
        {
            float count = 0;
            count += ModItemManager.GetBonusForStat(character, StatIndex.CountSecondary);
            count += character.inventory.GetItemCount(ItemIndex.SecondarySkillMagazine) * SecondarySkillMagazineCount;
            count *= 1 + ModItemManager.GetMultiplierForStat(character, StatIndex.CountSecondary);
            return count;
        }
        static public float Base_UtilityStackCount(CharacterBody character)
        {
            float count = 0;
            count += ModItemManager.GetBonusForStat(character, StatIndex.CountUtility);
            count += character.inventory.GetItemCount(ItemIndex.UtilitySkillMagazine) * UtilitySkillMagazineCount;
            count *= 1 + ModItemManager.GetMultiplierForStat(character, StatIndex.CountUtility);
            return count;
        }
        static public float Base_SpecialStackCount(CharacterBody character)
        {
            float count = 0;
            count += ModItemManager.GetBonusForStat(character, StatIndex.CountSpecial);

            count *= 1 + ModItemManager.GetMultiplierForStat(character, StatIndex.CountSpecial);
            return count;
        }
#pragma warning restore CS0618 // Disable Obsolete warning

        #endregion
        static public void ModdedRecalculate(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody character)
        {
            if (character == null)
                return;

            ModifyItem(character);

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
            

            character.SetPropertyValue("maxHealth", HookHandler("HealthRecalculation",character));


            character.SetPropertyValue("maxShield", HookHandler("ShieldRecalculation",character));

            character.SetPropertyValue("regen", HookHandler("RegenRecalculation",character));

            character.SetPropertyValue("moveSpeed" , HookHandler("MoveSpeedRecalculation",character));

            character.SetPropertyValue("acceleration" , character.moveSpeed / character.baseMoveSpeed* character.baseAcceleration); // No real need to add hook, well, if people come to ask me for it I guess I'll do

            character.SetPropertyValue("jumpPower" , HookHandler("JumpPower",character));

            character.SetPropertyValue("maxJumpHeight" , Trajectory.CalculateApex(character.jumpPower)); // No real need to add hook, well, if people come to ask me for it I guess I'll do

            character.SetPropertyValue("maxJumpCount" , (int)HookHandler("JumpCount",character));
            character.SetPropertyValue("damage", HookHandler("DamageRecalculation",character));

            character.SetPropertyValue("attackSpeed", HookHandler("AttackSpeedRecalculation",character));
            
            character.SetPropertyValue("crit", HookHandler("CritRecalculation",character));
            character.SetPropertyValue("armor", HookHandler("ArmorRecalculation",character));
            //CoolDown
            float CoolDownMultiplier = HookHandler("CoolDownRecalculation",character);
            if (character.inventory) { 
                if ((bool)character.GetFieldValue<SkillLocator>("skillLocator").primary) {
                    character.GetFieldValue<SkillLocator>("skillLocator").primary.cooldownScale = HookHandlerMultiplier("PrimaryCoolDownMultiplier", character) * CoolDownMultiplier;
                    if (character.GetFieldValue<SkillLocator>("skillLocator").primary.baseMaxStock > 1)
                        character.GetFieldValue<SkillLocator>("skillLocator").primary.SetBonusStockFromBody((int)HookHandler("PrimaryStackCount",character));
                }
                if ((bool) character.GetFieldValue<SkillLocator>("skillLocator").secondary)
                {
                    character.GetFieldValue<SkillLocator>("skillLocator").secondary.cooldownScale = HookHandlerMultiplier("SecondaryCoolDownMultiplier",character) * CoolDownMultiplier;
                    character.GetFieldValue<SkillLocator>("skillLocator").secondary.SetBonusStockFromBody((int)HookHandler("SecondaryStackCount",character));
                }
                if ((bool) character.GetFieldValue<SkillLocator>("skillLocator").utility)
                {
                    character.GetFieldValue<SkillLocator>("skillLocator").utility.cooldownScale = HookHandlerMultiplier("UtilityCoolDownMultiplier",character)* CoolDownMultiplier;
                    character.GetFieldValue<SkillLocator>("skillLocator").utility.SetBonusStockFromBody((int)HookHandler("UtilityStackCount",character));
                }
                if ((bool)character.GetFieldValue<SkillLocator>("skillLocator").special)
                {
                    character.GetFieldValue<SkillLocator>("skillLocator").special.cooldownScale = CoolDownMultiplier;
                    if (character.GetFieldValue<SkillLocator>("skillLocator").special.baseMaxStock > 1)
                        character.GetFieldValue<SkillLocator>("skillLocator").special.SetBonusStockFromBody((int)HookHandler("SpecialStackCount", character));
                }
                    

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
