
namespace PlexusUtils
{
    /*
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
       */
}
