using System;
using System.Windows;
using FramePFX.WPF.Controls;
using FramePFX.WPF.ServiceManaging.ColourPickers;
using Gpic.Core.Services;
using SkiaSharp;

namespace FramePFX.WPF.ServiceManaging {
    [ServiceImplementation(typeof(IColourPicker))]
    public class ColourPickerService : IColourPicker {
        public SKColor? PickARGB(SKColor? def = null) {
            ColourPickerWindow window = new ColourPickerWindow {
                WindowStyle = WindowStyle.None,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Height = 426
            };

            if (def is SKColor colour) {
                window.Colour = colour;
            }

            CursorUtils.POINT pos = CursorUtils.GetCursorPos();
            window.Left = pos.x;
            window.Top = Math.Max(pos.y - 426, 0d);

            if (window.ShowDialog() == true) {
                return window.ActiveColour;
            }

            return null;

            // ColorDialog dialog = new ColorDialog() {
            //     AnyColor = true,
            //     SolidColorOnly = false,
            //     AllowFullOpen = true,
            //     FullOpen = true
            // };
            // 
            // if (def is SKColor d) {
            //     dialog.Color = Color.FromArgb(d.Alpha, d.Red, d.Green, d.Blue);
            // }
            // 
            // if (dialog.ShowDialog() == DialogResult.OK) {
            //     Color c = dialog.Color;
            //     return new SKColor(c.R, c.G, c.B, c.A);
            // }
            // 
            // return null;
        }
    }
}