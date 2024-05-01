using Menu.Remix.MixedUI;
using UnityEngine;

namespace Cordyceps
{
    internal class CordycepsSettings : OptionInterface
    {
        public static CordycepsSettings Instance = new CordycepsSettings();

        public static Configurable<KeyCode> ToggleInfoPanelKey =
            Instance.config.Bind("ToggleInfoPanelKey", KeyCode.M, new ConfigurableInfo(
                "Press to toggle visibility of info panel."));

        public static Configurable<KeyCode> ToggleTickrateCapKey =
            Instance.config.Bind("ToggleTickrateCapyKey", KeyCode.Comma, new ConfigurableInfo(
                "Press to toggle tickrate cap on/off."));

        public static Configurable<KeyCode> IncreaseTickrateCapKey =
            Instance.config.Bind("IncreaseTickrateCapKey", KeyCode.Equals, new ConfigurableInfo(
                "Press or hold to increase tickrate cap."));

        public static Configurable<KeyCode> DecreaseTickrateCapKey =
            Instance.config.Bind("DecreaseTickrateCapKey", KeyCode.Minus, new ConfigurableInfo(
                "Press or hold to decrease tickrate cap."));

        public override void Initialize()
        {
            base.Initialize();
            
            Tabs = new[] { new OpTab(this, "Settings") };
            
            Tabs[0].AddItems(new UIelement[]
            {
                new OpLabel(10f, 580f, "Toggle Info Panel")
                    {description = ToggleInfoPanelKey.info.description},
                new OpKeyBinder(ToggleInfoPanelKey, new Vector2(150f, 575f), new Vector2(120f, 30f))
                    {description = ToggleInfoPanelKey.info.description},
                
                new OpLabel(10f, 545f, "Toggle Tickrate Cap")
                    {description = ToggleTickrateCapKey.info.description},
                new OpKeyBinder(ToggleTickrateCapKey, new Vector2(150f, 540f), new Vector2(120f, 30f))
                    {description = ToggleTickrateCapKey.info.description},
                
                new OpLabel(10f, 510f, "Increase Tickrate Cap")
                    {description = IncreaseTickrateCapKey.info.description},
                new OpKeyBinder(IncreaseTickrateCapKey, new Vector2(150f, 505f), new Vector2(120f, 30f))
                    {description = IncreaseTickrateCapKey.info.description},
                
                new OpLabel(10f, 475f, "Decrease Tickrate Cap")
                    {description = DecreaseTickrateCapKey.info.description},
                new OpKeyBinder(DecreaseTickrateCapKey, new Vector2(150f, 470f), new Vector2(120f, 30f))
                    {description = DecreaseTickrateCapKey.info.description}
            });
        }
    }
}