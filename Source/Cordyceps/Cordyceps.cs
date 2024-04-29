using System;
using BepInEx;
using UnityEngine;

namespace Cordyceps
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Cordyceps : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "Cordyceps";
        public const string PLUGIN_NAME = "Cordyceps TAS";
        public const string PLUGIN_VERSION = "0.1.0";

        public static int UnmodifiedTickrate = 40;
        public static bool TestTickrateModifier = false;

        private static bool initialized = false;
        private static bool keyReleased = true;
        
        private void OnEnable()
        {
            On.RainWorld.OnModsInit += RainWorld_OnModsInit_Hook;
        }

        private void RainWorld_OnModsInit_Hook(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            if (initialized) return;

            try
            {
                Log("Initializing");
                
                Log("Registering hooks");
                On.MainLoopProcess.RawUpdate += MainLoopProcess_RawUpdate_Hook;

                Log("Registering settings");
                MachineConnector.SetRegisteredOI("Cordyceps", new CordycepsSettings());
                
                initialized = true;
                Log("Initialized successfully");
            }
            catch (Exception e)
            {
                Log($"ERROR - Uncaught exception during initialization: {e}");
            }
        }

        private void MainLoopProcess_RawUpdate_Hook(On.MainLoopProcess.orig_RawUpdate orig, MainLoopProcess self,
            float dt)
        {
            // TODO: Make sure to manually update the speedrun timer!
            
            try
            {
                UnmodifiedTickrate = self.framesPerSecond;

                if (Input.GetKey(CordycepsSettings.testSlowTickrateKey.Value))
                {
                    if (keyReleased) {
                        TestTickrateModifier = !TestTickrateModifier;
                        keyReleased = false;
                    }
                }
                else
                {
                    keyReleased = true;
                }
                
                if (TestTickrateModifier)
                {
                    self.framesPerSecond = Math.Min(self.framesPerSecond, 20);
                }
            }
            catch (Exception e)
            {
                Log($"ERROR - Uncaught exception in MainLoopProcess.RawUpdate hook: {e}");
            }

            orig(self, dt);
        }

        private static void Log(string str) { Debug.Log($"[Cordyceps] {str}"); }
    }
}