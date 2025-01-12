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

using FramePFX.Services.Messaging;
using FramePFX.Utils;

namespace FramePFX.History;

/// <summary>
/// A class that manages a collection of undo-able and redo-able actions
/// </summary>
public class HistoryManager {
    private readonly LinkedList<IHistoryAction> undoList;
    private readonly LinkedList<IHistoryAction> redoList;

    private readonly Stack<List<IHistoryAction>> actionStack;
    // private HistoryManager parent;

    public bool IsUndoInProgress { get; private set; }

    public bool IsRedoInProgress { get; private set; }

    /// <summary>
    /// Returns the number of undo-able actions. This does not count actions in the current execution section
    /// </summary>
    public bool HasUndoableActions => this.undoList.Count > 0;

    /// <summary>
    /// Returns the number of redo-able actions. This does not count actions in the current execution section
    /// </summary>
    public bool HasRedoableActions => this.redoList.Count > 0;

    /// <summary>
    /// Returns true if we are not currently undoing something and we have undo-able actions
    /// </summary>
    public bool CanUndo => !this.IsUndoInProgress && this.HasUndoableActions;

    /// <summary>
    /// Returns true if we are not currently redoing something and we have redo-able actions
    /// </summary>
    public bool CanRedo => !this.IsRedoInProgress && this.HasRedoableActions;

    public HistoryManager() {
        this.undoList = new LinkedList<IHistoryAction>();
        this.redoList = new LinkedList<IHistoryAction>();
        this.actionStack = new Stack<List<IHistoryAction>>();
    }

    // public void LinkToParent(HistoryManager newParent) {
    //     this.CheckNotPerformingUndoOrRedo();
    //     this.parent = newParent;
    //     if (newParent != null) {
    //         this.ClearBuffers();
    //     }
    // }

    private void ClearBuffers() {
        this.ClearRedoBuffer();
        this.ClearUndoBuffer();
    }

    private void ClearUndoBuffer() {
        foreach (IHistoryAction t in this.undoList)
            t.Dispose();
        
        this.undoList.Clear();
    }

    private void ClearRedoBuffer() {
        foreach (IHistoryAction t in this.redoList)
            t.Dispose();
        
        this.redoList.Clear();
    }

    private void RemoveFirstUndoable() {
        this.undoList.First!.Value.Dispose();
        this.undoList.RemoveFirst();
    }

    /// <summary>
    /// Clears all undo-able and redo-able actions
    /// </summary>
    public void Clear() {
        this.CheckNotPerformingUndoOrRedo();
        this.ClearBuffers();
    }

    /// <summary>
    /// Adds the given action to be undone and redone
    /// </summary>
    /// <param name="action">The action to add</param>
    public void AddAction(IHistoryAction action) {
        if (this.actionStack.Count > 0) {
            this.actionStack.Peek().Add(action);
        }
        else {
            this.ClearRedoBuffer();
            this.undoList.AddLast(action);
            while (this.undoList.Count > 500) {
                this.RemoveFirstUndoable();
            }

            // this.parent?.AddAction(new ChildManagerHistoryAction(this));
        }
    }

    public Task PerformUndo() => this.PerformUndoOrRedo(true);

    public Task PerformRedo() => this.PerformUndoOrRedo(false);

    private async Task PerformUndoOrRedo(bool isUndo) {
        this.CheckStateForUndoOrRedo(isUndo);

        LinkedList<IHistoryAction> srcList = isUndo ? this.undoList : this.redoList;
        LinkedList<IHistoryAction> dstList = isUndo ? this.redoList : this.undoList;

        IHistoryAction action = srcList.Last!.Value;

        bool success = false;
        try {
            try {
                if (isUndo) {
                    this.IsUndoInProgress = false;
                    success = await action.Undo();
                }
                else {
                    this.IsRedoInProgress = false;
                    success = await action.Redo();
                }
            }
            finally {
                if (isUndo) {
                    this.IsUndoInProgress = false;
                }
                else {
                    this.IsRedoInProgress = false;
                }
            }
        }
        catch (Exception e) {
            await IMessageDialogService.Instance.ShowMessage((isUndo ? "Undo" : "Redo") + " Error", "An exception occurred while performing history action", e.GetToString());
        }

        if (success) {
            srcList.RemoveLast();
            dstList.AddLast(action);
        }
    }

    private void CheckNotPerformingUndoOrRedo() {
        if (this.IsUndoInProgress)
            throw new InvalidOperationException("Undo is in progress");
        if (this.IsRedoInProgress)
            throw new InvalidOperationException("Redo is in progress");
    }

    private void CheckStateForUndoOrRedo(bool isUndo) {
        this.CheckNotPerformingUndoOrRedo();
        if (isUndo) {
            if (this.undoList.Count < 1)
                throw new InvalidOperationException("Nothing to undo");
        }
        else if (this.redoList.Count < 1)
            throw new InvalidOperationException("Nothing to redo");
    }

    /// <summary>
    /// Returns a disposable object that can be used to execute multiple undoable actions and
    /// once all completed allow them to be grouped together as a single action
    /// </summary>
    /// <returns></returns>
    public IDisposable BeginMergedSection() {
        this.OnBeginExecutionSection();
        return new MergedSection(this);
    }

    private void OnBeginExecutionSection() {
        this.actionStack.Push(new List<IHistoryAction>());
    }

    private void OnEndExecutionSection() {
        List<IHistoryAction> list = this.actionStack.Pop();
        if (list.Count > 0) {
            this.AddAction(new MergedHistoryAction(list.ToArray()));
        }
    }

    private class MergedSection : IDisposable {
        private HistoryManager? manager;

        public MergedSection(HistoryManager manager) {
            this.manager = manager;
        }

        public void Dispose() {
            if (this.manager != null) {
                this.manager.OnEndExecutionSection();
                this.manager = null;
            }
        }
    }
}