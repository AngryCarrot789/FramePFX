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
using FramePFX.Editors.Automation;
using FramePFX.Editors.Automation.Keyframes;
using FramePFX.Editors.Automation.Params;

namespace FramePFX.Editors.Controls.Bindings {
    public sealed class AutomationBinder<TModel> : BaseBinder<TModel> where TModel : class, IHaveTimeline, IAutomatable {
        private readonly ParameterChangedEventHandler handler;

        public Parameter Parameter { get; }

        public event Action<AutomationBinder<TModel>> UpdateModel;
        public event Action<AutomationBinder<TModel>> UpdateControl;

        public AutomationBinder(Parameter parameter) {
            this.handler = this.OnParameterValueChanged;
            this.Parameter = parameter;
        }

        private void OnParameterValueChanged(AutomationSequence sequence) => this.OnModelValueChanged();

        protected override void OnAttached() {
            base.OnAttached();
            this.Model.AutomationData.AddParameterChangedHandler(this.Parameter, this.handler);
        }

        protected override void OnDetatched() {
            base.OnDetatched();
            this.Model.AutomationData.RemoveParameterChangedHandler(this.Parameter, this.handler);
        }

        protected override void UpdateModelCore() {
            this.UpdateModel?.Invoke(this);
        }

        protected override void UpdateControlCore() {
            this.UpdateControl?.Invoke(this);
        }
    }
}