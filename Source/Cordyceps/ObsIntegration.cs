// The following class handles connection with OBS and interacting with it via its websocket feature.
// Utilizes the ObsWebSocket.Net library, Github page here: https://github.com/wpscott/ObsWebSocket.Net 

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ObsWebSocket.Net;
using ObsWebSocket.Net.Protocol.Enums;
using UnityEngine;

using static Cordyceps.RecordStatus;

namespace Cordyceps
{
    public enum RecordStatus
    {
        Stopped,
        Stopping,
        Starting,
        Started
    }

    public static class ObsIntegration
    {
        public static bool Connected { get; private set; }
        public static RecordStatus RecordStatus { get; private set; } = Stopped;
        public static double RecordTime;
        public static bool RealtimeMode;
        public static bool DisconnectPause { get; private set; }

        private static bool _previousPauseState;
        private static bool _previousWaitingForTickState;

        private static ObsWebSocketClient _client;
        private static bool _connecting;
        private static bool _intentionalDisconnect;

        public static bool HasClient => _client != null;

        private static readonly EventWaitHandle RecordStartWaitHandle = new EventWaitHandle(false, 
            EventResetMode.AutoReset);

        private static readonly EventWaitHandle ReconnectWaitHandle = new EventWaitHandle(false,
            EventResetMode.AutoReset);

        private static readonly EventWaitHandle DisconnectWaitHandle = new EventWaitHandle(false,
            EventResetMode.AutoReset);
        
        private static bool _recordStartSuccess;

        public static void InitializeClient(int port, string password)
        {
            if (_client != null) return;

            // Technically you *could* use something other than the loopback address as the IP, but there's no
            // situation I can think of in which you would ever want to, at least in this case.
            _client = new ObsWebSocketClient("127.0.0.1", port, password);

            _client.OnIdentified += async () =>
            {
                Log("Connected to OBS, attempting to get Cordyceps-stalk status");

                var response = _client.CallVendorRequest("cordyceps_stalk",
                    "status", new Dictionary<string, object>());

                await response;

                object active = null;
                var gotStatus = response.Result?.ResponseData.TryGetValue("active", out active);

                if ((gotStatus ?? false) && (((JsonElement?)active)?.GetBoolean() ?? false))
                {
                    Log("Cordyceps-stalk presence verified");
                    Connected = true;
                }
                else Log("Connection failed, Cordyceps-stalk was not active");

                _connecting = false;

                ReconnectWaitHandle.Set();
            };

            _client.OnConnectionFailed += e =>
            {
                Log($"Failed to connect to OBS, reason: {e.Message}");

                _connecting = false;
            };

            _client.OnClosed += async () =>
            {
                if (_intentionalDisconnect)
                {
                    Log("Cordyceps disconnected intentionally.");
                    DisconnectWaitHandle.Set();
                    _intentionalDisconnect = false;
                    return;
                }
                
                Log("WARN - Connection closed unexpectedly, attempting to reconnect");

                DisconnectPause = true;
                _previousPauseState = Cordyceps.TickPauseOn;
                _previousWaitingForTickState = Cordyceps.WaitingForTick;
                Cordyceps.TickPauseOn = true;
                Cordyceps.WaitingForTick = false;
                
                Connected = false;
                ReconnectWaitHandle.Reset();
                
                AttemptConnection();

                var reconnectEvent = Task.Run(ReconnectWaitHandle.WaitOne);

                await Task.WhenAny(reconnectEvent, Task.Delay(2000));

                if (Connected) Log("Reconnection successful");
                else
                {
                    Log("Reconnection unsuccessful! Please restart OBS and re-initialize the client before " +
                        "attempting to reconnect again. It would be best to restart both Rain World and OBS.");
                    RecordStatus = Stopped;
                }

                Cordyceps.TickPauseOn = _previousPauseState;
                Cordyceps.WaitingForTick = _previousWaitingForTickState;
                DisconnectPause = false;
            };

            _client.OnVendorEvent += ve =>
            {
                if (ve == null) return;
                if (ve.VendorName != "cordyceps_stalk") return;

                switch (ve.EventType)
                {
                    case "record_start_success":
                        _recordStartSuccess = true;
                        RecordStartWaitHandle.Set();
                        break;
                    case "record_start_fail":
                        _recordStartSuccess = false;
                        RecordStartWaitHandle.Set();
                        break;
                }
            };
        }

