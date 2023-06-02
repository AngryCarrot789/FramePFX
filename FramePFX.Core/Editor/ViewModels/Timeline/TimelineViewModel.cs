using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows.Input;
using FramePFX.Core.Editor.Timeline;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ViewModels.Timeline {
    public class TimelineViewModel : BaseViewModel {
        private readonly ObservableCollectionEx<TimelineLayerViewModel> layers;
        public ReadOnlyObservableCollection<TimelineLayerViewModel> Layers { get; }

        private IList<TimelineLayerViewModel> selectedLayers = new List<TimelineLayerViewModel>();
        public IList<TimelineLayerViewModel> SelectedLayers {
            get => this.selectedLayers;
            set {
                this.RaisePropertyChanged(ref this.selectedLayers, value ?? new List<TimelineLayerViewModel>());
                this.OnSelectionChanged();
            }
        }

        private TimelineLayerViewModel primarySelectedLayer;
        public TimelineLayerViewModel PrimarySelectedLayer {
            get => this.primarySelectedLayer;
            set => this.RaisePropertyChanged(ref this.primarySelectedLayer, value);
        }

        public AsyncRelayCommand RemoveSelectedLayersCommand { get; }
        public RelayCommand MoveSelectedUpCommand { get; }
        public RelayCommand MoveSelectedDownCommand { get; }

        public AsyncRelayCommand AddVideoLayerCommand { get; }
        public AsyncRelayCommand AddAudioLayerCommand { get; }

        public ProjectViewModel Project { get; }

        public TimelineModel Model { get; }

        public TimelineViewModel(ProjectViewModel project, TimelineModel model) {
            this.Project = project ?? throw new ArgumentNullException(nameof(project));
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
            this.layers = new ObservableCollectionEx<TimelineLayerViewModel>();
            this.Layers = new ReadOnlyObservableCollection<TimelineLayerViewModel>(this.layers);
            this.RemoveSelectedLayersCommand = new AsyncRelayCommand(this.RemoveSelectedLayersAction, () => this.SelectedLayers.Count > 0);
            this.MoveSelectedUpCommand = new RelayCommand(this.MoveSelectedItemUpAction);
            this.MoveSelectedDownCommand = new RelayCommand(this.MoveSelectedItemDownAction);
            this.AddVideoLayerCommand = new AsyncRelayCommand(this.AddVideoLayerAction);
            this.AddAudioLayerCommand = new AsyncRelayCommand(this.AddAudioLayerAction, () => false);
            foreach (TimelineLayerModel layer in this.Model.Layers) {
                this.layers.Add(LayerRegistry.Instance.CreateViewModelFromModel(this, layer));
            }
        }

        public async Task AddVideoLayerAction() {

        }

        public async Task AddAudioLayerAction() {

        }

        public Task RemoveSelectedLayersAction() {
            return this.RemoveSelectedLayersAction(true);
        }

        public async Task RemoveSelectedLayersAction(bool confirm) {
            IList<TimelineLayerViewModel> list = this.SelectedLayers;
            if (list.Count < 1) {
                return;
            }

            string msg = list.Count == 1 ? "1 layer" : $"{list.Count} layers";
            if (confirm && !await IoC.MessageDialogs.ShowYesNoDialogAsync("Delete layers?", $"Are you sure you want to delete {msg}?")) {
                return;
            }

            await this.DisposeAndRemoveItemsAction(list);
        }

        public async Task DisposeAndRemoveItemsAction(IEnumerable<TimelineLayerViewModel> list) {
            try {
                this.DisposeAndRemoveItemsUnsafe(list.ToList());
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageExAsync("Error", "An error occurred while removing layers", e.GetToString());
            }
        }

        public void DisposeAndRemoveItemsUnsafe(IList<TimelineLayerViewModel> list) {
            using (ExceptionStack stack = new ExceptionStack("Exception disposing layers")) {
                foreach (TimelineLayerViewModel item in list) {
                    if (item is IDisposable disposable) {
                        try {
                            disposable.Dispose();
                        }
                        catch (Exception e) {
                            stack.Push(new Exception($"Failed to dispose {item.GetType()} properly", e));
                        }
                    }

                    this.layers.Remove(item);
                }
            }
        }

        public virtual void MoveSelectedItems(int offset) {
            if (offset == 0 || this.SelectedLayers.Count < 1) {
                return;
            }

            List<int> selection = new List<int>();
            foreach (TimelineLayerViewModel item in this.SelectedLayers) {
                int index = this.layers.IndexOf(item);
                if (index < 0) {
                    continue;
                }

                selection.Add(index);
            }

            if (offset > 0) {
                selection.Sort((a, b) => b.CompareTo(a));
            }
            else {
                selection.Sort((a, b) => a.CompareTo(b));
            }

            for (int i = 0; i < selection.Count; i++) {
                int target = selection[i] + offset;
                if (target < 0 || target >= this.layers.Count || selection.Contains(target)) {
                    continue;
                }

                this.layers.Move(selection[i], target);
                selection[i] = target;
            }
        }

        public virtual void MoveSelectedItemUpAction() {
            this.MoveSelectedItems(-1);
        }

        public virtual void MoveSelectedItemDownAction() {
            this.MoveSelectedItems(1);
        }

        protected virtual void OnSelectionChanged() {
            this.RemoveSelectedLayersCommand.RaiseCanExecuteChanged();
        }

        public void Dispose() {
            using (ExceptionStack stack = new ExceptionStack("Exception disposing timeline")) {
                try {
                    this.DisposeCore(stack);
                }
                catch (Exception e) {
                    stack.Push(new Exception(nameof(this.DisposeCore) + " method unexpectedly threw", e));
                }
            }
        }

        protected virtual void DisposeCore(ExceptionStack stack) {
            using (ExceptionStack innerStack = new ExceptionStack(false)) {
                foreach (TimelineLayerViewModel clip in this.layers) {
                    try {
                        clip.Dispose();
                    }
                    catch (Exception e) {
                        innerStack.Push(e);
                    }
                }

                this.layers.Clear();
                if (innerStack.TryGetException(out Exception ex)) {
                    stack.Push(ex);
                }
            }
        }
    }
}