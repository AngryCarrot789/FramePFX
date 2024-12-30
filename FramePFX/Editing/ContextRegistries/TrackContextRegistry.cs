// 
// Copyright (c) 2024-2024 REghZy
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

using FramePFX.AdvancedMenuService;
using FramePFX.Editing.Timelines.Tracks;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Editing.ContextRegistries;

public static class TrackContextRegistry {
    public static readonly ContextRegistry TimelineTrackContextRegistry = new ContextRegistry("Track");
    public static readonly ContextRegistry TrackControlSurfaceContextRegistry = new ContextRegistry("Track Control Surface");
    
    static TrackContextRegistry() {
        {
            FixedContextGroup modGeneric = TimelineTrackContextRegistry.GetFixedGroup("modify.general");
            modGeneric.AddHeader("General");
            modGeneric.AddCommand("commands.editor.RenameTrack", "Rename", "Open a dialog to rename this track", SimpleIcons.RenameIcon);
            modGeneric.AddCommand("commands.editor.SelectClipsInTracks", "Select All", "Select all clips in this track");
            modGeneric.AddDynamicSubGroup(GenerateEnableDisableCommands);

            FixedContextGroup modAdd = TimelineTrackContextRegistry.GetFixedGroup("ModifyAddClips");
            modAdd.AddHeader("Add new clips");
            modAdd.AddCommand("commands.editor.AddTextClip", "Add Text clip", "Create a new Text clip");
            modAdd.AddCommand("commands.editor.AddTimecodeClip", "Add Timecode clip", "Create a new Timecode clip");
            modAdd.AddCommand("commands.editor.AddAVMediaClip", "Add Video Media clip", "Create a new media clip for playing videos or most types of media", SimpleIcons.VideoIcon);
            modAdd.AddCommand("commands.editor.AddVideoClipShape", "Add Shape clip", "Create a new Shape clip");
            modAdd.AddCommand("commands.editor.AddImageVideoClip", "Add Image clip", "Create a new Image clip");
            modAdd.AddCommand("commands.editor.AddCompositionVideoClip", "Add Composition clip", "Create a new Composition clip");

            FixedContextGroup mod3 = TimelineTrackContextRegistry.GetFixedGroup("Modify2");
            // Removed from here and added to timeline sequence
            // mod3.AddCommand("commands.editor.SplitClipsCommand", "Split clips", "Slice this clip at the playhead");

            FixedContextGroup modExternal = TimelineTrackContextRegistry.GetFixedGroup("modify.externalmodify");
            modExternal.AddHeader("New Tracks");
            modExternal.AddCommand("commands.editor.CreateVideoTrack", "Insert Video Track Above", "Inserts a new Video Track above this track");
            modExternal.AddCommand("commands.editor.CreateAudioTrack", "Insert Audio Track Above", "Inserts a new Audio Track above this track");

            FixedContextGroup mod4 = TimelineTrackContextRegistry.GetFixedGroup("modify.destruction", 100000);
            mod4.AddCommand("commands.editor.DeleteSpecificTrack", "Delete Track", "Delete this track", SimpleIcons.BinIcon);
        }
        {
            FixedContextGroup modGeneric = TrackControlSurfaceContextRegistry.GetFixedGroup("modify.general");
            modGeneric.AddHeader("General");
            modGeneric.AddCommand("commands.editor.RenameTrack", "Rename", "Open a dialog to rename this track");
            modGeneric.AddDynamicSubGroup(GenerateEnableDisableCommands);

            FixedContextGroup modExternal = TrackControlSurfaceContextRegistry.GetFixedGroup("modify.externalmodify");
            modExternal.AddHeader("New Tracks");
            modExternal.AddCommand("commands.editor.CreateVideoTrack", "Insert Video Track Above", "Inserts a new Video Track above this track");
            modExternal.AddCommand("commands.editor.CreateAudioTrack", "Insert Audio Track Above", "Inserts a new Audio Track above this track");

            FixedContextGroup mod3 = TrackControlSurfaceContextRegistry.GetFixedGroup("modify.destruction", 100000);
            mod3.AddCommand("commands.editor.DeleteSpecificTrack", "Delete Track", "Delete this track", SimpleIcons.BinIcon);
        }
    }
    
    private static void GenerateEnableDisableCommands(DynamicContextGroup group, IContextData ctx, List<IContextObject> items) {
        if (DataKeys.TrackKey.TryGetContext(ctx, out Track? track) && track is VideoTrack videoTrack) {
            if (VideoTrack.IsEnabledParameter.GetCurrentValue(videoTrack)) {
                items.Add(new CommandContextEntry("commands.editor.DisableTracks", "Disable", "Disable this track"));
            }
            else {
                items.Add(new CommandContextEntry("commands.editor.EnableTracks", "Enable", "Enable this track"));
            }
        }
        else {
            items.Add(new CommandContextEntry("commands.editor.EnableTracks", "Enable", "Enable the selected tracks"));
            items.Add(new CommandContextEntry("commands.editor.DisableTracks", "Disable", "Disable the selected tracks"));
            items.Add(new CommandContextEntry("commands.editor.ToggleTracksEnabled", "Toggle Enabled", "Toggle the enabled state of the selected tracks"));
        }
    }
}