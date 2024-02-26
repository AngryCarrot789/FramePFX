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
using System.ComponentModel;
using System.Windows;

namespace FramePFX.Editors.Controls.Bindings {
    /// <summary>
    /// An object binder that contains UpdateControl and UpdateModel events. These are called by calling
    /// <see cref="BaseBinder{TModel}.OnModelValueChanged"/> and <see cref="BaseBinder{TModel}.OnControlValueChanged"/>
    /// respectively.
    /// <para>
    /// When a non-null property is provided, it is used to add a property changed handler on the control,
    /// so that <see cref="BaseBinder{TModel}.OnControlValueChanged"/> does not need to be called manually
    /// </para>
    /// </summary>
    /// <typeparam name="TModel">The type of model</typeparam>
    public class UpdaterPropertyBinder<TModel> : BaseBinder<TModel> where TModel : class {
        public event Action<IBinder<TModel>> UpdateControl;
        public event Action<IBinder<TModel>> UpdateModel;

        /// <summary>
        /// A dependency property used to register a value change handler when <see cref="OnAttached"/> is called,
        /// which will call <see cref="BaseBinder{TModel}.OnControlValueChanged"/> if the changed property matches.
        /// <para>
        /// This may be null if the control value change is processed elsewhere instead of automatically
        /// </para>
        /// </summary>
        public DependencyProperty Property { get; }

        private DependencyPropertyDescriptor descriptor;

        public UpdaterPropertyBinder(DependencyProperty property, Action<IBinder<TModel>> updateControl, Action<IBinder<TModel>> updateModel) {
            this.Property = property;
            this.UpdateControl = updateControl;
            this.UpdateModel = updateModel;
        }

        protected override void OnAttached() {
            base.OnAttached();
            if (this.Property != null) {
                this.descriptor = DependencyPropertyDescriptor.FromProperty(this.Property, this.Control.GetType());
                this.descriptor.AddValueChanged(this.Control, this.OnPropertyValueChanged);
            }
        }

        protected override void OnDetatched() {
            base.OnDetatched();
            if (this.descriptor != null) {
                this.descriptor.RemoveValueChanged(this.Control, this.OnPropertyValueChanged);
                this.descriptor = null;
            }
        }

        private void OnPropertyValueChanged(object sender, EventArgs e) => this.OnControlValueChanged();

        protected override void UpdateModelCore() {
            this.UpdateModel?.Invoke(this);
        }

        protected override void UpdateControlCore() {
            this.UpdateControl?.Invoke(this);
        }
    }
}