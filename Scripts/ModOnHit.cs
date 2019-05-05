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
using IL;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Linq;


namespace PlexusUtils
{

    class ModEffects
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

            ModItemManager.OnHitEnemyEffects(self, damageInfo, victim);


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
        static public void ModdedHitAll(On.RoR2.GlobalEventManager.orig_OnHitAll orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {

            if ((double)damageInfo.procCoefficient == 0.0)
                return;
            int Host = NetworkServer.active ? 1 : 0;
            if (!(bool)((UnityEngine.Object)damageInfo.attacker))
                return;
            CharacterBody component = damageInfo.attacker.GetComponent<CharacterBody>();
            if (!(bool)((UnityEngine.Object)component))
                return;
            CharacterMaster master = component.master;
            if (!(bool)((UnityEngine.Object)master))
                return;
            Inventory inventory = master.inventory;
            if (!(bool)((UnityEngine.Object)master.inventory))
                return;
            damageInfo.procChainMask.LinkToManager();


            ModItemManager.OnHitAllEffects(self, damageInfo, victim);

            //Buff
            if ((component.HasBuff(BuffIndex.AffixBlue) ? 1 : 0) <= 0)
                return;
            float damage = damageInfo.damage * 0.5f;
            float force = 0.0f;
            Vector3 position = damageInfo.position;
            #pragma warning disable CS0618 //Obsolete warning
            ProjectileManager.instance.FireProjectile(Resources.Load<GameObject>("Prefabs/Projectiles/LightningStake"), position, Quaternion.identity, damageInfo.attacker, damage, force, damageInfo.crit, DamageColorIndex.Item, (GameObject)null, -1f);
            #pragma warning restore CS0618 

            damageInfo.procChainMask.UnlinkToManager();
        }

        static public void OnPlayerCharacterDeath(
      DamageInfo damageInfo,
      GameObject victim,
      NetworkUser victimNetworkUser)
        {
            CharacterBody component = victim.GetComponent<CharacterBody>();
            string str = "PLAYER_DEATH_QUOTE_" + UnityEngine.Random.Range(0, 37);
            NetworkUser networkUser = Util.LookUpBodyNetworkUser(component);
            if (!(bool)networkUser)
                return;
            Chat.PlayerDeathChatMessage deathChatMessage = new Chat.PlayerDeathChatMessage();
            deathChatMessage.subjectNetworkUser = networkUser;
            deathChatMessage.baseToken = str;
            deathChatMessage.paramTokens = new string[1]
            {
        networkUser.userName
            };
            Chat.SendBroadcastChat(deathChatMessage);
        }

        static public void ModdedCharacterDeath(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
        {
            Debug.Log("CharacterDeathModded");
            orig(self,damageReport);
            
            return;
            if (!NetworkServer.active)
                return;
            GameObject VictimGo = damageReport.victim.gameObject;
            DamageInfo damageInfo = damageReport.damageInfo;
            TeamComponent Team = VictimGo.GetComponent<TeamComponent>();
            TeamIndex TeamIndex = TeamIndex.Neutral;
            CharacterBody VictimCharacterBody = VictimGo.GetComponent<CharacterBody>();
            EquipmentIndex equipmentIndex = VictimCharacterBody.equipmentSlot ? VictimCharacterBody.equipmentSlot.equipmentIndex : EquipmentIndex.None;
            //Some Unreleased Content
            if ((bool)Team)
            {
                TeamIndex = Team.teamIndex;
                if (TeamIndex == TeamIndex.Monster && Run.instance.enabledArtifacts.HasArtifact(ArtifactIndex.Bomb))
                {
                    Debug.Log("team and artifact OK");
                    ModelLocator component3 = VictimGo.GetComponent<ModelLocator>();
                    if ((bool)component3)
                    {
                        Debug.Log("victimModelLocator OK");
                        Transform modelTransform = component3.modelTransform;
                        if ((bool)modelTransform)
                        {
                            Debug.Log("victimModelTransform OK");
                            HurtBoxGroup component4 = modelTransform.GetComponent<HurtBoxGroup>();
                            if ((bool)component4)
                            {
                                Debug.Log("victimHurtBoxGroup OK");
                                float damage = 0.0f;
                                if ((bool)VictimCharacterBody)
                                    damage = VictimCharacterBody.damage;
                                HurtBoxGroup.VolumeDistribution volumeDistribution = component4.GetVolumeDistribution();
                                int num = Mathf.CeilToInt(volumeDistribution.totalVolume / 10f);
                                Debug.LogFormat("bombCount={0}", (object)num);
                                for (int index = 0; index < num; ++index)
                                    ProjectileManager.instance.FireProjectile(Resources.Load<GameObject>("Prefabs/Projectiles/Funball"), volumeDistribution.randomVolumePoint, Quaternion.identity, VictimGo, damage, 700f, false, DamageColorIndex.Default, (GameObject)null, -1f);
                            }
                        }
                    }
                }
            }

            if ((bool)VictimCharacterBody)
            {
                CharacterMaster master = VictimCharacterBody.master;
                if ((bool)master)
                {
                    PlayerCharacterMasterController MasterControler = master.GetComponent<PlayerCharacterMasterController>();
                    if ((bool)MasterControler)
                    {
                        GameObject networkUserObject = MasterControler.networkUserObject;
                        if ((bool)networkUserObject)
                        {
                            NetworkUser NetWorkUser = networkUserObject.GetComponent<NetworkUser>();
                            if ((bool)NetWorkUser)
                                self.GetPropertyValue<Action>("OnPlayerCharacterDeath").DynamicInvoke(damageInfo, VictimGo, NetWorkUser);
                        }
                    }
                    //Ice Elite Effect
                    if (VictimCharacterBody.HasBuff(BuffIndex.AffixWhite))
                    {
                        Vector3 corePosition = Util.GetCorePosition(VictimGo);
                        GameObject gameObject2 = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/NetworkedObjects/GenericDelayBlast"), corePosition, Quaternion.identity);
                        float num = 12f + VictimCharacterBody.radius;
                        gameObject2.transform.localScale = new Vector3(num, num, num);
                        DelayBlast component4 = gameObject2.GetComponent<DelayBlast>();
                        component4.position = corePosition;
                        component4.baseDamage = VictimCharacterBody.damage * 1.5f;
                        component4.baseForce = 2300f;
                        component4.attacker = VictimCharacterBody.gameObject;
                        component4.radius = num;
                        component4.crit = Util.CheckRoll(VictimCharacterBody.crit, master);
                        component4.procCoefficient = 0.75f;
                        component4.maxTimer = 2f;
                        component4.falloffModel = BlastAttack.FalloffModel.None;
                        component4.explosionEffect = Resources.Load<GameObject>("Prefabs/Effects/ImpactEffects/AffixWhiteExplosion");
                        component4.delayEffect = Resources.Load<GameObject>("Prefabs/Effects/AffixWhiteDelayEffect");
                        component4.damageType = DamageType.Freeze2s;
                        gameObject2.GetComponent<TeamFilter>().teamIndex = TeamComponent.GetObjectTeam(component4.attacker);
                    }
                }
            }
            if ((bool)damageInfo.attacker)
            {
                CharacterBody AttackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                if ((bool)AttackerBody)
                {
                    CharacterMaster AttackerMaster = AttackerBody.master;
                    if ((bool)AttackerMaster)
                    {
                        Inventory inventory = AttackerMaster.inventory;
                        TeamComponent AttackerTeam = AttackerBody.GetComponent<TeamComponent>();
                        TeamIndex teamIndex2 = AttackerTeam ? AttackerTeam.teamIndex : TeamIndex.Neutral;

                        //Gasoil
                        int GasoilCount = inventory.GetItemCount(ItemIndex.IgniteOnKill);
                        if (GasoilCount > 0)
                        {
                            ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(TeamIndex);
                            float num1 = (float)(8.0 + 4.0 * GasoilCount) + VictimCharacterBody.radius;
                            float num2 = num1 * num1;
                            Vector3 corePosition = Util.GetCorePosition(VictimGo);
                            EffectManager.instance.SpawnEffect(Resources.Load<GameObject>("Prefabs/Effects/ImpactEffects/IgniteExplosionVFX"), new EffectData()
                            {
                                origin = corePosition,
                                scale = num1,
                                rotation = Util.QuaternionSafeLookRotation(damageInfo.force)
                            }, true);
                            for (int index = 0; index < teamMembers.Count; ++index)
                            {
                                if ((teamMembers[index].transform.position - corePosition).sqrMagnitude <= num2)
                                    DotController.InflictDot(teamMembers[index].gameObject, damageInfo.attacker, DotController.DotIndex.Burn, (float)(1.5 + 1.5 * GasoilCount), 1f);
                            }
                        }

                        //Wisp
                        int WispCount = inventory.GetItemCount(ItemIndex.ExplodeOnDeath);
                        if (WispCount > 0)
                        {
                            Vector3 corePosition = Util.GetCorePosition(VictimGo);
                            float damageCoefficient = (float)(3.5 * (1.0 + (WispCount - 1) * 0.800000011920929));
                            float num = Util.OnKillProcDamage(AttackerBody.damage, damageCoefficient);
                            GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(self.explodeOnDeathPrefab, corePosition, Quaternion.identity);
                            DelayBlast component5 = gameObject2.GetComponent<DelayBlast>();
                            component5.position = corePosition;
                            component5.baseDamage = num;
                            component5.baseForce = 2000f;
                            component5.bonusForce = Vector3.up * 1000f;
                            component5.radius = (float)(12.0 + 2.40000009536743 * (WispCount - 1.0));
                            component5.attacker = damageInfo.attacker;
                            component5.inflictor = (GameObject)null;
                            component5.crit = Util.CheckRoll(AttackerBody.crit, AttackerMaster);
                            component5.maxTimer = 0.5f;
                            component5.damageColorIndex = DamageColorIndex.Item;
                            component5.falloffModel = BlastAttack.FalloffModel.SweetSpot;
                            gameObject2.GetComponent<TeamFilter>().teamIndex = TeamComponent.GetObjectTeam(component5.attacker);
                            NetworkServer.Spawn(gameObject2);
                        }
                        //Dagger
                        int DaggerCount = inventory.GetItemCount(ItemIndex.Dagger);
                        if (DaggerCount > 0)
                        {
                            for (int index = 0; index < DaggerCount * 3; ++index)
                            {
                                GameObject gameObject2 = UnityEngine.Object.Instantiate(self.daggerPrefab, VictimGo.transform.position + Vector3.up * 1.8f + UnityEngine.Random.insideUnitSphere * 0.5f, Util.QuaternionSafeLookRotation(Vector3.up + UnityEngine.Random.insideUnitSphere * 0.1f));
                                gameObject2.GetComponent<ProjectileController>().Networkowner = AttackerBody.gameObject;
                                gameObject2.GetComponent<TeamFilter>().teamIndex = teamIndex2;
                                gameObject2.GetComponent<DaggerController>().delayTimer += (float)index * 0.05f;
                                float damageCoefficient = 1.5f;
                                float num = Util.OnKillProcDamage(AttackerBody.damage, damageCoefficient);
                                ProjectileDamage component5 = gameObject2.GetComponent<ProjectileDamage>();
                                component5.damage = num;
                                component5.crit = Util.CheckRoll(AttackerBody.crit, AttackerMaster);
                                component5.force = 200f;
                                component5.damageColorIndex = DamageColorIndex.Item;
                                NetworkServer.Spawn(gameObject2);
                            }
                        }
                        //Tooth LOL USELESS
                        int ToothCount = inventory.GetItemCount(ItemIndex.Tooth);
                        if (ToothCount > 0)
                        {
                            float num = Mathf.Pow(ToothCount, 0.25f);
                            GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/NetworkedObjects/HealPack"), VictimGo.transform.position, UnityEngine.Random.rotation);
                            gameObject2.GetComponent<TeamFilter>().teamIndex = teamIndex2;
                            gameObject2.GetComponentInChildren<HealthPickup>().flatHealing = 4f * ToothCount;
                            gameObject2.transform.localScale = new Vector3(num, num, num);
                            NetworkServer.Spawn(gameObject2);
                        }
                        //Infusion
                        int InfusionCount = inventory.GetItemCount(ItemIndex.Infusion);
                        if (InfusionCount > 0)
                        {
                            int num = InfusionCount * 100;
                            if ((long)inventory.infusionBonus < num)
                            {
                                InfusionOrb infusionOrb = new InfusionOrb();
                                infusionOrb.origin = VictimGo.transform.position;
                                infusionOrb.target = Util.FindBodyMainHurtBox(AttackerBody);
                                infusionOrb.maxHpValue = 1;
                                OrbManager.instance.AddOrb((Orb)infusionOrb);
                            }
                        }

                        //MightNoTouchThat
                        if ((damageInfo.damageType & DamageType.ResetCooldownsOnKill) == DamageType.ResetCooldownsOnKill)
                        {
                            SkillLocator component5 = AttackerBody.GetComponent<SkillLocator>();
                            if ((bool)component5)
                                component5.ResetSkills();
                        }
                        //Talisman
                        if ((bool)inventory)
                        {
                            int itemCount6 = inventory.GetItemCount(ItemIndex.Talisman);
                            if (itemCount6 > 0 && AttackerBody.GetComponent<EquipmentSlot>())
                                inventory.DeductActiveEquipmentCooldown((float)(2.0 + itemCount6 * 2.0));
                        }
                        //TempestonKill
                        int itemCount7 = inventory.GetItemCount(ItemIndex.TempestOnKill);
                        if (itemCount7 > 0 && Util.CheckRoll(25f, AttackerMaster))
                        {
                            GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/NetworkedObjects/TempestWard"), VictimCharacterBody.footPosition, Quaternion.identity);
                            gameObject2.GetComponent<TeamFilter>().teamIndex = AttackerTeam.teamIndex;
                            gameObject2.GetComponent<BuffWard>().expireDuration = (float)(2.0 + 6.0 * itemCount7);
                            NetworkServer.Spawn(gameObject2);
                        }
                        //Drop Ammo on Death
                        int itemCount8 = inventory.GetItemCount(ItemIndex.Bandolier);
                        if (itemCount8 > 0 && Util.CheckRoll((float)((1.0 - 1.0 / Mathf.Pow((itemCount8 + 1), 0.33f)) * 100.0), AttackerMaster))
                        {
                            GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/NetworkedObjects/AmmoPack"), VictimGo.transform.position, UnityEngine.Random.rotation);
                            gameObject2.GetComponent<TeamFilter>().teamIndex = teamIndex2;
                            NetworkServer.Spawn(gameObject2);
                        }
                        //HeadHunter ? 
                        if (VictimCharacterBody && VictimCharacterBody.isElite)
                        {
                            int itemCount6 = inventory.GetItemCount(ItemIndex.HeadHunter);
                            int itemCount9 = inventory.GetItemCount(ItemIndex.KillEliteFrenzy);
                            if (itemCount6 > 0)
                            {
                                float duration = (float)(3.0 + 5.0 * (double)itemCount6);
                                for (int index = 0; index < BuffCatalog.eliteBuffIndices.Length; ++index)
                                {
                                    BuffIndex eliteBuffIndex = BuffCatalog.eliteBuffIndices[index];
                                    if (VictimCharacterBody.HasBuff(eliteBuffIndex))
                                        AttackerBody.AddTimedBuff(eliteBuffIndex, duration);
                                }
                            }
                            if (itemCount9 > 0)
                                AttackerBody.AddTimedBuff(BuffIndex.NoCooldowns, (float)(1.0 + (double)itemCount9 * 2.0));
                        }
                        //GhostMask !
                        int itemCount10 = inventory.GetItemCount(ItemIndex.GhostOnKill);
                        if (itemCount10 > 0 && VictimCharacterBody && Util.CheckRoll(10f, AttackerMaster))
                            Util.TryToCreateGhost(VictimCharacterBody, AttackerBody, itemCount10 * 30);
                        DeathRewards component6 = VictimCharacterBody.GetComponent<DeathRewards>();
                        if (Run.instance.enabledArtifacts.HasArtifact(ArtifactIndex.Sacrifice) && (bool)((UnityEngine.Object)component6) && Util.CheckRoll(3f * Mathf.Log((float)component6.expReward + 1f, 3f), AttackerMaster))
                        {
                            List<PickupIndex> pickupIndexList = Run.instance.smallChestDropTierSelector.Evaluate(UnityEngine.Random.value);
                            PickupIndex none = PickupIndex.none;
                            if (pickupIndexList.Count > 0)
                                none = pickupIndexList[UnityEngine.Random.Range(0, pickupIndexList.Count - 1)];
                            PickupDropletController.CreatePickupDroplet(none, VictimGo.transform.position, Vector3.up * 20f);
                        }
                        if (Util.CheckRoll(0.025f, AttackerMaster) && (bool)((UnityEngine.Object)VictimCharacterBody) && VictimCharacterBody.isElite)
                            PickupDropletController.CreatePickupDroplet(new PickupIndex(equipmentIndex), VictimCharacterBody.transform.position + Vector3.up * 1.5f, Vector3.up * 20f + self.transform.forward * 2f);
                    }
                }
            }
            int DoesItHaveGreenAfix = equipmentIndex == EquipmentIndex.AffixGreen ? 1 : 0;
            if (DoesItHaveGreenAfix > 0)
            {
                float num1 = 0.25f * VictimCharacterBody.maxHealth;
                float num2 = (float)(900.0 + (double)VictimCharacterBody.radius * (double)VictimCharacterBody.radius);
                Vector3 position1 = VictimGo.transform.position;
                ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(TeamIndex);
                for (int index = 0; index < teamMembers.Count; ++index)
                {
                    Vector3 position2 = teamMembers[index].transform.position;
                    if ((double)Vector3.SqrMagnitude(position1 - position2) < (double)num2)
                    {
                        HealOrb healOrb = new HealOrb();
                        healOrb.origin = position1;
                        healOrb.target = Util.FindBodyMainHurtBox(teamMembers[index].gameObject);
                        healOrb.healValue = num1 * (float)DoesItHaveGreenAfix;
                        healOrb.scaleOrb = false;
                        OrbManager.instance.AddOrb((Orb)healOrb);
                    }
                }
            }
        }


        static public void Init()
        {
            /*
            IL.RoR2.GlobalEventManager.OnCharacterDeath += il =>
            {
                var cursor = new ILCursor(il);

                Instruction

                cursor.GotoNext( x => x.MatchLdloc())
            };
            On.RoR2.GlobalEventManager.OnCharacterDeath += ModdedCharacterDeath;
            */
            On.RoR2.GlobalEventManager.OnHitEnemy += ModdedHitEnemy;
            On.RoR2.GlobalEventManager.OnHitAll += ModdedHitAll;
            
        }
    }
}


