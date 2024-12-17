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
using FramePFX.Editing.Timelines.Clips;
using FramePFX.Editing.Timelines.Clips.Core;
using FramePFX.Editing.Timelines.Clips.Video;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Editing.ContextRegistries;

public static class ClipContextRegistry
{
    public static readonly ContextRegistry Registry = new ContextRegistry("Clips");

    static ClipContextRegistry()
    {
        FixedContextGroup modGeneric = Registry.GetFixedGroup("modify.general");
        modGeneric.AddHeader("General");
        modGeneric.AddCommand("commands.editor.RenameClip", "Rename", "Open a dialog to rename this clip");
        modGeneric.AddDynamicSubGroup((group, ctx, items) =>
        {
            if (DataKeys.ClipKey.TryGetContext(ctx, out Clip? clip) && clip is VideoClip videoClip)
            {
                if (VideoClip.IsEnabledParameter.GetCurrentValue(videoClip))
                {
                    items.Add(new CommandContextEntry("commands.editor.DisableClips", "Disable", "Disable this clip"));
                }
                else
                {
                    items.Add(new CommandContextEntry("commands.editor.EnableClips", "Enable", "Enable this clip"));
                }
            }
            else
            {
                items.Add(new CommandContextEntry("commands.editor.EnableClips", "Enable", "Enable the selected clips"));
                items.Add(new CommandContextEntry("commands.editor.DisableClips", "Disable", "Disable the selected clips"));
                items.Add(new CommandContextEntry("commands.editor.ToggleClipsEnabled", "Toggle Enabled", "Toggle the enabled state of the selected clips"));
            }
        });

        FixedContextGroup modEdit = Registry.GetFixedGroup("modify.edit");
        modEdit.AddHeader("Edit");
        modEdit.AddCommand("commands.editor.SplitClipsCommand", "Split", "Slice this clip at the playhead");
        modEdit.AddCommand("commands.editor.ChangeClipPlaybackSpeed", "Change Speed", "Change the playback speed of this clip");
        modEdit.AddDynamicSubGroup((group, ctx, items) =>
        {
            if (DataKeys.ClipKey.TryGetContext(ctx, out Clip? clip) && clip is CompositionVideoClip)
            {
                items.Add(new CommandContextEntry("commands.editor.OpenCompositionClipTimeline", "Open Timeline", "Opens this clip's timeline"));
            }
        });

        FixedContextGroup modDestruction = Registry.GetFixedGroup("modify.destruction", 100000);
        modDestruction.AddCommand("commands.editor.DeleteClipOwnerTrack", "Delete Track", "Delete the track this clip resides in");
    }
}