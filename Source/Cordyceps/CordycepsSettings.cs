using Menu.Remix.MixedUI;
using UnityEngine;

namespace Cordyceps
{
    internal class CordycepsSettings : OptionInterface
    {
        private static readonly CordycepsSettings Instance = new CordycepsSettings();

        // First column
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

        public static readonly Configurable<KeyCode> TickAdvanceKey =
            Instance.config.Bind("TickAdvanceKey", KeyCode.Slash, new ConfigurableInfo(
                "Press to advance a single game tick. Only works while tick pause is active. Any inputs " +
                "held when frame is advanced will be registered on the frame you advance to. Please note that, for " +
                "technical reasons, this works best when the tickrate cap is on and set to a lower value."));
        
        // Second column
        public static readonly Configurable<bool> ShowTickCounter =
            Instance.config.Bind("ShowTickCounter", true, new ConfigurableInfo(
                "Toggle whether a tick counter should be added to the info panel."));

        public static readonly Configurable<KeyCode> ResetTickCounterKey =
            Instance.config.Bind("ResetTickCounterKey", KeyCode.Semicolon, new ConfigurableInfo(
                "Press to reset tick counter to 0."));

        public static readonly Configurable<KeyCode> ToggleTickCounterPauseKey =
            Instance.config.Bind("ToggleTickCounterPauseKey", KeyCode.Quote, new ConfigurableInfo(
                "Press to toggle pausing or unpausing the tick counter."));

        public override void Initialize()
        {
            base.Initialize();
            
            Tabs = new[] { new OpTab(this, "Settings") };
            
            Tabs[0].AddItems(new UIelement[]
            {
                // First column
                new OpLabel(10f, 575f, "Toggle Info Panel")
                    {description = ToggleInfoPanelKey.info.description},
                new OpKeyBinder(ToggleInfoPanelKey, new Vector2(150f, 570f), 
                        new Vector2(120f, 30f)) {description = ToggleInfoPanelKey.info.description},
                
                new OpLabel(10f, 540f, "Toggle Tickrate Cap")
                    {description = ToggleTickrateCapKey.info.description},
                new OpKeyBinder(ToggleTickrateCapKey, new Vector2(150f, 535f), 
                        new Vector2(120f, 30f)) {description = ToggleTickrateCapKey.info.description},
                
                new OpLabel(10f, 505f, "Increase Tickrate Cap")
                    {description = IncreaseTickrateCapKey.info.description},
                new OpKeyBinder(IncreaseTickrateCapKey, new Vector2(150f, 500f), 
                        new Vector2(120f, 30f)) {description = IncreaseTickrateCapKey.info.description},
                
                new OpLabel(10f, 470f, "Decrease Tickrate Cap")
                    {description = DecreaseTickrateCapKey.info.description},
                new OpKeyBinder(DecreaseTickrateCapKey, new Vector2(150f, 465f), 
                        new Vector2(120f, 30f)) {description = DecreaseTickrateCapKey.info.description},
                
                new OpLabel(10f, 435f, "Toggle Tick Pause")
                    {description = ToggleTickPauseKey.info.description},
                new OpKeyBinder(ToggleTickPauseKey, new Vector2(150f, 430f), 
                        new Vector2(120f, 30f)) {description = ToggleTickPauseKey.info.description},
                
                new OpLabel(10f, 400f, "Tick Advance")
                    {description = TickAdvanceKey.info.description},
                new OpKeyBinder(TickAdvanceKey, new Vector2(150f, 395f), 
                        new Vector2(120f, 30f)) {description = TickAdvanceKey.info.description},
                
                // Second column
                new OpLabel(300f, 575f, "Reset Tick Counter")
                    {description = ResetTickCounterKey.info.description},
                new OpKeyBinder(ResetTickCounterKey, new Vector2(450f, 570f), 
                    new Vector2(120f, 30f)) {description = ResetTickCounterKey.info.description},
                
                new OpLabel(300f, 540f, "Toggle Tick Counter Pause")
                    {description = ToggleTickPauseKey.info.description},
                new OpKeyBinder(ToggleTickCounterPauseKey, new Vector2(450f, 535f), 
                    new Vector2(120f, 30f)) {description = ToggleTickCounterPauseKey.info.description},
                
                new OpLabel(300f, 505f, "Show Tick Counter")
                    {description = ShowTickCounter.info.description},
                new OpCheckBox(ShowTickCounter, new Vector2(450f, 500f))
                    {description =  ShowTickCounter.info.description}
            });
        }
    }
}