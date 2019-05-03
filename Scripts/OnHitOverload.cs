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


    

    class ModProc
    {
        public Dictionary<string, bool> ProcList;

        public ModProc()
        {
            foreach (ProcType proc in (ProcType[])Enum.GetValues(typeof(ProcType)))
            {
                ProcList.Add(proc.ToString(), false);
            }
            foreach(string proc in ModProcManager.CustomProcList)
            {
                ProcList.Add(proc, false);
            }
        }

        public void ChangeProcState(string Proc, bool value)
        {
            if (ProcList.ContainsKey(Proc))
                ProcList[Proc] = value;
        }
    }

    static class ProcChainMaskExtention
    {
        public static void SetProcValue(this ProcChainMask procMask, string ProcName, bool value)
        {
            ModProcManager.SetProcValue(procMask, ProcName, value);
        }
        public static void SetProcValue(this ProcChainMask procMask, ProcType Proc, bool value)
        {
            ModProcManager.SetProcValue(procMask, Proc.ToString(), value);
        }
        public static bool GetProcValue(this ProcChainMask procMask, string ProcName)
        {
            return ModProcManager.GetProcValue(procMask, ProcName);
        }
        public static bool GetProcValue(this ProcChainMask procMask, ProcType Proc)
        {
            return ModProcManager.GetProcValue(procMask, Proc.ToString());
        }
        
    }

    class ModProcManager
    {

        public static List<string> CustomProcList;

        /// <summary>
        /// Used to decalre new Proc Type in addition to existing one
        /// </summary>
        /// <param name="ProcName"></param>
        public static void DeclareNewProc(string ProcName)
        {
            if (!CustomProcList.Contains(ProcName))
            {
                CustomProcList.Add(ProcName);
            }
            else
            {
                throw new Exception("Mod Proc Manager : Trying to declare an existing Proc : " + ProcName);
            }
        }

        public static Dictionary<ProcChainMask, ModProc> ProcChainLinker;

        public static void SetProcValue(ProcChainMask chain, string ProcName, bool value)
        {
            if (ProcChainLinker.ContainsKey(chain))
            {
                ProcChainLinker[chain].ChangeProcState(ProcName, value);
            }
        }
        public static bool GetProcValue(ProcChainMask chain, string ProcName)
        {
            if (ProcChainLinker.ContainsKey(chain))
            {
                if (ProcChainLinker[chain].ProcList.ContainsKey(ProcName))
                    return ProcChainLinker[chain].ProcList[ProcName];
            }
            return false;
        }

        public static void AddLink(ProcChainMask chain, ModProc modproc)
        {
            if (!ProcChainLinker.ContainsKey(chain))
            {
                ProcChainLinker.Add(chain,modproc);
            }
        }

        public static void RemoveLink(ProcChainMask chain)
        {
            if (ProcChainLinker.ContainsKey(chain))
            {
                ProcChainLinker.Remove(chain);
            }
        }

        public static void RemoveAllLink()
        {
            ProcChainLinker = new Dictionary<ProcChainMask, ModProc>();
        }


    }

    class HealOnCritHitReplace : ModHitEffect
    {
        public override bool Condition(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim)
        {
            ProcChainMask procChainMask = damageInfo.procChainMask;
            return procChainMask.GetProcValue(ProcType.HealOnCrit);
            return (!procChainMask.HasProc(ProcType.HealOnCrit));
        }

        public override void Effect(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim)
        {
            float procCoefficient = damageInfo.procCoefficient;
            CharacterBody body = damageInfo.attacker.GetComponent<CharacterBody>();
            CharacterMaster master = body.master;
            ProcChainMask procChainMask = damageInfo.procChainMask;

            Inventory inventory = master.inventory;

            procChainMask.AddProc(ProcType.HealOnCrit);
            int itemCount = inventory.GetItemCount(ItemIndex.HealOnCrit);
            if (itemCount > 0 && body.healthComponent)
            {
                int ProcHealthSoundId = (int)Util.PlaySound("Play_item_proc_crit_heal", body.gameObject);
                if (NetworkServer.active)
                {
                    double HealResult = body.healthComponent.Heal((float)(4f + itemCount * 4f) * procCoefficient, procChainMask, true);
                }
            }
        }

    }

    class CritHitReplace : ModHitEffect
    {
        public override bool Condition(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim)
        {
            return base.Condition(globalEventManager, damageInfo, victim);
        }

        public override void Effect(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim)
        {

            float procCoefficient = damageInfo.procCoefficient;
            CharacterBody body = damageInfo.attacker.GetComponent<CharacterBody>();
            CharacterMaster master = body.master;
            ProcChainMask procChainMask = damageInfo.procChainMask;


            if (!(bool)body || procCoefficient <= 0.0 || !(bool)body || !(bool)master || !(bool)master.inventory)
                return;
            Inventory inventory = master.inventory;
            if (!procChainMask.HasProc(ProcType.HealOnCrit))
            {
                
            }
            //Predatory Instincts 
            if (inventory.GetItemCount(ItemIndex.AttackSpeedOnCrit) > 0)
                body.AddTimedBuff(BuffIndex.AttackSpeedOnCrit, 2f * procCoefficient);

            int WickedRingCount = inventory.GetItemCount(ItemIndex.CooldownOnCrit);
            if (WickedRingCount <= 0)
                return;
            int num = (int)Util.PlaySound("Play_item_proc_crit_cooldown", body.gameObject);
            SkillLocator component = body.GetComponent<SkillLocator>();
            if (!(bool)component)
                return;
            float dt = WickedRingCount * procCoefficient;
            if ((bool)component.primary)
                component.primary.RunRecharge(dt);
            if ((bool)component.secondary)
                component.secondary.RunRecharge(dt);
            if ((bool)component.utility)
                component.utility.RunRecharge(dt);
            if (!(bool)component.special)
                return;
            component.special.RunRecharge(dt);
        }
    }

    class ModOnHitEnemy
    {


        public 

        public delegate void Hook_voidHook(GlobalEventManager self, DamageInfo damageInfo, GameObject victim);


        static public event Hook_voidHook CritHook;

        static public void Base_CritHook(GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            CharacterBody Attacker = damageInfo.attacker.GetComponent<CharacterBody>();
            CharacterBody characterBody = victim ? victim.GetComponent<CharacterBody>() : null;
            CharacterMaster master = Attacker.master;
            
            OnCrit(Attacker, master, damageInfo.procCoefficient, damageInfo.procChainMask);
        }

        static public void OnCrit(CharacterBody body,CharacterMaster master,float procCoefficient,ProcChainMask procChainMask)
        {
            
        }


        static public void ModdedHit(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self,DamageInfo damageInfo, GameObject victim)
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


            Inventory inventory = master.inventory;
            TeamComponent Team = Attacker.GetComponent<TeamComponent>();
            TeamIndex attackerTeamIndex = Team ? Team.teamIndex : TeamIndex.Neutral;

            Vector3 aimOrigin = Attacker.aimOrigin;

            //crit :
            if (damageInfo.crit)
                GlobalEventManager.instance.OnCrit(Attacker, master, damageInfo.procCoefficient, damageInfo.procChainMask);

            //Heal On Hit
            if (!damageInfo.procChainMask.HasProc(ProcType.HealOnHit))
            {
                int itemCount = inventory.GetItemCount(ItemIndex.Seed);
                if (itemCount > 0)
                {
                    HealthComponent AttackerHealthComponent = Attacker.GetComponent<HealthComponent>();
                    if (AttackerHealthComponent)
                    {
                        ProcChainMask procChainMask = damageInfo.procChainMask;
                        procChainMask.AddProc(ProcType.HealOnHit);
                        double num = AttackerHealthComponent.Heal(itemCount * damageInfo.procCoefficient, procChainMask, true);
                    }
                }
            }
            //Stun
            int StunChanceOnHit = inventory.GetItemCount(ItemIndex.StunChanceOnHit);
            if (StunChanceOnHit > 0 && Util.CheckRoll((1.0f - 1.0f / (damageInfo.procCoefficient * 0.05f * StunChanceOnHit + 1.0f) * 100f), master))
            {
                SetStateOnHurt HurtStat = victim.GetComponent<SetStateOnHurt>();
                if (HurtStat)
                    HurtStat.SetStun(2f);
            }

            //Bleed
            if (!damageInfo.procChainMask.HasProc(ProcType.BleedOnHit))
            {
                int BleedOnHitCount = inventory.GetItemCount(ItemIndex.BleedOnHit);
                bool flag = (uint)(damageInfo.damageType & RoR2.DamageType.BleedOnHit) > 0U;
                if (BleedOnHitCount > 0 | flag && (flag || Util.CheckRoll(15f * BleedOnHitCount * damageInfo.procCoefficient, master)))
                {
                    damageInfo.procChainMask.AddProc(ProcType.BleedOnHit);
                    DotController.InflictDot(victim, damageInfo.attacker, DotController.DotIndex.Bleed, 3f * damageInfo.procCoefficient, 1f);
                }
            }

            //SetOnFire
            int DamageType = (uint)(damageInfo.damageType & RoR2.DamageType.IgniteOnHit) > 0U ? 1 : 0;
            bool CanSetFire = (damageInfo.damageType & RoR2.DamageType.PercentIgniteOnHit) != RoR2.DamageType.Generic || Attacker.HasBuff(BuffIndex.AffixRed);
            int num2 = CanSetFire ? 1 : 0;
            if ((DamageType | num2) != 0)
                DotController.InflictDot(victim, damageInfo.attacker, CanSetFire ? DotController.DotIndex.PercentBurn : DotController.DotIndex.Burn, 4f * damageInfo.procCoefficient, 1f);

            //ApplyIceDebuff ?
            if ((Attacker.HasBuff(BuffIndex.AffixWhite) ? 1 : 0) > 0 && (bool)((UnityEngine.Object)characterBody))
                characterBody.AddTimedBuff(BuffIndex.Slow80, 1.5f * damageInfo.procCoefficient);

            //Chronobuble
            int itemCount3 = master.inventory.GetItemCount(ItemIndex.SlowOnHit);
            if (itemCount3 > 0 && (bool)((UnityEngine.Object)characterBody))
                characterBody.AddTimedBuff(BuffIndex.Slow60, 1f * (float)itemCount3);

            //GoldOnHit
            int itemCount4 = inventory.GetItemCount(ItemIndex.GoldOnHit);
            if (itemCount4 > 0 && Util.CheckRoll(30f * damageInfo.procCoefficient, master))
            {
                master.GiveMoney((uint)((double)itemCount4 * 2.0 * (double)Run.instance.difficultyCoefficient));
                EffectManager.instance.SimpleImpactEffect(Resources.Load<GameObject>("Prefabs/Effects/ImpactEffects/CoinImpact"), damageInfo.position, Vector3.up, true);
            }

            //Missiles
            if (!damageInfo.procChainMask.HasProc(ProcType.Missile))
                ProcMissile(inventory.GetItemCount(ItemIndex.Missile), Attacker, master, attackerTeamIndex, damageInfo.procChainMask, victim, damageInfo);

            //Ukelele
            int itemCount5 = inventory.GetItemCount(ItemIndex.ChainLightning);
            if (itemCount5 > 0 && !damageInfo.procChainMask.HasProc(ProcType.ChainLightning) && Util.CheckRoll(25f * damageInfo.procCoefficient, master))
            {
                float damageCoefficient = 0.8f;
                float num3 = Util.OnHitProcDamage(damageInfo.damage, Attacker.damage, damageCoefficient);
                LightningOrb lightningOrb = new LightningOrb();
                lightningOrb.origin = damageInfo.position;
                lightningOrb.damageValue = num3;
                lightningOrb.isCrit = damageInfo.crit;
                lightningOrb.bouncesRemaining = 2 * itemCount5;
                lightningOrb.teamIndex = attackerTeamIndex;
                lightningOrb.attacker = damageInfo.attacker;
                lightningOrb.bouncedObjects = new List<HealthComponent>()
                {
                  victim.GetComponent<HealthComponent>()
                };
                lightningOrb.procChainMask = damageInfo.procChainMask;
                lightningOrb.procChainMask.AddProc(ProcType.ChainLightning);
                lightningOrb.procCoefficient = 0.2f;
                lightningOrb.lightningType = LightningOrb.LightningType.Ukulele;
                lightningOrb.damageColorIndex = DamageColorIndex.Item;
                lightningOrb.range += 2 * itemCount5;
                HurtBox hurtBox = lightningOrb.PickNextTarget(damageInfo.position);
                if (hurtBox)
                {
                    lightningOrb.target = hurtBox;
                    OrbManager.instance.AddOrb(lightningOrb);
                }
            }

            //MeatHook
            int itemCount6 = inventory.GetItemCount(ItemIndex.BounceNearby);
            float num4 = (float)((1.0 - 100.0 / (100.0 + 20.0 * (double)itemCount6)) * 100.0);
            if (itemCount6 > 0 && !damageInfo.procChainMask.HasProc(ProcType.BounceNearby) && Util.CheckRoll(num4 * damageInfo.procCoefficient, master))
            {
                List<HealthComponent> healthComponentList = new List<HealthComponent>()
                    {
                      victim.GetComponent<HealthComponent>()
                    };
                float damageCoefficient = 1f;
                float num3 = Util.OnHitProcDamage(damageInfo.damage, Attacker.damage, damageCoefficient);
                for (int index = 0; index < 5 + itemCount6 * 5; ++index)
                {
                    BounceOrb bounceOrb = new BounceOrb();
                    bounceOrb.origin = damageInfo.position;
                    bounceOrb.damageValue = num3;
                    bounceOrb.isCrit = damageInfo.crit;
                    bounceOrb.teamIndex = attackerTeamIndex;
                    bounceOrb.attacker = damageInfo.attacker;
                    bounceOrb.procChainMask = damageInfo.procChainMask;
                    bounceOrb.procChainMask.AddProc(ProcType.BounceNearby);
                    bounceOrb.procCoefficient = 0.33f;
                    bounceOrb.damageColorIndex = DamageColorIndex.Item;
                    bounceOrb.bouncedObjects = healthComponentList;
                    HurtBox hurtBox = bounceOrb.PickNextTarget(victim.transform.position, 30f);
                    if ((bool)((UnityEngine.Object)hurtBox))
                    {
                        bounceOrb.target = hurtBox;
                        OrbManager.instance.AddOrb((Orb)bounceOrb);
                    }
                }
            }

            //Maracas...I mean Sticky bomb !
            int itemCount7 = inventory.GetItemCount(ItemIndex.StickyBomb);
            if (itemCount7 > 0 && Util.CheckRoll((float)(2.5 + 2.5 * (double)itemCount7) * damageInfo.procCoefficient, master) && (bool)((UnityEngine.Object)characterBody))
            {
                Vector3 position = damageInfo.position;
                Vector3 forward = characterBody.corePosition - position;
                Quaternion rotation = (double)forward.magnitude != 0.0 ? Util.QuaternionSafeLookRotation(forward) : UnityEngine.Random.rotationUniform;
                float damageCoefficient = (float)(1.25 + 1.25 * (double)itemCount7);
                float damage = Util.OnHitProcDamage(damageInfo.damage, Attacker.damage, damageCoefficient);
                ProjectileManager.instance.FireProjectile(Resources.Load<GameObject>("Prefabs/Projectiles/StickyBomb"), position, rotation, damageInfo.attacker, damage, 100f, damageInfo.crit, DamageColorIndex.Item, (GameObject)null, forward.magnitude * 60f);
            }

            //Ice and FireRight
            int itemCount8 = inventory.GetItemCount(ItemIndex.IceRing);
            int itemCount9 = inventory.GetItemCount(ItemIndex.FireRing);
            if ((itemCount8 | itemCount9) <= 0)
                return;
            Vector3 position1 = damageInfo.position;
            if (!Util.CheckRoll(8f * damageInfo.procCoefficient, master))
                return;
            ProcChainMask procChainMask1 = damageInfo.procChainMask;
            procChainMask1.AddProc(ProcType.Rings);
            if (itemCount8 > 0)
            {
                float damageCoefficient = (float)(1.25 + 1.25 * (double)itemCount8);
                float num3 = Util.OnHitProcDamage(damageInfo.damage, Attacker.damage, damageCoefficient);
                DamageInfo damageInfo1 = new DamageInfo()
                {
                    damage = num3,
                    damageColorIndex = DamageColorIndex.Item,
                    damageType = RoR2.DamageType.Generic,
                    attacker = damageInfo.attacker,
                    crit = damageInfo.crit,
                    force = Vector3.zero,
                    inflictor = (GameObject)null,
                    position = position1,
                    procChainMask = procChainMask1,
                    procCoefficient = 1f
                };
                EffectManager.instance.SimpleImpactEffect(Resources.Load<GameObject>("Prefabs/Effects/ImpactEffects/IceRingExplosion"), position1, Vector3.up, true);
                characterBody.AddTimedBuff(BuffIndex.Slow80, 3f);
                victim.GetComponent<HealthComponent>()?.TakeDamage(damageInfo1);
            }
            if (itemCount9 <= 0)
                return;

            //Projectile Parametters
            GameObject gameObject = Resources.Load<GameObject>("Prefabs/Projectiles/FireTornado");
            float resetInterval = gameObject.GetComponent<ProjectileOverlapAttack>().resetInterval;
            float lifetime = gameObject.GetComponent<ProjectileSimple>().lifetime;
            float damageCoefficient1 = (float)(2.5 + 2.5 * (double)itemCount9);
            float DamageProjectile = Util.OnHitProcDamage(damageInfo.damage, Attacker.damage, damageCoefficient1) / lifetime * resetInterval;
            float ProjectileSpeed = 0.0f;
            Quaternion quaternion = Quaternion.identity;
            Vector3 forward1 = position1 - aimOrigin;
            forward1.y = 0.0f;
            if (forward1 != Vector3.zero)
            {
                ProjectileSpeed = -1f;
                quaternion = Util.QuaternionSafeLookRotation(forward1, Vector3.up);
            }

            //LaunchAttack
            ProjectileManager.instance.FireProjectile(new FireProjectileInfo()
            {
                damage = DamageProjectile,
                crit = damageInfo.crit,
                damageColorIndex = DamageColorIndex.Item,
                position = position1,
                procChainMask = procChainMask1,
                force = 0.0f,
                owner = damageInfo.attacker,
                projectilePrefab = gameObject,
                rotation = quaternion,
                speedOverride = ProjectileSpeed,
                target = null
            });
        }

        static void ProcMissile(int stack,CharacterBody attackerBody,CharacterMaster attackerMaster,TeamIndex attackerTeamIndex,ProcChainMask procChainMask,GameObject victim,DamageInfo damageInfo)
        {
            if (stack <= 0)
                return;
            GameObject gameObject1 = attackerBody.gameObject;
            InputBankTest component1 = gameObject1.GetComponent<InputBankTest>();
            Vector3 position = (bool)((UnityEngine.Object)component1) ? component1.aimOrigin : GlobalEventManager.instance.transform.position;
            Vector3 vector3 = (bool)((UnityEngine.Object)component1) ? component1.aimDirection : GlobalEventManager.instance.transform.forward;
            Vector3 up = Vector3.up;
            if (!Util.CheckRoll(10f * damageInfo.procCoefficient, attackerMaster))
                return;
            GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(GlobalEventManager.instance.missilePrefab, position, Util.QuaternionSafeLookRotation(up + UnityEngine.Random.insideUnitSphere * 0.0f));
            ProjectileController component2 = gameObject2.GetComponent<ProjectileController>();
            component2.Networkowner = gameObject1.gameObject;
            component2.procChainMask = procChainMask;
            component2.procChainMask.AddProc(ProcType.Missile);
            gameObject2.GetComponent<TeamFilter>().teamIndex = attackerTeamIndex;
            gameObject2.GetComponent<MissileController>().target = victim.transform;
            float damageCoefficient = 3f * (float)stack;
            float num = Util.OnHitProcDamage(damageInfo.damage, attackerBody.damage, damageCoefficient);
            ProjectileDamage component3 = gameObject2.GetComponent<ProjectileDamage>();
            component3.damage = num;
            component3.crit = damageInfo.crit;
            component3.force = 200f;
            component3.damageColorIndex = DamageColorIndex.Item;
            NetworkServer.Spawn(gameObject2);
        }

        static public void Init()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += ModdedHit;
        }
    }
}


