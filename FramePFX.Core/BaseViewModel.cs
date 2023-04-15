using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SharpPadV2.Core {
    /// <summary>
    /// An abstract class that implements <see cref="INotifyPropertyChanged"/>, allowing data binding between a ViewModel and a View, 
    /// along with some helper function to, for example, run an <see cref="Action"/> before or after the PropertyRaised event has been risen
    /// <para>
    ///     This class should normally be inherited by a ViewModel, such as a MainViewModel for the main view
    /// </para>
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged {
        private Dictionary<object, object> internalData;

        public event PropertyChangedEventHandler PropertyChanged;

        protected BaseViewModel() {

        }

        public static T GetInternalData<T>(BaseViewModel viewModel, object key) {
            return GetInternalData(viewModel, key) is T t ? t : default;
        }

        public static object GetInternalData(BaseViewModel viewModel, object key) {
            Dictionary<object, object> map = (viewModel ?? throw new ArgumentNullException(nameof(viewModel))).internalData;
            return map == null ? null : map.TryGetValue(key, out object data) ? data : null;
        }

        public static void SetInternalData(BaseViewModel viewModel, object key, object value) {
            if (viewModel == null) {
                throw new ArgumentNullException(nameof(viewModel));
            }

            (viewModel.internalData ?? (viewModel.internalData = new Dictionary<object, object>()))[key] = value;
        }

        public void RaisePropertyChanged([CallerMemberName] string propertyName = null) {
            if (propertyName == null) {
                throw new ArgumentNullException(nameof(propertyName), "Property Name is null");
            }

            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void RaisePropertyChanged<T>(ref T property, T newValue, [CallerMemberName] string propertyName = null) {
            if (propertyName == null) {
                throw new ArgumentNullException(nameof(propertyName), "Property Name is null");
            }

            property = newValue;
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void RaisePropertyChanged<T>(ref T property, T newValue, Action postCallback, [CallerMemberName] string propertyName = null) {
            if (propertyName == null) {
                throw new ArgumentNullException(nameof(propertyName), "Property Name is null");
            }

            property = newValue;
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            postCallback?.Invoke();
        }

        public void RaisePropertyChanged<T>(ref T property, T newValue, Action<T> postCallback, [CallerMemberName] string propertyName = null) {
            if (propertyName == null) {
                throw new ArgumentNullException(nameof(propertyName), "Property Name is null");
            }

            property = newValue;
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            postCallback?.Invoke(property);
        }

        public void RaisePropertyChanged<T>(ref T property, T newValue, Action<T> preCallback, Action<T> postCallback, [CallerMemberName] string propertyName = null) {
            if (propertyName == null) {
                throw new ArgumentNullException(nameof(propertyName), "Property Name is null");
            }

            preCallback?.Invoke(property);
            property = newValue;
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            postCallback?.Invoke(property);
        }

        public void RaisePropertyChangedIfChanged<T>(ref T property, T newValue, [CallerMemberName] string propertyName = null) {
            if (propertyName == null) {
                throw new ArgumentNullException(nameof(propertyName), "Property Name is null");
            }

            if (EqualityComparer<T>.Default.Equals(property, newValue)) {
                return;
            }

            property = newValue;
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
