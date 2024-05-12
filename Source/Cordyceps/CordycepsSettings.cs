using Menu.Remix.MixedUI;
using UnityEngine;

namespace Cordyceps
{
    internal class CordycepsSettings : OptionInterface
    {
        public static readonly CordycepsSettings Instance = new CordycepsSettings();

        // First column
        public static Configurable<KeyCode> ToggleInfoPanelKey =
            Instance.config.Bind(nameof(ToggleInfoPanelKey), KeyCode.M, new ConfigurableInfo(
                "Press to toggle visibility of info panel."));

        public static Configurable<KeyCode> ToggleTickrateCapKey =
            Instance.config.Bind(nameof(ToggleTickrateCapKey), KeyCode.Comma, new ConfigurableInfo(
                "Press to toggle tickrate cap on/off."));

        public static Configurable<KeyCode> IncreaseTickrateCapKey =
            Instance.config.Bind(nameof(IncreaseTickrateCapKey), KeyCode.Equals, new ConfigurableInfo(
                "Press or hold to increase tickrate cap."));

        public static Configurable<KeyCode> DecreaseTickrateCapKey =
            Instance.config.Bind(nameof(DecreaseTickrateCapKey), KeyCode.Minus, new ConfigurableInfo(
                "Press or hold to decrease tickrate cap."));

        public static Configurable<KeyCode> ToggleTickPauseKey =
            Instance.config.Bind(nameof(ToggleTickPauseKey), KeyCode.Period, new ConfigurableInfo(
                "Press to toggle game pause without showing pause menu."));

        public static Configurable<KeyCode> TickAdvanceKey =
            Instance.config.Bind(nameof(TickAdvanceKey), KeyCode.Slash, new ConfigurableInfo(
                "Press to advance a single game tick. Only works while tick pause is active. Any inputs " +
                "held when tick is advanced will be registered on the frame you advance to."));
        
        // Second column
        public static Configurable<KeyCode> ResetTickCounterKey =
            Instance.config.Bind(nameof(ResetTickCounterKey), KeyCode.Semicolon, new ConfigurableInfo(
                "Press to reset tick counter to 0."));

        public static Configurable<KeyCode> ToggleTickCounterPauseKey =
            Instance.config.Bind(nameof(ToggleTickCounterPauseKey), KeyCode.Quote, new ConfigurableInfo(
                "Press to toggle pausing or unpausing the tick counter."));
        
        public static Configurable<bool> ShowTickCounter =
            Instance.config.Bind(nameof(ShowTickCounter), true, new ConfigurableInfo(
                "Toggle whether the tick counter should be added to the info panel."));
        
        // OBS Websocket Page
        // First column
        public static Configurable<KeyCode> StartRecordingKey =
            Instance.config.Bind(nameof(StartRecordingKey), KeyCode.R, new ConfigurableInfo(
                "Press to start recording. See log file for results."));
        
        public static Configurable<KeyCode> StopRecordingKey =
            Instance.config.Bind(nameof(StopRecordingKey), KeyCode.T, new ConfigurableInfo(
                "Press to stop recording. See log file for results."));

        public static Configurable<KeyCode> AttemptConnectionKey =
            Instance.config.Bind(nameof(AttemptConnectionKey), KeyCode.T, new ConfigurableInfo(
                "Press to make an attempt to connect with OBS. See log file for results."));
        
        // Second column
        public static Configurable<bool> ObsIntegrationOn =
            Instance.config.Bind(nameof(ObsIntegrationOn), true, new ConfigurableInfo(
                "Toggle to enable/disable OBS integration entirely."));

        public static Configurable<int> RecordingFps =
            Instance.config.Bind(nameof(RecordingFps), 60, new ConfigurableInfo(
                "The frames per second value OBS is set to for recording.", 
                new ConfigAcceptableRange<int>(1, 300)));
        
        public override void Initialize()
        {
            base.Initialize();
            
            Tabs = new[] { new OpTab(this, "Settings"), new OpTab(this, "OBS Integration") };
            
            // Settings
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
                    {description =  ShowTickCounter.info.description},
                
                // Footer
                new OpLabelLong(new Vector2(10f, 350f), new Vector2(570f, 0f), 
                    "Please see the README included in the mod's directory for more detailed information on " +
                    "the functions of this mod!")
            });

            // This might not work correctly if RecordStatus is Starting or Stopping, but there's a *very* tiny window
            // for that so I think it's unlikely to be an issue. Things are already broken if it comes up.
            ObsIntegrationOn.OnChange += async () =>
            {
                if (ObsIntegrationOn.Value)
                {
                    Log("OBS integration re-enabled, ensuring client initialization and attempting connection.");

                    if (!ObsIntegration.HasClient)
                    {
                        var (port, password) = ObsIntegration.GetClientConfig();
                        ObsIntegration.InitializeClient(port, password);
                    }
                    
                    ObsIntegration.AttemptConnection();
                }
                else
                {
                    Log("OBS integration disabled, disconnecting client.");
                    await ObsIntegration.StopRecording();
                    await ObsIntegration.Disconnect();
                }
            };
            
            // OBS Integration
            Tabs[1].AddItems(new UIelement[]
            {
                // First column
                new OpLabel(10f, 575f, "Start Recording")
                    {description = StartRecordingKey.info.description},
                new OpKeyBinder(StartRecordingKey, new Vector2(150f, 570f), 
                    new Vector2(120f, 30f)) {description = StartRecordingKey.info.description},
                
                new OpLabel(10f, 540f, "Stop Recording")
                    {description = StopRecordingKey.info.description},
                new OpKeyBinder(StopRecordingKey, new Vector2(150f, 535f), 
                    new Vector2(120f, 30f)) {description = StopRecordingKey.info.description},
                
                new OpLabel(10f, 505f, "Attempt Connection")
                    {description = AttemptConnectionKey.info.description},
                new OpKeyBinder(AttemptConnectionKey, new Vector2(150f, 500f), 
                    new Vector2(120f, 30f)) {description = AttemptConnectionKey.info.description},
                
                // Second column
                new OpLabel(300f, 575f, "Toggle OBS Integration")
                    {description = ObsIntegrationOn.info.description},
                new OpCheckBox(ObsIntegrationOn, new Vector2(450f, 570f))
                    {description = ObsIntegrationOn.info.description},
                
                new OpLabel(300f, 540f, "Recording FPS") 
                    {description = RecordingFps.info.description},
                new OpUpdown(RecordingFps, new Vector2(450f, 535f), 120)
                    {description = RecordingFps.info.description}
            });
        }
        
        private static void Log(string str) { Debug.Log($"[Cordyceps Settings] {str}"); }
    }
}