using System;
using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace Cordyceps
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Cordyceps : BaseUnityPlugin
    {
        public const string PluginGuid = "Cordyceps";
        public const string PluginName = "Cordyceps TAS";
        public const string PluginVersion = "0.8.0";

        public static int UnmodifiedTickrate = 40;
        public static int DesiredTickrate = 40;
        public static bool TickrateCapOn;
        public static bool TickPauseOn;
        public static bool WaitingForTick;
        public static bool ShowInfoPanel = true;
        public static uint TickCount;
        public static bool TickCounterPaused;

        private const float TickrateChangeInitialTime = 0.25f;
        private const float TickrateChangeHoldTickTime = 0.05f;

        private static float _keyHoldStopwatch;
        
        private static bool _initialized;
        
        // There's probably a better way to do this, but if it ain't broke don't fix it
        private static bool _toggleInfoPanelHeld;
        private static bool _toggleTickrateCapHeld;
        private static bool _increaseTickrateHeld;
        private static bool _decreaseTickrateHeld;
        private static bool _toggleTickPauseHeld;
        private static bool _tickAdvanceHeld;
        private static bool _resetTickCounterHeld;
        private static bool _pauseTickCounterHeld;

        private static bool _startRecordingHeld;
        private static bool _stopRecordingHeld;
        private static bool _attemptConnectionHeld;

        private static bool _inMenu;
        private static double _frameRequestCounter;

        private static bool? _recordingStarted;
        
        public static bool CanAffectTickrate()
        {
            return TickrateCapOn || TickPauseOn;
        }
        
        private void OnEnable()
        {
            On.RainWorld.OnModsInit += RainWorld_OnModsInit_Hook;
        }

        private static void RainWorld_OnModsInit_Hook(On.RainWorld.orig_OnModsInit orig, RainWorld self)
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
                On.RainWorldGame.Update += RainWorldGame_Update_Hook;
                On.MoreSlugcats.SpeedRunTimer.GetTimerTickIncrement +=
                    MoreSlugcats_SpeedRunTimer_GetTimerTickIncrement_Hook;
                On.MainLoopProcess.RawUpdate += MainLoopProcess_RawUpdate_Hook;
                
                Log("Registering IL hooks");
                IL.RainWorldGame.RawUpdate += RainWorldGame_RawUpdate_ILHook;

                Log("Registering settings");
                MachineConnector.SetRegisteredOI("Cordyceps", CordycepsSettings.Instance);
                
                _initialized = true;

                if (!CordycepsSettings.ObsIntegrationOn.Value) return;
                
                Log("Initializing OBS websocket client");

                var (port, password) = ObsIntegration.GetClientConfig();
                ObsIntegration.InitializeClient(port, password);
                ObsIntegration.AttemptConnection();
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

                    if (CanAffectTickrate()) 
                        game.framesPerSecond = TickPauseOn ? 0 : Math.Min(DesiredTickrate, game.framesPerSecond);
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
        
        private static void RainWorldGame_Update_Hook(On.RainWorldGame.orig_Update orig, RainWorldGame self)
        {
            orig(self);

            try
            {
                if (CordycepsSettings.ShowTickCounter.Value && !TickCounterPaused && !self.GamePaused) TickCount++;
                
                if (CordycepsSettings.ObsIntegrationOn.Value && ObsIntegration.RecordStatus == RecordStatus.Started)
                {
                    _frameRequestCounter += (double) CordycepsSettings.RecordingFps.Value / UnmodifiedTickrate;
                    var requestCount = (int) Math.Floor(_frameRequestCounter);
                    
                    ObsIntegration.RequestFrames(requestCount);

                    _frameRequestCounter -= requestCount;

                    ObsIntegration.RecordTime += (double) requestCount / CordycepsSettings.RecordingFps.Value;
                }
                
                if (!WaitingForTick) return;
                
                WaitingForTick = false;
                TickPauseOn = true;
            }
            catch (Exception e)
            {
                Log($"ERROR - Exception in RainWorldGame.Update hook: {e}");
            }
        }

        private static void MainLoopProcess_RawUpdate_Hook(On.MainLoopProcess.orig_RawUpdate orig, 
            MainLoopProcess self, float dt)
        {
            orig(self, dt);
            
            try
            {
                if (!CordycepsSettings.ObsIntegrationOn.Value) return;
                
                if (_recordingStarted != null)
                {
                    // Have to use null-coalescing since for some reason Rider doesn't realize _recordingStarted can't
                    // be null here
                    if ((_recordingStarted ?? false) && !(self.manager.currentMainLoop is RainWorldGame))
                    {
                        _inMenu = true;
                        ObsIntegration.SetRealtimeMode(true);
                    }
                    else _inMenu = false;
                    
                    _recordingStarted = null;
                }

                if (self.manager.currentMainLoop is RainWorldGame)
                {
                    if (_inMenu)
                    {
                        _inMenu = false;
                        ObsIntegration.SetRealtimeMode(false);
                    }
                }
                else
                {
                    if (!_inMenu)
                    {
                        _inMenu = true;
                        ObsIntegration.SetRealtimeMode(true);
                    }
                }
                
                CheckInputsObs();

                if (ObsIntegration.RealtimeMode) ObsIntegration.RecordTime += dt;
            }
            catch (Exception e)
            {
                Log($"ERROR - Exception in MainLoopProcess.RawUpdate hook: {e}");
            }
        }

        private static double MoreSlugcats_SpeedRunTimer_GetTimerTickIncrement_Hook(
            On.MoreSlugcats.SpeedRunTimer.orig_GetTimerTickIncrement orig, RainWorldGame game, double dt)
        {
            var originalReturn = orig(game, dt);

            try
            {
                if (!CanAffectTickrate()) return originalReturn;
            
                var timeDialationFactor = game.framesPerSecond / (double) UnmodifiedTickrate;
                return originalReturn * timeDialationFactor;
            }
            catch (Exception e)
            {
                Log($"ERROR - Exception in MoreSlugcats.SpeedRunTimer.GetTimerTickIncrement hook: {e}");
                return originalReturn;
            }
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

            if (Input.GetKey(CordycepsSettings.ResetTickCounterKey.Value))
            {
                if (_resetTickCounterHeld) return;

                _resetTickCounterHeld = true;
                TickCount = 0;
            }
            else _resetTickCounterHeld = false;

            if (Input.GetKey(CordycepsSettings.ToggleTickCounterPauseKey.Value))
            {
                if (_pauseTickCounterHeld) return;

                _pauseTickCounterHeld = true;
                TickCounterPaused = !TickCounterPaused;
            }
            else _pauseTickCounterHeld = false;

            if (Input.GetKey(CordycepsSettings.ToggleTickPauseKey.Value))
            {
                if (_toggleTickPauseHeld) return;

                _toggleTickPauseHeld = true;

                if (WaitingForTick || ObsIntegration.DisconnectPause) return;
                TickPauseOn = !TickPauseOn;
            }
            else _toggleTickPauseHeld = false;

            // The tick advance function works as such: When the key is pressed, the "WaitingForTick" flag is set,
            // Cordyceps releases the tick pause, and the game proceeds as normal until the next call to
            // RainWorldGame.Update(), at which point a hook checks if WaitingForTick is set, pausing the game and
            // unsetting it if it is. Effectively, the game automatically controls the tick pause while waiting
            // for the next tick for you.
            if (Input.GetKey(CordycepsSettings.TickAdvanceKey.Value))
            {
                if (_tickAdvanceHeld) return;

                _tickAdvanceHeld = true;

                if (!TickPauseOn || ObsIntegration.DisconnectPause) return;
                WaitingForTick = true;
                TickPauseOn = false;
            }
            else _tickAdvanceHeld = false;

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
                    _keyHoldStopwatch += dt;

                    if (!(_keyHoldStopwatch >= TickrateChangeInitialTime + TickrateChangeHoldTickTime)) return;

                    DesiredTickrate = Math.Min(DesiredTickrate + 1, 40);
                    _keyHoldStopwatch -= TickrateChangeHoldTickTime;

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
                    _keyHoldStopwatch += dt;

                    if (!(_keyHoldStopwatch >= TickrateChangeInitialTime + TickrateChangeHoldTickTime)) return;
                    
                    DesiredTickrate = Math.Max(DesiredTickrate - 1, 1);
                    _keyHoldStopwatch -= TickrateChangeHoldTickTime;
                    
                    return;
                }

                _decreaseTickrateHeld = true;
                DesiredTickrate = Math.Max(DesiredTickrate - 1, 1);
            }
            else _decreaseTickrateHeld = false;

            _keyHoldStopwatch = 0f;
        }

        private static async void CheckInputsObs()
        {
            if (Input.GetKey(CordycepsSettings.StartRecordingKey.Value))
            {
                if (_startRecordingHeld) return;

                _startRecordingHeld = true;
                _recordingStarted = await ObsIntegration.StartRecording();
            }
            else _startRecordingHeld = false;
            
            if (Input.GetKey(CordycepsSettings.StopRecordingKey.Value))
            {
                if (_stopRecordingHeld) return;

                _stopRecordingHeld = true;
                ObsIntegration.StopRecording();
            }
            else _stopRecordingHeld = false;
            
            if (Input.GetKey(CordycepsSettings.AttemptConnectionKey.Value))
            {
                if (_attemptConnectionHeld) return;

                _attemptConnectionHeld = true;
                ObsIntegration.AttemptConnection();
            }
            else _attemptConnectionHeld = false;
        }

        private static void Log(string str) { Debug.Log($"[Cordyceps] {str}"); }
    }
}