        public static (int, string) GetClientConfig()
        {
            // OBS websocket port and password are stored in a config file since Rain World settings aren't
            // really made for entering arbitrary text/numbers
            var configFilepath = Application.persistentDataPath + @"\ModConfigs\Cordyceps\" +
                                 "websocket_config.json";
            
            var port = 4455;
            var password = "";
            var configReadSuccessful = true;
            
            if (File.Exists(configFilepath))
            {
                try
                {
                    var config = JsonDocument.Parse(File.ReadAllText(configFilepath));
            
                    password = config.RootElement.GetProperty("password").GetString();
                    port = config.RootElement.GetProperty("port").GetInt32();
                }
                catch (Exception e)
                {
                    Log($"ERROR - Exception while attempting to read OBS websocket JSON config: {e}");
                    configReadSuccessful = false;
                }
            }
            else
            {
                Log("Failed to read OBS websocket JSON config, file did not exist; attempting to create " +
                    "with default values (path should be: %appdata%/../LocalLow/Videocult/Rain World/ModConfigs/" +
                    "Cordyceps/websocket_config.json)");
                const string defaultSettingsJson =
                    "{\n" +
                    "    \"password\": \"\",\n" +
                    "    \"port\": 4455\n" +
                    "}\n";
            
                Directory.CreateDirectory(Application.persistentDataPath + @"\ModConfigs\Cordyceps");
                File.WriteAllText(configFilepath, defaultSettingsJson);
                
                configReadSuccessful = false;
            }
            
            if (!configReadSuccessful)
            {
                Log("Could not read OBS websocket JSON config, assuming no password and default port 4455");
                port = 4455;
                password = "";
            }
            
            return (port, password);
        }

        public static void AttemptConnection()
        {
            if (_client == null) return;
            if (_connecting) return;
            if (Connected) return;

            _connecting = true;

            _client.Connect(EventSubscription.Vendors);
        }

        public static async Task<bool> Disconnect()
        {
            if (!CanSendRequest()) return false;

            DisconnectWaitHandle.Reset();
            _intentionalDisconnect = true;
            _client.Close();

            var disconnectEvent = Task.Run(DisconnectWaitHandle.WaitOne);
            if (await Task.WhenAny(disconnectEvent, Task.Delay(2000)) != disconnectEvent)
            {
                Log("ERROR - Cordyceps timed out on waiting for disconnect event. This should never happen, " +
                    "something is quite wrong!");
                return false;
            }
            
            Log("Disconnected successfully.");
            return true;
        }

        // Returns whether or not recording was started successfully
        // Returns false if recording was already started, starting, or being stopped
        public static async Task<bool> StartRecording()
        {
            if (!CanSendRequest() || RecordStatus != Stopped) return false;

            RecordStatus = Starting;

            await UpdateEncoderSettings();
            
            var response = _client.CallVendorRequest("cordyceps_stalk", 
                "start_recording", new Dictionary<string, object>());
            
            await response;

            object success = null;
            var gotSuccess = response.Result?.ResponseData.TryGetValue("success", out success);
            
            if (!((gotSuccess ?? false) && (((JsonElement?)success)?.GetBoolean() ?? false)))
            {
                Log("ERROR - Cordyceps-stalk output failed to start its initialization thread. This should never " +
                    "happen, something is quite wrong!");
                RecordStatus = Stopped;
                return false;
            }
            
            // The weird combination of async/await and this event wait handle is because the Cordyceps-stalk OBS
            // output uses a thread to start itself up. Unless that thread fails to start, the return of 
            // obs_output_start() will always be true, and the output will signal failure later if something goes
            // wrong. So here we're waiting for an event from Cordyceps-stalk saying whether it actually started.
            var recordStatusEvent = Task.Run(RecordStartWaitHandle.WaitOne);
            if (await Task.WhenAny(recordStatusEvent, Task.Delay(2000)) != recordStatusEvent)
            {
                Log("ERROR - Cordyceps timed out on waiting for Cordyceps-stalk output to initialize. This " +
                    "should never happen, something is quite wrong!");
                RecordStatus = Stopped;
                return false;
            }

            if (!_recordStartSuccess)
            {
                Log("WARN - Cordyceps-stalk output failed to start, please see the OBS log for more details " +
                    "(should be located at %appdata%/obs-studio/logs, check the most recent one)");
                RecordStatus = Stopped;
                return false;
            }

            Log("Recording started");
            RecordStatus = Started;
            RecordTime = 0;
            return true;
        }

        // Returns false if not able to send a request right now or recording status doesn't permit stopping, returns
        // true otherwise, since stopping an output should never fail
        public static async Task<bool> StopRecording()
        {
            if (!CanSendRequest() || RecordStatus != Started)
            {
                Log($"Could not stop recording, relevant variables: _client != null = {_client != null}; " +
                    $"!_connecting = {!_connecting}; Connected = {Connected}; RecordStatus = {RecordStatus}");
                return false;
            }

            RecordStatus = Stopping;

            await _client.CallVendorRequest("cordyceps_stalk", "stop_recording",
                new Dictionary<string, object>());

            Log("Recording stopped");
            RecordStatus = Stopped;
            RecordTime = 0;
            return true;
        }

