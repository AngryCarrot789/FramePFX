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

using System.Collections.ObjectModel;

namespace FramePFX.PropertyEditing;

public delegate void PropertyEditorSlotEventHandler(PropertyEditorSlot sender);

/// <summary>
/// The base class for a slot in a property editor. This is what stores the data used to
/// modify one or more actual data properties in the UI. This is basically a single row in the editor
/// </summary>
public abstract class PropertyEditorSlot : BasePropertyEditorItem {
    private static readonly ReadOnlyCollection<object> EmptyList = new List<object>().AsReadOnly();

    private bool isSelected;

    public abstract bool IsSelectable { get; }

    /// <summary>
    /// Gets whether this slot is selectable in the UI. While not used yet, it was used in FramePFX to signal
    /// to the automation controls to set the active automation sequence to the one this slot is related to
    /// </summary>
    public bool IsSelected {
        get => this.isSelected && this.IsSelectable;
        set {
            if (!this.IsSelectable)
                throw new InvalidOperationException("Not selectable");
            if (this.isSelected == value)
                return;
            this.isSelected = value;
            PropertyEditor.InternalProcessSelectionChanged(this);
            this.IsSelectedChanged?.Invoke(this);
        }
    }

    public IReadOnlyList<object> Handlers { get; private set; }

    /// <summary>
    /// Whether or not there are handlers currently using this property editor. Inverse of <see cref="IsEmpty"/>
    /// </summary>
    public bool HasHandlers => this.Handlers.Count > 0;

    /// <summary>
    /// Whether or not there are no handlers currently using this property editor. Inverse of <see cref="HasHandlers"/>
    /// </summary>
    public bool IsEmpty => this.Handlers.Count < 1;

    /// <summary>
    /// Whether or not this editor has only 1 active handler
    /// </summary>
    public bool IsSingleHandler => this.Handlers.Count == 1;

    /// <summary>
    /// Whether or not this editor has more than 1 active handlers
    /// </summary>
    public bool IsMultiHandler => this.Handlers.Count > 1;

    /// <summary>
    /// A mode which helps determine if this editor can be used based on the input handler list
    /// </summary>
    public virtual ApplicabilityMode ApplicabilityMode => ApplicabilityMode.All;

    public event PropertyEditorSlotEventHandler? IsSelectedChanged;
    public event PropertyEditorSlotEventHandler? HandlersLoaded;
    public event PropertyEditorSlotEventHandler? HandlersCleared;

    protected PropertyEditorSlot(Type applicableType) : base(applicableType) {
        this.Handlers = EmptyList;
    }

    protected override void OnPropertyEditorChanged(PropertyEditor? oldEditor, PropertyEditor? newEditor) {
        base.OnPropertyEditorChanged(oldEditor, newEditor);
        PropertyEditor.InternalProcessSelectionForEditorChanged(this, oldEditor, newEditor);
    }

    /// <summary>
    /// Clears this editor's active handlers. This will not clear the underlying list, and instead, assigns it to an empty list
    /// <para>
    /// If there are no handlers currently loaded, then this function does nothing
    /// </para>
    /// </summary>
    public void ClearHandlers() {
        if (this.Handlers.Count < 1) {
            return;
        }

        this.OnClearingHandlers();
        this.Handlers = EmptyList;
        this.HandlersCleared?.Invoke(this);
        this.IsCurrentlyApplicable = false;
    }

    /// <summary>
    /// Called just before the handlers are cleared. When this is cleared, there is guaranteed to be 1 or more loaded handlers
    /// </summary>
    protected virtual void OnClearingHandlers() {
    }

    /// <summary>
    /// Clears the handler list and then sets up the new handlers for the given list. If the
    /// <see cref="HandlerCountMode"/> for this group is unacceptable,
    /// then nothing else happens. If all of the input objects are not applicable, then nothing happens. Otherwise,
    /// <see cref="BasePropertyObjectViewModel.IsCurrentlyApplicable"/> is set to true and the handlers are loaded
    /// </summary>
    /// <param name="input">Input list of objects</param>
    public void SetHandlers(IReadOnlyList<object> targets) {
        this.ClearHandlers();
        if (!this.IsHandlerCountAcceptable(targets.Count)) {
            return;
        }

        if (!GetApplicable(this, targets, out IReadOnlyList<object> list)) {
            return;
        }

        this.IsCurrentlyApplicable = true;
        this.Handlers = list;
        this.OnHandlersLoaded();
    }

    /// <summary>
    /// Called just after all handlers are fulled loaded. When this is cleared, there is guaranteed to be 1 or more loaded handlers
    /// </summary>
    protected virtual void OnHandlersLoaded() {
        this.HandlersLoaded?.Invoke(this);
    }

    private static bool GetApplicable(PropertyEditorSlot slot, IReadOnlyList<object> input, out IReadOnlyList<object> output) {
        switch (slot.ApplicabilityMode) {
            case ApplicabilityMode.All: {
                // return sources.All(x => editor.IsApplicable(x));
                for (int i = 0, c = input.Count; i < c; i++) {
                    if (!slot.IsObjectApplicable(input[i])) {
                        output = null;
                        return false;
                    }
                }

                output = input;
                return true;
            }
            case ApplicabilityMode.Any: {
                for (int i = 0, c = input.Count; i < c; i++) {
                    if (slot.IsObjectApplicable(input[i])) {
                        List<object> list = new List<object>();
                        do {
                            list.Add(input[i++]);
                        } while (i < c);

                        output = list;
                        return true;
                    }
                }

                output = null;
                return false;
            }
            default: throw new Exception("Invalid " + nameof(ApplicabilityMode));
        }
    }
}