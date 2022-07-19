using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TA;
using UnityEngine;
using UnityModManagerNet;
using static RuleDefinitions;
using Logger = UnityModManagerNet.UnityModManager.Logger;

namespace CoomzysCollection
{
    [HarmonyPatch(typeof(GameAchievementsData), nameof(GameAchievementsData.RecordEnemyKill))]
    class Patch_Achievements_DamageTypes
    {
        static Dictionary<string, string> typeToAchievement = new Dictionary<string, string>()
        {
            { "DamagePiercing", "ACH_PIERCEKILL" },
            { "DamageRadiant", "ACH_RADIANTKILL" },
            { "DamageSlashing", "ACH_SLASHKILL" },
            { "DamageThunder", "ACH_THUNDERKILL" },
            { "DamageForce", "ACH_FORCEKILL" },
            { "DamagePsychic", "ACH_PSYCHICKILL" },
            { "DamagePoison", "ACH_POISONKILL" },
            { "DamageFire", "ACH_FIREKILL" },
            { "DamageBludgeoning", "ACH_BLUDGEONKILL" },
            { "DamageNecrotic", "ACH_NECROTICKILL" },
            { "DamageLightning", "ACH_LIGHTNINGKILL" },
            { "DamageAcid", "ACH_ACIDKILL" },
            { "DamageCold", "ACH_ICEKILL" },
        };

        static void Postfix(string damageType)
        {
            if (!Main.settings.enableAchievemntUseItemOnce)
            {
                return;
            }

            //Logger.Log($"GameAchievementsData::RecordEnemyKill() damageType =  {damageType}");

            if (typeToAchievement.TryGetValue(damageType, out string achievmentName))
            {
                GamingPlatform.UnlockAchievement(achievmentName);
            }
        }
    }

    [HarmonyPatch(typeof(GameAchievementsData), nameof(GameAchievementsData.RecordToolKitUse))]
    class Patch_Achievements_ToolTypes
    {
        static Dictionary<string, string> typeToAchievement = new Dictionary<string, string>()
        {
            { "PoisonersKitType", "ACH_POISONKIT" },
            { "HerbalismKitType", "ACH_HERBORISTKIT" },
            { "ScrollKitType", "ACH_SCROLLKIT" },
            { "EnchantingToolType", "ACH_ENCHANTMENTKIT" },
        };

        static void Postfix(ToolTypeDefinition toolType)
        {
            if (!Main.settings.enableAchievemntUseItemOnce)
            {
                return;
            }

            //Logger.Log($"GameAchievementsData::RecordToolKitUse() toolType =  {toolType.Name}");

            if (typeToAchievement.TryGetValue(toolType.Name, out string achievmentName))
            {
                GamingPlatform.UnlockAchievement(achievmentName);
            }
        }
    }

    [HarmonyPatch(typeof(GameAchievementsData), nameof(GameAchievementsData.RecordIdentifiedItem))]
    class Patch_Achievements_Identifed
    {
        static void Postfix()
        {
            if (!Main.settings.enableAchievemntUseItemOnce)
            {
                return;
            }

            //Logger.Log($"GameAchievementsData::RecordIdentifiedItem()");

            GamingPlatform.UnlockAchievement("ACH_IDENTIFY");
        }
    }
}