        public static void SetRealtimeMode(bool value)
        {
            if (!CanSendRequest() || RecordStatus != Started) return;
            
            Log("Attempting to switch realtime mode " + (value ? "on" : "off"));

            _client.CallVendorRequest("cordyceps_stalk", "set_realtime_mode",
                new Dictionary<string, object>
                {
                    {"value", value}
                });

            RealtimeMode = value;
        }

        public static void RequestFrames(int count)
        {
            if (!CanSendRequest() || RecordStatus != Started) return;

            _client.CallVendorRequest("cordyceps_stalk", "request_frames",
                new Dictionary<string, object>
                {
                    {"count", count}
                });
        }

        private static Task UpdateEncoderSettings()
        {
            if (!CanSendRequest()) return null;

            var encoderSettingsFilepath = Application.persistentDataPath + @"\ModConfigs\Cordyceps\" +
                                          "encoder_config.json";
            var dirpath = "C:/cordyceps/";
            var gopSize = 120; // Also known as keyframe interval
            var crf = 23.0;
            var preset = "veryfast";
            var configReadSuccessful = true;

            if (File.Exists(encoderSettingsFilepath))
            {
                try
                {
                    var config = JsonDocument.Parse(File.ReadAllText(encoderSettingsFilepath));

                    dirpath = config.RootElement.GetProperty("dirpath").GetString();
                    gopSize = config.RootElement.GetProperty("keyframe_interval").GetInt32();
                    crf = config.RootElement.GetProperty("crf").GetDouble();
                    preset = config.RootElement.GetProperty("preset").GetString();
                }
                catch (Exception e)
                {
                    Log($"ERROR - Exception while attempting to read encoder settings JSON: {e}");
                    configReadSuccessful = false;
                }
            }
            else
            {
                Log("Failed to read encoder settings config, file did not exist; attempting to create with " +
                    "default values (path should be: %appdata%/../LocalLow/Videocult/Rain World/ModConfigs/" +
                    "Cordyceps/encoder_config.json)");
                const string defaultEncoderSettingsJson =
                    "{\n" +
                    "    \"dirpath\": \"C:/cordyceps/\",\n" +
                    "    \"keyframe_interval\": 120,\n" +
                    "    \"crf\": 23.0,\n" +
                    "    \"preset\": \"veryfast\"\n" +
                    "}\n";

                Directory.CreateDirectory(Application.persistentDataPath + @"\ModConfigs\Cordyceps");
                File.WriteAllText(encoderSettingsFilepath, defaultEncoderSettingsJson);

                configReadSuccessful = false;
            }

            if (!configReadSuccessful)
            {
                Log("Could not read encoder config JSON, using the following default settings:" +
                    "\nOuput directory: C:/cordyceps/" +
                    "\nKeyframe interval: 120" +
                    "\nCRF: 23.0" +
                    "\nlibx264 codec preset: veryfast");
                dirpath = "C:/cordyceps/";
                gopSize = 120;
                crf = 23.0;
                preset = "veryfast";
            }

            if (!Path.IsPathRooted(dirpath) || (dirpath?[dirpath.Length - 1] != '/'
                                                && dirpath?[dirpath.Length - 1] != '\\'))
            {
                Log($"WARN - Provided dirpath \"{dirpath}\" either was not a valid rooted directory, or did not " +
                    "end in a '/' or '\\', using \"C:/cordyceps/\" instead (for technical reasons, the given output " +
                    "directory must end in a slash and start at a root directory)");
                dirpath = "C:/cordyceps/";
            }

            if (gopSize < 1)
            {
                Log($"WARN - Provided keyframe interval {gopSize} was invalid, using 120 instead (keyframe " +
                    "interval must be at least 1)");
                gopSize = 120;
            }

            if (crf < 0 || crf > 51)
            {
                Log($"WARN - Provided CRF value {crf} was outside valid range, using 23.0 instead (CRF must be " +
                    "between 0 and 51)");
                crf = 23.0;
            }
            
            string[] validPresets = 
                { "ultrafast", "veryfast", "faster", "fast", "medium", "slow", "slower", "veryslow", "placebo" };
            
            if (!validPresets.Contains(preset))
            {
                Log($"WARN - Provided preset \"{preset}\" was not a valid libx264 codec preset, using " +
                    "\"veryfast\" instead (valid presets are \"ultrafast\", \"veryfast\", \"faster\", \"fast\", " +
                    "\"medium\", \"slow\", \"slower\", \"veryslow\", \"placebo\")");
                preset = "veryfast";
            }

            var response= _client.CallVendorRequest("cordyceps_stalk", 
                "update_settings",
                new Dictionary<string, object>
                {
                    {"dirpath", dirpath},
                    {"gop_size", gopSize},
                    {"crf", crf},
                    {"preset", preset}
                });

            return response;
        }

        public static bool CanSendRequest()
        {
            return _client != null && !_connecting && Connected;
        }
        
        private static void Log(string str) { Debug.Log($"[Cordyceps OBS] {str}"); }
    }
}