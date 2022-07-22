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
using static CoomzysCollection.Solasta;

namespace CoomzysCollection
{
    [HarmonyPatch(typeof(RulesetCharacter), "ComputeAttackModifier")]
    class Patch_Flanking
    {
        static void Postfix(ref RulesetCharacter __instance, RulesetCharacter defender, RulesetAttackMode attackMode, ActionModifier attackModifier, bool isWithin5Feet, bool isAllyWithin5Feet, bool rangedAttack, int defenderSustainedAttacks, bool defenderAlreadyAttackedByAttackerThisTurn)
        {
            if (!Main.settings.enableFlanking)
            {
                return;
            }

            if (defender == null)
            {
                return;
            }

            //string instigatorName = "UKNOWN";
            //string instigatorName = __instance != null ? __instance.Name : "UKNOWN";

            if (!(defender.EntityImplementation is GameLocationCharacter))
            {
                //Logger.Log($"RulesetCharacter::ComputeAttackModifier() instigator: {instigatorName} defender: {defender.Name} WRONG TYPE: {defender.EntityImplementation?.GetType()}");
                return;
            }

            //instigatorName = __instance != null ? __instance.Name : "UKNOWN";
            //Logger.Log($"RulesetCharacter::ComputeAttackModifier() instigator: {instigatorName} defender: {defender.Name}");

            var target = defender.EntityImplementation as GameLocationCharacter;
            bool isFlanked = false;

            if (Main.settings.enableFlankingGroup)
            {
                isFlanked = Flanking.IsFlanked(target);
            }
            else
            {
                GameLocationCharacter flanker = null;
                if (__instance?.EntityImplementation is GameLocationCharacter)
                {
                    flanker = __instance?.EntityImplementation as GameLocationCharacter;
                }
                isFlanked = Flanking.IsFlankedBy(target, flanker);
            }

            if (!isFlanked)
            {
                return;
            }

            if (Main.settings.enableFlankingPlus2)
            {
                attackModifier.AttackRollModifier += 2;
                attackModifier.AttacktoHitTrends.Add(new RuleDefinitions.TrendInfo(2, FeatureSourceType.EffectProxy, "Flanking", null));
            }
            else
            {
                attackModifier.AttackAdvantageTrends.Add(new RuleDefinitions.TrendInfo(1, FeatureSourceType.EffectProxy, "Flanking", null));
            }
        }
    }

    public struct FlankLine
    {
        public GameLocationCharacter target;
        public int3 startPoint;
        public int3 endPoint;
    }

    public static class Flanking
    {
        static GameObject debugFlankingGO = null;

        [InitializeAttribute]
        public static void Init()
        {
            if (!Main.settings.debugFlanking)
            {
                return;
            }
            debugFlankingGO = new GameObject("DebugFlanking", typeof(DebugFlanking));
        }

        [DeinitializeAttribute]
        public static void Deinit()
        {
            if (debugFlankingGO != null)
            {
                GameObject.Destroy(debugFlankingGO);
            }
        }

