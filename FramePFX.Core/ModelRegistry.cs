using System;
using System.Collections.Generic;

namespace FramePFX.Core {
    public class ModelRegistry<TModel, TViewModel> where TModel : class where TViewModel : BaseViewModel {
        private readonly Dictionary<string, Entry> IdToRegistry;
        private readonly Dictionary<Type, Entry> ViewModelToRegistry;
        private readonly Dictionary<Type, Entry> ModelToRegistry;

        public ModelRegistry() {
            this.IdToRegistry = new Dictionary<string, Entry>();
            this.ViewModelToRegistry = new Dictionary<Type, Entry>();
            this.ModelToRegistry = new Dictionary<Type, Entry>();
        }

        protected void Register<TCustomModel, TCustomViewModel>(string id) where TCustomModel : TModel where TCustomViewModel : TViewModel {
            this.RegisterUnsafe(id, typeof(TCustomModel), typeof(TCustomViewModel));
        }

        protected void RegisterUnsafe(string id, Type modelType, Type viewModelType) {
            this.ValidateId(id);
            this.AddEntry(new Entry(id, modelType, viewModelType));
        }

        private void ValidateId(string id) {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("ID cannot be null or empty", nameof(id));
            if (this.IdToRegistry.ContainsKey(id))
                throw new Exception($"A registration already exists with the id {id}");
        }

        public Type GetModelType(string id) {
            return this.GetEntry(id).ModelType;
        }

        public Type GetViewModelType(string id) {
            return this.GetEntry(id).ViewModelType;
        }

        public Type GetViewModelTypeFromModel(TModel model) {
            if (this.ModelToRegistry.TryGetValue(model.GetType(), out Entry entry)) {
                return entry.ViewModelType;
            }

            throw new Exception($"No such registration for model type: {model.GetType()}");
        }

        public string GetTypeId(TModel model) {
            return this.GetTypeIdForModel(model.GetType());
        }

        public string GetTypeId(TViewModel model) {
            return this.GetTypeIdForViewModel(model.GetType());
        }

        public string GetTypeIdForModel(Type modelType) {
            return this.ModelToRegistry.TryGetValue(modelType, out Entry entry) ? entry.Id : null;
        }

        public string GetTypeIdForViewModel(Type viewModelType) {
            return this.ViewModelToRegistry.TryGetValue(viewModelType, out Entry entry) ? entry.Id : null;
        }

        private void AddEntry(Entry entry) {
            this.IdToRegistry[entry.Id] = entry;
            this.ModelToRegistry[entry.ModelType] = entry;
            this.ViewModelToRegistry[entry.ViewModelType] = entry;
        }

        protected Entry GetEntry(string id) {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("ID cannot be null or empty", nameof(id));
            if (!this.IdToRegistry.TryGetValue(id, out var entry))
                throw new Exception($"No such registration with id: {id}");
            return entry;
        }

        protected class Entry {
            public readonly string Id;
            public readonly Type ModelType;
            public readonly Type ViewModelType;

            public Entry(string id, Type modelType, Type viewModelType) {
                this.Id = id;
                this.ModelType = modelType;
                this.ViewModelType = viewModelType;
            }
        }
    }
}