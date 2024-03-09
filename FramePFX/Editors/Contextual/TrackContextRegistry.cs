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
using FramePFX.AdvancedMenuService.ContextService;
using FramePFX.Editors.Factories;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Clips.Core;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Editors.Contextual
{
    public class TrackContextRegistry : IContextGenerator
    {
        public static TrackContextRegistry Instance { get; } = new TrackContextRegistry();

        public void Generate(List<IContextEntry> list, IContextData context)
        {
            Timeline timeline = null;
            if (DataKeys.TrackKey.TryGetContext(context, out Track track) && track.Timeline != null)
            {
                int selectedCount = track.Timeline.SelectedTracks.Count;
                if (!track.IsSelected)
                    selectedCount++;

                timeline = track.Timeline;
                list.Add(new CommandContextEntry("RenameTrackCommand", "Rename track"));
                list.Add(new SeparatorEntry());
                list.Add(new EventContextEntry((c) => AddClipByType(c, ClipFactory.Instance.GetId(typeof(ImageVideoClip))), "Add Image Clip"));
                list.Add(new EventContextEntry((c) => AddClipByType(c, ClipFactory.Instance.GetId(typeof(TimecodeClip))), "Add Timecode Clip"));
                list.Add(new SeparatorEntry());
                list.Add(new CommandContextEntry("DeleteSelectedTracks", selectedCount == 1 ? "Delete Track" : "Delete Tracks", "Delete selected tracks in this timeline"));
            }

            if (timeline != null || DataKeys.TimelineKey.TryGetContext(context, out timeline))
            {
                if (list.Count > 0)
                {
                    list.Add(SeparatorEntry.NewInstance);
                }

                list.Add(new EventContextEntry(AddVideoTrack, "New Video Track"));
                list.Add(new EventContextEntry(AddAudioTrack, "New Audio Track"));
            }
        }

        private static void AddVideoTrack(IContextData context)
        {
            Timeline timeline;
            if (DataKeys.TrackKey.TryGetContext(context, out Track track) && (timeline = track.Timeline) != null || DataKeys.TimelineKey.TryGetContext(context, out timeline))
            {
                timeline.AddTrack(new VideoTrack()
                {
                    DisplayName = "New Video Track"
                });
            }
        }

        private static void AddAudioTrack(IContextData context)
        {
            Timeline timeline;
            if (DataKeys.TrackKey.TryGetContext(context, out Track track) && (timeline = track.Timeline) != null || DataKeys.TimelineKey.TryGetContext(context, out timeline))
            {
                timeline.AddTrack(new AudioTrack()
                {
                    DisplayName = "New Audio Track"
                });
            }
        }

        private static void AddClipByType(IContextData context, string id)
        {
            if (!DataKeys.TrackKey.TryGetContext(context, out Track track))
            {
                return;
            }

            FrameSpan span = new FrameSpan(0, 300);
            if (DataKeys.TrackContextMouseFrameKey.TryGetContext(context, out long frame))
            {
                span = span.WithBegin(frame);
            }

            Clip clip = ClipFactory.Instance.NewClip(id);
            clip.FrameSpan = span;
            track.AddClip(clip);
        }
    }
}