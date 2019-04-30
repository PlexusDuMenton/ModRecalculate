using BepInEx;
using UnityEngine;
using RoR2;
using BepInEx.Configuration;
using AetherLib.Util.Reflection;
using System;
namespace plexus
{
    //This is an example plugin that can be put in BepInEx/plugins/ExamplePlugin/ExamplePlugin.dll to test out.
    //It's a very simple plugin that adds Bandit to the game, and gives you a tier 3 item whenever you press F2.
    //Lets examine what each line of code is for:

    //This attribute specifies that we have a dependency on R2API, as we're using it to add Bandit to the game.
    //You don't need this if you're not using R2API in your plugin, it's just to tell BepInEx to initialize R2API before this plugin so it's safe to use R2API.
    [BepInDependency("com.bepis.r2api")]
    [BepInDependency("at.aster.aetherlib")]

    //This attribute is required, and lists metadata for your plugin.
    //The GUID should be a unique ID for this plugin, which is human readable (as it is used in places like the config). I like to use the java package notation, which is "com.[your name here].[your plugin name here]"
    //The name is the name of the plugin that's displayed on load, and the version number just specifies what version the plugin is.

    [BepInPlugin("com.Plexus.RandomSpices", "RandomSpices", "0.1.0")]

    //This is the main declaration of our plugin class. BepInEx searches for all classes inheriting from BaseUnityPlugin to initialize on startup.
    //BaseUnityPlugin itself inherits from MonoBehaviour, so you can use this as a reference for what you can declare and use in your plugin class: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    public class RandomSpices : BaseUnityPlugin
    {
        public ConfigWrapper<string> exponentialValueHealth;
        public ConfigWrapper<string> ShieldBoostPerLevel;
        public float HExpo = 1;
        public float ShieldBoost = 1;

        public void InitConfig()
        {
            exponentialValueHealth = Config.Wrap(
                    "exponential Health value",
                    "exponentialValueHealth",
                    "Sets exponential value applied to level when calculating health. default = 1",
            "1.5");

            ShieldBoostPerLevel = Config.Wrap(
                    "Shield Boost per level",
                    "ShieldBoostPerLevel",
                    "Increase the bonus from shield item per player level. default = 0",
            "5");
        }

        public void Recalulate(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody character)
        {
             
            orig(character);
            float MaxHealth = character.baseMaxHealth;
            float additionalHealth = 0;
            if (character.inventory.GetItemCount(ItemIndex.Infusion) > 0)
                additionalHealth += character.inventory.infusionBonus;
            additionalHealth += character.inventory.GetItemCount(ItemIndex.Knurl) * 40;

            float hpbooster = 1 + character.inventory.GetItemCount(ItemIndex.BoostHp) * 0.1f;

            MaxHealth = Utils.exponentialFormula(MaxHealth, character.levelMaxHealth, HExpo, character.level, 0) * hpbooster / character.CalcLunarDaggerPower();
            character.SetPropertyValue("maxHealth", MaxHealth);


            float MaxShield = character.baseMaxShield + character.levelMaxShield * (character.level-1);

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

            character.SetPropertyValue("maxShield", MaxShield);

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
                Debug.Log(exponentialValueHealth.Value);
                System.Globalization.NumberStyles S = System.Globalization.NumberStyles.AllowDecimalPoint;
                System.Globalization.CultureInfo CI = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");
               HExpo = float.Parse(exponentialValueHealth.Value,S,CI);
                Debug.Log(HExpo);
            }
            catch (InvalidCastException e)
            {
                Debug.LogWarning(e.Message);
            }

            On.RoR2.CharacterBody.RecalculateStats += Recalulate;
        }
    }
}