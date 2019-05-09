using BepInEx;
using UnityEngine;
using RoR2;
using System.Reflection;
using System;
using UnityEngine.Networking;

namespace PlexusUtils
{
    [BepInPlugin(SurvivorStatsAPI.Dependency, "ModRecalculate", SurvivorStatsAPI.Version)]

    public class PlexusUtils : BaseUnityPlugin
    {
        public void Awake()
        {
            SurvivorStatsAPI.Init();

        }
    }
}
