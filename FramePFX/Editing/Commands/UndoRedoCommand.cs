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

using FramePFX.CommandSystem;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Editing.Commands;

public abstract class UndoRedoCommand : Command {
    public bool IsUndo { get; }

    protected UndoRedoCommand(bool isUndo) {
        this.IsUndo = isUndo;
    }

    public override Executability CanExecute(CommandEventArgs e) {
        if (!DataKeys.VideoEditorKey.TryGetContext(e.ContextData, out var editor))
            return Executability.Invalid;

        bool canExecute = this.IsUndo ? editor.HistoryManager.CanUndo : editor.HistoryManager.CanRedo;
        return canExecute ? Executability.Valid : Executability.ValidButCannotExecute;
    }

    public static void Undo(IContextData context) {
        if (!DataKeys.VideoEditorKey.TryGetContext(context, out VideoEditor editor))
            return;
        if (!editor.HistoryManager.HasUndoableActions) {
            IoC.MessageService.ShowMessage("Undo attempt", "Nothing to undo!");
        }
        else if (editor.HistoryManager.IsUndoInProgress) {
            IoC.MessageService.ShowMessage("Cannot Undo", "An undo action is already in progress... somehow");
        }
        else {
            editor.HistoryManager.Undo();
        }
    }

    public static void Redo(IContextData context) {
        if (!DataKeys.VideoEditorKey.TryGetContext(context, out VideoEditor editor))
            return;
        if (!editor.HistoryManager.HasRedoableActions) {
            IoC.MessageService.ShowMessage("Redo attempt", "Nothing to redo!");
        }
        else if (editor.HistoryManager.IsRedoInProgress) {
            IoC.MessageService.ShowMessage("Cannot Redo", "A redo action is already in progress... somehow");
        }
        else {
            editor.HistoryManager.Redo();
        }
    }
}

public class UndoCommand : UndoRedoCommand {
    public UndoCommand() : base(true) {
    }

    protected override void Execute(CommandEventArgs e) {
        Undo(e.ContextData);
    }
}

public class RedoCommand : UndoRedoCommand {
    public RedoCommand() : base(false) {
    }

    protected override void Execute(CommandEventArgs e) {
        Redo(e.ContextData);
    }
}