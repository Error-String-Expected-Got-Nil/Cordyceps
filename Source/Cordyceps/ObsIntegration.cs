// The following class handles connection with OBS and interacting with it via its websocket feature.
// Utilizes the ObsWebSocket.Net library, Github page here: https://github.com/wpscott/ObsWebSocket.Net 

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ObsWebSocket.Net;
using ObsWebSocket.Net.Protocol.Enums;
using UnityEngine;

namespace Cordyceps
{
    public class ObsIntegration
    {
        public static bool Connected { get; private set; }
        public static bool Recording { get; private set; }

        private static ObsWebSocketClient _client;
        private static bool _connecting;

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

                if ((gotStatus ?? false) && (((JsonElement?) active)?.GetBoolean() ?? false))
                {
                    Log("Cordyceps-stalk presence verified");
                    Connected = true;
                }
                else Log("Connection failed, Cordyceps-stalk was not active");

                _connecting = false;
            };

            _client.OnConnectionFailed += e =>
            {
                Log($"Failed to connect to OBS, reason: {e.Message}");
                
                _connecting = false;
            };
        }

        public static void AttemptConnection()
        {
            if (_client == null) return;
            if (_connecting) return;
            if (Connected) return;

            _connecting = true;

            _client.Connect(EventSubscription.Vendors);
        }

        public static async void StartRecording()
        {
            if (!CanSendRequest()) return;

            await UpdateEncoderSettings();
            
            // NOT FINISHED
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
                crf = 23f;
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