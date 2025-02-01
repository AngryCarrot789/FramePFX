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
using FramePFX.Utils.Collections.Observable;

namespace FramePFX.Themes;

public abstract class ThemeManager {
    public static ThemeManager Instance => Application.Instance.ServiceManager.GetService<ThemeManager>();

    /// <summary>
    /// Gets the themes that currently exist
    /// </summary>
    public abstract ReadOnlyObservableList<Theme> Themes { get; }

    /// <summary>
    /// Gets the theme that is currently active
    /// </summary>
    public abstract Theme ActiveTheme { get; }

    public ThemeConfigurationPage ThemeConfigurationPage { get; }

    protected ThemeManager() {
        ThemeConfigurationPage p = this.ThemeConfigurationPage = new ThemeConfigurationPage();
        // Standard theme options since 2020 when WPFDarkTheme was made
        p.AssignMapping("Standard2020/Foreground (Static)",                         ThemeKeys.S20Foreground_Static);
        p.AssignMapping("Standard2020/Foreground (Deeper)",                         ThemeKeys.S20Foreground_Deeper);
        p.AssignMapping("Standard2020/Foreground (Disabled)",                       ThemeKeys.S20Foreground_Disabled);
        p.AssignMapping("Standard2020/Glyphs/Glyph (Static)",                       ThemeKeys.S20Glyph_Static);
        p.AssignMapping("Standard2020/Glyphs/Glyph (Disabled)",                     ThemeKeys.S20Glyph_Disabled);
        p.AssignMapping("Standard2020/Glyphs/Glyph (MouseOver)",                    ThemeKeys.S20Glyph_MouseOver);
        p.AssignMapping("Standard2020/Glyphs/Glyph (MouseDown)",                    ThemeKeys.S20Glyph_MouseDown);
        p.AssignMapping("Standard2020/Glyphs/Glyph (Selected)",                     ThemeKeys.S20Glyph_Selected);
        p.AssignMapping("Standard2020/Glyphs/Glyph (Selected Inactive)",            ThemeKeys.S20Glyph_Selected_Inactive);
        p.AssignMapping("Standard2020/Glyphs/ColourfulGlyph (Static)",              ThemeKeys.S20ColourfulGlyph_Static);
        p.AssignMapping("Standard2020/Glyphs/ColourfulGlyph (Disabled)",            ThemeKeys.S20ColourfulGlyph_Disabled);
        p.AssignMapping("Standard2020/Glyphs/ColourfulGlyph (MouseOver)",           ThemeKeys.S20ColourfulGlyph_MouseOver);
        p.AssignMapping("Standard2020/Glyphs/ColourfulGlyph (MouseDown)",           ThemeKeys.S20ColourfulGlyph_MouseDown);
        p.AssignMapping("Standard2020/Glyphs/ColourfulGlyph (Selected)",            ThemeKeys.S20ColourfulGlyph_Selected);
        p.AssignMapping("Standard2020/Glyphs/ColourfulGlyph (Selected Inactive)",   ThemeKeys.S20ColourfulGlyph_Selected_Inactive);
        p.AssignMapping("Standard2020/AccentTone1/Background (Static)",             ThemeKeys.S20AccentTone1_Background_Static);
        p.AssignMapping("Standard2020/AccentTone1/Border (Static)",                 ThemeKeys.S20AccentTone1_Border_Static);
        p.AssignMapping("Standard2020/AccentTone2/Background (Static)",             ThemeKeys.S20AccentTone2_Background_Static);
        p.AssignMapping("Standard2020/AccentTone2/Border (Static)",                 ThemeKeys.S20AccentTone2_Border_Static);
        p.AssignMapping("Standard2020/AccentTone3/Background (Static)",             ThemeKeys.S20AccentTone3_Background_Static);
        p.AssignMapping("Standard2020/AccentTone3/Border (Static)",                 ThemeKeys.S20AccentTone3_Border_Static);
        p.AssignMapping("Standard2020/Panels/Tone0 Background (Static)",            ThemeKeys.S20Tone0_Background_Static);
        p.AssignMapping("Standard2020/Panels/Tone0 Border (Static)",                ThemeKeys.S20Tone0_Border_Static);
        p.AssignMapping("Standard2020/Panels/Tone1 Background (Static)",            ThemeKeys.S20Tone1_Background_Static);
        p.AssignMapping("Standard2020/Panels/Tone1 Border (Static)",                ThemeKeys.S20Tone1_Border_Static);
        p.AssignMapping("Standard2020/Panels/Tone2 Background (Static)",            ThemeKeys.S20Tone2_Background_Static);
        p.AssignMapping("Standard2020/Panels/Tone2 Border (Static)",                ThemeKeys.S20Tone2_Border_Static);
        p.AssignMapping("Standard2020/Panels/Tone3 Background (Static)",            ThemeKeys.S20Tone3_Background_Static);
        p.AssignMapping("Standard2020/Panels/Tone3 Border (Static)",                ThemeKeys.S20Tone3_Border_Static);
        p.AssignMapping("Standard2020/Panels/Tone4 Background (Static)",            ThemeKeys.S20Tone4_Background_Static);
        p.AssignMapping("Standard2020/Panels/Tone4 Border (Static)",                ThemeKeys.S20Tone4_Border_Static);
        p.AssignMapping("Standard2020/Panels/Tone4 Background (MouseOver)",         ThemeKeys.S20Tone4_Background_MouseOver);
        p.AssignMapping("Standard2020/Panels/Tone4 Border (MouseOver)",             ThemeKeys.S20Tone4_Border_MouseOver);
        p.AssignMapping("Standard2020/Panels/Tone4 Background (MouseDown)",         ThemeKeys.S20Tone4_Background_MouseDown);
        p.AssignMapping("Standard2020/Panels/Tone4 Border (MouseDown)",             ThemeKeys.S20Tone4_Border_MouseDown);
        p.AssignMapping("Standard2020/Panels/Tone4 Background (Selected)",          ThemeKeys.S20Tone4_Background_Selected);
        p.AssignMapping("Standard2020/Panels/Tone4 Background (Selected Inactive)", ThemeKeys.S20Tone4_Background_Selected_Inactive);
        p.AssignMapping("Standard2020/Panels/Tone4 Border (Selected)",              ThemeKeys.S20Tone4_Border_Selected);
        p.AssignMapping("Standard2020/Panels/Tone4 Border (Selected Inactive)",     ThemeKeys.S20Tone4_Border_Selected_Inactive);
        p.AssignMapping("Standard2020/Panels/Tone4 Background (Disabled)",          ThemeKeys.S20Tone4_Background_Disabled);
        p.AssignMapping("Standard2020/Panels/Tone4 Border (Disabled)",              ThemeKeys.S20Tone4_Border_Disabled);
        p.AssignMapping("Standard2020/Panels/Tone5 Background (Static)",            ThemeKeys.S20Tone5_Background_Static);
        p.AssignMapping("Standard2020/Panels/Tone5 Border (Static)",                ThemeKeys.S20Tone5_Border_Static);
        p.AssignMapping("Standard2020/Panels/Tone5 Background (MouseOver)",         ThemeKeys.S20Tone5_Background_MouseOver);
        p.AssignMapping("Standard2020/Panels/Tone5 Border (MouseOver)",             ThemeKeys.S20Tone5_Border_MouseOver);
        p.AssignMapping("Standard2020/Panels/Tone5 Background (MouseDown)",         ThemeKeys.S20Tone5_Background_MouseDown);
        p.AssignMapping("Standard2020/Panels/Tone5 Border (MouseDown)",             ThemeKeys.S20Tone5_Border_MouseDown);
        p.AssignMapping("Standard2020/Panels/Tone5 Background (Selected)",          ThemeKeys.S20Tone5_Background_Selected);
        p.AssignMapping("Standard2020/Panels/Tone5 Background (Selected Inactive)", ThemeKeys.S20Tone5_Background_Selected_Inactive);
        p.AssignMapping("Standard2020/Panels/Tone5 Border (Selected)",              ThemeKeys.S20Tone5_Border_Selected);
        p.AssignMapping("Standard2020/Panels/Tone5 Border (Selected Inactive)",     ThemeKeys.S20Tone5_Border_Selected_Inactive);
        p.AssignMapping("Standard2020/Panels/Tone5 Background (Disabled)",          ThemeKeys.S20Tone5_Background_Disabled);
        p.AssignMapping("Standard2020/Panels/Tone5 Border (Disabled)",              ThemeKeys.S20Tone5_Border_Disabled);
        p.AssignMapping("Standard2020/Panels/Tone6 Background (Static)",            ThemeKeys.S20Tone6_Background_Static);
        p.AssignMapping("Standard2020/Panels/Tone6 Border (Static)",                ThemeKeys.S20Tone6_Border_Static);
        p.AssignMapping("Standard2020/Panels/Tone6 Background (MouseOver)",         ThemeKeys.S20Tone6_Background_MouseOver);
        p.AssignMapping("Standard2020/Panels/Tone6 Border (MouseOver)",             ThemeKeys.S20Tone6_Border_MouseOver);
        p.AssignMapping("Standard2020/Panels/Tone6 Background (MouseDown)",         ThemeKeys.S20Tone6_Background_MouseDown);
        p.AssignMapping("Standard2020/Panels/Tone6 Border (MouseDown)",             ThemeKeys.S20Tone6_Border_MouseDown);
        p.AssignMapping("Standard2020/Panels/Tone6 Background (Selected)",          ThemeKeys.S20Tone6_Background_Selected);
        p.AssignMapping("Standard2020/Panels/Tone6 Background (Selected Inactive)", ThemeKeys.S20Tone6_Background_Selected_Inactive);
        p.AssignMapping("Standard2020/Panels/Tone6 Border (Selected)",              ThemeKeys.S20Tone6_Border_Selected);
        p.AssignMapping("Standard2020/Panels/Tone6 Border (Selected Inactive)",     ThemeKeys.S20Tone6_Border_Selected_Inactive);
        p.AssignMapping("Standard2020/Panels/Tone6 Background (Disabled)",          ThemeKeys.S20Tone6_Background_Disabled);
        p.AssignMapping("Standard2020/Panels/Tone6 Border (Disabled)",              ThemeKeys.S20Tone6_Border_Disabled);
        p.AssignMapping("Standard2020/Panels/Tone7 Background (Static)",            ThemeKeys.S20Tone7_Background_Static);
        p.AssignMapping("Standard2020/Panels/Tone7 Border (Static)",                ThemeKeys.S20Tone7_Border_Static);
        p.AssignMapping("Standard2020/Panels/Tone7 Background (MouseOver)",         ThemeKeys.S20Tone7_Background_MouseOver);
        p.AssignMapping("Standard2020/Panels/Tone7 Border (MouseOver)",             ThemeKeys.S20Tone7_Border_MouseOver);
        p.AssignMapping("Standard2020/Panels/Tone7 Background (MouseDown)",         ThemeKeys.S20Tone7_Background_MouseDown);
        p.AssignMapping("Standard2020/Panels/Tone7 Border (MouseDown)",             ThemeKeys.S20Tone7_Border_MouseDown);
        p.AssignMapping("Standard2020/Panels/Tone7 Background (Selected)",          ThemeKeys.S20Tone7_Background_Selected);
        p.AssignMapping("Standard2020/Panels/Tone7 Background (Selected Inactive)", ThemeKeys.S20Tone7_Background_Selected_Inactive);
        p.AssignMapping("Standard2020/Panels/Tone7 Border (Selected)",              ThemeKeys.S20Tone7_Border_Selected);
        p.AssignMapping("Standard2020/Panels/Tone7 Border (Selected Inactive)",     ThemeKeys.S20Tone7_Border_Selected_Inactive);
        p.AssignMapping("Standard2020/Panels/Tone7 Background (Disabled)",          ThemeKeys.S20Tone7_Background_Disabled);
        p.AssignMapping("Standard2020/Panels/Tone7 Border (Disabled)",              ThemeKeys.S20Tone7_Border_Disabled);
        p.AssignMapping("Standard2020/Panels/Tone8 Background (Static)",            ThemeKeys.S20Tone8_Background_Static);
        p.AssignMapping("Standard2020/Panels/Tone8 Border (Static)",                ThemeKeys.S20Tone8_Border_Static);
        p.AssignMapping("Standard2020/Panels/Tone8 Background (MouseOver)",         ThemeKeys.S20Tone8_Background_MouseOver);
        p.AssignMapping("Standard2020/Panels/Tone8 Border (MouseOver)",             ThemeKeys.S20Tone8_Border_MouseOver);
        p.AssignMapping("Standard2020/Panels/Tone8 Background (MouseDown)",         ThemeKeys.S20Tone8_Background_MouseDown);
        p.AssignMapping("Standard2020/Panels/Tone8 Border (MouseDown)",             ThemeKeys.S20Tone8_Border_MouseDown);
        p.AssignMapping("Standard2020/Panels/Tone8 Background (Selected)",          ThemeKeys.S20Tone8_Background_Selected);
        p.AssignMapping("Standard2020/Panels/Tone8 Background (Selected Inactive)", ThemeKeys.S20Tone8_Background_Selected_Inactive);
        p.AssignMapping("Standard2020/Panels/Tone8 Border (Selected)",              ThemeKeys.S20Tone8_Border_Selected);
        p.AssignMapping("Standard2020/Panels/Tone8 Border (Selected Inactive)",     ThemeKeys.S20Tone8_Border_Selected_Inactive);
        p.AssignMapping("Standard2020/Panels/Tone8 Background (Disabled)",          ThemeKeys.S20Tone8_Background_Disabled);
        p.AssignMapping("Standard2020/Panels/Tone8 Border (Disabled)",              ThemeKeys.S20Tone8_Border_Disabled);

        // FramePFX specific
        p.AssignMapping("Video Editor/Status Bar Background",                                          ThemeKeys.PFX_Editor_StatusBar_Background,                                     "The background of the status bar (at the very bottom of the editor window)");
        p.AssignMapping("Video Editor/Timeline/Background (Static)",                                   ThemeKeys.PFX_Editor_Timeline_Background,                                      "The background of the bottom part of the timeline (below tracks)");
        p.AssignMapping("Video Editor/Timeline/Gap Between Tracks",                                    ThemeKeys.PFX_Editor_Timeline_GapBetweenTracks,                                "The colour of the gap between each track (and also the bottom border of the last track)");
        p.AssignMapping("Video Editor/Timeline/Timestamp Background",                                  ThemeKeys.PFX_Editor_Timeline_TimestampBoard_Background,                       "The background of the panel where the timestamp indicator is (as in, what shows the current frame time among other info)");
        p.AssignMapping("Video Editor/Timeline/Toolbar Background",                                    ThemeKeys.PFX_Editor_Timeline_ToolBar_Background,                              "The background of the toolbars at the bottom of the timeline and control surface list");
        p.AssignMapping("Video Editor/Timeline/Ruler Background",                                      ThemeKeys.PFX_Editor_Timeline_Ruler_Background,                                "The background of the timeline ruler");
        p.AssignMapping("Video Editor/Timeline/Header Background",                                     ThemeKeys.PFX_Editor_Timeline_Header_Background,                               "The background of the timeline's group box header (which contains the timeline name and a close button)");
        p.AssignMapping("Video Editor/Timeline/Track/Background (Static)",                             ThemeKeys.PFX_Editor_Timeline_Track_Background,                                "The background of tracks when not selected");
        p.AssignMapping("Video Editor/Timeline/Track/Background (Selected)",                           ThemeKeys.PFX_Editor_Timeline_Track_Background_Selected,                       "The background of tracks when selected");
        p.AssignMapping("Video Editor/Timeline/Control Surface/Background (Static)",                   ThemeKeys.PFX_Editor_Timeline_ControlSurface_Background,                       "The background of the track control surface list box");
        p.AssignMapping("Video Editor/Timeline/Control Surface/Item Background (Static)",              ThemeKeys.PFX_Editor_Timeline_ControlSurfaceItem_Background,                   "The background of a track control surface item");
        p.AssignMapping("Video Editor/Timeline/Control Surface/Item Background (Mouse Over)",          ThemeKeys.PFX_Editor_Timeline_ControlSurfaceItem_Background_MouseOver,         "The background of the item when the mouse is over it");
        p.AssignMapping("Video Editor/Timeline/Control Surface/Item Background (Selected, Focused)",   ThemeKeys.PFX_Editor_Timeline_ControlSurfaceItem_Background_SelectedFocused,   "The background of the item when it's selected and has UI focus");
        p.AssignMapping("Video Editor/Timeline/Control Surface/Item Background (Selected, Unfocused)", ThemeKeys.PFX_Editor_Timeline_ControlSurfaceItem_Background_SelectedUnfocused, "The background of the item when it's selected but does not have UI focus");
        p.AssignMapping("Video Editor/Resource Manager/Tab Strip Background",                          ThemeKeys.PFX_Editor_ResourceManager_TabStrip_Background,                      "The background of the resource manager's tab strip (aka tab item panel, at the top)");
        p.AssignMapping("Video Editor/Resource Manager/Tab Item Background",                           ThemeKeys.PFX_Editor_ResourceManager_TabItem_Background,                       "The background of tab items");
        p.AssignMapping("Video Editor/Resource Manager/List Background",                               ThemeKeys.PFX_Editor_ResourceManager_List_Background,                          "The background of the list panel");
        p.AssignMapping("Video Editor/Resource Manager/Tree Background",                               ThemeKeys.PFX_Editor_ResourceManager_Tree_Background,                          "The background of the tree panel");
        p.AssignMapping("Video Editor/Automation/Fill (Active)",                                       ThemeKeys.PFX_Automation_Active_Fill,                                          "The colour for most automation LED indicators and pixels");
        p.AssignMapping("Video Editor/Automation/Fill (Track, Active)",                                ThemeKeys.PFX_TrackAutomation_Active_Fill,                                     "The same as above except only for tracks (to provide some contrast between the two)");
        p.AssignMapping("Video Editor/Timeline/PlayHead Background",                                   ThemeKeys.PFX_Editor_Timeline_PlayHeadThumb_Background,                        "The background of the play head thumb");
        p.AssignMapping("Video Editor/Property Editor/Separator Line (Mouse Over)",                    ThemeKeys.PFX_PropertyEditor_Separator_MouseOverBrush,                         "The background of the play head thumb");
    }

    /// <summary>
    /// Sets the current application theme
    /// </summary>
    /// <param name="themeName"></param>
    public abstract void SetTheme(Theme theme);

    public Theme? GetTheme(string name) {
        return this.Themes.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Creates a new theme that is based on the given theme
    /// </summary>
    /// <param name="name">The new name of the theme</param>
    /// <param name="basedOn">A theme whose colours are copied into the new one</param>
    /// <param name="copyAllKeys">True to copy all keys to create effectively a complete clone</param>
    /// <returns>The new theme</returns>
    public abstract Theme RegisterTheme(string name, Theme basedOn, bool copyAllKeys = false);
}