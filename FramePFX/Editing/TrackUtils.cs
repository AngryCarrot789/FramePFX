// 
// Copyright (c) 2026-2026 REghZy
// 
// This file is part of FramePFX.
// 
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using SkiaSharp;

namespace FramePFX.Editing;

public static class TrackUtils {
    private static readonly Random Rnd = new Random();

    public static SKColor RandomColour() {
        switch (Rnd.Next(63)) {
            case 0:  return SKColors.Black;
            case 1:  return SKColors.Brown;
            case 2:  return SKColors.CadetBlue;
            case 3:  return SKColors.Chocolate;
            case 4:  return SKColors.Coral;
            case 5:  return SKColors.CornflowerBlue;
            case 6:  return SKColors.Crimson;
            case 7:  return SKColors.DarkBlue;
            case 8:  return SKColors.DarkCyan;
            case 9:  return SKColors.DarkGoldenrod;
            case 10: return SKColors.DarkGray;
            case 11: return SKColors.DarkGreen;
            case 12: return SKColors.DarkKhaki;
            case 13: return SKColors.DarkMagenta;
            case 14: return SKColors.DarkOliveGreen;
            case 15: return SKColors.DarkOrange;
            case 16: return SKColors.DarkOrchid;
            case 17: return SKColors.DarkRed;
            case 18: return SKColors.DarkSalmon;
            case 19: return SKColors.DarkSlateBlue;
            case 20: return SKColors.DarkSlateGray;
            case 21: return SKColors.DarkViolet;
            case 22: return SKColors.DeepPink;
            case 23: return SKColors.DeepSkyBlue;
            case 24: return SKColors.DimGray;
            case 25: return SKColors.DodgerBlue;
            case 26: return SKColors.Firebrick;
            case 27: return SKColors.ForestGreen;
            case 28: return SKColors.Fuchsia;
            case 29: return SKColors.Gray;
            case 30: return SKColors.Green;
            case 31: return SKColors.HotPink;
            case 32: return SKColors.IndianRed;
            case 33: return SKColors.Indigo;
            case 34: return SKColors.Magenta;
            case 35: return SKColors.Maroon;
            case 36: return SKColors.MediumPurple;
            case 37: return SKColors.MediumSlateBlue;
            case 38: return SKColors.MediumVioletRed;
            case 39: return SKColors.MidnightBlue;
            case 40: return SKColors.Navy;
            case 41: return SKColors.Olive;
            case 42: return SKColors.OliveDrab;
            case 43: return SKColors.Orange;
            case 44: return SKColors.OrangeRed;
            case 45: return SKColors.Orchid;
            case 46: return SKColors.PaleVioletRed;
            case 47: return SKColors.Peru;
            case 48: return SKColors.Plum;
            case 49: return SKColors.PowderBlue;
            case 50: return SKColors.Purple;
            case 51: return SKColors.RosyBrown;
            case 52: return SKColors.RoyalBlue;
            case 53: return SKColors.SaddleBrown;
            case 54: return SKColors.SeaGreen;
            case 55: return SKColors.Sienna;
            case 56: return SKColors.SkyBlue;
            case 57: return SKColors.SlateBlue;
            case 58: return SKColors.SlateGray;
            case 59: return SKColors.SteelBlue;
            case 60: return SKColors.Teal;
            case 61: return SKColors.Thistle;
            case 62: return SKColors.Tomato;
            default: throw new Exception("Random error");
        }
    }
}