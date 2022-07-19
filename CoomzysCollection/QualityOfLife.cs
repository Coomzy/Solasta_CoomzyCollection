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
    [HarmonyPatch(typeof(GameLocationCharacter), "WaitForHitAnimation")]
    class Patch_Loading_WaitForHitAnim : IEnumerable
    {
        public float WAIT_FOR_ANIM_TIME = 2.5f;
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(null); }

        static void Postfix(ref IEnumerator __result, ref GameLocationCharacter __instance)
        {
            if (!Main.settings.enableWaitForAnim)
            {
                return;
            }

            //Logger.Log($"GameLocationCharacter::WaitForHitAnimation() {__instance?.RulesetCharacter.Name}");

            __result = GetEnumerator(__instance);
        }

        public static IEnumerator GetEnumerator(GameLocationCharacter instance)
        {
            float duration = 0f;
            while (instance != null && instance.IsReceivingDamage && duration < Main.settings.waitForAnimTime)
            {
                duration += Time.deltaTime;
                yield return null;
            }
            if (instance != null && instance.IsReceivingDamage)
            {
                //Logger.Log("WaitForHitAnimation() aborted. Waiting too long on character " + instance.RulesetCharacter.Name + ".");
                instance.CurrentDamageCount = 0;
            }
            yield break;
        }
    }

    [HarmonyPatch(typeof(GameLocationCharacter), "WaitForDeathAnimation")]
    class Patch_Loading_WaitForDeathAnim : IEnumerable
    {
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(null); }

        static void Postfix(ref IEnumerator __result, ref GameLocationCharacter __instance)
        {
            if (!Main.settings.enableWaitForAnim)
            {
                return;
            }

            //Logger.Log($"GameLocationCharacter::WaitForDeathAnimation() {__instance?.RulesetCharacter.Name}");

            __result = GetEnumerator(__instance);
        }

        public static IEnumerator GetEnumerator(GameLocationCharacter instance)
        {
            float duration = 0f;
            while (instance != null && instance.IsDying && duration < Main.settings.waitForAnimTime)
            {
                duration += Time.deltaTime;
                yield return null;
            }
            if (instance != null && instance.IsDying)
            {
                //Logger.Log("WaitForDeathAnimation() aborted. Waiting too long on character " + instance.RulesetCharacter.Name + ".");
                instance.CurrentDamageCount = 0;
            }
            yield break;
        }
    }
}