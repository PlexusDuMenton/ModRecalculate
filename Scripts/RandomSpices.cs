using BepInEx;
using UnityEngine;
using RoR2;
using BepInEx.Configuration;
using AetherLib.Util.Reflection;
using System;
using UnityEngine.Networking;



namespace plexus
{
    //character is an example plugin that can be put in BepInEx/plugins/ExamplePlugin/ExamplePlugin.dll to test out.
    //It's a very simple plugin that adds Bandit to the game, and gives you a tier 3 item whenever you press F2.
    //Lets examine what each line of code is for:

    //character attribute specifies that we have a dependency on R2API, as we're using it to add Bandit to the game.
    //You don't need character if you're not using R2API in your plugin, it's just to tell BepInEx to initialize R2API before character plugin so it's safe to use R2API.
    [BepInDependency("com.bepis.r2api")]
    [BepInDependency("at.aster.aetherlib")]

    //character attribute is required, and lists metadata for your plugin.
    //The GUID should be a unique ID for character plugin, which is human readable (as it is used in places like the config). I like to use the java package notation, which is "com.[your name here].[your plugin name here]"
    //The name is the name of the plugin that's displayed on load, and the version number just specifies what version the plugin is.

    [BepInPlugin("com.Plexus.RandomSpices", "RandomSpices", "0.1.3")]

    //character is the main declaration of our plugin class. BepInEx searches for all classes inheriting from BaseUnityPlugin to initialize on startup.
    //BaseUnityPlugin itself inherits from MonoBehaviour, so you can use character as a reference for what you can declare and use in your plugin class: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    public class RandomSpiceSaving
    {
        float BaseHealthLevel;
        float LevelHealthLevel;
        float ExpoHealthLevel;

        float BaseShieldLevel;
        float LevelShieldLevel;
        float ExpoShieldLevel;

        float BaseDamageLevel;
        float LevelDamageLevel;
    }



    public class RandomSpices : BaseUnityPlugin
    {
        public ConfigWrapper<string> exponentialValueHealth;
        public ConfigWrapper<string> ShieldBoostPerLevel;


        public float ShieldBoost = 5;

        public void InitConfig()
        {
            exponentialValueHealth = Config.Wrap(
                    "exponential Health value",
                    "exponentialValueHealth",
                    "Sets exponential value applied to level when calculating health. default = 1",
            "1.3");

            ShieldBoostPerLevel = Config.Wrap(
                    "Shield Boost per level",
                    "ShieldBoostPerLevel",
                    "Increase the bonus from shield item per player level. default = 0",
            "5");
        }

        public void LevelUpExpHealth()
        {
            int price = 1;
            Chat.AddMessage("Exponential Health Improved for "+ price + " lunar coin");
            Chat.AddMessage("Exponential from : ");
        }


