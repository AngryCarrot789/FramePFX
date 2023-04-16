using System;
using System.Windows.Media;

namespace FramePFX.Timeline.ViewModels.Layer {
    public static class LayerColours {
        private static readonly Random random = new Random();

        private static readonly string[] colours = {
            nameof(Colors.BlueViolet),
            nameof(Colors.Brown),
            nameof(Colors.BurlyWood),
            nameof(Colors.CadetBlue),
            nameof(Colors.Chocolate),
            nameof(Colors.Coral),
            nameof(Colors.CornflowerBlue),
            nameof(Colors.Crimson),
            nameof(Colors.DarkBlue),
            nameof(Colors.DarkCyan),
            nameof(Colors.DarkGoldenrod),
            nameof(Colors.DarkGray),
            nameof(Colors.DarkGreen),
            nameof(Colors.DarkKhaki),
            nameof(Colors.DarkMagenta),
            nameof(Colors.DarkOliveGreen),
            nameof(Colors.DarkOrange),
            nameof(Colors.DarkOrchid),
            nameof(Colors.DarkRed),
            nameof(Colors.DarkSalmon),
            nameof(Colors.DarkSeaGreen),
            nameof(Colors.DarkSlateBlue),
            nameof(Colors.DarkSlateGray),
            nameof(Colors.DarkTurquoise),
            nameof(Colors.DarkViolet),
            nameof(Colors.DeepPink),
            nameof(Colors.DimGray),
            nameof(Colors.DodgerBlue),
            nameof(Colors.Firebrick),
            nameof(Colors.ForestGreen),
            nameof(Colors.Fuchsia),
            nameof(Colors.Goldenrod),
            nameof(Colors.Gray),
            nameof(Colors.Green),
            nameof(Colors.HotPink),
            nameof(Colors.IndianRed),
            nameof(Colors.Indigo),
            nameof(Colors.LightBlue),
            nameof(Colors.LightCoral),
            nameof(Colors.LightSalmon),
            nameof(Colors.LightSeaGreen),
            nameof(Colors.LightSkyBlue),
            nameof(Colors.LightSlateGray),
            nameof(Colors.LightSteelBlue),
            nameof(Colors.Magenta),
            nameof(Colors.Maroon),
            nameof(Colors.MediumBlue),
            nameof(Colors.MediumOrchid),
            nameof(Colors.MediumPurple),
            nameof(Colors.MediumSeaGreen),
            nameof(Colors.MediumSlateBlue),
            nameof(Colors.MediumVioletRed),
            nameof(Colors.MidnightBlue),
            nameof(Colors.Navy),
            nameof(Colors.Olive),
            nameof(Colors.OliveDrab),
            nameof(Colors.Orange),
            nameof(Colors.OrangeRed),
            nameof(Colors.Orchid),
            nameof(Colors.PaleVioletRed),
            nameof(Colors.Peru),
            nameof(Colors.Pink),
            nameof(Colors.Plum),
            nameof(Colors.PowderBlue),
            nameof(Colors.Purple),
            nameof(Colors.RosyBrown),
            nameof(Colors.RoyalBlue),
            nameof(Colors.SaddleBrown),
            nameof(Colors.Salmon),
            nameof(Colors.SandyBrown),
            nameof(Colors.SeaGreen),
            nameof(Colors.Sienna),
            nameof(Colors.SkyBlue),
            nameof(Colors.SlateBlue),
            nameof(Colors.SlateGray),
            nameof(Colors.SteelBlue),
            nameof(Colors.Tan),
            nameof(Colors.Teal),
            nameof(Colors.Tomato),
            nameof(Colors.Violet),
            nameof(Colors.YellowGreen)
        };

        public static string GetRandomColour() {
            return colours[random.Next(0, colours.Length)];
        }
    }
}