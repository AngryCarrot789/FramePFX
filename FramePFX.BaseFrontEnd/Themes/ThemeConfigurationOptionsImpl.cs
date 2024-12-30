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

using System.Xml;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using FramePFX.Persistence;
using FramePFX.Persistence.Serialisation;
using FramePFX.Themes;
using FramePFX.Utils;
using SkiaSharp;

namespace FramePFX.BaseFrontEnd.Themes;

public class ThemeConfigurationOptionsImpl : ThemeConfigurationOptions {
    public static readonly PersistentProperty<List<ThemeOptions>?> ThemesProperty = PersistentProperty.RegisterCustom<List<ThemeOptions>?, ThemeConfigurationOptionsImpl>("ThemeList", null, x => x.myThemeList, (x, y) => x.myThemeList = y, new ThemeListSerializer());

    private List<ThemeOptions>? myThemeList;

    public List<ThemeOptions>? ThemeList {
        get => ThemesProperty.GetValue(this);
        set => ThemesProperty.SetValue(this, value);
    }

    public ThemeConfigurationOptionsImpl() {
        
    }

    public override void OnLoaded() {
        this.LoadModelsIntoThemes(ThemeManager.Instance);
    }

    public override void SaveThemesToModels(ThemeManager themes) {
        List<ThemeOptions> list = new List<ThemeOptions>();
        foreach (Theme theme in themes.Themes) {
            if (theme.BasedOn == null) {
                continue;
            }

            ThemeOptions options = new ThemeOptions(theme.Name, theme.BasedOn.Name);
            foreach (string key in theme.ThemeKeys) {
                if (((ThemeManagerImpl.ThemeImpl) theme).TryFindBrushInHierarchy(key, out IBrush? brush)) {
                    options.brushes[key] = brush.ToImmutable();
                }
            }

            list.Add(options);
        }

        this.ThemeList = list;
    }

    public override void LoadModelsIntoThemes(ThemeManager manager) {
        if (this.ThemeList is List<ThemeOptions> list) {
            foreach (ThemeOptions options in list) {
                Theme? theme = manager.GetTheme(options.themeName);
                if (theme == null) {
                    Theme? basedOn = manager.GetTheme(options.basedOn);
                    if (basedOn == null) {
                        throw new Exception($"Theme ordering error: could not find basedOn theme '{options.basedOn}' before {options.themeName}");
                    }

                    theme = manager.RegisterTheme(options.themeName, basedOn, false);
                }

                foreach (KeyValuePair<string, IImmutableBrush> pair in options.brushes) {
                    ((ThemeManagerImpl.ThemeImpl) theme).SetBrushInternal(pair.Key, pair.Value);
                }
            }
        }
    }

    /// <summary>
    /// Represents a theme in serialized form
    /// </summary>
    public class ThemeOptions {
        public readonly Dictionary<string, IImmutableBrush> brushes;
        public readonly string themeName;
        public readonly string basedOn;

        public ThemeOptions(string themeName, string basedOn) {
            Validate.NotNullOrWhiteSpaces(themeName);
            Validate.NotNullOrWhiteSpaces(basedOn);

            this.themeName = themeName;
            this.basedOn = basedOn;
            this.brushes = new Dictionary<string, IImmutableBrush>();
        }
    }

    private class ThemeListSerializer : IValueSerializer<List<ThemeOptions>?> {
        public bool Serialize(List<ThemeOptions>? value, XmlDocument document, XmlElement parent) {
            if (value == null) {
                return false;
            }

            foreach (ThemeOptions option in value) {
                XmlElement theme = (XmlElement) parent.AppendChild(document.CreateElement("Theme"))!;
                theme.SetAttribute("name", option.themeName);
                theme.SetAttribute("basedOn", option.basedOn);
                foreach (KeyValuePair<string, IImmutableBrush> pair in option.brushes) {
                    XmlElement brush = (XmlElement) theme.AppendChild(document.CreateElement("Brush"))!;
                    brush.SetAttribute("name", pair.Key);
                    BrushSerializer.Instance.Serialize(pair.Value, document, brush);
                }
            }

            return true;
        }

        public List<ThemeOptions>? Deserialize(XmlElement element) {
            List<ThemeOptions> list = new List<ThemeOptions>();
            foreach (XmlElement theme in element.GetElementsByTagName("Theme").OfType<XmlElement>()) {
                ThemeOptions options = new ThemeOptions(theme.GetAttribute("name"), theme.GetAttribute("basedOn"));
                foreach (XmlElement brush in theme.GetElementsByTagName("Brush").OfType<XmlElement>()) {
                    IImmutableBrush theBrush = BrushSerializer.Instance.Deserialize(brush);
                    options.brushes[brush.GetAttribute("name")] = theBrush;
                }

                list.Add(options);
            }

            return list;
        }
    }

    private class BrushSerializer : IValueSerializer<IImmutableBrush> {
        public static readonly BrushSerializer Instance = new BrushSerializer();

        private const string SolidColorBrush_AttributeName = "solid_argb";

        public bool Serialize(IImmutableBrush value, XmlDocument document, XmlElement parent) {
            if (value is IImmutableSolidColorBrush solid) {
                Color c = solid.Color;
                SKColor skColour = new SKColor(c.R, c.G, c.B, c.A);
                parent.SetAttribute(SolidColorBrush_AttributeName, skColour.ToString());
                return true;
            }
            else {
                throw new Exception($"Cannot serialize {value.GetType().FullName}");
            }
        }

        public IImmutableBrush Deserialize(XmlElement element) {
            if (element.HasAttribute(SolidColorBrush_AttributeName)) {
                SKColor c = SKColor.Parse(element.GetAttribute(SolidColorBrush_AttributeName));
                return new ImmutableSolidColorBrush(new Color(c.Alpha, c.Red, c.Green, c.Blue));
            }
            else {
                throw new Exception("Invalid brush to deserialize");
            }
        }
    }
}