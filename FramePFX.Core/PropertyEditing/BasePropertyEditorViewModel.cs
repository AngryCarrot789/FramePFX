using System;
using System.Collections.Generic;
using System.Linq;

namespace FramePFX.Core.PropertyEditing
{
    /// <summary>
    /// The base property editor view model class for handling a single (or multiple) properties, and updating a collection of handlers
    /// </summary>
    public abstract class BasePropertyEditorViewModel : BasePropertyObjectViewModel
    {
        private static readonly List<object> EmptyList = new List<object>();

        private readonly Dictionary<object, PropertyHandler> handlerToDataMap;

        // this list is shared with all current property editors, in order to reduce memory usage

        /// <summary>
        /// The list of active handlers. This list is shared with other editors, so it must not be modified
        /// </summary>
        public IReadOnlyList<object> Handlers { get; private set; }

        /// <summary>
        /// The unordered backing collection of property handler data. This collection may be incomplete as handler data is created on-demand
        /// </summary>
        public IReadOnlyCollection<PropertyHandler> HandlerData => this.handlerToDataMap.Values;

        /// <summary>
        /// Whether or not there are handlers currently using this property editor. Inverse of <see cref="IsEmpty"/>
        /// </summary>
        public bool HasHandlers => this.Handlers.Count > 0;

        /// <summary>
        /// Whether or not there are no handlers currently using this property editor. Inverse of <see cref="HasHandlers"/>
        /// </summary>
        public bool IsEmpty => this.Handlers.Count < 1;

        /// <summary>
        /// Whether or not this editor has more than 1 handler
        /// </summary>
        public bool IsMultiSelection => this.Handlers.Count > 1;

        protected BasePropertyEditorViewModel(Type applicableType) : base(applicableType)
        {
            this.handlerToDataMap = new Dictionary<object, PropertyHandler>();
            this.Handlers = EmptyList;
        }

        /// <summary>
        /// Attempts to extract a value which is equal across all objects, using the given getter function
        /// </summary>
        /// <param name="objects">Input objects</param>
        /// <param name="getter">Getter function</param>
        /// <param name="equal">
        /// The value that is equal across all objects (will be set to <see cref="objects"/>[0]'s value)
        /// </param>
        /// <typeparam name="T">Type of object to get</typeparam>
        /// <returns>True if there is 1 object, or more than 1 and they have the same value, otherwise false</returns>
        public static bool GetEqualValue<T>(IReadOnlyList<object> objects, Func<object, T> getter, out T equal)
        {
            int count;
            if (objects == null || (count = objects.Count) < 1)
            {
                equal = default;
                return false;
            }

            equal = getter(objects[0]);
            if (count > 1)
            {
                EqualityComparer<T> comparator = EqualityComparer<T>.Default;
                for (int i = 1; i < count; i++)
                {
                    if (!comparator.Equals(getter(objects[i]), equal))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Clears this editor's active handlers. This will not clear the underlying list, and instead, assigns it to an empty list
        /// <para>
        /// If there are no handlers currently loaded, then this function does nothing
        /// </para>
        /// </summary>
        public void ClearHandlers()
        {
            if (this.Handlers.Count < 1)
            {
                return;
            }

            this.OnClearHandlers();
            this.Handlers = EmptyList;
            this.handlerToDataMap.Clear();
            this.IsCurrentlyApplicable = false;
            this.RaisePropertyChanged(nameof(this.HasHandlers));
            this.RaisePropertyChanged(nameof(this.IsEmpty));
            this.RaisePropertyChanged(nameof(this.IsMultiSelection));
        }

        /// <summary>
        /// Sets this editor's handler list, and calls <see cref="OnHandlersLoaded"/>. If there are currently
        /// handlers loaded, then <see cref="ClearHandlers"/> must be called before this function
        /// </summary>
        /// <param name="targets">A reference to the handler list, which will be stored in this editor</param>
        /// <exception cref="InvalidOperationException"><see cref="ClearHandlers"/> was not called, and there were handlers already loaded</exception>
        /// <exception cref="ArgumentException">An invalid number of objects were provided in relation to <see cref="BasePropertyObjectViewModel.HandlerCountMode"/></exception>
        public void SetHandlers(IReadOnlyList<object> targets)
        {
            if (this.Handlers.Count > 0)
            {
                throw new InvalidOperationException("Editor was not cleared before handlers were set again");
            }

            switch (this.HandlerCountMode)
            {
                case HandlerCountMode.Single when targets.Count != 1: throw new ArgumentException($"Expected list to contain only 1 handler, as {nameof(this.HandlerCountMode)} is {nameof(HandlerCountMode.Single)}");
                case HandlerCountMode.Multi when targets.Count < 2: throw new ArgumentException($"Expected list to contain more than 1 handler, as {nameof(this.HandlerCountMode)} is {nameof(HandlerCountMode.Multi)}");
            }

            foreach (object entry in targets)
            {
                this.handlerToDataMap[entry] = null;
            }

            this.Handlers = targets;
            this.OnHandlersLoaded();
            this.RaisePropertyChanged(nameof(this.HasHandlers));
            this.RaisePropertyChanged(nameof(this.IsEmpty));
            this.RaisePropertyChanged(nameof(this.IsMultiSelection));
        }

        /// <summary>
        /// Called just before the handlers are cleared. When this is cleared, there is guaranteed to be 1 or more loaded handlers
        /// </summary>
        protected virtual void OnClearHandlers()
        {
        }

        /// <summary>
        /// Called just after all handlers are fulled loaded. When this is cleared, there is guaranteed to be 1 or more loaded handlers
        /// </summary>
        protected virtual void OnHandlersLoaded()
        {
        }

        /// <summary>
        /// Creates a new instance of the property handler for a specific target.
        /// This is invoked on demand when an instance is required
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        protected virtual PropertyHandler NewHandler(object target) => new PropertyHandler(target);

        protected PropertyHandler GetHandlerData(object target)
        {
            PropertyHandler data = this.handlerToDataMap[target];
            if (data == null)
                this.handlerToDataMap[target] = data = this.NewHandler(target);
            return data;
        }

        protected PropertyHandler GetHandlerData(int index) => this.GetHandlerData(this.Handlers[index]);

        protected T GetHandlerData<T>(int index) where T : PropertyHandler => (T) this.GetHandlerData(index);

        protected IEnumerable<T> GetHandlerData<T>() where T : PropertyHandler => this.Handlers.Select(this.GetHandlerData).Cast<T>();

        /// <summary>
        /// Creates an instance of the <see cref="PropertyHandler"/> for each object currently loaded, to save dynamic creation
        /// <para>
        /// There are very few reason to use this
        /// </para>
        /// </summary>
        protected void PreallocateHandlerData()
        {
            foreach (object obj in this.Handlers)
            {
                this.handlerToDataMap[obj] = this.NewHandler(obj);
            }
        }
    }
}