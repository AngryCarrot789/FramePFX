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

using Avalonia;

namespace FramePFX.Avalonia.Bindings;

/// <summary>
/// A base class which inherits <see cref="BaseAutoUpdatePropertyBinder{TModel}"/> and also implements
/// an event handler for the model which fires the <see cref="IBinder.UpdateControl"/> method
/// </summary>
/// <typeparam name="TModel">The model type</typeparam>
public abstract class BaseAutoUpdateAndEventPropertyBinder<TModel> : BaseAutoUpdatePropertyBinder<TModel> where TModel : class
{
    private readonly AutoEventHelper autoEventHelper;

    protected BaseAutoUpdateAndEventPropertyBinder(string eventName) : this(null, eventName) {
    }

    protected BaseAutoUpdateAndEventPropertyBinder(AvaloniaProperty? property, string eventName) : base(property)
    {
        this.autoEventHelper = new AutoEventHelper(eventName, typeof(TModel), this.OnModelValueChanged);
    }

    /// <summary>
    /// Invoked by the model's value changed event handler. By default this method invokes <see cref="IBinder.UpdateControl"/>
    /// </summary>
    protected virtual void OnModelValueChanged() => this.UpdateControl();

    protected override void OnAttached()
    {
        base.OnAttached();
        this.autoEventHelper.AddEventHandler(this.myModel!);
    }

    protected override void OnDetached()
    {
        base.OnDetached();
        this.autoEventHelper.RemoveEventHandler(this.myModel!);
    }
}