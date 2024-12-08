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

namespace FramePFX.Avalonia.Bindings;

/// <summary>
/// A generic interface for a binder
/// </summary>
/// <typeparam name="TModel">The type of model this binder attaches to</typeparam>
public interface IBinder<TModel> : IBinder where TModel : class
{
    /// <summary>
    /// The currently attached element that owns this binder
    /// </summary>
    Control Control { get; }

    /// <summary>
    /// The current attached model that this binder uses to update the model value from the view, and vice versa
    /// </summary>
    TModel Model { get; }

    /// <summary>
    /// Attaches both the control and the model to this binder. This is equivalent to
    /// calling <see cref="AttachControl"/> and <see cref="AttachModel"/>.
    /// </summary>
    /// <param name="control">The control to be associated with</param>
    /// <param name="model">The model to be associated with</param>
    void Attach(Control control, TModel model);

    /// <summary>
    /// Attaches the control to this binder. If a model is attached (via <see cref="AttachModel"/>) then the binder becomes fully attached
    /// </summary>
    /// <param name="control">The control to be associated with</param>
    void AttachControl(Control control);

    /// <summary>
    /// Attaches the model to this binder. If a control is attached (via <see cref="AttachControl"/>) then the binder becomes fully attached
    /// </summary>
    /// <param name="model">The model to be associated with</param>
    void AttachModel(TModel model);

    /// <summary>
    /// Detaches both the control and model from this binder. This is equivalent to
    /// calling <see cref="DetachControl"/> and <see cref="DetachModel"/>.
    /// </summary>
    void Detach();

    /// <summary>
    /// Detaches the control from this binder
    /// </summary>
    void DetachControl();

    /// <summary>
    /// Detaches the model from this binder
    /// </summary>
    void DetachModel();

    /// <summary>
    /// Detaches the current model, if there is one, and then attaches the new model if it's non-null. This is a convenience method
    /// </summary>
    /// <param name="newModel">The new model to attach, if non-null</param>
    void SwitchModel(TModel? newModel);
}

/// <summary>
/// A non-generic interface for a binder
/// </summary>
public interface IBinder
{
    /// <summary>
    /// Returns true when this binder is fully attached to a control and model,
    /// meaning <see cref="Control"/> and <see cref="Model"/> are non-null
    /// </summary>
    bool IsFullyAttached { get; }

    /// <summary>
    /// Returns true when the binder is currently processing the model change signal, and
    /// is now updating the control's value. This is used to prevent a stack overflow exception
    /// </summary>
    bool IsUpdatingControl { get; }

    /// <summary>
    /// Updates the control's value based on the model's value. This is typically called when the model's value changes
    /// </summary>
    void UpdateControl();

    /// <summary>
    /// Updates the model's value based on the control's value. This is typically called when the control's value changes
    /// </summary>
    void UpdateModel();
}