        public static bool IsFlanked(GameLocationCharacter targetCharacter)
        {
            if (targetCharacter == null)
            {
                return false;
            }

            var flankLines = GetFlankLines(targetCharacter);

            for (int i = 0; i < flankLines.Length; i++)
            {
                if (!LocationHasFlanker(targetCharacter, flankLines[i].startPoint))
                {
                    continue;
                }

                if (!LocationHasFlanker(targetCharacter, flankLines[i].endPoint))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        public static bool IsFlankedBy(GameLocationCharacter targetCharacter, GameLocationCharacter flankerCharacter)
        {
            if (targetCharacter == null || flankerCharacter == null)
            {
                return false;
            }

            if (!MeetsFlankingConditions(targetCharacter, flankerCharacter))
            {
                return false;
            }

            var flankLines = GetFlankLines(targetCharacter);

            for (int i = 0; i < flankLines.Length; i++)
            {
                if (flankLines[i].startPoint == flankerCharacter.LocationPosition)
                {
                    return LocationHasFlanker(targetCharacter, flankLines[i].endPoint);
                }
                else if (flankLines[i].endPoint == flankerCharacter.LocationPosition)
                {
                    return LocationHasFlanker(targetCharacter, flankLines[i].startPoint);
                }
            }

            return false;
        }

        public static bool LocationHasFlanker(GameLocationCharacter targetCharacter, int3 location)
        {
            GridAccessor gridAccessor = new GridAccessor(targetCharacter.LocationPosition);
            List<GameLocationCharacter> list = null;
            gridAccessor.Occupants_TryGet(location, out list);
            if (list == null || list.Count < 1)
            {
                return false;
            }

            for (int i = 0; i < list.Count; i++)
            {
                if (MeetsFlankingConditions(targetCharacter, list[i]))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool MeetsFlankingConditions(GameLocationCharacter targetCharacter, GameLocationCharacter gameLocationCharacter)
        {
            if (targetCharacter == null || gameLocationCharacter?.RulesetCharacter == null)
                return false;

            if (!AreEnemies(targetCharacter, gameLocationCharacter))
                return false;

            if (gameLocationCharacter.RulesetCharacter.IsDeadOrDyingOrUnconscious)
                return false;

            if (gameLocationCharacter.RulesetCharacter.HasConditionOfType("ConditionIncapacitated"))
                return false;

            if (!Main.settings.enableFlankingForRangedAttacks)
            {
                bool hasMeleeWeapon = true;
                RulesetItem equipedItem = gameLocationCharacter.RulesetCharacter.CharacterInventory.InventorySlotsByName[EquipmentDefinitions.SlotTypeMainHand].EquipedItem;
                if (equipedItem != null && equipedItem.ItemDefinition.IsWeapon)
                {
                    hasMeleeWeapon = (DatabaseRepository.GetDatabase<WeaponTypeDefinition>().GetElement(equipedItem.ItemDefinition.WeaponDescription.WeaponType, false).WeaponProximity == RuleDefinitions.AttackProximity.Melee);
                }

                if (!hasMeleeWeapon)
                {
                    RulesetItem equipedItem2 = gameLocationCharacter.RulesetCharacter.CharacterInventory.InventorySlotsByName[EquipmentDefinitions.SlotTypeOffHand].EquipedItem;
                    if (equipedItem2 != null && equipedItem2.ItemDefinition.IsWeapon)
                    {
                        hasMeleeWeapon = (DatabaseRepository.GetDatabase<WeaponTypeDefinition>().GetElement(equipedItem2.ItemDefinition.WeaponDescription.WeaponType, false).WeaponProximity == RuleDefinitions.AttackProximity.Melee);
                    }
                }

                if (!hasMeleeWeapon)
                    return false;
            }

            return true;
        }

        public static FlankLine[] GetFlankLines(GameLocationCharacter targetCharacter)
        {
            if (targetCharacter == null)
            {
                return new FlankLine[0];
            }

            //var behindPos = targetCharacter.LocationPosition + new int3(-1, -targetCharacter.SizeParameters.maxExtent.y, -1);
            var behindPos = targetCharacter.LocationPosition + new int3(-1, 0, -1);
            int size = 1 + targetCharacter.SizeParameters.maxExtent.x;
            int3[] flankingChecks = new int3[2 + (size * 2)];

            flankingChecks[0] = behindPos;
            flankingChecks[flankingChecks.Length - 1] = behindPos + new int3(1 * (size + 1), 0, 0);
            var flankLines = new FlankLine[flankingChecks.Length];

            for (int i = 0; i < size; i++)
            {
                var checkingPosX = behindPos + new int3(i + 1, 0, 0);
                var checkingPosZ = behindPos + new int3(0, 0, i + 1);

                flankingChecks[i + 1] = checkingPosX;
                flankingChecks[i + 1 + size] = checkingPosZ;
            }

            for (int i = 0; i < flankingChecks.Length; i++)
            {
                var horizontalExtent = targetCharacter.SizeParameters.maxExtent;
                horizontalExtent.y = 0;
                var flankingPos = targetCharacter.LocationPosition + horizontalExtent + (targetCharacter.LocationPosition - flankingChecks[i]);

                flankLines[i].startPoint = flankingChecks[i];
                flankLines[i].endPoint = flankingPos;
            }

            return flankLines;
        }
    }

    public class DebugFlanking : MonoBehaviour
    {
        GameLocationCharacter playingCharacter;
        GameLocationCharacter currentTargetCharacter;
        public bool isFlanked = false;
        GameObject debugCube = null;
        GameObject debugCylinder = null;
        GameObject debugSphere = null;

        void Awake()
        {
            DontDestroyOnLoad(gameObject);

            debugCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            debugCylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            debugSphere.transform.localScale = Vector3.one * 1.0f;

            DontDestroyOnLoad(debugCube);
            DontDestroyOnLoad(debugCylinder);
            DontDestroyOnLoad(debugSphere);
        }

        void OnDestroy()
        {
            Destroy(debugCube);
            Destroy(debugCylinder);
            Destroy(debugSphere);
        }

        void Update()
        {
            IGameLocationPositioningService locationPosService = ServiceRepository.GetService<IGameLocationPositioningService>();
            IGameLocationSelectionService locationSelectionService = ServiceRepository.GetService<IGameLocationSelectionService>();

            if (locationPosService == null || locationSelectionService == null)
            {
                return;
            }

            if (locationSelectionService != null)
            {
                if (locationSelectionService.HoveredCharacters.Count > 0)
                {
                    currentTargetCharacter = locationSelectionService.HoveredCharacters[0];
                }
                else
                {
                    currentTargetCharacter = null;
                }

                if (locationSelectionService.SelectedCharacters.Count > 0)
                {
                    playingCharacter = locationSelectionService.SelectedCharacters[0];
                }
                else
                {
                    playingCharacter = null;
                }
            }
            else
            {
                playingCharacter = null;
                currentTargetCharacter = null;
            }
            if (playingCharacter != null)
            {
                //debugSphere.transform.position = locationPosService.GetWorldPositionFromGridPosition(playingCharacter.LocationPosition);
            }
            if (currentTargetCharacter != null)
            {
                //debugCube.transform.position = locationPosService.GetWorldPositionFromGridPosition(currentTargetCharacter.LocationPosition);
                //debugCube.transform.localScale = (Vector3)currentTargetCharacter.SizeParameters.maxExtent;

                //var behindPos = currentTargetCharacter.LocationPosition + new int3(-1, 0, -1);
                //debugSphere.transform.position = locationPosService.GetWorldPositionFromGridPosition(behindPos);
            }

            //var flankingPos = currentTargetCharacter.LocationPosition + currentTargetCharacter.SizeParameters.maxExtent + (currentTargetCharacter.LocationPosition - playingCharacter.LocationPosition);
            //GridAccessor gridAccessor = new GridAccessor(flankingPos);
            //List<GameLocationCharacter> list;
            //bool flag = gridAccessor.Occupants_TryGet(flankingPos, out list);

            //debugCylinder.transform.position = locationPosService.GetWorldPositionFromGridPosition(flankingPos);

            isFlanked = Flanking.IsFlanked(currentTargetCharacter);
            var flankLines = Flanking.GetFlankLines(currentTargetCharacter);
            DebugFlankLines(currentTargetCharacter, flankLines);
            DebugDraw();
        }

        void OnGUI()
        {
            GUILayout.Space(250);
            var style = new GUIStyle();
            style.fontSize = 50;
            style.normal.textColor = Color.gray;

            GUILayout.Label("", style);
            GUILayout.Label("", style);
            GUILayout.Label("", style);
            if (playingCharacter != null)
            {
                if (playingCharacter.RulesetActor != null)
                {
                    GUILayout.Label($"playingCharacter = {playingCharacter.Name} pos: {playingCharacter.LocationPosition.x},{playingCharacter.LocationPosition.y},{playingCharacter.LocationPosition.z} size min: {playingCharacter.BattleSizeParameters.minExtent.x}, {playingCharacter.BattleSizeParameters.minExtent.y}, {playingCharacter.BattleSizeParameters.minExtent.z} size max: {playingCharacter.BattleSizeParameters.maxExtent.x}, {playingCharacter.BattleSizeParameters.maxExtent.y}, {playingCharacter.BattleSizeParameters.maxExtent.z}", style);
                }
                else
                {
                    GUILayout.Label($"playingCharacter = {playingCharacter.Name} pos: {playingCharacter.LocationPosition.x},{playingCharacter.LocationPosition.y},{playingCharacter.LocationPosition.z} size unknown", style);
                }
            }
            else
            {
                GUILayout.Label($"playingCharacter = none", style);
            }
            if (currentTargetCharacter != null)
            {
                if (currentTargetCharacter.RulesetActor != null)
                {
                    GUILayout.Label($"currentTargetCharacter = {currentTargetCharacter.Name} pos: {currentTargetCharacter.LocationPosition.x},{currentTargetCharacter.LocationPosition.y},{currentTargetCharacter.LocationPosition.z} size min: {currentTargetCharacter.BattleSizeParameters.minExtent.x}, {currentTargetCharacter.BattleSizeParameters.minExtent.y}, {currentTargetCharacter.BattleSizeParameters.minExtent.z} size max: {currentTargetCharacter.BattleSizeParameters.maxExtent.x}, {currentTargetCharacter.BattleSizeParameters.maxExtent.y}, {currentTargetCharacter.BattleSizeParameters.maxExtent.z}", style);
                }
                else
                {
                    GUILayout.Label($"currentTargetCharacter = {currentTargetCharacter.Name} pos: {currentTargetCharacter.LocationPosition.x},{currentTargetCharacter.LocationPosition.y},{currentTargetCharacter.LocationPosition.z} size unknown", style);
                }
            }
            else
            {
                GUILayout.Label($"currentTargetCharacter = none", style);
            }
            GUILayout.Label($"IsFlanked {isFlanked}", style);

            for (int i = 0; i < lastUsedPoints.Count; i++)
            {
                var startCharacterDisplay = $"None";
                if (lastUsedPoints[i].startCharacter != null)
                {
                    if (lastUsedPoints[i].startCharacter.RulesetActor != null)
                    {
                        startCharacterDisplay = $"{lastUsedPoints[i].startCharacter.Name} {lastUsedPoints[i].startCharacter.Side}";
                    }
                    else
                    {

                        startCharacterDisplay = $"{lastUsedPoints[i].startCharacter.Name} UNKNOWN";
                    }
                }
                var endCharacterDisplay = $"None";
                if (lastUsedPoints[i].endCharacter != null)
                {
                    if (lastUsedPoints[i].endCharacter.RulesetActor != null)
                    {
                        endCharacterDisplay = $"{lastUsedPoints[i].endCharacter.Name} {lastUsedPoints[i].endCharacter.Side}";
                    }
                    else
                    {
                        endCharacterDisplay = $"{lastUsedPoints[i].endCharacter.Name} UNKNOWN";
                    }
                }
                //GUILayout.Label($"lastUsedPoints[{i}] start:{lastUsedPoints[i].startPoint} end:{lastUsedPoints[i].endPoint}", style);
                GUILayout.Label($"FP[{i}] start:{startCharacterDisplay} end:{endCharacterDisplay}", style);
            }
        }

        public void DebugFlankLines(GameLocationCharacter targetCharacter, FlankLine[] flankLines)
        {
            lastUsedPoints.Clear();
            if (targetCharacter == null)
            {
                return;
            }

            IGameLocationPositioningService locationPosService = ServiceRepository.GetService<IGameLocationPositioningService>();
            if (locationPosService == null)
            {
                return;
            }

            List<GameLocationCharacter> startList;
            List<GameLocationCharacter> endList;
            for (int i = 0; i < flankLines.Length; i++)
            {
                PointData pd = new PointData();

                GridAccessor gridAccessor = new GridAccessor(targetCharacter.LocationPosition);
                //GridAccessor gridAccessor = new GridAccessor(flankLines[i].startPoint);
                gridAccessor.Occupants_TryGet(flankLines[i].startPoint, out startList);
                //gridAccessor = new GridAccessor(flankLines[i].endPoint);
                gridAccessor.Occupants_TryGet(flankLines[i].endPoint, out endList);
                var endPoint = flankLines[i].endPoint + new int3(0, -1, 0);
                //gridAccessor.Occupants_TryGet(endPoint, out endList);

                pd.startCharacter = (startList != null && startList.Count > 0) ? startList[0] : null;
                pd.endCharacter = (endList != null && endList.Count > 0) ? endList[0] : null;

                pd.startPoint = locationPosService.GetWorldPositionFromGridPosition(flankLines[i].startPoint);
                pd.endPoint = locationPosService.GetWorldPositionFromGridPosition(flankLines[i].endPoint);

                lastUsedPoints.Add(pd);
            }
        }

        struct PointData
        {
            public Vector3 startPoint;
            public Vector3 endPoint;
            public GameLocationCharacter startCharacter;
            public GameLocationCharacter endCharacter;
        }

        List<PointData> lastUsedPoints = new List<PointData>();

        void DebugDraw()
        {
            for (int i = 0; i < lastUsedPoints.Count; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                go.transform.position = lastUsedPoints[i].startPoint;
                go.GetComponent<MeshRenderer>().material.color = CharacterToColor(lastUsedPoints[i].startCharacter);

                var goEnd = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                goEnd.transform.position = lastUsedPoints[i].endPoint;
                goEnd.GetComponent<MeshRenderer>().material.color = CharacterToColor(lastUsedPoints[i].endCharacter);

                var goMid = GameObject.CreatePrimitive(PrimitiveType.Cube);
                goMid.transform.position = Vector3.Lerp(lastUsedPoints[i].startPoint, lastUsedPoints[i].endPoint, 0.5f);
                goMid.transform.LookAt(lastUsedPoints[i].endPoint);
                goMid.transform.localScale = Vector3.one * 0.1f + (Vector3.forward * (Vector3.Distance(lastUsedPoints[i].startPoint, lastUsedPoints[i].endPoint)));

                go.AddComponent<DestroyAfterOneFrame>();
                goEnd.AddComponent<DestroyAfterOneFrame>();
                goMid.AddComponent<DestroyAfterOneFrame>();

                if (lastUsedPoints[i].startCharacter == null || lastUsedPoints[i].endCharacter == null)
                {
                    Destroy(goMid);
                }
            }
        }

        Color CharacterToColor(GameLocationCharacter characterInGrid)
        {
            if (currentTargetCharacter == null)
            {
                return Color.yellow;
            }
            if (characterInGrid != null)
            {
                return AreEnemies(currentTargetCharacter, characterInGrid) ? Color.green : Color.red;
            }
            return Color.white;
        }
    }
}


