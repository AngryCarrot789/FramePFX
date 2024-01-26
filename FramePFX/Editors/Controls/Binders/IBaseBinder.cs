using System.Windows.Controls;

namespace FramePFX.Editors.Controls.Binders {
    public interface IBaseBinder {
        /// <summary>
        /// Gets whether this binder is attached, meaning <see cref="Control"/> and <see cref="Model"/> are non-null
        /// </summary>
        bool IsAttached { get; }

        /// <summary>
        /// Notifies the binder that the model value has changed, and to therefore update the control value
        /// </summary>
        void OnModelValueChanged();

        /// <summary>
        /// Notifies the binder that the control value has changed, and to therefore update the model value
        /// </summary>
        void OnControlValueChanged();
    }
}