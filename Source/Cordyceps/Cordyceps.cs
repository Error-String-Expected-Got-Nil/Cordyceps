using BepInEx;
using UnityEngine;

namespace Cordyceps
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Cordyceps : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "Cordyceps";
        public const string PLUGIN_NAME = "Cordyceps TAS";
        public const string PLUGIN_VERSION = "0.0.0";

        private static bool initialized = false;
        
        private void OnEnable()
        {
            On.RainWorld.OnModsInit += RainWorld_OnModsInit_Hook;
        }

        private void RainWorld_OnModsInit_Hook(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);

            if (initialized) return;

            Debug.Log("[Cordyceps] Initializing");
            
            initialized = true;
        }
    }
}