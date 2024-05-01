using Menu.Remix.MixedUI;
using UnityEngine;

namespace Cordyceps
{
    internal class CordycepsSettings : OptionInterface
    {
        private static readonly CordycepsSettings Instance = new CordycepsSettings();

        public static readonly Configurable<KeyCode> ToggleInfoPanelKey =
            Instance.config.Bind("ToggleInfoPanelKey", KeyCode.M, new ConfigurableInfo(
                "Press to toggle visibility of info panel."));

        public static readonly Configurable<KeyCode> ToggleTickrateCapKey =
            Instance.config.Bind("ToggleTickrateCapyKey", KeyCode.Comma, new ConfigurableInfo(
                "Press to toggle tickrate cap on/off."));

        public static readonly Configurable<KeyCode> IncreaseTickrateCapKey =
            Instance.config.Bind("IncreaseTickrateCapKey", KeyCode.Equals, new ConfigurableInfo(
                "Press or hold to increase tickrate cap."));

        public static readonly Configurable<KeyCode> DecreaseTickrateCapKey =
            Instance.config.Bind("DecreaseTickrateCapKey", KeyCode.Minus, new ConfigurableInfo(
                "Press or hold to decrease tickrate cap."));

        public static readonly Configurable<KeyCode> ToggleTickPauseKey =
            Instance.config.Bind("ToggleTickPauseKey", KeyCode.Period, new ConfigurableInfo(
                "Press to toggle game pause without showing pause menu."));

        public override void Initialize()
        {
            base.Initialize();
            
            Tabs = new[] { new OpTab(this, "Settings") };
            
            Tabs[0].AddItems(new UIelement[]
            {
                new OpLabel(10f, 575f, "Toggle Info Panel")
                    {description = ToggleInfoPanelKey.info.description},
                new OpKeyBinder(ToggleInfoPanelKey, new Vector2(150f, 570f), new Vector2(120f, 30f))
                    {description = ToggleInfoPanelKey.info.description},
                
                new OpLabel(10f, 540f, "Toggle Tickrate Cap")
                    {description = ToggleTickrateCapKey.info.description},
                new OpKeyBinder(ToggleTickrateCapKey, new Vector2(150f, 535f), new Vector2(120f, 30f))
                    {description = ToggleTickrateCapKey.info.description},
                
                new OpLabel(10f, 505f, "Increase Tickrate Cap")
                    {description = IncreaseTickrateCapKey.info.description},
                new OpKeyBinder(IncreaseTickrateCapKey, new Vector2(150f, 500f), new Vector2(120f, 30f))
                    {description = IncreaseTickrateCapKey.info.description},
                
                new OpLabel(10f, 470f, "Decrease Tickrate Cap")
                    {description = DecreaseTickrateCapKey.info.description},
                new OpKeyBinder(DecreaseTickrateCapKey, new Vector2(150f, 465f), new Vector2(120f, 30f))
                    {description = DecreaseTickrateCapKey.info.description},
                
                new OpLabel(10f, 435f, "Toggle Tick Pause")
                    {description = ToggleTickPauseKey.info.description},
                new OpKeyBinder(ToggleTickPauseKey, new Vector2(150f, 430f), new Vector2(120f, 30f))
                    {description = ToggleTickPauseKey.info.description}
            });
        }
    }
}