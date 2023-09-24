using System;
using System.Collections.Generic;

namespace FramePFX {
    /// <summary>
    /// A helper "registry" class, for mapping type of models to view models and the reverse, along with storing unique identifiers for a model-viewmodel entry
    /// </summary>
    /// <typeparam name="TModel">The type of model</typeparam>
    /// <typeparam name="TViewModel">The type of view model</typeparam>
    public class ModelFactory<TModel, TViewModel> where TModel : class where TViewModel : BaseViewModel {
        private readonly Dictionary<string, Entry> IdToEntry;
        private readonly Dictionary<Type, Entry> ViewModelToEntry;
        private readonly Dictionary<Type, Entry> ModelToEntry;

        public ModelFactory() {
            this.IdToEntry = new Dictionary<string, Entry>();
            this.ViewModelToEntry = new Dictionary<Type, Entry>();
            this.ModelToEntry = new Dictionary<Type, Entry>();
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
            if (this.IdToEntry.ContainsKey(id))
                throw new Exception($"A registration already exists with the id {id}");
        }

        public Type GetModelType(string id) {
            return this.GetEntry(id).ModelType;
        }

        public Type GetViewModelType(string id) {
            return this.GetEntry(id).ViewModelType;
        }

        public bool GetEntry(string id, out Type modelType, out Type viewModelType) {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("ID cannot be null or empty", nameof(id));
            if (this.IdToEntry.TryGetValue(id, out Entry entry)) {
                modelType = entry.ModelType;
                viewModelType = entry.ViewModelType;
                return true;
            }

            modelType = viewModelType = null;
            return false;
        }

        public Type GetViewModelTypeFromModel(TModel model) {
            if (this.ModelToEntry.TryGetValue(model.GetType(), out Entry entry)) {
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
            return this.ModelToEntry.TryGetValue(modelType, out Entry entry) ? entry.Id : null;
        }

        public string GetTypeIdForViewModel(Type viewModelType) {
            return this.ViewModelToEntry.TryGetValue(viewModelType, out Entry entry) ? entry.Id : null;
        }

        private void AddEntry(Entry entry) {
            this.IdToEntry[entry.Id] = entry;
            this.ModelToEntry[entry.ModelType] = entry;
            this.ViewModelToEntry[entry.ViewModelType] = entry;
        }

        protected Entry GetEntry(string id) {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("ID cannot be null or empty", nameof(id));
            if (!this.IdToEntry.TryGetValue(id, out var entry))
                throw new Exception($"No such registration with id: {id}");
            return entry;
        }

        protected TModel CreateModel(string id) {
            return (TModel) Activator.CreateInstance(this.GetModelType(id));
        }

        protected TModel CreateModel(string id, params object[] args) {
            return (TModel) Activator.CreateInstance(this.GetModelType(id), args);
        }

        protected TViewModel CreateViewModel(string id) {
            return this.CreateViewModelFromModel(this.CreateModel(id));
        }

        protected TViewModel CreateViewModel(string id, params object[] args) {
            return (TViewModel) Activator.CreateInstance(this.GetViewModelType(id), args);
        }

        protected TViewModel CreateViewModelFromModel(TModel model) {
            return (TViewModel) Activator.CreateInstance(this.GetViewModelTypeFromModel(model), model);
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