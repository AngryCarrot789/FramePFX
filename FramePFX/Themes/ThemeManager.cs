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

using FramePFX.Themes.Configurations;
using SkiaSharp;

namespace FramePFX.Themes;

public abstract class ThemeManager {
    public static ThemeManager Instance => Application.Instance.ServiceManager.GetService<ThemeManager>();

    /// <summary>
    /// Gets the themes that currently exist
    /// </summary>
    public abstract IEnumerable<Theme> Themes { get; }

    /// <summary>
    /// Gets the theme that is currently active
    /// </summary>
    public abstract Theme ActiveTheme { get; }

    public ThemeConfigurationPage ThemeConfigurationPage { get; }
    
    /// <summary>
    /// An event fired when a colour changes in the active theme
    /// </summary>
    public event ThemeColourChangedEventHandler? ActiveThemeColourChanged;

    protected ThemeManager() {
        this.ThemeConfigurationPage = new ThemeConfigurationPage();
        // Standard theme options across all application based on this framework
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Foreground (Static)",                         "ABrush.Foreground.Static");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Foreground (Deeper)",                         "ABrush.Foreground.Deeper");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Foreground (Disabled)",                       "ABrush.Foreground.Disabled");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Glyphs/Glyph (Static)",                       "ABrush.Glyph.Static");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Glyphs/Glyph (Disabled)",                     "ABrush.Glyph.Disabled");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Glyphs/Glyph (MouseOver)",                    "ABrush.Glyph.MouseOver");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Glyphs/Glyph (MouseDown)",                    "ABrush.Glyph.MouseDown");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Glyphs/Glyph (Selected)",                     "ABrush.Glyph.Selected");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Glyphs/Glyph (Selected Inactive)",            "ABrush.Glyph.Selected.Inactive");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Glyphs/ColourfulGlyph (Static)",              "ABrush.ColourfulGlyph.Static");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Glyphs/ColourfulGlyph (Disabled)",            "ABrush.ColourfulGlyph.Disabled");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Glyphs/ColourfulGlyph (MouseOver)",           "ABrush.ColourfulGlyph.MouseOver");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Glyphs/ColourfulGlyph (MouseDown)",           "ABrush.ColourfulGlyph.MouseDown");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Glyphs/ColourfulGlyph (Selected)",            "ABrush.ColourfulGlyph.Selected");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Glyphs/ColourfulGlyph (Selected Inactive)",   "ABrush.ColourfulGlyph.Selected.Inactive");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/AccentTone1/Background (Static)",             "ABrush.AccentTone1.Background.Static");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/AccentTone1/Border (Static)",                 "ABrush.AccentTone1.Border.Static");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/AccentTone2/Background (Static)",             "ABrush.AccentTone2.Background.Static");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/AccentTone2/Border (Static)",                 "ABrush.AccentTone2.Border.Static");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/AccentTone3/Background (Static)",             "ABrush.AccentTone3.Background.Static");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/AccentTone3/Border (Static)",                 "ABrush.AccentTone3.Border.Static");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone0 Background (Static)",            "ABrush.Tone0.Background.Static");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone0 Border (Static)",                "ABrush.Tone0.Border.Static");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone1 Background (Static)",            "ABrush.Tone1.Background.Static");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone1 Border (Static)",                "ABrush.Tone1.Border.Static");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone2 Background (Static)",            "ABrush.Tone2.Background.Static");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone2 Border (Static)",                "ABrush.Tone2.Border.Static");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone3 Background (Static)",            "ABrush.Tone3.Background.Static");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone3 Border (Static)",                "ABrush.Tone3.Border.Static");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone4 Background (Static)",            "ABrush.Tone4.Background.Static");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone4 Border (Static)",                "ABrush.Tone4.Border.Static");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone4 Background (MouseOver)",         "ABrush.Tone4.Background.MouseOver");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone4 Border (MouseOver)",             "ABrush.Tone4.Border.MouseOver");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone4 Background (MouseDown)",         "ABrush.Tone4.Background.MouseDown");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone4 Border (MouseDown)",             "ABrush.Tone4.Border.MouseDown");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone4 Background (Selected)",          "ABrush.Tone4.Background.Selected");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone4 Background (Selected Inactive)", "ABrush.Tone4.Background.Selected.Inactive");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone4 Border (Selected)",              "ABrush.Tone4.Border.Selected");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone4 Border (Selected Inactive)",     "ABrush.Tone4.Border.Selected.Inactive");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone4 Background (Disabled)",          "ABrush.Tone4.Background.Disabled");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone4 Border (Disabled)",              "ABrush.Tone4.Border.Disabled");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone5 Background (Static)",            "ABrush.Tone5.Background.Static");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone5 Border (Static)",                "ABrush.Tone5.Border.Static");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone5 Background (MouseOver)",         "ABrush.Tone5.Background.MouseOver");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone5 Border (MouseOver)",             "ABrush.Tone5.Border.MouseOver");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone5 Background (MouseDown)",         "ABrush.Tone5.Background.MouseDown");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone5 Border (MouseDown)",             "ABrush.Tone5.Border.MouseDown");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone5 Background (Selected)",          "ABrush.Tone5.Background.Selected");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone5 Background (Selected Inactive)", "ABrush.Tone5.Background.Selected.Inactive");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone5 Border (Selected)",              "ABrush.Tone5.Border.Selected");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone5 Border (Selected Inactive)",     "ABrush.Tone5.Border.Selected.Inactive");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone5 Background (Disabled)",          "ABrush.Tone5.Background.Disabled");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone5 Border (Disabled)",              "ABrush.Tone5.Border.Disabled");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone6 Background (Static)",            "ABrush.Tone6.Background.Static");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone6 Border (Static)",                "ABrush.Tone6.Border.Static");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone6 Background (MouseOver)",         "ABrush.Tone6.Background.MouseOver");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone6 Border (MouseOver)",             "ABrush.Tone6.Border.MouseOver");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone6 Background (MouseDown)",         "ABrush.Tone6.Background.MouseDown");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone6 Border (MouseDown)",             "ABrush.Tone6.Border.MouseDown");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone6 Background (Selected)",          "ABrush.Tone6.Background.Selected");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone6 Background (Selected Inactive)", "ABrush.Tone6.Background.Selected.Inactive");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone6 Border (Selected)",              "ABrush.Tone6.Border.Selected");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone6 Border (Selected Inactive)",     "ABrush.Tone6.Border.Selected.Inactive");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone6 Background (Disabled)",          "ABrush.Tone6.Background.Disabled");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone6 Border (Disabled)",              "ABrush.Tone6.Border.Disabled");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone7 Background (Static)",            "ABrush.Tone7.Background.Static");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone7 Border (Static)",                "ABrush.Tone7.Border.Static");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone7 Background (MouseOver)",         "ABrush.Tone7.Background.MouseOver");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone7 Border (MouseOver)",             "ABrush.Tone7.Border.MouseOver");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone7 Background (MouseDown)",         "ABrush.Tone7.Background.MouseDown");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone7 Border (MouseDown)",             "ABrush.Tone7.Border.MouseDown");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone7 Background (Selected)",          "ABrush.Tone7.Background.Selected");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone7 Background (Selected Inactive)", "ABrush.Tone7.Background.Selected.Inactive");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone7 Border (Selected)",              "ABrush.Tone7.Border.Selected");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone7 Border (Selected Inactive)",     "ABrush.Tone7.Border.Selected.Inactive");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone7 Background (Disabled)",          "ABrush.Tone7.Background.Disabled");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone7 Border (Disabled)",              "ABrush.Tone7.Border.Disabled");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone8 Background (Static)",            "ABrush.Tone8.Background.Static");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone8 Border (Static)",                "ABrush.Tone8.Border.Static");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone8 Background (MouseOver)",         "ABrush.Tone8.Background.MouseOver");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone8 Border (MouseOver)",             "ABrush.Tone8.Border.MouseOver");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone8 Background (MouseDown)",         "ABrush.Tone8.Background.MouseDown");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone8 Border (MouseDown)",             "ABrush.Tone8.Border.MouseDown");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone8 Background (Selected)",          "ABrush.Tone8.Background.Selected");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone8 Background (Selected Inactive)", "ABrush.Tone8.Background.Selected.Inactive");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone8 Border (Selected)",              "ABrush.Tone8.Border.Selected");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone8 Border (Selected Inactive)",     "ABrush.Tone8.Border.Selected.Inactive");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone8 Background (Disabled)",          "ABrush.Tone8.Background.Disabled");
        this.ThemeConfigurationPage.AssignMapping("Standard2020/Panels/Tone8 Border (Disabled)",              "ABrush.Tone8.Border.Disabled");
        
        // FramePFX specific
        this.ThemeConfigurationPage.AssignMapping("VideoEditor/Status Bar Background",                                          "ABrush.PFX.Editor.StatusBar.Background");
        this.ThemeConfigurationPage.AssignMapping("VideoEditor/Timeline/Background (Static)",                                   "ABrush.PFX.Editor.Timeline.Background");
        this.ThemeConfigurationPage.AssignMapping("VideoEditor/Timeline/Gap Between Tracks",                                    "ABrush.PFX.Editor.Timeline.GapBetweenTracks");
        this.ThemeConfigurationPage.AssignMapping("VideoEditor/Timeline/Timestamp Background",                                  "ABrush.PFX.Editor.Timeline.TimestampBoard.Background");
        this.ThemeConfigurationPage.AssignMapping("VideoEditor/Timeline/Toolbar Background",                                    "ABrush.PFX.Editor.Timeline.ToolBar.Background");
        this.ThemeConfigurationPage.AssignMapping("VideoEditor/Timeline/Ruler Background",                                      "ABrush.PFX.Editor.Timeline.Ruler.Background");
        this.ThemeConfigurationPage.AssignMapping("VideoEditor/Timeline/Header Background",                                     "ABrush.PFX.Editor.Timeline.Header.Background");
        this.ThemeConfigurationPage.AssignMapping("VideoEditor/Timeline/Track/Background (Static)",                             "ABrush.PFX.Editor.Timeline.Track.Background");
        this.ThemeConfigurationPage.AssignMapping("VideoEditor/Timeline/Track/Background (Selected)",                           "ABrush.PFX.Editor.Timeline.Track.Background.Selected");
        this.ThemeConfigurationPage.AssignMapping("VideoEditor/Timeline/Control Surface/Background (Static)",                   "ABrush.PFX.Editor.Timeline.ControlSurface.Background");
        this.ThemeConfigurationPage.AssignMapping("VideoEditor/Timeline/Control Surface/Item Background (Static)",              "ABrush.PFX.Editor.Timeline.ControlSurfaceItem.Background");
        this.ThemeConfigurationPage.AssignMapping("VideoEditor/Timeline/Control Surface/Item Background (Mouse Over)",          "ABrush.PFX.Editor.Timeline.ControlSurfaceItem.Background.MouseOver");
        this.ThemeConfigurationPage.AssignMapping("VideoEditor/Timeline/Control Surface/Item Background (Selected, Focused)",   "ABrush.PFX.Editor.Timeline.ControlSurfaceItem.Background.SelectedFocused");
        this.ThemeConfigurationPage.AssignMapping("VideoEditor/Timeline/Control Surface/Item Background (Selected, Unfocused)", "ABrush.PFX.Editor.Timeline.ControlSurfaceItem.Background.SelectedUnfocused");
        this.ThemeConfigurationPage.AssignMapping("VideoEditor/Resource Manager/Tab Strip Background",                          "ABrush.PFX.Editor.ResourceManager.TabStrip.Background");
        this.ThemeConfigurationPage.AssignMapping("VideoEditor/Resource Manager/Tab Item Background",                           "ABrush.PFX.Editor.ResourceManager.TabItem.Background");
        this.ThemeConfigurationPage.AssignMapping("VideoEditor/Resource Manager/List Background",                               "ABrush.PFX.Editor.ResourceManager.List.Background");
        this.ThemeConfigurationPage.AssignMapping("VideoEditor/Resource Manager/Tree Background",                               "ABrush.PFX.Editor.ResourceManager.Tree.Background");
    }

    /// <summary>
    /// Sets the current application theme
    /// </summary>
    /// <param name="themeName"></param>
    public abstract void SetTheme(Theme theme);

    /// <summary>
    /// Creates a new theme that is based on the given theme
    /// </summary>
    /// <param name="name">The new name of the theme</param>
    /// <param name="basedOn">A theme whose colours are copied into the new one</param>
    /// <returns></returns>
    public abstract Theme RegisterTheme(string name, Theme basedOn);
    
    protected void OnActiveThemeColourChanged(Theme theme, string key, SKColor newColour) => this.ActiveThemeColourChanged?.Invoke(theme, key, newColour);
}