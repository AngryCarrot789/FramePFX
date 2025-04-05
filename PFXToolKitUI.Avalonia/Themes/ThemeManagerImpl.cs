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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using PFXToolKitUI.Avalonia.Themes.BrushFactories;
using PFXToolKitUI.Logging;
using PFXToolKitUI.Themes;
using PFXToolKitUI.Utils;
using PFXToolKitUI.Utils.Collections.Observable;
using SkiaSharp;

namespace PFXToolKitUI.Avalonia.Themes;

public class ThemeManagerImpl : ThemeManager {
    private readonly ObservableList<Theme> themes;
    private Theme myActiveTheme = null!;

    public Application App { get; }

    public ResourceDictionary ApplicationResources => (ResourceDictionary) this.App.Resources;

    public IDictionary<ThemeVariant, IThemeVariantProvider> ThemeDictionaries => this.ApplicationResources.ThemeDictionaries;

    public override ReadOnlyObservableList<Theme> Themes { get; }

    public override Theme ActiveTheme => this.myActiveTheme;

    public ThemeManagerImpl(Application app) {
        this.App = app;
        this.App.ActualThemeVariantChanged += this.OnActiveVariantChanged;
        this.themes = new ObservableList<Theme>();
        this.Themes = new ReadOnlyObservableList<Theme>(this.themes);
    }

    private void OnActiveVariantChanged(object? sender, EventArgs e) {
        this.myActiveTheme = this.GetThemeByVariant(this.App.ActualThemeVariant) ?? throw new Exception("Active theme variant is invalid");
    }

    public Theme? GetThemeByVariant(ThemeVariant variant) {
        foreach (Theme theme in this.themes) {
            if (((ThemeImpl) theme).Variant == variant) {
                return theme;
            }
        }

        return null;
    }

    public void SetupBuiltInThemes() {
        ApplicationPFX.Instance.EnsureBeforePhase(ApplicationStartupPhase.Running);

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

        this.App.RequestedThemeVariant = impl.Variant;
    }

    public override Theme RegisterTheme(string name, Theme basedOn, bool copyAllKeys = false) {
        Validate.NotNullOrWhiteSpaces(name);
        Validate.NotNull(basedOn);
        if (this.GetTheme(name) != null) {
            throw new InvalidOperationException($"Theme already exists with the name '{name}'");
        }

        ThemeImpl newTheme = new ThemeImpl(this, name, new ResourceDictionary(), (ThemeImpl) basedOn);
        if (copyAllKeys) {
            newTheme.CopyKeysFrom(newTheme.BasedOn!);
        }

        this.themes.Add(newTheme);
        this.ThemeDictionaries[newTheme.Variant] = newTheme.Resources;
        return newTheme;
    }

    public static bool TryFindBrushInApplicationResources(string themeKey, [NotNullWhen(true)] out IBrush? brush) {
        if (themeKey.EndsWith(ThemeImpl.ColourSuffix)) {
            themeKey = themeKey.Substring(0, themeKey.Length - ThemeImpl.ColourSuffix.Length);
        }

        if (Application.Current!.TryGetResource(themeKey, Application.Current.ActualThemeVariant, out object? value)) {
            return (brush = value as IBrush) != null;
        }

        brush = null;
        return false;
    }

