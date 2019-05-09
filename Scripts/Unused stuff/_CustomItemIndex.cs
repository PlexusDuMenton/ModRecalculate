using BepInEx;
using UnityEngine;
using RoR2;
using System.Reflection;
using System.Linq;
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

    /*
     * No longer needed thank to iDeathHD
     * 
     * 
    class CustomItemManager
    {

        public static int ItemCount = Enum.GetValues(typeof(ProcType)).Cast<int>().Max();



        static public Dictionary<string, ItemIndex> IndexList = new Dictionary<string, ItemIndex>();

        /// <summary>
        /// Used to decalre new ItemIndex in addition to existing one
        /// </summary>
        /// <param name="ProcName"></param>
        public static void DeclareNewItem(string ItemName)
        {
            ItemCount++;
            IndexList.Add(ItemName,(ItemIndex)ItemCount);
        }
        static void InventoryConstructorHook(On.RoR2.Inventory.orig_ctor orig, Inventory self)
        {
            orig(self);
            self.SetFieldValue("itemStacks", new int[ItemCount]);

        }

        public static void Init()
        {
            On.RoR2.Inventory.ctor += InventoryConstructorHook;
        }

    }
    */

}
