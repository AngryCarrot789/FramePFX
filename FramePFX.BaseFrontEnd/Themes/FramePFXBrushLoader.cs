// 
// Copyright (c) 2024-2025 REghZy
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

using PFXToolKitUI.Themes;
using PFXToolKitUI.Themes.Configurations;

namespace FramePFX.BaseFrontEnd.Themes;

public static class FramePFXBrushLoader {
    public static void Init() {
        if (ThemeManager.Instance.Themes.Count(x => x.IsBuiltIn) < 2) {
            throw new InvalidOperationException("Called too early; expected at least two built-in themes: dark and light");
        }
        
        ThemeManager manager = ThemeManager.Instance;
        ThemeConfigurationPage p = manager.ThemeConfigurationPage;
        
        // MAKE SURE TO UPDATE ANY INHERITANCE IN MemoryEngineThemes.axaml TOO! Otherwise, the app won't look the same at runtime compared to design time | This is the inheritance column |
        List<(string, string, string?)> items = [
            ("FramePFX/Editor/Background",                                                          "ABrush.PFX.Editor.Timeline.Background",                                      "ABrush.Tone0.Background.Static"),
            ("FramePFX/Editor/Track/Background",                                                    "ABrush.PFX.Editor.Timeline.Track.Background",                                "ABrush.Tone2.Background.Static"),
            ("FramePFX/Editor/Track/Background (Selected)",                                         "ABrush.PFX.Editor.Timeline.Track.Background.Selected",                       "ABrush.Tone4.Background.Static"),
            ("FramePFX/Editor/Timeline/Gap Between Tracks",                                         "ABrush.PFX.Editor.Timeline.GapBetweenTracks",                                "ABrush.Tone1.Border.Static"),
            ("FramePFX/Editor/Timeline/Control Surface/Background",                                 "ABrush.PFX.Editor.Timeline.ControlSurface.Background",                       "ABrush.Tone3.Background.Static"),
            ("FramePFX/Editor/Timeline/Control Surface/Item/Background",                            "ABrush.PFX.Editor.Timeline.ControlSurfaceItem.Background",                   "ABrush.Tone5.Background.Static"),
            ("FramePFX/Editor/Timeline/Control Surface/Item/Background (Mouse Over)",               "ABrush.PFX.Editor.Timeline.ControlSurfaceItem.Background.MouseOver",         "ABrush.Tone6.Background.Static"),
            ("FramePFX/Editor/Timeline/Control Surface/Item/Background (Selected, Focused)",        "ABrush.PFX.Editor.Timeline.ControlSurfaceItem.Background.SelectedFocused",   "ABrush.Tone5.Background.Selected"),
            ("FramePFX/Editor/Timeline/Control Surface/Item/Background (Selected, Not Focused)",    "ABrush.PFX.Editor.Timeline.ControlSurfaceItem.Background.SelectedUnfocused", "ABrush.Tone7.Background.Selected.Inactive"),
            ("FramePFX/Editor/Timeline/TimestampBoard Background",                                  "ABrush.PFX.Editor.Timeline.TimestampBoard.Background",                       "ABrush.Tone6.Background.Static"),
            ("FramePFX/Editor/StatusBar Background",                                                "ABrush.PFX.Editor.StatusBar.Background",                                     "ABrush.Tone5.Background.Static"),
            ("FramePFX/Editor/ToolBar Background",                                                  "ABrush.PFX.Editor.Timeline.ToolBar.Background",                              "ABrush.Tone4.Background.Static"),
            ("FramePFX/Editor/Ruler Background",                                                    "ABrush.PFX.Editor.Timeline.Ruler.Background",                                "ABrush.Tone3.Background.Static"),
            ("FramePFX/Editor/Header Background",                                                   "ABrush.PFX.Editor.Timeline.Header.Background",                               "ABrush.Tone5.Background.Static"),
            ("FramePFX/Editor/Resource Manager/TabStrip Background",                                "ABrush.PFX.Editor.ResourceManager.TabStrip.Background",                      "ABrush.Tone3.Background.Static"),
            ("FramePFX/Editor/Resource Manager/TabItem Background",                                 "ABrush.PFX.Editor.ResourceManager.TabItem.Background",                       "ABrush.Tone5.Background.Static"),
            ("FramePFX/Editor/Resource Manager/List Background",                                    "ABrush.PFX.Editor.ResourceManager.List.Background",                          "ABrush.Tone2.Background.Static"),
            ("FramePFX/Editor/Resource Manager/Tree Background",                                    "ABrush.PFX.Editor.ResourceManager.Tree.Background",                          "ABrush.Tone3.Background.Static"),
            ("FramePFX/Editor/Automation/Active Colour",                                            "ABrush.PFX.Automation.Active.Fill",                                          null),
            ("FramePFX/Editor/Automation/Override Colour",                                          "ABrush.PFX.Automation.Override.Fill",                                        null),
            ("FramePFX/Editor/Automation (Track)/Active Colour",                                    "ABrush.PFX.TrackAutomation.Active.Fill",                                     null),
            ("FramePFX/Editor/Property editor/Separator (Mouse Over)",                              "ABrush.PFX.PropertyEditor.Separator.MouseOverBrush",                         "ABrush.AccentTone2.Border.Static"),
        ];
        
        Dictionary<string, string?> inheritMap = new Dictionary<string, string?>();
        foreach ((string path, string theme, string? inherit) item in items) {
            inheritMap[item.theme] = item.inherit;
        }
        
        foreach (Theme theme in ThemeManager.Instance.GetBuiltInThemes()) {
            theme.SetInheritance(inheritMap);
        }
        
        foreach ((string path, string theme, string? inherit) item in items) {
            p.AssignMapping(item.path, item.theme);
        }
    }
}