    public static bool TryFindColourInApplicationResources(string themeKey, out Color colour) {
        if (!themeKey.EndsWith(ThemeImpl.ColourSuffix)) {
            themeKey += ThemeImpl.ColourSuffix;
        }

        if (Application.Current!.TryGetResource(themeKey, Application.Current.ActualThemeVariant, out object? value)) {
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

        private readonly HashSet<string> registeredBrushKeys;

        public override string Name { get; }

        public override ThemeImpl? BasedOn { get; }

        public ThemeVariant Variant { get; }

        public override ThemeManagerImpl ThemeManager { get; }

        public override IEnumerable<string> ThemeKeys => this.registeredBrushKeys;

        /// <summary>
        /// Gets the live resource dictionary that contains all the theme keys for this theme
        /// </summary>
        public ResourceDictionary Resources { get; }

        public ThemeImpl(ThemeManagerImpl manager, string name, ResourceDictionary resources, ThemeImpl? basedOn) : base(basedOn == null) {
            this.Name = name;
            this.BasedOn = basedOn;
            this.ThemeManager = manager;
            this.Variant = new ThemeVariant(name, basedOn?.Variant);
            this.Resources = resources;
            this.registeredBrushKeys = new HashSet<string>(32);
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

        public void CopyKeysFrom(ThemeImpl theme) {
            foreach (string key in theme.registeredBrushKeys) {
                if (theme.Resources.TryGetResource(key, null, out object? brush)) {
                    // assert brush is IBrush

                    this.registeredBrushKeys.Add(key);
                    if (theme.Resources.TryGetResource(key + ColourSuffix, null, out object? colour)) {
                        this.Resources[key] = colour;
                    }

                    this.Resources[key] = brush;
                }
            }
        }

        public override void SetThemeColour(string key, SKColor colour) {
            // Our theme library creates a colour and brush, because some of the UI may use both
            GetKeys(key, out string brushKey, out string colourKey);

            Color avColour = new Color(colour.Alpha, colour.Red, colour.Green, colour.Blue);
            IBrush avBrush = new ImmutableSolidColorBrush(avColour);

            this.registeredBrushKeys.Add(brushKey);
            this.Resources[colourKey] = avColour;
            this.Resources[brushKey] = avBrush;
        }

        public override void SetThemeBrush(string key, IColourBrush brush) {
            // Our theme library creates a colour and brush, because some of the UI may use both
            GetKeys(key, out string brushKey, out string colourKey);

            this.registeredBrushKeys.Add(brushKey);
            if (brush is ConstantAvaloniaColourBrush i) {
                this.Resources[colourKey] = i.Brush.Color;
                this.Resources[brushKey] = i.Brush;
            }
            else if (brush is AvaloniaColourBrush bruh) {
                if (bruh.Brush is ISolidColorBrush colourBrush) {
                    this.Resources[colourKey] = colourBrush.Color;
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

        public void SetBrushInternal(string themeKey, IImmutableBrush brush) {
            GetKeys(themeKey, out string brushKey, out string colourKey);

            this.registeredBrushKeys.Add(brushKey);
            if (brush is ISolidColorBrush scb) {
                this.Resources[colourKey] = scb.Color;
                this.Resources[brushKey] = scb;
            }
            else {
                this.Resources[brushKey] = brush;
            }
        }

        public bool TryFindBrushInHierarchy(string themeKey, [NotNullWhen(true)] out IBrush? brush) {
            if (themeKey.EndsWith(ColourSuffix)) {
                themeKey = themeKey.Substring(0, themeKey.Length - ColourSuffix.Length);
            }

            return this.TryFindObjectInHierarchy(themeKey, out brush);
        }

        public bool TryFindColourInHierarchy(string themeKey, out Color colour) {
            if (!themeKey.EndsWith(ColourSuffix)) {
                themeKey += ColourSuffix;
            }

            return this.TryFindObjectInHierarchy(themeKey, out colour);
        }

        public bool TryFindObjectInHierarchy<T>(string themeKey, [MaybeNullWhen(false)] out T val, bool canSearchHierarchy = true) {
            for (ThemeImpl? theme = this; canSearchHierarchy && theme != null; theme = theme.BasedOn) {
                // We create a new ThemeVariant without a parent because we don't need to scan theme dictionaries
                if (theme.Resources.TryGetResource(themeKey, new ThemeVariant(theme.Name, null), out object? value)) {
                    if (value is T t) {
                        val = t;
                        return true;
                    }
                }
            }

            val = default;
            return false;
        }

        public override bool IsThemeKeyValid(string themeKey, bool allowInherited = true) {
            return this.TryFindObjectInHierarchy<object>(themeKey, out _, allowInherited);
        }

        public override ISavedThemeEntry SaveThemeEntry(string themeKey) {
            this.TryFindBrushInHierarchy(themeKey, out IBrush? brush);
            Color? col = this.TryFindColourInHierarchy(themeKey, out Color colour) ? colour : null;
            return new SavedThemeEntryImpl(this, brush?.ToImmutable(), col);
        }

        public override void ApplyThemeEntry(string themeKey, ISavedThemeEntry entry) {
            SavedThemeEntryImpl impl = (SavedThemeEntryImpl) entry;
            GetKeys(themeKey, out string brushKey, out string colourKey);

            if (impl.Colour.HasValue)
                this.Resources[colourKey] = impl.Colour.Value;

            if (impl.Brush != null) {
                this.Resources[brushKey] = impl.Brush;
                this.registeredBrushKeys.Add(brushKey);
            }
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
            ApplicationPFX.Instance.EnsureBeforePhase(ApplicationStartupPhase.Running);
            foreach (object key in this.Resources.Keys) {
                if (key is string keyString) {
                    // Only load brush keys. We do colours along the side
                    if (!keyString.EndsWith(ColourSuffix)) {
                        this.registeredBrushKeys.Add(keyString);
                    }
                }
            }
        }
    }
}