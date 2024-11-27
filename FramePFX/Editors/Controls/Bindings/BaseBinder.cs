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

namespace FramePFX.Editors.Controls.Bindings
{
    /// <summary>
    /// The base class for general binders, which are used to create a "bind" between model and event.
    /// <para>
    /// The typical behaviour is to add an event handler in user code and call <see cref="OnModelValueChanged"/>
    /// which will cause <see cref="UpdateControlOverride"/> to be called, allowing you to update the control's value. An internal bool
    /// will stop a stack overflow when the control's value ends up calling <see cref="OnControlValueChanged"/> which ignores
    /// the call if that bool is true
    /// </para>
    /// <para>
    /// Then, an event handler should be added for the control and it should call <see cref="OnControlValueChanged"/>, which causes
    /// <see cref="UpdateModelOverride"/>. As before, an internal bool stops a stack overflow when the value changes ends up
    /// calling <see cref="OnModelValueChanged"/>
    /// </para>
    /// </summary>
    /// <typeparam name="TModel">The type of model</typeparam>
    public abstract class BaseBinder<TModel> : IBinder<TModel> where TModel : class
    {
        public FrameworkElement Control { get; private set; }

        public TModel Model { get; private set; }

        public bool IsAttached { get; private set; }

        /// <summary>
        /// Gets whether the binder is currently processing the model change signal, and is now updating
        /// the control's value. This is used to prevent a stack overflow exception
        /// </summary>
        public bool IsUpdatingControl { get; protected set; }

        protected BaseBinder()
        {
        }

        public void OnModelValueChanged()
        {
            if (!this.IsAttached)
            {
                return;
            }

            // We don't check if we are updating the control, just in case the model
            // decided to coerce its own value which is different from the UI control

            try
            {
                this.IsUpdatingControl = true;
                this.UpdateControlOverride();
            }
            finally
            {
                this.IsUpdatingControl = false;
            }
        }

        public void OnControlValueChanged()
        {
            if (!this.IsUpdatingControl && this.IsAttached)
            {
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

        /// <summary>
        /// Called when we become attached to a control and model (both are set to a valid value before this method call)
        /// </summary>
        protected virtual void OnAttached()
        {
        }

        /// <summary>
        /// Called when we become detached from our control and model (both will be set to null after this method call)
        /// </summary>
        protected virtual void OnDetached()
        {
        }

        public void Attach(FrameworkElement control, TModel model, bool autoUpdateControlValue = true)
        {
            if (this.IsAttached)
                throw new Exception("Already attached");
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            if (control == null)
                throw new ArgumentNullException(nameof(control));

            this.Model = model;
            this.Control = control;
            this.IsAttached = true;
            this.OnAttached();
            if (autoUpdateControlValue)
            {
                this.OnModelValueChanged();
            }
        }

        public void Detach()
        {
            if (!this.IsAttached)
                throw new Exception("Not attached");
            this.IsAttached = false;
            this.OnDetached();
            this.Model = null;
            this.Control = null;
        }
    }
}