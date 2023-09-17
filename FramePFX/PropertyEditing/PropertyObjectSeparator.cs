namespace FramePFX.PropertyEditing {
    public class PropertyObjectSeparator : BaseViewModel, IPropertyObject {
        private bool isVisible;
        public bool IsVisible {
            get => this.isVisible;
            set {
                if (this.isVisible != value)
                    this.RaisePropertyChanged(ref this.isVisible, value);
            }
        }
    }
}