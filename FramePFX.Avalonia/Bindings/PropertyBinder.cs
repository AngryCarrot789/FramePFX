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

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace FramePFX.Avalonia.Bindings;

/// <summary>
/// A helper class for binding 
/// </summary>
/// <typeparam name="TModel"></typeparam>
public class PropertyBinder<TModel> {
    public AvaloniaProperty<TModel> SourceProperty { get; }
    public AvaloniaProperty<TModel> TargetProperty { get; }

    /// <summary>
    /// The control who owns the model object
    /// </summary>
    public Control SourceControl { get; }

    /// <summary>
    /// The control which needs to receive the model object when the source's model changes
    /// </summary>
    public Control? TargetControl { get; private set; }

    private IDisposable? binder;

    public PropertyBinder(Control sourceControl, AvaloniaProperty<TModel> sourceProperty, AvaloniaProperty<TModel> targetProperty) {
        this.SourceControl = sourceControl;
        this.SourceProperty = sourceProperty;
        this.TargetProperty = targetProperty;
    }

    public void SetTargetControl(Control? control) {
        this.binder?.Dispose();
        this.TargetControl?.ClearValue(this.TargetProperty); // Is this necessary?
        if ((this.TargetControl = control) != null) {
            this.binder = this.TargetControl.Bind(this.TargetProperty, new IndexerDescriptor() {
                Property = this.SourceProperty, Mode = BindingMode.OneWay, Source = this.SourceControl
            });
        }
    }
}