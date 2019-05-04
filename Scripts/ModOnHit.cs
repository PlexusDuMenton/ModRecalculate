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
using RoR2.Orbs;
using RoR2.Projectile;
using System.Collections.ObjectModel;

namespace PlexusUtils
{

    class ModOnHitEnemy
    {
        /// <summary>
        /// Please don't touch this value, Used by ice and Fire ring since they share the SAME roll
        /// 0 mean Unset, 1 = True, 2 = False
        /// </summary>
        static public int ringBuffer = 0;


        static public void ModdedHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self,DamageInfo damageInfo, GameObject victim)
        {

            if (damageInfo.procCoefficient == 0 || !NetworkServer.active || (!(bool)damageInfo.attacker || damageInfo.procCoefficient <= 0))
                return;

            CharacterBody Attacker = damageInfo.attacker.GetComponent<CharacterBody>();
            CharacterBody characterBody = victim ? victim.GetComponent<CharacterBody>() : null;

            if (!Attacker)
                return;
            CharacterMaster master = Attacker.master;
            if (!master)
                return;
            damageInfo.procChainMask.LinkToManager();

            Inventory inventory = master.inventory;
            TeamComponent Team = Attacker.GetComponent<TeamComponent>();
            TeamIndex attackerTeamIndex = Team ? Team.teamIndex : TeamIndex.Neutral;

            Vector3 aimOrigin = Attacker.aimOrigin;

            ModItemManager.ExecuteOrderSixtySix(self, damageInfo, victim);


            //SetOnFire . Can't realy do much for this one 
            int DamageType = (uint)(damageInfo.damageType & RoR2.DamageType.IgniteOnHit) > 0U ? 1 : 0;
            bool CanSetFire = (damageInfo.damageType & RoR2.DamageType.PercentIgniteOnHit) != RoR2.DamageType.Generic || Attacker.HasBuff(BuffIndex.AffixRed);
            int num2 = CanSetFire ? 1 : 0;
            if ((DamageType | num2) != 0)
                DotController.InflictDot(victim, damageInfo.attacker, CanSetFire ? DotController.DotIndex.PercentBurn : DotController.DotIndex.Burn, 4f * damageInfo.procCoefficient, 1f);

            //Apply Ice Elite (Will have to wait for Buff Change for that)
            if ((Attacker.HasBuff(BuffIndex.AffixWhite) ? 1 : 0) > 0 && (bool)((UnityEngine.Object)characterBody))
                characterBody.AddTimedBuff(BuffIndex.Slow80, 1.5f * damageInfo.procCoefficient);

            damageInfo.procChainMask.UnlinkToManager();
        }

        

        static public void Init()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += ModdedHitEnemy;
        }
    }
}


