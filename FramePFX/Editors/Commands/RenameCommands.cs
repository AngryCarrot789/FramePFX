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

using FramePFX.CommandSystem;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Editors.Commands {
    public class RenameResourceCommand : Command {
        public override ExecutabilityState CanExecute(CommandEventArgs e) {
            return e.ContextData.ContainsKey(DataKeys.ResourceObjectKey) ? ExecutabilityState.Executable : ExecutabilityState.Invalid;
        }

        public override void Execute(CommandEventArgs e) {
            if (!DataKeys.ResourceObjectKey.TryGetContext(e.ContextData, out BaseResource resource)) {
                return;
            }

            if (IoC.UserInputService.ShowSingleInputDialog("Rename resource item", "Input a new name for this resource", resource.DisplayName) is string newDisplayName) {
                resource.DisplayName = newDisplayName;
            }
        }
    }

    public class RenameClipCommand : Command {
        public override ExecutabilityState CanExecute(CommandEventArgs e) {
            return e.ContextData.ContainsKey(DataKeys.ClipKey) ? ExecutabilityState.Executable : ExecutabilityState.Invalid;
        }

        public override void Execute(CommandEventArgs e) {
            if (!DataKeys.ClipKey.TryGetContext(e.ContextData, out Clip clip)) {
                return;
            }

            if (IoC.UserInputService.ShowSingleInputDialog("Rename clip", "Input a new name for this clip", clip.DisplayName) is string newDisplayName) {
                clip.DisplayName = newDisplayName;
            }
        }
    }

    public class RenameTrackCommand : Command {
        public override ExecutabilityState CanExecute(CommandEventArgs e) {
            return e.ContextData.ContainsKey(DataKeys.TrackKey) ? ExecutabilityState.Executable : ExecutabilityState.Invalid;
        }

        public override void Execute(CommandEventArgs e) {
            if (!DataKeys.TrackKey.TryGetContext(e.ContextData, out Track track)) {
                return;
            }

            if (IoC.UserInputService.ShowSingleInputDialog("Rename track", "Input a new name for this track", track.DisplayName) is string newDisplayName) {
                track.DisplayName = newDisplayName;
            }
        }
    }
}