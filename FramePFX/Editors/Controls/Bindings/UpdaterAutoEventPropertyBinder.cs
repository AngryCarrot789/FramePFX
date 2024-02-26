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

using System;
using System.Reflection;
using System.Windows;
using FramePFX.Utils;

namespace FramePFX.Editors.Controls.Bindings {
    /// <summary>
    /// A binder that automatically handles a dependency property value change signal to update the model. A model value
    /// changed event handler will be auto-registered to tell the control value to update.
    /// The model and control value update are 2 actions passed in the constructor
    /// </summary>
    /// <typeparam name="TModel">The type of model</typeparam>
    public class UpdaterAutoEventPropertyBinder<TModel> : UpdaterPropertyBinder<TModel> where TModel : class {
        private readonly EventInfo eventInfo;
        private readonly Delegate handlerInternal;

        public UpdaterAutoEventPropertyBinder(string eventName, Action<IBinder<TModel>> updateControl, Action<IBinder<TModel>> updateModel) : this(null, eventName, updateControl, updateModel) {
        }

        public UpdaterAutoEventPropertyBinder(DependencyProperty property, string eventName, Action<IBinder<TModel>> updateControl, Action<IBinder<TModel>> updateModel) : base(property, updateControl, updateModel) {
            this.eventInfo = typeof(TModel).GetEvent(eventName, BindingFlags.Public | BindingFlags.Instance);
            if (this.eventInfo == null)
                throw new Exception("Could not find event by name: " + typeof(TModel).Name + "." + eventName);
            this.handlerInternal = EventUtils.CreateDelegateToInvokeActionFromEvent(this.eventInfo.EventHandlerType, this.OnModelValueChanged);
        }

        protected override void OnAttached() {
            base.OnAttached();
            this.eventInfo.AddEventHandler(this.Model, this.handlerInternal);
        }

        protected override void OnDetatched() {
            base.OnDetatched();
            this.eventInfo.RemoveEventHandler(this.Model, this.handlerInternal);
        }
    }
}