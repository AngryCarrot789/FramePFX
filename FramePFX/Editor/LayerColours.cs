using System;

namespace FramePFX.Editor
{
    public static class TrackColours
    {
        private static readonly Random random = new Random();

        private static readonly string[] colours =
        {
            "BlueViolet",
            "Brown",
            "BurlyWood",
            "CadetBlue",
            "Chocolate",
            "Coral",
            "CornflowerBlue",
            "Crimson",
            "DarkBlue",
            "DarkCyan",
            "DarkGoldenrod",
            "DarkGray",
            "DarkGreen",
            "DarkKhaki",
            "DarkMagenta",
            "DarkOliveGreen",
            "DarkOrange",
            "DarkOrchid",
            "DarkRed",
            "DarkSalmon",
            "DarkSeaGreen",
            "DarkSlateBlue",
            "DarkSlateGray",
            "DarkTurquoise",
            "DarkViolet",
            "DeepPink",
            "DimGray",
            "DodgerBlue",
            "Firebrick",
            "ForestGreen",
            "Fuchsia",
            "Goldenrod",
            "Gray",
            "Green",
            "HotPink",
            "IndianRed",
            "Indigo",
            "LightBlue",
            "LightCoral",
            "LightSalmon",
            "LightSeaGreen",
            "LightSkyBlue",
            "LightSlateGray",
            "LightSteelBlue",
            "Magenta",
            "Maroon",
            "MediumBlue",
            "MediumOrchid",
            "MediumPurple",
            "MediumSeaGreen",
            "MediumSlateBlue",
            "MediumVioletRed",
            "MidnightBlue",
            "Navy",
            "Olive",
            "OliveDrab",
            "Orange",
            "OrangeRed",
            "Orchid",
            "PaleVioletRed",
            "Peru",
            "Pink",
            "Plum",
            "PowderBlue",
            "Purple",
            "RosyBrown",
            "RoyalBlue",
            "SaddleBrown",
            "Salmon",
            "SandyBrown",
            "SeaGreen",
            "Sienna",
            "SkyBlue",
            "SlateBlue",
            "SlateGray",
            "SteelBlue",
            "Tan",
            "Teal",
            "Tomato",
            "Violet",
            "YellowGreen"
        };

        public static string GetRandomColour()
        {
            return colours[random.Next(0, colours.Length)];
        }
    }
}