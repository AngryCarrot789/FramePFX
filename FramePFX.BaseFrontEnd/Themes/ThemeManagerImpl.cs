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

using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Skia;
using Avalonia.Styling;
using FramePFX.BaseFrontEnd.Themes.BrushFactories;
using FramePFX.Logging;
using FramePFX.Themes;
using SkiaSharp;

namespace FramePFX.Avalonia.Themes;

public class ThemeManagerImpl : ThemeManager {
    private readonly List<ThemeImpl> themes;
    private Theme myActiveTheme = null!;

    public global::Avalonia.Application App { get; }

    public ResourceDictionary ApplicationResources => (ResourceDictionary) this.App.Resources;

    public IDictionary<ThemeVariant, IThemeVariantProvider> ThemeDictionaries => this.ApplicationResources.ThemeDictionaries;

    public override IEnumerable<Theme> Themes => this.themes;

    public override Theme ActiveTheme => this.myActiveTheme;

    public ThemeManagerImpl(global::Avalonia.Application app) {
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
    
    public static bool TryFindBrush(string themeKey, [NotNullWhen(true)] out IBrush? brush) {
        if (themeKey.EndsWith(ThemeImpl.ColourSuffix)) {
            themeKey = themeKey.Substring(0, themeKey.Length - ThemeImpl.ColourSuffix.Length);
        }

        if (global::Avalonia.Application.Current!.TryGetResource(themeKey, global::Avalonia.Application.Current.ActualThemeVariant, out object? value)) {
            return (brush = value as IBrush) != null;
        }

        brush = null;
        return false;
    }
    
    public static bool TryFindColour(string themeKey, out Color colour) {
        if (!themeKey.EndsWith(ThemeImpl.ColourSuffix)) {
            themeKey += ThemeImpl.ColourSuffix;
        }

        if (global::Avalonia.Application.Current!.TryGetResource(themeKey, global::Avalonia.Application.Current.ActualThemeVariant, out object? value)) {
            if (value is Color c) {
                colour = c;
                return true;
            }
        }

        colour = default;
        return false;
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

        private static void GetKeys(string key, out string brushKey, out string colourKey) {
            if (key.EndsWith(ColourSuffix)) {
                colourKey = key;
                brushKey = key.Substring(0, key.Length - ColourSuffix.Length);
                AppLogger.Instance.WriteLine("Attempt to set theme colour using '.Color' suffix. This should not be done");
            }
            else {
                colourKey = key + ColourSuffix;
                brushKey = key;
            }
        }

        public override void SetThemeColour(string key, SKColor colour) {
            // Our theme library creates a colour and brush, because some of the UI may use both
            GetKeys(key, out string brushKey, out string colourKey);

            Color avColour = new Color(colour.Alpha, colour.Red, colour.Green, colour.Blue);
            IBrush avBrush = new ImmutableSolidColorBrush(avColour);

            this.registeredKeys.Add(brushKey);
            this.Resources[colourKey] = avColour;
            this.Resources[brushKey] = avBrush;
            if (this.ThemeManager.ActiveTheme == this)
                this.ThemeManager.OnColourChanged(brushKey, colour);
        }

        public override void SetThemeBrush(string key, IColourBrush brush) {
            // Our theme library creates a colour and brush, because some of the UI may use both
            GetKeys(key, out string brushKey, out string colourKey);

            this.registeredKeys.Add(brushKey);
            if (brush is ConstantAvaloniaColourBrush i) {
                SKColor colour = i.Brush.Color.ToSKColor();
                this.Resources[colourKey] = colour;
                this.Resources[brushKey] = i.Brush;
                if (this.ThemeManager.ActiveTheme == this)
                    this.ThemeManager.OnColourChanged(brushKey, colour);
            }
            else if (brush is AvaloniaColourBrush bruh) {
                if (bruh.Brush is ISolidColorBrush colourBrush) {
                    this.Resources[colourKey] = colourBrush.Color.ToSKColor();
                }
                else {
                    AppLogger.Instance.WriteLine($"Could not update colour value for theme key '{key}', because the brush was not solid");
                }
                
                this.Resources[brushKey] = bruh.Brush;
            }
            else {
                return;
            }
        }

        public override bool IsThemeKeyValid(string themeKey) {
            return TryFindBrush(themeKey, out _);
        }

        public override ISavedThemeEntry SaveThemeEntry(string themeKey) {
            TryFindBrush(themeKey, out IBrush? brush);
            TryFindColour(themeKey, out Color colour);
            return new SavedThemeEntryImpl(this, brush?.ToImmutable(), colour);
        }

        public override void RestoreThemeEntry(string themeKey, ISavedThemeEntry entry) {
            SavedThemeEntryImpl impl = (SavedThemeEntryImpl) entry;
            SKColor? colour = impl.Colour?.ToSKColor();

            GetKeys(themeKey, out string brushKey, out string colourKey);
            
            if (colour.HasValue)
                this.Resources[colourKey] = colour.Value;
            if (impl.Brush != null)
                this.Resources[brushKey] = impl.Brush;
            
            if (this.ThemeManager.ActiveTheme == this && colour.HasValue)
                this.ThemeManager.OnColourChanged(brushKey, colour.Value);
        }

        private class SavedThemeEntryImpl : ISavedThemeEntry {
            public Theme Theme { get; }

            public IImmutableBrush? Brush { get; }

            public Color? Colour { get; }

            public SavedThemeEntryImpl(ThemeImpl theme, IImmutableBrush? brush, Color? colour) {
                this.Theme = theme;
                this.Brush = brush;
                this.Colour = colour;
            }
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