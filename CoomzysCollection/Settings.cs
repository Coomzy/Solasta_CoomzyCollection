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
    public class Settings : UnityModManager.ModSettings, IDrawable
    {
        [Header("Homebrew")]
        [Draw("Remove 1 Spell Per Turn Restriction")] public bool enableRemoveOneSpellPerTurnRestriction = false;
        [Draw("Alternate Crit Rule (1 dice set max)")] public bool enableAlternateCritRule = false;
        [Draw("Unlimited Interactions")] public bool enableUnlimitedInteractions = false;

        [Header("Homebrew - Flanking")]
        [Draw("Enabled")] public bool enableFlanking = false;
        [Draw("+2 rather than advantage")] public bool enableFlankingPlus2 = false;
        [Draw("Can get bonus from other flankers")] public bool enableFlankingGroup = false;
        [Draw("Ranged attacks allowed")] public bool enableFlankingForRangedAttacks = false;

        [Header("Auto Confirms")]
        [Draw("Auto Enter Loaded Level")] public bool enableAutoEnterLoadedLevel = false;
        [Draw("Auto Exit Area")] public bool enableAutoExitArea = false;

        [Header("Character Move Speed")]
        [Draw("Enabled")] public bool enableCharacterMoveSpeed = false;
        [Draw("Out of combat multiplier")] public float outOfCombatSpeedMultiplier = 4.0f;
        [Draw("In combat multiplier")] public float inCombatSpeedMultiplier = 5.0f;

        [Header("Skip Dialog")]
        [Draw("Use Harded Coded Mouse Buttons")] public bool useHardedCodedMouseButton_Dialog = false;
        [Draw("Keybind")] public KeyBinding skipDialogKeybind = new KeyBinding() { keyCode = KeyCode.Z };
        public KeyCode testKeybind = KeyCode.Mouse4;

        [Header("Time Speed")]
        [Draw("Use Harded Coded Mouse Buttons")] public bool useHardedCodedMouseButton_TimeSpeed = false;
        [Draw("Keybind")] public KeyBinding speedyTimeKeybind = new KeyBinding() { keyCode = KeyCode.X };
        [Draw("Multiplier")] public float speedyTimeMultiplier = 5.0f;

        [Header("Wait For Hit/Death Anim")]
        [Draw("Enabled")] public bool enableWaitForAnim = true;
        [Draw("New Wait For Time")] public float waitForAnimTime = 2.5f;

        [Header("Achievements")]
        [Draw("Use/Kill X once rather than 20 times")] public bool enableAchievemntUseItemOnce = false;

        [Header("Debug")]
        [Draw("Flanking")] public bool debugFlanking = false;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

        public void OnChange()
        {

        }
    }
}