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
    [HarmonyPatch(typeof(LoadingModal), "LocationPostLoaded")]
    class Patch_Loading_AutoLoad
    {
        static void Postfix(ref LoadingModal __instance)
        {
            if (!Main.settings.enableAutoEnterLoadedLevel)
            {
                return;
            }

            //Logger.Log($"LoadingModal::LocationPostLoaded() {__instance?.gameObject?.name}");

            if (__instance != null)
            {
                __instance.OnProceedToLocationCb();
            }
        }
    }

    [HarmonyPatch(typeof(FunctorQuitLocation), "Execute")]
    class Patch_Loading_AutoConfirm
    {
        static void Prefix(ref FunctorQuitLocation __instance, ref FunctorParametersDescription functorParameters, ref Functor.FunctorExecutionContext context)
        {
            if (!Main.settings.enableAutoExitArea)
            {
                return;
            }

            //Logger.Log($"FunctorQuitLocation::Execute()");

            functorParameters.BoolParameter = true;
        }
    }
}


