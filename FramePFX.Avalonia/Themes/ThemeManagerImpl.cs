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
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using FramePFX.Logging;
using FramePFX.Themes;
using SkiaSharp;

namespace FramePFX.Avalonia.Themes;

public class ThemeManagerImpl : ThemeManager {
    private readonly List<ThemeImpl> themes;
    private Theme myActiveTheme = null!;

    public App App { get; }

    public ResourceDictionary ApplicationResources => (ResourceDictionary) this.App.Resources;

    public IDictionary<ThemeVariant, IThemeVariantProvider> ThemeDictionaries => this.ApplicationResources.ThemeDictionaries;

    public override IEnumerable<Theme> Themes => this.themes;

    public override Theme ActiveTheme => this.myActiveTheme;

    public ThemeManagerImpl(App app) {
        this.App = app;
        this.App.ActualThemeVariantChanged += this.OnActiveVariantChanged;
        this.themes = new List<ThemeImpl>();
    }

    private void OnActiveVariantChanged(object? sender, EventArgs e) {
        this.myActiveTheme = this.GetThemeByVariant(this.App.ActualThemeVariant) ?? throw new Exception("Active theme variant is invalid");
    }

    public Theme? GetThemeByVariant(ThemeVariant variant) {
        foreach (ThemeImpl theme in this.themes) {
            if (theme.Variant == variant) {
                return theme;
            }
        }

        return null;
    }

    public void SetupBuiltInThemes() {
        Application.Instance.EnsureBeforePhase(ApplicationStartupPhase.Running);

        foreach (KeyValuePair<ThemeVariant, IThemeVariantProvider> entry in this.ThemeDictionaries) {
            string? themeName = entry.Key.Key.ToString();
            if (themeName == null) {
                continue;
            }

            if (!(entry.Value is ResourceDictionary dictionary)) {
                continue;
            }

            ThemeImpl theme = new ThemeImpl(this, themeName, dictionary, null);
            theme.LoadKeysFromDictionary();
            this.themes.Add(theme);
        }

        this.myActiveTheme = this.GetThemeByVariant(this.App.ActualThemeVariant) ?? throw new Exception("Active theme variant is invalid");
    }

    public override void SetTheme(Theme theme) {
        if (!(theme is ThemeImpl impl) || !this.themes.Contains(impl))
            throw new InvalidOperationException("Theme is not registered");

        this.App.RequestedThemeVariant = new ThemeVariant(impl.Name, null);
    }

    public override Theme RegisterTheme(string name, Theme basedOn) {
        ThemeImpl newTheme = new ThemeImpl(this, name, new ResourceDictionary(), (ThemeImpl) basedOn);
        this.themes.Add(newTheme);
        this.ThemeDictionaries[newTheme.Variant] = newTheme.Resources;
        return newTheme;
    }

    private void OnColourChanged(string themeKey, SKColor newColour) {
        this.OnActiveThemeColourChanged(this.ActiveTheme, themeKey, newColour);
    }

    public class ThemeImpl : Theme {
        public const string ColourSuffix = ".Color";

        private readonly HashSet<string> registeredKeys;

        public override string Name { get; }

        public ThemeImpl? BasedOn { get; }

        public ThemeVariant Variant { get; }

        public override ThemeManagerImpl ThemeManager { get; }

        public override IEnumerable<string> ThemeKeys => this.registeredKeys;

        /// <summary>
        /// Gets the live resource dictionary that contains all the theme keys for this theme
        /// </summary>
        public ResourceDictionary Resources { get; }

        public ThemeImpl(ThemeManagerImpl manager, string name, ResourceDictionary resources, ThemeImpl? basedOn) {
            this.Name = name;
            this.BasedOn = basedOn;
            this.ThemeManager = manager;
            this.Variant = new ThemeVariant(name, basedOn?.Variant);
            this.Resources = resources;
            this.registeredKeys = new HashSet<string>(32);
        }

        public override void SetThemeColour(string key, SKColor colour) {
            // Our theme library creates a colour and brush, because some of the UI may use both
            string colourKey, brushKey;
            if (key.EndsWith(ColourSuffix)) {
                colourKey = key;
                brushKey = key.Substring(0, key.Length - ColourSuffix.Length);
                AppLogger.Instance.WriteLine("Attempt to set theme colour using '.Color' suffix. This should not be done");
            }
            else {
                colourKey = key + ColourSuffix;
                brushKey = key;
            }

            Color avColour = new Color(colour.Alpha, colour.Red, colour.Green, colour.Blue);
            IBrush avBrush = new ImmutableSolidColorBrush(avColour);

            this.registeredKeys.Add(brushKey);
            this.Resources[colourKey] = avColour;
            this.Resources[brushKey] = avBrush;
            if (this.ThemeManager.ActiveTheme == this)
                this.ThemeManager.OnColourChanged(brushKey, colour);
        }

        public void LoadKeysFromDictionary() {
            Application.Instance.EnsureBeforePhase(ApplicationStartupPhase.Running);
            foreach (object key in this.Resources.Keys) {
                if (key is string keyString) {
                    // Only load brush keys. We do colours along the side
                    if (!keyString.EndsWith(ColourSuffix)) {
                        this.registeredKeys.Add(keyString);
                    }
                }
            }
        }
    }
}