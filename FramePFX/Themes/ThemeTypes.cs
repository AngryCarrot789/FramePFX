using System;

namespace FramePFX.Themes {
    public enum ThemeType {
        SoftDark,
        SoftDarkAndBlue,
        RedBlackTheme,
        DeepDark,
        GreyTheme
    }

    public static class ThemeTypeExtension {
        public static string GetName(this ThemeType type) {
            switch (type) {
                case ThemeType.SoftDark:        return "SoftDark";
                case ThemeType.SoftDarkAndBlue: return "SoftDarkAndBlue";
                case ThemeType.RedBlackTheme:   return "RedBlackTheme";
                case ThemeType.DeepDark:        return "DeepDark";
                case ThemeType.GreyTheme:       return "GreyTheme";
                default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}