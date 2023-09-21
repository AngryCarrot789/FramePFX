namespace FramePFX.PropertyEditing {
    public class PropertyObjectSeparator : BaseViewModel, IPropertyEditorObject {
        private bool isVisible;
        public bool IsVisible {
            get => this.isVisible;
            set {
                if (this.isVisible != value)
                    this.RaisePropertyChanged(ref this.isVisible, value);
            }
        }

        public BasePropertyGroupViewModel Parent { get; }

        public PropertyObjectSeparator(BasePropertyGroupViewModel parent) {
            this.Parent = parent;
        }
    }
}