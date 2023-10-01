using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FramePFX
{
    /// <summary>
    /// An abstract class that implements <see cref="INotifyPropertyChanged"/>, allowing data binding between a ViewModel and a View
    /// <para>
    /// This class should normally be inherited by a ViewModel, such as a MainViewModel for the main view
    /// </para>
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        // allows view models to store additional runtime objects in a compositional way,
        // instead of using inheritance and properties. No events are raised when this data changes
        private Dictionary<object, object> internalData;

        public event PropertyChangedEventHandler PropertyChanged;

        protected BaseViewModel()
        {
        }

        #region Public functions

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for the given property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName), "Property name is null");
            this.RaisePropertyChangedCore(propertyName);
        }

        /// <summary>
        /// Sets <see cref="property"/> to <see cref="newValue"/>, and then raises the <see cref="PropertyChanged"/> event
        /// </summary>
        public void RaisePropertyChanged<T>(ref T property, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName), "Property name is null");
            property = newValue;
            this.RaisePropertyChangedCore(propertyName);
        }

        /// <summary>
        /// If <see cref="property"/> and <see cref="newValue"/> are equal according to the <see cref="EqualityComparer{T}"/> for
        /// type <see cref="T"/>, then nothing happens. Otherwise, <see cref="property"/> is set to <see cref="newValue"/>,
        /// and then the <see cref="PropertyChanged"/> event is raised
        /// </summary>
        public void RaisePropertyChangedIfChanged<T>(ref T property, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName), "Property name is null");
            if (EqualityComparer<T>.Default.Equals(property, newValue))
                return;
            property = newValue;
            this.RaisePropertyChangedCore(propertyName);
        }

        #endregion

        #region Virtual event raisers

        protected virtual void RaisePropertyChangedCore(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Internal data

        /// <summary>
        /// Gets a generic object with the given key. Returns the default value of
        /// <see cref="T"/> if the internal dictionary is null/empty or no such key is present
        /// </summary>
        /// <param name="viewModel">View model instance</param>
        /// <param name="key">The key</param>
        /// <typeparam name="T">The type of value</typeparam>
        /// <returns>The value, or default</returns>
        /// <exception cref="NullReferenceException">The view model is null</exception>
        /// <exception cref="ArgumentNullException">The key is null</exception>
        public static T GetInternalData<T>(BaseViewModel viewModel, object key)
        {
            return GetInternalData(viewModel, key) is T t ? t : default;
        }

        /// <summary>
        /// Gets an object with the given key. Returns null if the internal
        /// dictionary is null/empty or no such key is present
        /// </summary>
        /// <param name="viewModel">View model instance</param>
        /// <param name="key">The key</param>
        /// <returns>The value, or default</returns>
        /// <exception cref="NullReferenceException">The view model is null</exception>
        /// <exception cref="ArgumentNullException">The key is null</exception>
        public static object GetInternalData(BaseViewModel viewModel, object key, object def = null)
        {
            if (viewModel == null)
                throw new NullReferenceException(nameof(viewModel));
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            Dictionary<object, object> map = viewModel.internalData;
            return map != null && map.TryGetValue(key, out object data) ? data : def;
        }

        /// <summary>
        /// Tries to get an object with the given key
        /// </summary>
        /// <param name="viewModel">View model instance</param>
        /// <param name="key">The key</param>
        /// <param name="value">
        /// The output value, or default, if the internal dictionary is null/empty
        /// or the key is not present or not of the correct type
        /// </param>
        /// <typeparam name="T">The type of value</typeparam>
        /// <returns>True if the key was found, otherwise false</returns>
        /// <exception cref="NullReferenceException">The view model is null</exception>
        /// <exception cref="ArgumentNullException">The key is null</exception>
        public static bool TryGetInternalData<T>(BaseViewModel viewModel, object key, out T value)
        {
            if (viewModel == null)
                throw new NullReferenceException(nameof(viewModel));
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            Dictionary<object, object> map = viewModel.internalData;
            if (map != null && map.TryGetValue(key, out object data) && data is T t)
            {
                value = t;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Adds or replaces a value with the given key
        /// </summary>
        /// <param name="viewModel">View model instance</param>
        /// <param name="key">The key</param>
        /// <param name="value">The value to add or replace</param>
        /// <exception cref="NullReferenceException">The view model is null</exception>
        /// <exception cref="ArgumentNullException">The key is null</exception>
        public static void SetInternalData(BaseViewModel viewModel, object key, object value)
        {
            if (viewModel == null)
                throw new NullReferenceException(nameof(viewModel));
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            (viewModel.internalData ?? (viewModel.internalData = new Dictionary<object, object>()))[key] = value;
        }

        /// <summary>
        /// Removes a value with the given key
        /// </summary>
        /// <param name="viewModel">View model instance</param>
        /// <param name="key">The key</param>
        /// <returns>
        /// True if the value was removed, otherwise false (internal map was null/empty or no such key)
        /// </returns>
        /// <exception cref="NullReferenceException">The view model is null</exception>
        /// <exception cref="ArgumentNullException">The key is null</exception>
        public static bool RemoveInternalData(BaseViewModel viewModel, object key)
        {
            if (viewModel == null)
                throw new NullReferenceException(nameof(viewModel));
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            Dictionary<object, object> map = viewModel.internalData;
            return map != null && map.Remove(key);
        }

        #endregion
    }
}