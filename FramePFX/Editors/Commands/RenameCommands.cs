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

using System.Threading.Tasks;
using FramePFX.CommandSystem;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.History;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Editors.Commands {
    public class RenameResourceCommand : Command {
        public override ExecutabilityState CanExecute(CommandEventArgs e) {
            return e.ContextData.ContainsKey(DataKeys.ResourceObjectKey) ? ExecutabilityState.Executable : ExecutabilityState.Invalid;
        }

        public override Task Execute(CommandEventArgs e) {
            if (!DataKeys.ResourceObjectKey.TryGetContext(e.ContextData, out BaseResource target)) {
                return Task.CompletedTask;
            }

            if (IoC.UserInputService.ShowSingleInputDialog("Rename resource item", "Input a new name for this resource", target.DisplayName) is string newDisplayName) {
                string oldName = target.DisplayName;
                target.DisplayName = newDisplayName;
                if (DataKeys.VideoEditorKey.TryGetContext(e.ContextData, out VideoEditor editor)) {
                    editor.HistoryManager.AddAction(new HistoryActionDisplayName(target, oldName));
                }
            }

            return Task.CompletedTask;
        }
    }

    public class RenameClipCommand : Command {
        public override ExecutabilityState CanExecute(CommandEventArgs e) {
            return e.ContextData.ContainsKey(DataKeys.ClipKey) ? ExecutabilityState.Executable : ExecutabilityState.Invalid;
        }

        public override Task Execute(CommandEventArgs e) {
            if (!DataKeys.ClipKey.TryGetContext(e.ContextData, out Clip target)) {
                return Task.CompletedTask;
            }

            if (IoC.UserInputService.ShowSingleInputDialog("Rename clip", "Input a new name for this clip", target.DisplayName) is string newDisplayName) {
                string oldName = target.DisplayName;
                target.DisplayName = newDisplayName;
                if (DataKeys.VideoEditorKey.TryGetContext(e.ContextData, out VideoEditor editor)) {
                    editor.HistoryManager.AddAction(new HistoryActionDisplayName(target, oldName));
                }
            }

            return Task.CompletedTask;
        }
    }

    public class RenameTrackCommand : Command {
        public override ExecutabilityState CanExecute(CommandEventArgs e) {
            return e.ContextData.ContainsKey(DataKeys.TrackKey) ? ExecutabilityState.Executable : ExecutabilityState.Invalid;
        }

        public override Task Execute(CommandEventArgs e) {
            if (!DataKeys.TrackKey.TryGetContext(e.ContextData, out Track target)) {
                return Task.CompletedTask;
            }

            if (IoC.UserInputService.ShowSingleInputDialog("Rename track", "Input a new name for this track", target.DisplayName) is string newDisplayName) {
                string oldName = target.DisplayName;
                target.DisplayName = newDisplayName;
                if (DataKeys.VideoEditorKey.TryGetContext(e.ContextData, out VideoEditor editor)) {
                    editor.HistoryManager.AddAction(new HistoryActionDisplayName(target, oldName));
                }
            }

            return Task.CompletedTask;
        }
    }

    public class HistoryActionDisplayName : HistoryAction {
        private readonly IDisplayName target;
        private string swapName;

        public HistoryActionDisplayName(IDisplayName target, string previousName) {
            this.target = target;
            this.swapName = previousName;
        }

        protected override bool OnUndo() {
            this.SwapNames();
            return true;
        }

        protected override bool OnRedo() {
            this.SwapNames();
            return true;
        }

        private void SwapNames() {
            string newName = this.swapName;
            this.swapName = this.target.DisplayName;
            this.target.DisplayName = newName;
        }
    }
}