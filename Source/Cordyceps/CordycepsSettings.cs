using Menu.Remix.MixedUI;
using UnityEngine;

namespace Cordyceps
{
    internal class CordycepsSettings : OptionInterface
    {
        public static CordycepsSettings Instance = new CordycepsSettings();
        public static Configurable<KeyCode> testSlowTickrateKey = 
            Instance.config.Bind("testSlowTickrateKey", KeyCode.Period, new ConfigurableInfo(
                "Key for debug testing"));

        public override void Initialize()
        {
            base.Initialize();
            
            Tabs = new OpTab[] { new OpTab(this, "Settings") };
            
            Tabs[0].AddItems(new UIelement[]
            {
                new OpKeyBinder(testSlowTickrateKey, new Vector2(155f, 555f), new Vector2(150f, 30f))
                    {description = testSlowTickrateKey.info.description},
                new OpLabel(15f, 560f, "Test Slow Tickrate Key") 
                    {description = testSlowTickrateKey.info.description}
            });
        }
    }
}