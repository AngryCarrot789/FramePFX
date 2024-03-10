// 
// Copyright (c) 2023-2024 REghZy
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

using System;
using System.Windows;
using System.Windows.Media;

namespace FramePFX.Themes
{
    public class ThemeController
    {
        public static ThemeController Instance { get; } = new ThemeController();

        public static void SetTheme(ThemeType theme)
        {
            string themeName = theme.GetName();
            if (string.IsNullOrEmpty(themeName))
            {
                return;
            }

            CurrentTheme = theme;
            ThemeDictionary = new ResourceDictionary() {Source = new Uri($"Themes/ColourDictionaries/{themeName}.xaml", UriKind.Relative)};
            ControlColours = new ResourceDictionary() {Source = new Uri("Themes/ControlColours.xaml", UriKind.Relative)};
            // App.Controls = new ResourceDictionary() { Source = new Uri("Themes/Controls.xaml", UriKind.Relative) };
            RefreshControlsDictionary();
        }

        public static void RefreshControlsDictionary()
        {
            ResourceDictionary resources = Application.Current.Resources;
            ResourceDictionary resource = resources.MergedDictionaries[2];
            resources.MergedDictionaries.RemoveAt(2);
            resources.MergedDictionaries.Insert(2, resource);
        }

        public static object GetResource(object key) => ThemeDictionary[key];

        public static SolidColorBrush GetBrush(string name) => GetResource(name) is SolidColorBrush brush ? brush : new SolidColorBrush(Colors.White);
        public static ThemeType CurrentTheme { get; set; }

        public static ResourceDictionary ThemeDictionary
        {
            get => Application.Current.Resources.MergedDictionaries[0];
            set => Application.Current.Resources.MergedDictionaries[0] = value;
        }

        public static ResourceDictionary ControlColours
        {
            get => Application.Current.Resources.MergedDictionaries[1];
            set => Application.Current.Resources.MergedDictionaries[1] = value;
        }

        public static ResourceDictionary I18NText
        {
            get => Application.Current.Resources.MergedDictionaries[3];
            set => Application.Current.Resources.MergedDictionaries[3] = value;
        }
    }
}