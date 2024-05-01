using UnityEngine;

namespace Cordyceps
{
    // Implementation mostly copied from Alphappy's TAMacro, credit to him for that
    internal abstract class InfoPanel
    {
        private static readonly Color TextColor = new Color(255, 215, 36);
        private const float TextAlpha = 0.5f;
        
        public static FLabel Header;
        public static FLabel InfoLabel;
        public static FLabel InfoLabelData;
        public static FContainer Container;
        public static float LineHeight;
        public static Vector2 OriginalGrabMousePosition;
        public static Vector2 OriginalGrabAnchorPosition;
        public static bool PanelIsGrabbed;

        public static Vector2 PanelAnchor = new Vector2(100.5f, 700f);
        
        public static float HeaderHeight => Header.text.Split('\n').Length * LineHeight;
        public static float InfoLabelHeight => InfoLabel.text.Split('\n').Length * LineHeight;
        public static Vector2 PanelBounds => new Vector2(280f, HeaderHeight + InfoLabelHeight);

        public static void Initialize()
        {
            Container = new FContainer();
            Futile.stage.AddChild(Container);
            Container.SetPosition(Vector2.zero);

            Header = new FLabel(RWCustom.Custom.GetFont(),
                    $"Cordyceps v{Cordyceps.PLUGIN_VERSION}\nPress " +
                    $"[{CordycepsSettings.ToggleInfoPanelKey.Value.ToString()}] to toggle visibility of this " +
                    "panel.\n" + 
                    "You can also click and drag it to change its position.\n")
            {
                isVisible = true,
                alpha = TextAlpha,
                color = TextColor,
                alignment = FLabelAlignment.Left
            };
            Container.AddChild(Header);

            InfoLabel = new FLabel(RWCustom.Custom.GetFont(), "")
            {
                isVisible = true,
                alpha = TextAlpha,
                color = TextColor,
                alignment = FLabelAlignment.Left
            };
            Container.AddChild(InfoLabel);

            InfoLabelData = new FLabel(RWCustom.Custom.GetFont(), "")
            {
                isVisible = true,
                alpha = TextAlpha,
                color = TextColor,
                alignment = FLabelAlignment.Left
            };
            Container.AddChild(InfoLabelData);

            LineHeight = Header.FontLineHeight * Header.scale;

            UpdatePosition();
        }

        public static void Update()
        {
            InfoLabel.text =
                "Base Tickrate:\n" +
                "Desired Tickrate:\n" +
                "Tickrate Cap:";
            
            InfoLabelData.text =
                $"{Cordyceps.UnmodifiedTickrate}\n" +
                $"{Cordyceps.DesiredTickrate}\n" +
                (Cordyceps.TickrateCapOn ? "On" : "Off");
        }

        public static void UpdatePosition()
        {
            Header.SetPosition(PanelAnchor);
            InfoLabel.SetPosition(PanelAnchor - new Vector2(0f, HeaderHeight));
            InfoLabelData.SetPosition(PanelAnchor - new Vector2(-110f, HeaderHeight));
        }

        public static void UpdateVisibility()
        {
            Container.isVisible = Cordyceps.ShowInfoPanel;
        }

        public static void Remove()
        {
            Container.RemoveFromContainer();
            Container.RemoveAllChildren();
            Container = null;
        }

        public static void CheckGrab()
        {
            Vector2 mpos = Input.mousePosition;
            if (Input.GetMouseButton(0))
            {
                if (!PanelIsGrabbed
                    && mpos.x >= PanelAnchor.x
                    && mpos.x <= PanelAnchor.x + PanelBounds.x
                    && mpos.y <= PanelAnchor.y
                    && mpos.y >= PanelAnchor.y - PanelBounds.y)
                {
                    PanelIsGrabbed = true;
                    OriginalGrabAnchorPosition = PanelAnchor;
                    OriginalGrabMousePosition = mpos;
                }

                if (!PanelIsGrabbed) return;
                
                PanelAnchor = OriginalGrabAnchorPosition + mpos - OriginalGrabMousePosition;
                // Text is crisper if forced into alignment like this
                PanelAnchor.x = Mathf.Floor(PanelAnchor.x) + 0.5f;
                
                UpdatePosition();
            }
            else
            {
                PanelIsGrabbed = false;
            }
        }
    }
}