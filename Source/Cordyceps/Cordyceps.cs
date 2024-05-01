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
        public const string PLUGIN_VERSION = "0.3.1";

        public static int UnmodifiedTickrate = 40;
        public static int DesiredTickrate = 40;
        public static bool TickrateCapOn;
        public static bool ShowInfoPanel = true;

        private const float TickrateChangeInitialTime = 0.25f;
        private const float TickrateChangeHoldTickTime = 0.05f;

        private static float _tickrateChangeStopwatch;
        
        private static bool _initialized;
        private static bool _toggleInfoPanelHeld;
        private static bool _toggleTickrateCapHeld;
        private static bool _increaseTickrateHeld;
        private static bool _decreaseTickrateHeld;
        
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
                
                Log("Registering hooks");
                On.RoomCamera.ctor += RoomCamera_ctor_Hook;
                On.RoomCamera.ClearAllSprites += RoomCamera_ClearAllSprites_Hook;
                On.RainWorldGame.GrafUpdate += RainWorldGame_GrafUpdate_Hook;
                
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

        private static void RainWorldGame_RawUpdate_ILHook(ILContext il)
        {
            var cursor = new ILCursor(il);
            
            // Finds `this.oDown = Input.GetKey("o");` in RainWorldGame.RawUpdate
            cursor.GotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdstr("o"),
                x => x.MatchCall<Input>("GetKey"),
                x => x.MatchStfld<RainWorldGame>("oDown")
                );

            // TODO: Fix speedrun timer, it does not slow down with tickrate
            
            // Put the current RainWorldGame object onto the stack so we can use it to get the tickrate
            cursor.Emit(OpCodes.Ldarg, 0);
            // Put the dt argument from the RawUpdate function onto the stack so we can use it for input checks
            cursor.Emit(OpCodes.Ldarg, 1);
            
            // This code will sit after all vanilla tickrate-modifying code and before any vanilla code which uses
            // the tickrate
            cursor.EmitDelegate<Action<RainWorldGame, float>>((RainWorldGame game, float dt) =>
            {
                try
                {
                    UnmodifiedTickrate = game.framesPerSecond;

                    CheckInputs(dt);

                    if (TickrateCapOn) game.framesPerSecond = Math.Min(DesiredTickrate, game.framesPerSecond);
                }
                catch (Exception e)
                {
                    Log($"ERROR - Exception in RainWorldGame.RawUpdate IL hook: {e}");
                }
            });
        }

        private static void RoomCamera_ctor_Hook(On.RoomCamera.orig_ctor orig, RoomCamera self, RainWorldGame game, 
            int cameraNumber)
        {
            orig(self, game, cameraNumber);
            InfoPanel.Initialize();
        }

        private static void RoomCamera_ClearAllSprites_Hook(On.RoomCamera.orig_ClearAllSprites orig, RoomCamera self)
        {
            InfoPanel.Remove();
            orig(self);
        }

        private static void RainWorldGame_GrafUpdate_Hook(On.RainWorldGame.orig_GrafUpdate orig, RainWorldGame self,
            float timeStacker)
        {
            orig(self, timeStacker);
            InfoPanel.CheckGrab();
            InfoPanel.Update();
        }

        private static void CheckInputs(float dt)
        {
            if (Input.GetKey(CordycepsSettings.ToggleInfoPanelKey.Value))
            {
                if (_toggleInfoPanelHeld) return;

                _toggleInfoPanelHeld = true;
                ShowInfoPanel = !ShowInfoPanel;
                InfoPanel.UpdateVisibility();
            }
            else _toggleInfoPanelHeld = false;

            if (Input.GetKey(CordycepsSettings.ToggleTickrateCapKey.Value))
            {
                if (_toggleTickrateCapHeld) return;

                _toggleTickrateCapHeld = true;
                TickrateCapOn = !TickrateCapOn;
            }
            else _toggleTickrateCapHeld = false;

            if (Input.GetKey(CordycepsSettings.IncreaseTickrateCapKey.Value))
            {
                if (_increaseTickrateHeld)
                {
                    _tickrateChangeStopwatch += dt;

                    if (!(_tickrateChangeStopwatch >= TickrateChangeInitialTime + TickrateChangeHoldTickTime)) return;

                    DesiredTickrate = Math.Min(DesiredTickrate + 1, 40);
                    _tickrateChangeStopwatch -= TickrateChangeHoldTickTime;

                    return;
                }

                _increaseTickrateHeld = true;
                DesiredTickrate = Math.Min(DesiredTickrate + 1, 40);
            }
            else _increaseTickrateHeld = false;

            if (Input.GetKey(CordycepsSettings.DecreaseTickrateCapKey.Value))
            {
                if (_decreaseTickrateHeld)
                {
                    _tickrateChangeStopwatch += dt;

                    if (!(_tickrateChangeStopwatch >= TickrateChangeInitialTime + TickrateChangeHoldTickTime)) return;
                    
                    DesiredTickrate = Math.Max(DesiredTickrate - 1, 1);
                    _tickrateChangeStopwatch -= TickrateChangeHoldTickTime;
                    
                    return;
                }

                _decreaseTickrateHeld = true;
                DesiredTickrate = Math.Max(DesiredTickrate - 1, 1);
            }
            else _decreaseTickrateHeld = false;

            _tickrateChangeStopwatch = 0f;
        }

        private static void Log(string str) { Debug.Log($"[Cordyceps] {str}"); }
    }
}