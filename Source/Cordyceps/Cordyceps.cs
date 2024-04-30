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
        public const string PLUGIN_VERSION = "0.1.2";

        public static int UnmodifiedTickrate = 40;

        private static bool _initialized;
        
        private void OnEnable()
        {
            On.RainWorld.OnModsInit += RainWorld_OnModsInit_Hook;
        }

        private void RainWorld_OnModsInit_Hook(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            if (_initialized) return;

            try
            {
                Log("Initializing");
                
                //Log("Registering hooks");
                
                Log("Registering IL hooks");
                IL.RainWorldGame.RawUpdate += RainWorldGame_RawUpdate_ILHook;

                Log("Registering settings");
                MachineConnector.SetRegisteredOI("Cordyceps", new CordycepsSettings());
                
                _initialized = true;
                Log("Initialized successfully");
            }
            catch (Exception e)
            {
                Log($"ERROR - Exception during initialization: {e}");
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
            
            // This code will sit after all vanilla tickrate-modifying code and before any vanilla code which uses
            // the tickrate
            cursor.EmitDelegate<Action<RainWorldGame>>((RainWorldGame game) =>
            {
                try
                {
                    UnmodifiedTickrate = game.framesPerSecond;
                }
                catch (Exception e)
                {
                    Log($"ERROR - Exception in RainWorldGame.RawUpdate IL hook: {e}");
                }
            });
        }

        private static void Log(string str) { Debug.Log($"[Cordyceps] {str}"); }
    }
}