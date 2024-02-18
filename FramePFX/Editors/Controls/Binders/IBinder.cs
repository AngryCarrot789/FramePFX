using System.Windows;

namespace FramePFX.Editors.Controls.Binders {
    /// <summary>
    /// A generic interface for a binder
    /// </summary>
    /// <typeparam name="TModel">The type of model this binder attaches to</typeparam>
    public interface IBinder<TModel> : IBinder where TModel : class {
        /// <summary>
        /// The currently attached element that owns this binder
        /// </summary>
        FrameworkElement Control { get; }

        /// <summary>
        /// The current attached model that this binder uses to update the model value from the view, and vice versa
        /// </summary>
        TModel Model { get; }

        /// <summary>
        /// Attaches this binder using the control and model. Throws an exception if already attached
        /// </summary>
        /// <param name="control">The control to be associated with</param>
        /// <param name="model">The model to be associated with</param>
        /// <param name="autoUpdateControlValue">True to automatically call the update control method</param>
        void Attach(FrameworkElement control, TModel model, bool autoUpdateControlValue = true);

        /// <summary>
        /// Detaches this binder
        /// </summary>
        void Detatch();
    }

    /// <summary>
    /// A non-generic interface for a binder
    /// </summary>
    public interface IBinder {
        /// <summary>
        /// Gets whether this binder is attached, meaning <see cref="Control"/> and <see cref="Model"/> are non-null
        /// </summary>
        bool IsAttached { get; }

        /// <summary>
        /// Notifies the binder that the model value has changed, and to therefore the control value will be updated
        /// </summary>
        void OnModelValueChanged();

        /// <summary>
        /// Notifies the binder that the control value has changed, and to therefore the model value will be updated
        /// </summary>
        void OnControlValueChanged();
    }
}