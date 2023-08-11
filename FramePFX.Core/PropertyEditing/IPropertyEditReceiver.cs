using System.ComponentModel;

namespace FramePFX.Core.PropertyEditing {
    public interface IPropertyEditReceiver : INotifyPropertyChanged {
        void OnExternalPropertyModified(BasePropertyEditorViewModel handler, string property);
    }
}