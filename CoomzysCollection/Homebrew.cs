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
    [HarmonyPatch(typeof(GameLocationCharacter), "CanOnlyUseCantrips", MethodType.Getter)]
    class Patch_Loading_RemoveSpellPerTurn
    {
        static void Postfix(ref bool __result, ref GameLocationCharacter __instance)
        {
            if (!Main.settings.enableRemoveOneSpellPerTurnRestriction)
            {
                return;
            }

            //Logger.Log($"GameLocationCharacter::CanOnlyUseCantrips() {__instance.Name} result: {__result}");

            __result = false;
        }
    }

    [HarmonyPatch(typeof(RulesetActor), "RollDamage")]
    class Patch_Crits_Brutal
    {
        static void Postfix(ref int __result, DamageForm damageForm, int addDice, bool criticalSuccess, int additionalDamage, int damageRollReduction, float damageMultiplier, bool useVersatileDamage, bool attackModeDamage, List<int> rolledValues = null, bool canRerollDice = true)
        {
            if (!Main.settings.enableAlternateCritRule)
            {
                return;
            }

            if (!criticalSuccess)
            {
                return;
            }

            Logger.Log($"RulesetActor::RollDamage() result: {__result}");

            if (rolledValues != null)
            {
                int totalDamage = 0;
                for (int i = 0; i < rolledValues.Count; i++)
                {
                    if (i < (rolledValues.Count / 2))
                    {
                        rolledValues[i] = RuleDefinitions.DiceMaxValue[(int)damageForm.DieType];
                    }
                    totalDamage += rolledValues[i];
                }
                __result = totalDamage;
            }

            //__result = false;
        }
    }

    [HarmonyPatch(typeof(GameLocationCharacter), "GetActionTypeStatus")]
    class Patch_Unlimited_Interactions
    {
        static void Postfix(ref ActionDefinitions.ActionStatus __result, ref GameLocationCharacter __instance, ActionDefinitions.ActionType actionType, ActionDefinitions.ActionScope actionScope = ActionDefinitions.ActionScope.Battle, bool ignoreMovePoints = false)
        {
            if (!Main.settings.enableUnlimitedInteractions)
            {
                return;
            }

            //Logger.Log($"GameLocationCharacter::CanOnlyUseCantrips() {__instance.Name} result: {__result}");

            if (actionType == ActionDefinitions.ActionType.FreeOnce)
            {
                __result = ActionDefinitions.ActionStatus.Available;
            }
        }
    }
}


