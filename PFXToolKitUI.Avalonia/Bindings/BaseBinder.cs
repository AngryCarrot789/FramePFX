// 
// Copyright (c) 2024-2024 REghZy
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

using Avalonia.Controls;

namespace PFXToolKitUI.Avalonia.Bindings;

/// <summary>
/// The base class for general binders, which are used to create a "bind" between model and event.
/// <para>
/// The typical behaviour is to add an event handler in user code and call <see cref="UpdateControl"/>
/// which will cause <see cref="UpdateControlOverride"/> to be called, allowing you to update the control's value. An internal bool
/// will stop a stack overflow when the control's value ends up calling <see cref="UpdateModel"/> which ignores
/// the call if that bool is true
/// </para>
/// <para>
/// Then, an event handler should be added for the control and it should call <see cref="UpdateModel"/>, which causes
/// <see cref="UpdateModelOverride"/>. As before, an internal bool stops a stack overflow when the value changes ends up
/// calling <see cref="UpdateControl"/>
/// </para>
/// </summary>
/// <typeparam name="TModel">The type of model</typeparam>
public abstract class BaseBinder<TModel> : IBinder<TModel> where TModel : class {
    protected Control? myControl;
    protected TModel? myModel;

    public Control Control => this.myControl ?? throw new InvalidOperationException("No control is attached");

    public TModel Model => this.myModel ?? throw new InvalidOperationException("No model is attached");

    public bool IsFullyAttached { get; private set; }

    public bool IsUpdatingControl { get; protected set; }

    /// <summary>
    /// A unique name for this instance to identify it while debugging
    /// </summary>
    public string? DebugName { get; set; }

    protected BaseBinder() { }

    public void UpdateControl() {
        if (!this.IsFullyAttached) {
            return;
        }

        // We don't check if we are updating the control, just in case the model
        // decided to coerce its own value which is different from the UI control

        try {
            this.IsUpdatingControl = true;
            this.UpdateControlOverride();
        }
        finally {
            this.IsUpdatingControl = false;
        }
    }

    public void UpdateModel() {
        if (!this.IsUpdatingControl && this.IsFullyAttached) {
            this.UpdateModelOverride();
        }
    }

    /// <summary>
    /// This method should be overridden to update the model's value using the element's value
    /// </summary>
    protected abstract void UpdateModelOverride();

    /// <summary>
    /// This method should be overridden to update the control's value using the model's value
    /// </summary>
    protected abstract void UpdateControlOverride();

    public void Attach(Control control, TModel model) {
        if (this.IsFullyAttached)
            throw new Exception("Already fully attached");
        if (this.myControl != null)
            throw new InvalidOperationException("A control is already attached");
        if (this.myModel != null)
            throw new InvalidOperationException("A model is already attached");
        if (model == null)
            throw new ArgumentNullException(nameof(model));
        if (control == null)
            throw new ArgumentNullException(nameof(control));

        this.CheckAttachControl(control);
        this.CheckAttachModel(model);

        this.myModel = model;
        this.myControl = control;
        this.AttachInternal();
    }

    public void AttachControl(Control control) {
        if (this.IsFullyAttached)
            throw new Exception("Already fully attached");
        if (this.myControl != null)
            throw new InvalidOperationException("A control is already attached");
        if (control == null)
            throw new ArgumentNullException(nameof(control));

        this.CheckAttachControl(control);
        this.myControl = control;
        if (this.myModel != null) {
            this.AttachInternal();
        }
    }

    public void AttachModel(TModel model) {
        if (this.IsFullyAttached)
            throw new Exception("Already fully attached");
        if (this.myModel != null)
            throw new InvalidOperationException("A model is already attached");
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        this.CheckAttachModel(model);
        this.myModel = model;
        if (this.myControl != null) {
            this.AttachInternal();
        }
    }

    public void Detach() {
        if (!this.IsFullyAttached)
            throw new Exception("Not fully attached");

        this.TryDetachInternal();
        this.myModel = null;
        this.myControl = null;
    }

    public void DetachControl() {
        if (this.myControl == null)
            throw new InvalidOperationException("No control is attached");

        this.TryDetachInternal();
        this.myControl = null;
    }

    public void DetachModel() {
        if (this.myModel == null)
            throw new InvalidOperationException("No model is attached");

        this.TryDetachInternal();
        this.myModel = null;
    }

    public void SwitchModel(TModel? newModel) {
        if (this.myModel != null)
            this.DetachModel();

        if (newModel != null)
            this.AttachModel(newModel);
    }

    /// <summary>
    /// A method that can be overridden to throw an exception if the control cannot be attached for whatever reason
    /// </summary>
    /// <param name="control">The control being attached</param>
    protected virtual void CheckAttachControl(Control control) {
    }

    /// <summary>
    /// A method that can be overridden to throw an exception if the model cannot be attached for whatever reason
    /// </summary>
    /// <param name="model">The model being attached</param>
    protected virtual void CheckAttachModel(TModel model) {
    }

    protected abstract void OnAttached();

    protected abstract void OnDetached();

    private void AttachInternal() {
        this.IsFullyAttached = true;
        this.OnAttached();
        this.UpdateControl();
    }

    private void TryDetachInternal() {
        if (this.IsFullyAttached) {
            this.OnDetached();
            this.IsFullyAttached = false;
        }
    }
}