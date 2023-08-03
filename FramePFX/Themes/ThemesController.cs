using System;
using System.Windows;
using System.Windows.Media;

namespace FramePFX.Themes {
    public static class ThemesController {
        public static ThemeType CurrentTheme { get; set; }

        public static ResourceDictionary ThemeDictionary {
            get => Application.Current.Resources.MergedDictionaries[0];
            set => Application.Current.Resources.MergedDictionaries[0] = value;
        }

        public static ResourceDictionary ControlColours {
            get => Application.Current.Resources.MergedDictionaries[1];
            set => Application.Current.Resources.MergedDictionaries[1] = value;
        }

        public static void RefreshControls() {
            ResourceDictionary resource = Application.Current.Resources.MergedDictionaries[2];
            Application.Current.Resources.MergedDictionaries.RemoveAt(2);
            Application.Current.Resources.MergedDictionaries.Insert(2, resource);
        }

        public static void SetTheme(ThemeType theme) {
            string themeName = theme.GetName();
            if (string.IsNullOrEmpty(themeName)) {
                return;
            }

            CurrentTheme = theme;
            ThemeDictionary = new ResourceDictionary() { Source = new Uri($"Themes/ColourDictionaries/{themeName}.xaml", UriKind.Relative) };
            ControlColours = new ResourceDictionary() { Source = new Uri("Themes/ControlColours.xaml", UriKind.Relative) };
            // App.Controls = new ResourceDictionary() { Source = new Uri("Themes/Controls.xaml", UriKind.Relative) };
            RefreshControls();
        }

        public static object GetResource(object key) => ThemeDictionary[key];

        public static SolidColorBrush GetBrush(string name) => GetResource(name) is SolidColorBrush brush ? brush : new SolidColorBrush(Colors.White);
    }
}