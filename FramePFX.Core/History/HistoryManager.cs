using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.Core.Utils;

namespace FramePFX.Core.History
{
    public class HistoryManager
    {
        private readonly LinkedList<IHistoryAction> undoList;
        private readonly LinkedList<IHistoryAction> redoList;

        public bool IsUndoing { get; private set; }
        public bool IsRedoing { get; private set; }

        /// <summary>
        /// Convenient property for checking if <see cref="IsUndoing"/> or <see cref="IsRedoing"/> is true
        /// </summary>
        public bool IsActionActive => this.IsUndoing || this.IsRedoing;

        public bool CanUndo => this.undoList.Count > 0;
        public bool CanRedo => this.redoList.Count > 0;

        public int MaxUndo { get; private set; }
        public int MaxRedo { get; private set; }

        /// <summary>
        /// Returns the action that is next to be undone
        /// </summary>
        public IHistoryAction NextUndo => this.undoList.Last?.Value;

        /// <summary>
        /// Returns the action that is next to be redone
        /// </summary>
        public IHistoryAction NextRedo => this.redoList.Last?.Value;

        public const int DefaultUndo = 2000;
        public const int DefaultRedo = 2000;

        public HistoryManager(int maxUndo = DefaultUndo, int maxRedo = DefaultRedo)
        {
            this.undoList = new LinkedList<IHistoryAction>();
            this.redoList = new LinkedList<IHistoryAction>();
            this.MaxUndo = maxUndo < 1 ? throw new ArgumentOutOfRangeException(nameof(maxUndo), "maxUndo must be greater than 0") : maxUndo;
            this.MaxRedo = maxRedo < 1 ? throw new ArgumentOutOfRangeException(nameof(maxRedo), "maxRedo must be greater than 0") : maxRedo;
        }

        public void Reset()
        {
            this.IsUndoing = false;
            this.IsRedoing = false;
            this.Clear();
        }

        public void UnsafeReset()
        {
            this.IsUndoing = false;
            this.IsRedoing = false;
            this.undoList.Clear();
            this.redoList.Clear();
        }

        private static void RemoveFirst(LinkedList<IHistoryAction> list)
        {
            IHistoryAction model = list.First.Value;
            list.RemoveFirst();
            model.OnRemoved();
        }

        public void SetMaxUndoAsync(int maxUndo)
        {
            if (maxUndo < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxUndo), "Value must be greater than 0");
            }

            int oldUndo = this.MaxUndo;
            this.MaxUndo = maxUndo;
            if (maxUndo >= oldUndo)
            {
                return;
            }

            int count = this.undoList.Count - maxUndo;
            using (ErrorList stack = new ErrorList())
            {
                for (int i = 0; i < count; i++)
                {
                    try
                    {
                        RemoveFirst(this.undoList);
                    }
                    catch (Exception e)
                    {
                        stack.Add(new Exception("Failed to remove excessive undo-able action", e));
                    }
                }
            }
        }

        public void SetMaxRedo(int maxRedo)
        {
            if (maxRedo < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxRedo), "Value must be greater than 0");
            }

            int oldRedo = this.MaxRedo;
            this.MaxRedo = maxRedo;
            if (maxRedo >= oldRedo)
            {
                return;
            }

            int count = this.redoList.Count - maxRedo;
            using (ErrorList stack = new ErrorList())
            {
                for (int i = 0; i < count; i++)
                {
                    try
                    {
                        RemoveFirst(this.redoList);
                    }
                    catch (Exception e)
                    {
                        stack.Add(new Exception("Failed to remove excessive redo-able action", e));
                    }
                }
            }
        }

        public void AddAction(IHistoryAction action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (this.IsUndoing)
                throw new Exception("Undo is in progress");
            if (this.IsRedoing)
                throw new Exception("Redo is in progress");

            foreach (IHistoryAction item in this.redoList)
            {
                item.OnRemoved();
            }

            this.redoList.Clear();
            this.undoList.AddLast(action);
            while (this.undoList.Count > this.MaxUndo)
            {
                // loop just in case
                RemoveFirst(this.undoList);
            }
        }

        public void Clear()
        {
            if (this.IsUndoing)
                throw new Exception("Undo is in progress");
            if (this.IsRedoing)
                throw new Exception("Redo is in progress");

            foreach (IHistoryAction item in this.undoList)
                item.OnRemoved();
            foreach (IHistoryAction item in this.redoList)
                item.OnRemoved();

            this.undoList.Clear();
            this.redoList.Clear();
        }

        /// <summary>
        /// Undoes the last action or last redone action
        /// </summary>
        /// <returns>A task containing the undone action</returns>
        /// <exception cref="Exception">Undo or redo is in progress</exception>
        /// <exception cref="InvalidOperationException">Nothing to undo</exception>
        public async Task<IHistoryAction> OnUndoAsync()
        {
            if (this.IsUndoing)
                throw new Exception("Undo is already in progress");
            if (this.IsRedoing)
                throw new Exception("Redo is in progress");
            if (this.undoList.Count < 1)
                throw new InvalidOperationException("Nothing to undo");

            IHistoryAction action = this.undoList.Last.Value;
            this.undoList.RemoveLast();

            using (ErrorList stack = new ErrorList())
            {
                try
                {
                    this.IsUndoing = true;
                    await action.UndoAsync();
                }
                catch (Exception e)
                {
                    stack.Add(new Exception("Failed to undo action", e));
                }
                finally
                {
                    this.IsUndoing = false;
                }

                this.redoList.AddLast(action);
                while (this.redoList.Count > this.MaxRedo)
                {
                    // loop just in case
                    try
                    {
                        RemoveFirst(this.redoList);
                    }
                    catch (Exception e)
                    {
                        stack.Add(new Exception("Failed to remove excessive undo-able action", e));
                    }
                }
            }

            return action;
        }

        /// <summary>
        /// Redoes the last undone action
        /// </summary>
        /// <returns>A task containing the redone action</returns>
        /// <exception cref="Exception">Undo or redo is in progress</exception>
        /// <exception cref="InvalidOperationException">Nothing to redo</exception>
        public async Task<IHistoryAction> OnRedoAsync()
        {
            if (this.IsUndoing)
                throw new Exception("Undo is in progress");
            if (this.IsRedoing)
                throw new Exception("Redo is already in progress");
            if (this.redoList.Count < 1)
                throw new InvalidOperationException("Nothing to redo");

            IHistoryAction action = this.redoList.Last.Value;
            this.redoList.RemoveLast();

            using (ErrorList stack = new ErrorList())
            {
                try
                {
                    this.IsRedoing = true;
                    await action.RedoAsync();
                }
                catch (Exception e)
                {
                    stack.Add(new Exception("Failed to redo action", e));
                }
                finally
                {
                    this.IsRedoing = false;
                }

                this.undoList.AddLast(action);
                while (this.undoList.Count > this.MaxUndo)
                {
                    // loop just in case
                    try
                    {
                        RemoveFirst(this.undoList);
                    }
                    catch (Exception e)
                    {
                        stack.Add(new Exception("Failed to remove excessive redo-able action", e));
                    }
                }
            }

            return action;
        }
    }
}