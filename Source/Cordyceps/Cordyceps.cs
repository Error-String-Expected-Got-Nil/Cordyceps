using System;
using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;
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
                
                //Log("Registering hooks");
                
                Log("Registering IL hooks");
                IL.RainWorldGame.RawUpdate += RainWorldGame_RawUpdate_ILHook;

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

        private void RainWorldGame_RawUpdate_ILHook(ILContext il)
        {
            var cursor = new ILCursor(il);
            
            // Finds `this.oDown = Input.GetKey("o");` in RainWorldGame.RawUpdate
            cursor.GotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdstr("o"),
                x => x.MatchCall<Input>("GetKey"),
                x => x.MatchStfld<RainWorldGame>("oDown")
                );

            // Put the current RainWorldGame object onto the stack so we can use it to get the tickrate
            cursor.Emit(OpCodes.Ldarg, 0);
            
            // This code will sit after all vanilla tickrate-modifying code and before any code which uses the
            // tickrate
            cursor.EmitDelegate<Action<RainWorldGame>>((RainWorldGame game) =>
            {
                try
                {
                    UnmodifiedTickrate = game.framesPerSecond;
                    
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
                        game.framesPerSecond = Math.Min(game.framesPerSecond, 20);
                    }
                }
                catch (Exception e)
                {
                    Log($"ERROR - Uncaught exception in MainLoopProcess.RawUpdate hook: {e}");
                }
            });
        }

        private static void Log(string str) { Debug.Log($"[Cordyceps] {str}"); }
    }
}