# ModRecalculate
#### by PlexusDuMenton

Library/API to easily change CharacterBody.Recalculate()
Create a list of hook to add/remove stats, and ItemSpecific hook, even PostRecalculate hook that apply before Health/Shield update (which is a bit issue if you edit maxHealth/MaxShield after)




###Developer Info
	Plugin ID : com.Plexus.ModRecalculate

	CharacterDefault*** : Before Item are apllied
        ***Effect : Item specific hook
        ***Recalculate : Applyed after all item are applied
        PostRecalculate apply hook before the Health and Shield are updated

        Hook are additional,if you want to act multiplicative, do it in PostRecalculate
        Cooldown hook are MULTIPLICATIVE with other mod/BaseValue, if you want to make it Additional/Substractif, do it in PostRecalculate
		
		To modify ItemValue use ModifyItem Hook, Item value are inside ModRecalculate class as public, Don't try to edit the private one ^^
		
		To overide an existing hook (ignoring Original or Other hook) use the ResetHook(string HookName,bool TotalReset) function, please, only use it only when REALY needed, a hook can only be overided Once !
		
		

	LIST (IN ORDER OF CALL) :
	
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
			DamageBoostEffect
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



###Changelog
0.3.0 : 
Added Ability to overide hook, either allowing or blocking other hooks

0.2.0 : 
Added specific hook for DamageBoostEffect (multiplied with Character DefaultDamage
Added Item Specific public value to edit from ModifyItem Hook
Fixed Slug having no effect
Fixed a LogSpamming bug

0.1.0 : Initial Release