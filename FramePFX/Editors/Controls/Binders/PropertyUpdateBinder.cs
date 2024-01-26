using System;
using System.Windows;

namespace FramePFX.Editors.Controls.Binders {
    /// <summary>
    /// An object binder that contains UpdateControl and UpdateModel events. These are called by a notification
    /// to <see cref="BaseObjectBinder{TModel}.OnModelValueChanged"/> and either
    /// <see cref="BaseObjectBinder{TModel}.OnControlValueChanged"/> or <see cref="OnPropertyChanged"/>, respectively
    /// </summary>
    /// <typeparam name="TModel">The type of model</typeparam>
    public class PropertyUpdateBinder<TModel> : BaseObjectBinder<TModel> where TModel : class {
        public event Action<IBinder<TModel>> UpdateControl;
        public event Action<IBinder<TModel>> UpdateModel;

        /// <summary>
        /// A dependency property, matched when <see cref="OnPropertyChanged"/> is called, which will
        /// call <see cref="BaseObjectBinder{TModel}.OnControlValueChanged"/> if the changed property matches
        /// </summary>
        public DependencyProperty Property { get; set; }

        public PropertyUpdateBinder(Action<IBinder<TModel>> updateControl, Action<IBinder<TModel>> updateModel) {
            this.UpdateControl = updateControl;
            this.UpdateModel = updateModel;
        }

        public PropertyUpdateBinder(DependencyProperty property, Action<IBinder<TModel>> updateControl, Action<IBinder<TModel>> updateModel) : this(updateControl, updateModel) {
            this.Property = property;
        }

        /// <summary>
        /// This method calls <see cref="BaseObjectBinder{TModel}.OnControlValueChanged"/> if
        /// our <see cref="Property"/> matches the changed property, we are not already updating
        /// the control and our property is non-null
        /// </summary>
        /// <param name="e">The property changed event args</param>
        public void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
            if (!this.IsUpdatingControl && e.Property == this.Property && this.Property != null) {
                this.OnControlValueChanged();
            }
        }

        protected override void UpdateModelCore() {
            this.UpdateModel?.Invoke(this);
        }

        protected override void UpdateControlCore() {
            this.UpdateControl?.Invoke(this);
        }
    }
}