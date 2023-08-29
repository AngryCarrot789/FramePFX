using System.ComponentModel;

namespace FramePFX.PropertyEditing {
    public interface IPropertyEditReceiver : INotifyPropertyChanged {
        void OnExternalPropertyModified(BasePropertyEditorViewModel handler, string property);
    }
}