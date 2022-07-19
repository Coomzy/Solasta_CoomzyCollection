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
    [HarmonyPatch(typeof(GameLocationCharacter), "GetMoveDuration")]
    class Patch_Loading_MoveQuicker
    {
        static void Postfix(ref float __result, ref GameLocationCharacter __instance)
        {
            if (!Main.settings.enableCharacterMoveSpeed)
            {
                return;
            }

            //Logger.Log($"GameLocationCharacter::GetMoveDuration() {__instance.Name} result: {__result}");

            IGameLocationBattleService service = ServiceRepository.GetService<IGameLocationBattleService>();
            bool inBattle = (service == null || !service.IsBattleInProgress);

            float moveDurationMultiplier = Main.settings.outOfCombatSpeedMultiplier;
            if (inBattle)
            {
                moveDurationMultiplier = Main.settings.inCombatSpeedMultiplier;
            }


            moveDurationMultiplier = 1.0f / moveDurationMultiplier;
            __result *= moveDurationMultiplier;
        }
    }
}


