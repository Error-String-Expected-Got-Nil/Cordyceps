// The following class handles connection with OBS and interacting with it via its websocket feature.
// Utilizes the ObsWebSocket.Net library, Github page here: https://github.com/wpscott/ObsWebSocket.Net 

using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ObsWebSocket.Net;
using UnityEngine;

namespace Cordyceps
{
    public class ObsIntegration
    {
        public static bool Connected => _connected;
        
        private static ObsWebSocketClient _client;
        private static bool _connected;
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
                    _connected = true;
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

            _client.Connect();
        }
        
        private static void Log(string str) { Debug.Log($"[Cordyceps OBS] {str}"); }
    }
}