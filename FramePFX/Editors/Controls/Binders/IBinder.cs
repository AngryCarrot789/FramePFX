using System.Windows;

namespace FramePFX.Editors.Controls.Binders {
    public interface IBinder<TModel> : IBaseBinder where TModel : class {
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
}