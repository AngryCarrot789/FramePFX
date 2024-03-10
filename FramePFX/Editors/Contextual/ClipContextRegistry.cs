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

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FramePFX.AdvancedMenuService.ContextService;
using FramePFX.CommandSystem;
using FramePFX.Editors.ResourceManaging.Resources;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Clips.Core;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Editors.Contextual
{
    public class ClipContextRegistry : IContextGenerator
    {
        public static ClipContextRegistry Instance { get; } = new ClipContextRegistry();

        public void Generate(List<IContextEntry> list, IContextData context)
        {
            if (!GetClipSelection(context, out Clip[] clips))
            {
                return;
            }

            if (clips.Length == 1)
            {
                Clip clip = clips[0];
                list.Add(new CommandContextEntry("RenameClipCommand", "Rename clip"));
                if (clip is ICompositionClip)
                    list.Add(new EventContextEntry(OpenClipTimeline, "Open Composition Timeline"));
                list.Add(new SeparatorEntry());
                list.Add(new CommandContextEntry("DeleteSelectedClips", "Delete Clip", "Deletes this clip from the timeline"));
                list.Add(new CommandContextEntry("DeleteClipOwnerTrack", "Delete Track", "Deletes the track that this clip resides in"));
            }
            else
            {
                list.Add(new CommandContextEntry("DeleteSelectedClips", "Delete Clips", "Deletes these selected clips"));
            }
        }

        private static void OpenClipTimeline(IContextData ctx)
        {
            if (DataKeys.ClipKey.TryGetContext(ctx, out Clip clip) && clip is ICompositionClip compositionClip)
            {
                if (clip.Project is Project project && compositionClip.ResourceCompositionKey.TryGetResource(out ResourceComposition resource))
                {
                    project.ActiveTimeline = resource.Timeline;
                }
            }
        }

        public static Executability CanGetClipSelection(IContextData ctx, bool doNotUseTimelineSelection = false)
        {
            if (DataKeys.ClipKey.TryGetContext(ctx, out Clip clip))
            {
                Track track = clip.Track;
                Timeline timeline;
                if (track == null || (timeline = track.Timeline) == null)
                {
                    return Executability.ValidButCannotExecute;
                }

                int selectedClips = doNotUseTimelineSelection ? track.SelectedClipsCount : timeline.SelectedClipsCount;
                if (!clip.IsSelected || selectedClips == 1)
                {
                    return Executability.Valid;
                }
                else
                {
                    Debug.Assert(selectedClips > 1, "Selection corruption 1");
                    return Executability.Valid;
                }
            }
            else if (doNotUseTimelineSelection)
            {
                if (DataKeys.TrackKey.TryGetContext(ctx, out Track track))
                {
                    return track.SelectedClipsCount > 0 ? Executability.Valid : Executability.ValidButCannotExecute;
                }
                else
                {
                    return Executability.Invalid;
                }
            }
            else if (DataKeys.TimelineKey.TryGetContext(ctx, out Timeline timeline))
            {
                return timeline.SelectedClipsCount > 0 ? Executability.Valid : Executability.ValidButCannotExecute;
            }
            else
            {
                return Executability.Invalid;
            }
        }

        public static bool GetClipSelection(IContextData ctx, out Clip[] clips, bool doNotUseTimelineSelection = false)
        {
            if (DataKeys.ClipKey.TryGetContext(ctx, out Clip clip))
            {
                Track track = clip.Track;
                Timeline timeline;
                if (track == null || (timeline = track.Timeline) == null)
                {
                    clips = null;
                    return false;
                }

                int selectedClips = doNotUseTimelineSelection ? track.SelectedClipsCount : timeline.SelectedClipsCount;
                if (!clip.IsSelected || selectedClips == 1)
                {
                    // Interacted with a non-selected or the only selected clip
                    clips = new Clip[] {clip};
                }
                else
                {
                    Debug.Assert(selectedClips > 1, "Selection corruption 1");
                    clips = doNotUseTimelineSelection ? track.SelectedClips.ToArray() : timeline.SelectedClips.ToArray();
                }

                Debug.Assert(clips.Length > 0, "Selection corruption 2");
                return clips.Length > 0;
            }
            else if (doNotUseTimelineSelection)
            {
                if (DataKeys.TrackKey.TryGetContext(ctx, out Track track))
                {
                    clips = track.SelectedClips.ToArray();
                    return clips.Length > 0;
                }
            }
            else if (DataKeys.TimelineKey.TryGetContext(ctx, out Timeline timeline))
            {
                clips = timeline.SelectedClips.ToArray();
                return clips.Length > 0;
            }

            clips = null;
            return false;
        }
    }
}