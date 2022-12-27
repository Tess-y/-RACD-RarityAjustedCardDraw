using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RarityLib.Utils;

namespace _RACD_RarityAjustedCardDraw{
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.moddingutils", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.cardchoicespawnuniquecardpatch", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(ModId, ModName, Version)]
    [BepInProcess("Rounds.exe")]
    public class Main : BaseUnityPlugin{
        private const string ModId = "Root.Rarity.Dard.Draw";
        private const string ModName = "RarityAjustedCardDraw";
        public const string Version = "1.0.0";
        public static ConfigEntry<bool> DEBUG;
        public static Dictionary<string, CardInfo> Cards = new Dictionary<string, CardInfo>();
        public static int CardCount;
        public static Main instance { get; private set; }

        void Awake(){ 
            var harmony = new Harmony(ModId);
            harmony.PatchAll();

            DEBUG = base.Config.Bind<bool>("RACD", "Debug", false, "Enable to turn on debug messages from this mod");
        }
        void Start(){
            instance = this;
        }

        //Use calls to this method when debugging things (Prevents log spam to users who arnt debugging)
        public static void Debug(object message){
            if (DEBUG.Value){
                UnityEngine.Debug.Log($"{ModName}=> {message}");
            }
        }
    }

    [Serializable]
    [HarmonyPatch(typeof(ModdingUtils.Patches.CardChoicePatchGetRanomCard), "OrignialGetRanomCard", new Type[] { typeof(CardInfo[]) })]
    [HarmonyPriority(int.MinValue)]
    public class Patch{
        public static bool running = false;
        public static bool isEnabled = true;
        public static void Postfix(ref GameObject __result, CardInfo[] cards){
            if (!isEnabled) return;
            Main.Debug($"Called with card: ${__result}");
            if (running) { 
                running = false;
                Main.Debug("Patch is running");
                return; 
            }
            running = true;
            float r = 0f;
            List<CardInfo.Rarity> rarities = cards.Select(c => c.rarity).Distinct().ToList();
            CardInfo.Rarity rarity = CardInfo.Rarity.Common;
            for (int i = 0; i < rarities.Count; i++){
                r += RarityUtils.GetRarityData(rarities[i]).calculatedRarity;
            }
            float cr = UnityEngine.Random.Range(0, r);
            for (int i = 0; i < rarities.Count; i++){
                cr -= RarityUtils.GetRarityData(rarities[i]).calculatedRarity;
                if(cr <= 0f){
                    rarity = rarities[i];
                    break;
                }
            }
            Main.Debug($"Genned rarity is ${rarity}");
            __result = ModdingUtils.Patches.CardChoicePatchGetRanomCard.OrignialGetRanomCard(cards.Where(c=> c.rarity == rarity).ToArray());
            Main.Debug($"New Genned rarity is ${__result}");
        }
    }
}
