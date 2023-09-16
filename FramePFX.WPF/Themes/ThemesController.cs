using System;
using System.Windows;
using System.Windows.Media;

namespace FramePFX.WPF.Themes {
    public static class ThemesController {
        public static void SetTheme(ThemeType theme) {
            string themeName = theme.GetName();
            if (string.IsNullOrEmpty(themeName)) {
                return;
            }

            App.CurrentTheme = theme;
            App.ThemeDictionary = new ResourceDictionary() {Source = new Uri($"Themes/ColourDictionaries/{themeName}.xaml", UriKind.Relative)};
            App.ControlColours = new ResourceDictionary() {Source = new Uri("Themes/ControlColours.xaml", UriKind.Relative)};
            // App.Controls = new ResourceDictionary() { Source = new Uri("Themes/Controls.xaml", UriKind.Relative) };
            App.RefreshControlsDictionary();
        }

        public static object GetResource(object key) => App.ThemeDictionary[key];

        public static SolidColorBrush GetBrush(string name) => GetResource(name) is SolidColorBrush brush ? brush : new SolidColorBrush(Colors.White);
    }
}