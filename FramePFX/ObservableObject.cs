using System.Runtime.CompilerServices;

namespace FramePFX {
    public delegate void ObservablePropertyChangedEventHandler(IObservableObject sender, string propertyName);

    public interface IObservableObject {
        event ObservablePropertyChangedEventHandler PropertyChanged;
    }

    public class ObservableObject : IObservableObject {
        public event ObservablePropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            this.PropertyChanged?.Invoke(this, propertyName);
        }

        protected virtual void OnPropertyChanged<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null) {
            field = newValue;
            this.PropertyChanged?.Invoke(this, propertyName);
        }
    }
}