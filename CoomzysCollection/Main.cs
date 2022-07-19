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
    [EnableReloading]
    public static class Main
    {
        public static Settings settings;
        static GameObject instance = null;

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnUnload = Unload;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            var harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            instance = new GameObject("Coomzy Collection Manager", typeof(CoomzySolastaManager));
            GameObject.DontDestroyOnLoad(instance);

            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                // Test for a static class
                if (type.IsSealed == false) continue;
                if (type.IsClass == false) continue;

                // Check each method for the attribute.
                foreach (var method in type.GetRuntimeMethods())
                {
                    // Make sure the method is static
                    if (method.IsStatic == false) continue;

                    // Test for presence of the attribute
                    var attribute = method.GetCustomAttribute<InitializeAttribute>();

                    if (attribute == null)
                        continue;

                    method.Invoke(null, null);
                }
            }

            return true;
        }

        static bool Unload(UnityModManager.ModEntry modEntry)
        {
            var harmony = new Harmony(modEntry.Info.Id);
            harmony.UnpatchAll();

            GameObject.Destroy(instance);
            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Draw(modEntry);
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        //static void OnGUI(UnityModManager.ModEntry modEntry)
        //{
        //    if (GUILayout.Button("Change Framerate", GUILayout.ExpandWidth(false)))
        //    {
        //        Logger.Log($"OnGUI() Button Log");
        //    }
        //}
    }

    public class InitializeAttribute : Attribute { }
    public class DeinitializeAttribute : Attribute { }

    public class CoomzySolastaManager : MonoBehaviour
    {
        void Update()
        {
            SkipDialougeOnKeybind();
            SpeedTimeOnKeybind();
        }

        void SkipDialougeOnKeybind()
        {
            KeyCode keyCode = KeyCode.Mouse4;
            if (!Main.settings.useHardedCodedMouseButton_Dialog)
            {
                keyCode = Main.settings.speedyTimeKeybind.keyCode;
            }

            if (Input.GetKeyDown(keyCode))
            {
                ServiceRepository.GetService<ICutsceneCommandService>().SkipCurrentSequence();
            }
        }

        void SpeedTimeOnKeybind()
        {
            KeyCode keyCode = KeyCode.Mouse3;
            if (!Main.settings.useHardedCodedMouseButton_TimeSpeed)
            {
                keyCode = Main.settings.speedyTimeKeybind.keyCode;
            }

            if (Input.GetKey(keyCode))
            {
                Time.timeScale = Main.settings.speedyTimeMultiplier;
            }
            else if (Input.GetKeyUp(keyCode))
            {
                Time.timeScale = 1.0f;
            }
        }
    }

    public class DestroyAfterOneFrame : MonoBehaviour
    {
        IEnumerator Start()
        {
            yield return null;
            Destroy(gameObject);
        }
    }
}


