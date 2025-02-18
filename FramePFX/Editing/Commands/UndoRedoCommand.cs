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

using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Services.Messaging;

namespace FramePFX.Editing.Commands;

public abstract class UndoRedoCommand : Command {
    public bool IsUndo { get; }

    protected UndoRedoCommand(bool isUndo) {
        this.IsUndo = isUndo;
    }

    protected override Executability CanExecuteCore(CommandEventArgs e) {
        if (!DataKeys.VideoEditorKey.TryGetContext(e.ContextData, out VideoEditor? editor))
            return Executability.Invalid;

        bool canExecute = this.IsUndo ? editor.HistoryManager.CanUndo : editor.HistoryManager.CanRedo;
        return canExecute ? Executability.Valid : Executability.ValidButCannotExecute;
    }

    public static void Undo(IContextData context) {
        if (!DataKeys.VideoEditorKey.TryGetContext(context, out VideoEditor? editor))
            return;
        if (!editor.HistoryManager.HasUndoableActions) {
            IMessageDialogService.Instance.ShowMessage("Undo attempt", "Nothing to undo!");
        }
        else if (editor.HistoryManager.IsUndoInProgress) {
            IMessageDialogService.Instance.ShowMessage("Cannot Undo", "An undo action is already in progress... somehow");
        }
        else {
            editor.HistoryManager.PerformUndo();
        }
    }

    public static void Redo(IContextData context) {
        if (!DataKeys.VideoEditorKey.TryGetContext(context, out VideoEditor? editor))
            return;
        if (!editor.HistoryManager.HasRedoableActions) {
            IMessageDialogService.Instance.ShowMessage("Redo attempt", "Nothing to redo!");
        }
        else if (editor.HistoryManager.IsRedoInProgress) {
            IMessageDialogService.Instance.ShowMessage("Cannot Redo", "A redo action is already in progress... somehow");
        }
        else {
            editor.HistoryManager.PerformRedo();
        }
    }
}

public class UndoCommand : UndoRedoCommand {
    public UndoCommand() : base(true) {
    }

    protected override Task ExecuteCommandAsync(CommandEventArgs e) {
        Undo(e.ContextData);
        return Task.CompletedTask;
    }
}

public class RedoCommand : UndoRedoCommand {
    public RedoCommand() : base(false) {
    }

    protected override Task ExecuteCommandAsync(CommandEventArgs e) {
        Redo(e.ContextData);
        return Task.CompletedTask;
    }
}