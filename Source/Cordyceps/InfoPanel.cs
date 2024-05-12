using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Cordyceps
{
    // Implementation mostly copied from Alphappy's TAMacro, credit to him for that
    internal abstract class InfoPanel
    {
        private static readonly Color TextColor = new Color(255, 215, 36);
        private const float TextAlpha = 0.5f;
        
        private static FLabel _header;
        private static FLabel _infoLabel;
        private static FLabel _infoLabelData;
        private static FContainer _container;
        private static float _lineHeight;
        private static Vector2 _originalGrabMousePosition;
        private static Vector2 _originalGrabAnchorPosition;
        private static bool _panelIsGrabbed;

        private static Vector2 _panelAnchor = new Vector2(100.5f, 700f);
        
        private static float HeaderHeight => (Regex.Matches(_header.text, "\n").Count + 1) * _lineHeight;
        private static float InfoLabelHeight => (Regex.Matches(_infoLabel.text, "\n").Count + 1) 
                                                * _lineHeight;
        private static Vector2 PanelBounds => new Vector2(280f, HeaderHeight + InfoLabelHeight);

        public static void Initialize()
        {
            _container = new FContainer();
            Futile.stage.AddChild(_container);
            _container.SetPosition(Vector2.zero);

            _header = new FLabel(RWCustom.Custom.GetFont(),
                    $"Cordyceps v{Cordyceps.PluginVersion}\nPress " +
                    $"[{CordycepsSettings.ToggleInfoPanelKey.Value.ToString()}] to toggle visibility of this " +
                    "panel.\n" + 
                    "You can also click and drag it to change its position.\n")
            {
                isVisible = true,
                alpha = TextAlpha,
                color = TextColor,
                alignment = FLabelAlignment.Left
            };
            _container.AddChild(_header);

            _infoLabel = new FLabel(RWCustom.Custom.GetFont(), "")
            {
                isVisible = true,
                alpha = TextAlpha,
                color = TextColor,
                alignment = FLabelAlignment.Left
            };
            _container.AddChild(_infoLabel);

            _infoLabelData = new FLabel(RWCustom.Custom.GetFont(), "")
            {
                isVisible = true,
                alpha = TextAlpha,
                color = TextColor,
                alignment = FLabelAlignment.Left
            };
            _container.AddChild(_infoLabelData);

            _lineHeight = _header.FontLineHeight * _header.scale;

            Update();
            UpdatePosition();
        }

        public static void Update()
        {
            _infoLabel.text =
                "Base Tickrate:\n" +
                "Desired Tickrate:\n" +
                "Tickrate Cap:\n" +
                "Tick Pause:" +
                (CordycepsSettings.ShowTickCounter.Value ? "\nTick Count:" : "");
            
            _infoLabelData.text =
                $"{Cordyceps.UnmodifiedTickrate}\n" +
                $"{Cordyceps.DesiredTickrate}\n" +
                (Cordyceps.TickrateCapOn ? "On" : "Off") + "\n" +
                (Cordyceps.TickPauseOn ? "On" : "Off") +
                (CordycepsSettings.ShowTickCounter.Value ? $"\n{Cordyceps.TickCount}" : "");

            if (!CordycepsSettings.ObsIntegrationOn.Value) return;
            
            _infoLabel.text +=
                "\n \n-=- OBS Integration -=-" +
                "\nRecord time is approximate." +
                "\nConnection Status: " +
                "\nRecord Status:" +
                "\nRecord Time:";

            _infoLabelData.text +=
                "\n \n \n" +
                "\n" + (ObsIntegration.Connected ? "Connected" : "Disconnected") +
                $"\n{ObsIntegration.RecordStatus.ToString()}" +
                $"\n{FormatTime(ObsIntegration.RecordTime)}";
        }

        private static void UpdatePosition()
        {
            _header.SetPosition(_panelAnchor);
            _infoLabel.SetPosition(_panelAnchor - new Vector2(0f, HeaderHeight * 2f));
            _infoLabelData.SetPosition(_panelAnchor - new Vector2(-110f, HeaderHeight * 2f));
        }

        public static void UpdateVisibility()
        {
            _container.isVisible = Cordyceps.ShowInfoPanel;
        }

        public static void Remove()
        {
            _container.RemoveFromContainer();
            _container.RemoveAllChildren();
            _container = null;
        }

        public static void CheckGrab()
        {
            if (_header == null || !_header.isVisible) return;
            
            Vector2 mpos = Input.mousePosition;
            if (Input.GetMouseButton(0))
            {
                if (!_panelIsGrabbed
                    && mpos.x >= _panelAnchor.x
                    && mpos.x <= _panelAnchor.x + PanelBounds.x
                    && mpos.y <= _panelAnchor.y
                    && mpos.y >= _panelAnchor.y - PanelBounds.y)
                {
                    _panelIsGrabbed = true;
                    _originalGrabAnchorPosition = _panelAnchor;
                    _originalGrabMousePosition = mpos;
                }

                if (!_panelIsGrabbed) return;
                
                _panelAnchor = _originalGrabAnchorPosition + mpos - _originalGrabMousePosition;
                // Text is crisper if forced into alignment like this
                _panelAnchor.x = Mathf.Floor(_panelAnchor.x) + 0.5f;
                
                UpdatePosition();
            }
            else
            {
                _panelIsGrabbed = false;
            }
        }

        private static string FormatTime(double sec)
        {
            var hours = Math.Floor(sec / 3600);
            var minutes = Math.Floor(sec / 60) % 60;
            var seconds = sec % 60;
            var secondsFloored = Math.Floor(seconds);
            var secondsDecimal = Math.Round(seconds - secondsFloored, 2) * 100;

            return (hours < 10 ? "0" : "") + $"{(int)hours}:" +
                   (minutes < 10 ? "0" : "") + $"{(int)minutes}:" +
                   (secondsFloored < 10 ? "0" : "") + $"{(int)secondsFloored}." +
                   (secondsDecimal < 10 ? "0" : "") + $"{(int)secondsDecimal}";
        }
    }
}