using System;
using System.Collections.Generic;
using FFmpeg.AutoGen;

namespace FramePFX {
    /// <summary>
    /// A base class for view models which map a model event to a view model event
    /// </summary>
    public class AdvancedBaseViewModel : BaseViewModel {
        // typeof(model) -> registration info
        private static readonly Dictionary<Type, M2VMInfo> ModelRegistryInfo = new Dictionary<Type, M2VMInfo>();
        private static readonly Dictionary<Type, VM2MInfo> ViewModelRegistryInfo = new Dictionary<Type, VM2MInfo>();

        private Dictionary<object, List<string>> modelList;

        public AdvancedBaseViewModel() {
        }

        protected void FireModelChanged<T>(string property) {
            if (ModelRegistryInfo.TryGetValue(typeof(T), out M2VMInfo info)) {
                if (info.modelToViewModel.TryGetValue(property, out var list)) {
                    foreach (string vmProp in list) {
                        this.RaisePropertyChanged(vmProp);
                    }
                }
            }
        }

        /// <summary>
        /// Sets up a procedure that allows model property changed events to notify view models
        /// </summary>
        /// <param name="propModel"></param>
        /// <param name="propViewModel"></param>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TViewModel"></typeparam>
        public static void RegisterProperty<TModel, TViewModel>(string propModel, string propViewModel) where TModel : AdvancedBaseViewModel {
            if (!ModelRegistryInfo.TryGetValue(typeof(TModel), out M2VMInfo info)) {
                ModelRegistryInfo[typeof(TModel)] = info = new M2VMInfo(typeof(TModel), typeof(TViewModel));
            }

            if (!info.modelToViewModel.TryGetValue(propModel, out List<string> list)) {
                info.modelToViewModel[propModel] = list = new List<string>();
            }

            if (!list.Contains(propViewModel)) {
                list.Add(propViewModel);
            }
        }

        private class M2VMInfo {
            public readonly Type modelType;
            public readonly Type viewModelType;
            public readonly Dictionary<string, List<string>> modelToViewModel;

            public M2VMInfo(Type modelType, Type viewModelType) {
                this.modelType = modelType;
                this.viewModelType = viewModelType;
                this.modelToViewModel = new Dictionary<string, List<string>>();
            }
        }

        private class VM2MInfo {
            public readonly Type modelType;
            public readonly Type viewModelType;

            public VM2MInfo(Type modelType, Type viewModelType) {
                this.modelType = modelType;
                this.viewModelType = viewModelType;
            }
        }
    }
}