using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FramePFX.Core.Actions.Contexts;

namespace FramePFX.Core.AdvancedContextService.Base {
    public class BaseInteractableEntry : BaseViewModel, IContextEntry, IDataContext {
        private Dictionary<string, object> additionalData;
        private readonly ObservableCollection<object> context;

        public IEnumerable<object> Context => this.context;

        public IEnumerable<(string, object)> CustomData => this.additionalData != null ? this.additionalData.Select(x => (x.Key, x.Value)) : Enumerable.Empty<(string, object)>();

        protected BaseInteractableEntry(object dataContext) {
            this.context = new ObservableCollection<object> {
                dataContext
            };
        }

        public void AddContext(object context) {
            this.context.Add(context);
        }

        public T GetContext<T>() {
            this.TryGetContext(out T value);
            return value;
        }

        public bool TryGetContext<T>(out T value) {
            foreach (object obj in this.context) {
                if (obj is T t) {
                    value = t;
                    return true;
                }
            }

            value = default;
            return false;
        }

        public bool TryGet<T>(string key, out T value) {
            return DefaultDataContext.TryGetData(this.additionalData, key, out value);
        }

        public T Get<T>(string key) {
            this.TryGet(key, out T value);
            return value; // ValueType will be default, object will be null
        }

        public void Set(string key, object value) {
            DefaultDataContext.SetData(ref this.additionalData, key, value);
        }
    }
}