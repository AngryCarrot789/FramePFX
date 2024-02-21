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
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System.Windows;
using FramePFX.CommandSystem;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Editors.Commands {
    public class CloseProjectCommand : Command{
        public override ExecutabilityState CanExecute(CommandEventArgs e) {
            if (!DataKeys.VideoEditorKey.TryGetContext(e.ContextData, out VideoEditor editor))
                return ExecutabilityState.Invalid;
            return editor.Project == null ? ExecutabilityState.ValidButCannotExecute : ExecutabilityState.Executable;
        }

        public override void Execute(CommandEventArgs e) {
            if (!DataKeys.VideoEditorKey.TryGetContext(e.ContextData, out VideoEditor editor))
                return;
            CloseProject(editor);
        }

        public static bool CloseProject(VideoEditor editor, string msgTitle = "Project is open", string message = "A project is open. Do you want to save it?") {
            Project oldProject = editor.Project;
            if (oldProject == null) {
                return true;
            }

            MessageBoxResult result = IoC.MessageService.ShowMessage(msgTitle, message, MessageBoxButton.YesNoCancel);
            switch (result) {
                case MessageBoxResult.Cancel: return false;
                case MessageBoxResult.Yes: {
                    bool? saveResult = SaveProjectCommand.SaveProject(editor.Project);
                    if (!saveResult.HasValue) {
                        return false;
                    }

                    editor.CloseProject();
                    break;
                }
                default: {
                    editor.CloseProject();
                    break;
                }
            }

            return true;
        }
    }
}