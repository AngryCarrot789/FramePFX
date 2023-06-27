namespace FramePFX.Core.PropertyPages {
    /// <summary>
    /// The base view model for a property page, which contains properties regarding a
    /// </summary>
    public abstract class PropertyPageViewModel<T> : BaseViewModel {
        public T Target { get; }

        protected PropertyPageViewModel(T target) {
            this.Target = target;
        }
    }
}