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
using System.Windows;
using FramePFX.Editors.Automation;
using FramePFX.Editors.Automation.Keyframes;
using FramePFX.Editors.Automation.Params;

namespace FramePFX.Editors.Controls.Bindings
{
    /// <summary>
    /// A <see cref="UpdaterPropertyBinder{TModel}"/> that also listens to automation value changed events.
    /// <see cref="UpdaterPropertyBinder{TModel}.UpdateModel"/> should be handled in order to actually update the automation
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public class AutomationUpdaterPropertyBinder<TModel> : UpdaterPropertyBinder<TModel> where TModel : class, IHaveTimeline, IAutomatable
    {
        public Parameter Parameter { get; }

        public AutomationUpdaterPropertyBinder(DependencyProperty property, Parameter parameter, Action<IBinder<TModel>> updateControl, Action<IBinder<TModel>> updateModel) : base(property, updateControl, updateModel)
        {
            this.Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            this.Model.AutomationData.AddParameterChangedHandler(this.Parameter, this.OnParameterValueChanged);
        }

        protected override void OnDetached()
        {
            base.OnDetached();
            this.Model.AutomationData.RemoveParameterChangedHandler(this.Parameter, this.OnParameterValueChanged);
        }

        private void OnParameterValueChanged(AutomationSequence sequence) => this.OnModelValueChanged();

        private void OnPropertyValueChanged(object sender, EventArgs e) => this.OnControlValueChanged();
    }

    /// <summary>
    /// A <see cref="AutomationUpdaterPropertyBinder{TModel}"/> that directly updates the control value.
    /// This class does not fire <see cref="UpdaterPropertyBinder{TModel}.UpdateControl"/>.
    /// <see cref="UpdaterPropertyBinder{TModel}.UpdateModel"/> should be handled in order to actually update the automation
    /// </summary>
    /// <typeparam name="TModel">The type of model</typeparam>
    public class DirectAutomationPropertyBinder<TModel> : AutomationUpdaterPropertyBinder<TModel> where TModel : class, IHaveTimeline, IAutomatable
    {
        public DirectAutomationPropertyBinder(DependencyProperty property, Parameter parameter, Action<IBinder<TModel>> updateControl, Action<IBinder<TModel>> updateModel) : base(property, parameter, updateControl, updateModel)
        {
        }

        protected override void UpdateControlCore()
        {
            object value = this.Parameter.GetCurrentObjectValue(this.Model);
            this.Control.SetValue(this.Property, value);
        }
    }
}