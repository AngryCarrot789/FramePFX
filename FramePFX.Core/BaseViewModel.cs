using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FramePFX.Core {
    /// <summary>
    /// An abstract class that implements <see cref="INotifyPropertyChanged"/>, allowing data binding between a ViewModel and a View, 
    /// along with some helper function to, for example, run an <see cref="Action"/> before or after the PropertyRaised event has been risen
    /// <para>
    ///     This class should normally be inherited by a ViewModel, such as a MainViewModel for the main view
    /// </para>
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

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
