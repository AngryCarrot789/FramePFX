using System.Windows;

namespace FramePFX.Editors.Controls {
    /// <summary>
    /// An interface that represents a UI component that follows the connection-disconnection pattern
    /// </summary>
    public interface IUIConnection<TParent, TModel> where TParent : DependencyObject where TModel : class {
        /// <summary>
        /// Gets the owner control for this object
        /// </summary>
        TParent Owner { get; }

        /// <summary>
        /// Gets the model control for this object
        /// </summary>
        TModel Model { get; }

        /// <summary>
        /// Connects this UI object to the given owner and model. This should be called after the control is
        /// </summary>
        /// <param name="owner">The connected parent</param>
        /// <param name="model">The connected model</param>
        void Connect(TParent owner, TModel model);

        /// <summary>
        /// Disconnects this UI object from its owner and model
        /// </summary>
        void Disconnect();
    }
}