        public void Recalulate(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody character) //Here we go again
        {
            character.SetPropertyValue("experience",(float)TeamManager.instance.GetTeamExperience(character.teamComponent.teamIndex));
            character.SetPropertyValue("level", (float)TeamManager.instance.GetTeamLevel(character.teamComponent.teamIndex));
            int InfusionCount = 0;
            int HealWhileSafeCount = 0;
            int PersonalShieldCount = 0;
            int HoofCount = 0;
            int RedWimpCount = 0;
            int FeatherCount = 0;
            int SyringeCount = 0;
            int GlassesCount = 0;
            int AttackSpeedCritCount = 0;
            int CoolDownCritCount = 0;
            int HealOnCritCount = 0;
            int BettleJuiceCount = 0;
            int ShieldOnlyCount = 0;
            int AlienHeadCount = 0;
            int KnurlCount = 0;
            int BoostHpCount = 0;
            int CritHealCount = 0;
            int SodaCount = 0;
            int newBonusStockFromBody = 0;
            int BucklerCount = 0;
            int UtilityMagazineCount = 0;
            int HealthDecayCount = 0;
            int DrizzlePlayerHelperCount = 0;
            float GlassCanonPower = 1f;
            EquipmentIndex equipmentIndex = EquipmentIndex.None;
            uint InfusionPower = 0;
            if ((bool)character.inventory)
            {
                character.SetPropertyValue("level", character.level + (float)character.inventory.GetItemCount(ItemIndex.LevelBonus));
                InfusionCount = character.inventory.GetItemCount(ItemIndex.Infusion);
                HealWhileSafeCount = character.inventory.GetItemCount(ItemIndex.HealWhileSafe);
                PersonalShieldCount = character.inventory.GetItemCount(ItemIndex.PersonalShield);
                HoofCount = character.inventory.GetItemCount(ItemIndex.Hoof);
                RedWimpCount = character.inventory.GetItemCount(ItemIndex.SprintOutOfCombat);
                FeatherCount = character.inventory.GetItemCount(ItemIndex.Feather);
                SyringeCount = character.inventory.GetItemCount(ItemIndex.Syringe);
                GlassesCount = character.inventory.GetItemCount(ItemIndex.CritGlasses);
                AttackSpeedCritCount = character.inventory.GetItemCount(ItemIndex.AttackSpeedOnCrit);
                CoolDownCritCount = character.inventory.GetItemCount(ItemIndex.CooldownOnCrit);
                HealOnCritCount = character.inventory.GetItemCount(ItemIndex.HealOnCrit);
                BettleJuiceCount = character.GetBuffCount(BuffIndex.BeetleJuice);
                ShieldOnlyCount = character.inventory.GetItemCount(ItemIndex.ShieldOnly);
                AlienHeadCount = character.inventory.GetItemCount(ItemIndex.AlienHead);
                KnurlCount = character.inventory.GetItemCount(ItemIndex.Knurl);
                BoostHpCount = character.inventory.GetItemCount(ItemIndex.BoostHp);
                CritHealCount = character.inventory.GetItemCount(ItemIndex.CritHeal);
                SodaCount = character.inventory.GetItemCount(ItemIndex.SprintBonus);
                newBonusStockFromBody = character.inventory.GetItemCount(ItemIndex.SecondarySkillMagazine);
                BucklerCount = character.inventory.GetItemCount(ItemIndex.SprintArmor);
                UtilityMagazineCount = character.inventory.GetItemCount(ItemIndex.UtilitySkillMagazine);
                HealthDecayCount = character.inventory.GetItemCount(ItemIndex.HealthDecay);
                GlassCanonPower = character.CalcLunarDaggerPower();
                equipmentIndex = character.inventory.currentEquipmentIndex;
                InfusionPower = character.inventory.infusionBonus;
                DrizzlePlayerHelperCount = character.inventory.GetItemCount(ItemIndex.DrizzlePlayerHelper);
            }
            float Level = character.level - 1f;
            character.SetPropertyValue("isElite", character.GetFieldValue<BuffMask>("buffMask").containsEliteBuff);

            float preHealth = character.maxHealth;
            float preShield = character.maxShield;

            //Health
            float MaxHealth = character.baseMaxHealth;
            float additionalHealth = 0;
            if (character.inventory.GetItemCount(ItemIndex.Infusion) > 0)
                additionalHealth += character.inventory.infusionBonus;
            additionalHealth += character.inventory.GetItemCount(ItemIndex.Knurl) * 40;

            float hpbooster = 1 + character.inventory.GetItemCount(ItemIndex.BoostHp) * 0.1f;

            float e = 1;
            if (character.isPlayerControlled)
                e = HExpo;

            MaxHealth = Utils.exponentialFormula(MaxHealth, character.levelMaxHealth, e, character.level, 0) * hpbooster / character.CalcLunarDaggerPower();
            character.SetPropertyValue("maxHealth", MaxHealth);


            //regen
            float BaseRegen = (character.baseRegen + character.levelRegen * Level) * 2.5f;

            if (character.outOfDanger && HealWhileSafeCount > 0)
                BaseRegen *= (float)(2.5 + (double)(HealWhileSafeCount - 1) * 1.5);

            float LifeRegen = BaseRegen + (float)KnurlCount * 1.6f;

            if (HealthDecayCount > 0)
                LifeRegen -= character.maxHealth / (float)HealthDecayCount;

            character.SetPropertyValue("regen", LifeRegen);


            //Shield
            float MaxShield = character.baseMaxShield + character.levelMaxShield * (character.level - 1);

            if (character.inventory.GetItemCount(ItemIndex.ShieldOnly) > 0)
            {

                MaxShield += character.maxHealth * (1.5f + (character.inventory.GetItemCount(ItemIndex.ShieldOnly) - 1) * 0.25f);
                character.SetPropertyValue("maxHealth", 1);
            }
            if (character.HasBuff(BuffIndex.EngiShield))
                MaxShield += character.maxHealth * 1f;
            if (character.HasBuff(BuffIndex.EngiTeamShield))
                MaxShield += character.maxHealth * 0.5f;
            MaxShield += character.inventory.GetItemCount(ItemIndex.PersonalShield) * (25 + ShieldBoost * (character.level - 1));
            if (character.GetFieldValue<BuffMask>("buffMask").HasBuff(BuffIndex.AffixBlue))
            {
                character.SetPropertyValue("maxHealth", character.maxHealth * 0.5f);
                MaxShield += character.maxHealth;
            }
            character.SetPropertyValue("maxShield", MaxShield);

            //moveSpeed
            float BaseMoveSpeed = character.baseMoveSpeed + character.levelMoveSpeed * Level;
            float RedWinpBOnus = 1f;
            if (Run.instance.enabledArtifacts.HasArtifact(ArtifactIndex.Spirit))
            {
                float num33 = 1f;
                if ((bool)character.healthComponent)
                    num33 = character.healthComponent.combinedHealthFraction;
                RedWinpBOnus += 1f - num33;
            }
            if (equipmentIndex == EquipmentIndex.AffixYellow)
                BaseMoveSpeed += 2f;
            if (character.isSprinting)
                BaseMoveSpeed *= character.GetFieldValue<float>("sprintingSpeedMultiplier");
            if (character.outOfCombat && character.outOfDanger && RedWimpCount > 0)
                RedWinpBOnus += (float)RedWimpCount * 0.3f;
            float SpeedBonus = RedWinpBOnus + (float)HoofCount * 0.14f;
            if (character.isSprinting && SodaCount > 0)
                SpeedBonus += (float)(0.100000001490116 + 0.200000002980232 * (double)SodaCount) / character.GetFieldValue<float>("sprintingSpeedMultiplier");
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
            if (BettleJuiceCount > 0)
                MoveSpeed *= (float)(1.0 - 0.0500000007450581 * (double)BettleJuiceCount);
            character.SetPropertyValue("moveSpeed" , MoveSpeed);

            //OtherMovement Bonus
            character.SetPropertyValue("acceleration" , character.moveSpeed / character.baseMoveSpeed * character.baseAcceleration);
            character.SetPropertyValue("jumpPower" , character.baseJumpPower + character.levelJumpPower * Level);
            character.SetPropertyValue("maxJumpHeight" , Trajectory.CalculateApex(character.jumpPower));
            character.SetPropertyValue("maxJumpCount" , character.baseJumpCount + FeatherCount);

            //Damage
            float BaseDamage = character.baseDamage + character.levelDamage * Level;
            float DamageBoost = 1f;
            int DamageBoostCount = character.inventory ? character.inventory.GetItemCount(ItemIndex.BoostDamage) : 0;
            if (DamageBoostCount > 0)
                DamageBoost += DamageBoostCount * 0.1f;
            if (BettleJuiceCount > 0)
                DamageBoost -= 0.05f * BettleJuiceCount;
            if (character.HasBuff(BuffIndex.GoldEmpowered))
                ++DamageBoost;
            float DamageMult = DamageBoost + (GlassCanonPower - 1f);
            character.SetPropertyValue("damage", BaseDamage * DamageMult);

            //Attack Speed
            float BaseAttackSpeed = character.baseAttackSpeed + character.levelAttackSpeed * Level;
            float AttackSpeedBonus = 1f + SyringeCount * 0.15f;
            if (equipmentIndex == EquipmentIndex.AffixYellow)
                AttackSpeedBonus += 0.5f;
            float AttackSpeedMult = AttackSpeedBonus + character.GetFieldValue<int[]>("buffs")[2] * 0.12f;
            if (character.HasBuff(BuffIndex.Warbanner))
                AttackSpeedMult += 0.3f;
            if (character.HasBuff(BuffIndex.EnrageAncientWisp))
                AttackSpeedMult += 2f;
            if (character.HasBuff(BuffIndex.WarCryBuff))
                ++AttackSpeedMult;
            float AttackSpeed = BaseAttackSpeed * AttackSpeedMult;
            if (BettleJuiceCount > 0)
                AttackSpeed *= (float)(1.0 - 0.05 * BettleJuiceCount);
            character.SetPropertyValue("attackSpeed", AttackSpeed);

            //Crit
            float CriticalChance = character.baseCrit + character.levelCrit * Level + GlassesCount * 10f;
            if (AttackSpeedCritCount > 0)
                CriticalChance += 5f;
            if (CoolDownCritCount > 0)
                CriticalChance += 5f;
            if (HealOnCritCount > 0)
                CriticalChance += 5f;
            if (CritHealCount > 0)
                CriticalChance += 5f;
            if (character.HasBuff(BuffIndex.FullCrit))
                CriticalChance += 100f;
            character.SetPropertyValue("crit", CriticalChance);


            //Armor
            character.SetPropertyValue("armor" , value: (float)(character.baseArmor + character.levelArmor * (double)Level + (character.HasBuff(BuffIndex.ArmorBoost) ? 200.0 : 0.0)));
            character.SetPropertyValue("armor" , character.armor + DrizzlePlayerHelperCount * 70f);
            if (character.HasBuff(BuffIndex.Cripple))
                character.SetPropertyValue("armor", character.armor - 20f);
            if (character.isSprinting && BucklerCount > 0)
                character.SetPropertyValue("armor" , character.armor  + BucklerCount * 30);



            //CoolDown
            float CoolDownMultiplier = 1f;
            if (character.HasBuff(BuffIndex.GoldEmpowered))
                CoolDownMultiplier *= 0.25f;
            for (int index = 0; index < AlienHeadCount; ++index)
                CoolDownMultiplier *= 0.75f;
            if (character.HasBuff(BuffIndex.NoCooldowns))
                CoolDownMultiplier = 0.0f;
            if ((bool)character.GetFieldValue<SkillLocator>("skillLocator").primary)
                character.GetFieldValue<SkillLocator>("skillLocator").primary.cooldownScale = CoolDownMultiplier;
            if ((bool)character.GetFieldValue<SkillLocator>("skillLocator").secondary)
            {
                character.GetFieldValue<SkillLocator>("skillLocator").secondary.cooldownScale = CoolDownMultiplier;
                character.GetFieldValue<SkillLocator>("skillLocator").secondary.SetBonusStockFromBody(newBonusStockFromBody);
            }
            if ((bool)character.GetFieldValue<SkillLocator>("skillLocator").utility)
            {
                float num33 = CoolDownMultiplier;
                if (UtilityMagazineCount > 0)
                    num33 *= 0.6666667f;
                character.GetFieldValue<SkillLocator>("skillLocator").utility.cooldownScale = num33;
                character.GetFieldValue<SkillLocator>("skillLocator").utility.SetBonusStockFromBody(UtilityMagazineCount * 2);
            }
            if ((bool)character.GetFieldValue<SkillLocator>("skillLocator").special)
                character.GetFieldValue<SkillLocator>("skillLocator").special.cooldownScale = CoolDownMultiplier;


            //CriticalHeal
            character.SetPropertyValue("critHeal", 0.0f);
            if (CritHealCount > 0)
            {
                float crit = character.crit;
                character.SetPropertyValue("crit", character.crit/ (CritHealCount + 1));
                character.SetPropertyValue("critHeal", crit - character.crit);
            }

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

        /*
        if (gameResultType == GameResultType.Unknown)
        {
            for (int i = 0; i < NetworkUser.readOnlyInstancesList.Count; i++)
            {
                NetworkUser networkUser = NetworkUser.readOnlyInstancesList[i];
                if ((bool)networkUser && networkUser.isParticipating)
                {
                    networkUser.AwardLunarCoins(5u);
                }
            }
        }
        */
        public void Awake()
        {
            Debug.Log("Exponential Health, load Completed");
            InitConfig();

            try {
                System.Globalization.NumberStyles S = System.Globalization.NumberStyles.AllowDecimalPoint;
                System.Globalization.CultureInfo CI = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");
               HExpo = float.Parse(exponentialValueHealth.Value,S,CI);
                ShieldBoost = float.Parse(ShieldBoostPerLevel.Value, S, CI);
            }
            catch (InvalidCastException e)
            {
                Debug.LogWarning(e.Message);
            }

            On.RoR2.CharacterBody.RecalculateStats += Recalulate;
        }
    }
}