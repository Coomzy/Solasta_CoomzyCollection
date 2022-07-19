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
    public static class Solasta
    {
        public static bool IsSurroundedByEnemies(GameLocationCharacter targetCharacter, GameLocationCharacter a, GameLocationCharacter b)
        {
            return AreEnemies(targetCharacter, a) && AreEnemies(targetCharacter, b);
        }

        public static bool AreEnemies(GameLocationCharacter a, GameLocationCharacter b)
        {
            // If the target is an enemy
            if (a.Side == Side.Enemy)
            {
                if (b.Side == RuleDefinitions.Side.Enemy)
                {
                    return false;
                }
            }
            else
            {
                if (b.Side != RuleDefinitions.Side.Enemy)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
