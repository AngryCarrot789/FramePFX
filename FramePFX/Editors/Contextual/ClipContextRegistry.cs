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
using FramePFX.Editors.ResourceManaging.Resources;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Clips.Core;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Editors.Contextual {
    public class ClipContextRegistry : IContextGenerator {
        public static ClipContextRegistry Instance { get; } = new ClipContextRegistry();

        public void Generate(List<IContextEntry> list, IContextData context) {
            if (!DataKeys.ClipKey.TryGetContext(context, out Clip clip)) {
                return;
            }

            int selectedCount = clip.Timeline.GetSelectedClipCountWith(clip);

            // list.Add(new EventContextEntry(DeleteSelectedClips, selectedCount == 1 ? "Delete Clip" : "Delete Clips"));
            list.Add(new CommandContextEntry("RenameClipCommand", "Rename clip"));
            if (clip is ICompositionClip)
                list.Add(new EventContextEntry(OpenClipTimeline, "Open Composition Timeline"));
            list.Add(new SeparatorEntry());
            list.Add(new CommandContextEntry("DeleteSelectedClips", selectedCount == 1 ? "Delete Clip" : "Delete Clips", "Delete all selected clips in this timeline"));
            list.Add(new CommandContextEntry("DeleteClipOwnerTrack", "Delete Track", "Deletes the track that this clip resides in"));
        }

        private static void OpenClipTimeline(IContextData ctx) {
            if (DataKeys.ClipKey.TryGetContext(ctx, out Clip clip) && clip is ICompositionClip compositionClip) {
                if (clip.Project is Project project && compositionClip.ResourceCompositionKey.TryGetResource(out ResourceComposition resource)) {
                    project.ActiveTimeline = resource.Timeline;
                }
            }
        }
    }
}