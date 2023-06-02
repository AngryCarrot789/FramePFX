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
    public abstract class BaseViewModel : INotifyPropertyChanged, INotifyPropertyChanging {
        private Dictionary<object, object> internalData;

        public event PropertyChangingEventHandler PropertyChanging;
        public event PropertyChangedEventHandler PropertyChanged;

        protected BaseViewModel() {
        }

        #region Public functions

        /// <summary>
        /// Raises
        /// </summary>
        /// <param name="propertyName"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null) {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName), "Property Name is null");
            this.RaisePropertyChangingOverride(propertyName);
            this.RaisePropertyChangedOverride(propertyName);
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanging"/> event, sets <see cref="property"/> to <see cref="newValue"/>, and then raises the <see cref="PropertyChanged"/> event
        /// </summary>
        public virtual void RaisePropertyChanged<T>(ref T property, T newValue, [CallerMemberName] string propertyName = null) {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName), "Property Name is null");

            this.RaisePropertyChangingOverride(propertyName);
            property = newValue;
            this.RaisePropertyChangedOverride(propertyName);
        }

        /// <summary>
        /// If <see cref="property"/> and <see cref="newValue"/> are equal according to the <see cref="EqualityComparer{T}"/> for
        /// type <see cref="T"/>, then nothing happens. Otherwise, the <see cref="PropertyChanging"/> event is raised, <see cref="property"/> is
        /// set to <see cref="newValue"/>, and then the <see cref="PropertyChanged"/> event is raised
        /// </summary>
        public virtual void RaisePropertyChangedIfChanged<T>(ref T property, T newValue, [CallerMemberName] string propertyName = null) {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName), "Property Name is null");

            if (EqualityComparer<T>.Default.Equals(property, newValue))
                return;

            this.RaisePropertyChangingOverride(propertyName);
            property = newValue;
            this.RaisePropertyChangedOverride(propertyName);
        }

        #endregion

        #region Virtual event raisers

        protected virtual void RaisePropertyChangingOverride(string propertyName) {
            this.PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        }

        protected virtual void RaisePropertyChangedOverride(string propertyName) {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Internal data

        private static Dictionary<object, object> GetMap(BaseViewModel vm) {
            return vm.internalData ?? (vm.internalData = new Dictionary<object, object>());
        }

        public static T GetInternalData<T>(BaseViewModel viewModel, object key) {
            return GetInternalData(viewModel, key) is T t ? t : default;
        }

        public static object GetInternalData(BaseViewModel viewModel, object key) {
            Dictionary<object, object> map = (viewModel ?? throw new ArgumentNullException(nameof(viewModel))).internalData;
            return map == null ? null : map.TryGetValue(key, out object data) ? data : null;
        }

        public static bool TryGetInternalData<T>(BaseViewModel viewModel, object key, out T value) {
            Dictionary<object, object> map = (viewModel ?? throw new ArgumentNullException(nameof(viewModel))).internalData;
            if (map != null && map.TryGetValue(key, out object data) && data is T t) {
                value = t;
                return true;
            }

            value = default;
            return false;
        }

        public static void SetInternalData(BaseViewModel viewModel, object key, object value) {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            GetMap(viewModel)[key] = value;
        }

        public static bool ClearInternalData(BaseViewModel viewModel, object key) {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            Dictionary<object, object> map = viewModel.internalData;
            return map != null && map.Remove(key);
        }

        #endregion
    }
}
