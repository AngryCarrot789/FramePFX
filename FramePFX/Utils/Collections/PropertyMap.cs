using System;
using System.Collections.Generic;
using System.Threading;

namespace FramePFX.Utils.Collections {
    public delegate void PropertyMapEventHandler();

    /// <summary>
    /// A class that helps with mapping model properties to view model properties for when models raise property changed notifications
    /// </summary>
    public class PropertyMap {
        private readonly InheritanceDictionary<TypeEntry> map;
        private volatile int lockState;

        public PropertyMap() {
            this.map = new InheritanceDictionary<TypeEntry>();
        }

        private void AcquireLock() {
            Thread.BeginCriticalRegion();
            while (Interlocked.CompareExchange(ref this.lockState, 1, 0) != 0) {
                Thread.Sleep(1);
            }
        }

        private void ReleaseLock() {
            this.lockState = 0;
            Thread.EndCriticalRegion();
        }

        private TypeEntry GetTypeEntry(Type modelType) {
            if (!this.map.TryGetLocalValue(modelType, out TypeEntry entry))
                this.map[modelType] = entry = new TypeEntry(modelType);
            return entry;
        }

        public void AddTranslation(Type modelType, string modelProperty, string viewModelProperty) {
            this.AcquireLock();
            try {
                this.GetTypeEntry(modelType).AddTranslation(modelProperty, viewModelProperty);
            }
            finally {
                this.ReleaseLock();
            }
        }

        // public void AddHandler(Type modelType, string modelProperty, PropertyMapEventHandler handler) {
        //     this.AcquireLock();
        //     try {
        //         this.GetTypeEntry(modelType).AddHandler(modelProperty, handler);
        //     }
        //     finally {
        //         this.ReleaseLock();
        //     }
        // }

        public bool GetPropertyForModel(Type modelType, string modelProperty, out string viewModelProperty) {
            this.AcquireLock();
            try {
                var enumerator = this.map.GetLocalValueEnumerator(modelType);
                while (enumerator.MoveNext()) {
                    TypeEntry entry = enumerator.Current.LocalValue;
                    if (entry.modelToViewModel.TryGetValue(modelProperty, out viewModelProperty)) {
                        return true;
                    }
                }

                viewModelProperty = null;
                return false;
            }
            finally {
                this.ReleaseLock();
            }
        }

        private class TypeEntry {
            public readonly Type modelType;
            public readonly Dictionary<string, string> modelToViewModel;
            public readonly Dictionary<string, PropertyMapEventHandler> handlers;

            public TypeEntry(Type modelType) {
                this.modelType = modelType;
                this.modelToViewModel = new Dictionary<string, string>();
                this.handlers = new Dictionary<string, PropertyMapEventHandler>();
            }

            public void AddTranslation(string modelProperty, string viewModelProperty) {
                this.modelToViewModel.Add(modelProperty, viewModelProperty);
            }

            public void AddHandler(string modelProperty, PropertyMapEventHandler handler) {
                if (handler == null)
                    throw new ArgumentNullException(nameof(handler));

                if (!this.handlers.TryGetValue(modelProperty, out PropertyMapEventHandler oldHandler)) {
                    this.handlers[modelProperty] = handler;
                }
                else {
                    this.handlers[modelProperty] = (PropertyMapEventHandler) Delegate.Combine(oldHandler, handler);
                }
            }
        }
    }
}