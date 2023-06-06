using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Core.Editor.Timeline;
using FramePFX.Core.Editor.Timeline.Layers;
using FramePFX.Core.Editor.ViewModels.Timeline.Layers;
using FramePFX.Core.Utils;
using FramePFX.Core.Views.Dialogs.UserInputs;

namespace FramePFX.Core.Editor.ViewModels.Timeline {
    public class TimelineViewModel : BaseViewModel {
        private readonly ObservableCollectionEx<LayerViewModel> layers;
        public ReadOnlyObservableCollection<LayerViewModel> Layers { get; }

        public ObservableCollectionEx<LayerViewModel> SelectedLayers { get; }

        private LayerViewModel primarySelectedLayer;
        private bool ignorePlayHeadPropertyChange;
        private bool isFramePropertyChangeScheduled;

        public LayerViewModel PrimarySelectedLayer {
            get => this.primarySelectedLayer;
            set => this.RaisePropertyChanged(ref this.primarySelectedLayer, value);
        }

        public long PlayHeadFrame {
            get => this.Model.PlayHead;
            set {
                long oldValue = this.Model.PlayHead;
                if (oldValue == value) {
                    return;
                }

                if (value >= this.MaxDuration) {
                    value = this.MaxDuration - 1;
                }

                if (value < 0) {
                    value = 0;
                }

                this.Model.PlayHead = value;
                if (!this.ignorePlayHeadPropertyChange) {
                    this.RaisePropertyChanged();
                    this.OnPlayHeadMoved(oldValue, value, true);
                }
            }
        }

        public long MaxDuration {
            get => this.Model.MaxDuration;
            set {
                this.Model.MaxDuration = value;
                this.RaisePropertyChanged();
            }
        }

        public AsyncRelayCommand RemoveSelectedLayersCommand { get; }
        public RelayCommand MoveSelectedUpCommand { get; }
        public RelayCommand MoveSelectedDownCommand { get; }

        public AsyncRelayCommand AddVideoLayerCommand { get; }
        public AsyncRelayCommand AddAudioLayerCommand { get; }

        public ProjectViewModel Project { get; }

        public TimelineModel Model { get; }
        public InputValidator LayerNameValidator { get; set; }

        public TimelineViewModel(ProjectViewModel project, TimelineModel model) {
            this.Project = project ?? throw new ArgumentNullException(nameof(project));
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
            this.layers = new ObservableCollectionEx<LayerViewModel>();
            this.Layers = new ReadOnlyObservableCollection<LayerViewModel>(this.layers);
            this.SelectedLayers = new ObservableCollectionEx<LayerViewModel>();
            this.SelectedLayers.CollectionChanged += (sender, args) => {
                this.RemoveSelectedLayersCommand.RaiseCanExecuteChanged();
            };
            this.RemoveSelectedLayersCommand = new AsyncRelayCommand(this.RemoveSelectedLayersAction, () => this.SelectedLayers.Count > 0);
            this.MoveSelectedUpCommand = new RelayCommand(this.MoveSelectedItemUpAction);
            this.MoveSelectedDownCommand = new RelayCommand(this.MoveSelectedItemDownAction);
            this.AddVideoLayerCommand = new AsyncRelayCommand(this.AddVideoLayerAction);
            this.AddAudioLayerCommand = new AsyncRelayCommand(this.AddAudioLayerAction, () => false);
            this.LayerNameValidator = InputValidator.FromFunc((x) => string.IsNullOrEmpty(x) ? "Layer name cannot be empty" : null);
            foreach (LayerModel layer in this.Model.Layers) {
                this.layers.Add(LayerRegistry.Instance.CreateViewModelFromModel(this, layer));
            }

            this.AddLayer(new VideoLayerViewModel(this, new VideoLayerModel(this.Model)) {
                Name = "Video Layer 1"
            });
        }

        public void AddLayer(VideoLayerViewModel layer, bool addToModel = true) {
            if (addToModel)
                this.Model.AddLayer(layer.Model);
            this.layers.Add(layer);
        }

        public void OnPlayHeadMoved(long oldFrame, long newFrame, bool? schedule) {
            if (oldFrame == newFrame || !(schedule is bool b)) {
                return;
            }

            this.Project.Editor.View.RenderViewPort(b);
        }

        // TODO: Could optimise this, maybe create "chunks" of clips that span 10 frame sections across the entire timeline
        public IEnumerable<ClipViewModel> GetClipsAtPlayHead() {
            return this.GetClipsAtFrame(this.PlayHeadFrame);
        }

        public IEnumerable<ClipViewModel> GetClipsAtFrame(long frame) {
            return this.Layers.SelectMany(layer => layer.GetClipsAtFrame(frame));
        }

        public async Task<VideoLayerViewModel> AddVideoLayerAction() {
            VideoLayerViewModel layer = new VideoLayerViewModel(this, new VideoLayerModel(this.Model));
            this.AddLayer(layer);
            this.DoRender(true);
            return layer;
        }

        public void DoRender(bool schedule = false) {
            this.Project.Editor?.DoRender(schedule);
        }

        public async Task AddAudioLayerAction() {

        }

        public Task RemoveSelectedLayersAction() {
            return this.RemoveSelectedLayersAction(true);
        }

        public async Task RemoveSelectedLayersAction(bool confirm) {
            IList<LayerViewModel> list = this.SelectedLayers;
            if (list.Count < 1) {
                return;
            }

            string msg = list.Count == 1 ? "1 layer" : $"{list.Count} layers";
            if (confirm && !await IoC.MessageDialogs.ShowYesNoDialogAsync("Delete layers?", $"Are you sure you want to delete {msg}?")) {
                return;
            }

            await this.DisposeAndRemoveItemsAction(list);
        }

        public async Task DisposeAndRemoveItemsAction(IEnumerable<LayerViewModel> list) {
            try {
                this.DisposeAndRemoveItemsUnsafe(list.ToList());
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageExAsync("Error", "An error occurred while removing layers", e.GetToString());
            }
        }

        public void DisposeAndRemoveItemsUnsafe(IList<LayerViewModel> list) {
            using (ExceptionStack stack = new ExceptionStack("Exception disposing layers")) {
                foreach (LayerViewModel item in list) {
                    if (item is IDisposable disposable) {
                        try {
                            disposable.Dispose();
                        }
                        catch (Exception e) {
                            stack.Push(new Exception($"Failed to dispose {item.GetType()} properly", e));
                        }
                    }

                    this.layers.Remove(item);
                    this.Model.RemoveLayer(item.Model);
                }
            }
        }

        public virtual void MoveSelectedItems(int offset) {
            if (offset == 0 || this.SelectedLayers.Count < 1) {
                return;
            }

            List<int> selection = new List<int>();
            foreach (LayerViewModel item in this.SelectedLayers) {
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
                this.Model.Layers.MoveItem(selection[i], target);
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

        public void OnStepFrameTick() {
            this.StepFrame();
        }

        public void StepFrame(long change = 1L, bool schedule = false) {
            this.ignorePlayHeadPropertyChange = true;
            long oldFrame = this.PlayHeadFrame;
            this.PlayHeadFrame = Periodic.Add(oldFrame, change, 0L, this.MaxDuration);
            this.OnPlayHeadMoved(oldFrame, this.PlayHeadFrame, schedule);
            if (!this.isFramePropertyChangeScheduled) {
                this.isFramePropertyChangeScheduled = true;
                IoC.Dispatcher.Invoke(() => {
                    this.RaisePropertyChanged(nameof(this.PlayHeadFrame));
                    this.isFramePropertyChangeScheduled = false;
                });
            }

            this.ignorePlayHeadPropertyChange = false;
        }

        public void OnPlayBegin() {
            foreach (LayerViewModel layer in this.layers) {
                foreach (ClipViewModel clip in layer.Clips) {
                    clip.OnTimelinePlayBegin();
                }
            }
        }

        public void OnPlayEnd() {
            foreach (LayerViewModel layer in this.layers) {
                foreach (ClipViewModel clip in layer.Clips) {
                    clip.OnTimelinePlayEnd();
                }
            }
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
                foreach (LayerViewModel clip in this.layers) {
                    try {
                        clip.Dispose();
                    }
                    catch (Exception e) {
                        innerStack.Push(e);
                    }
                }

                this.layers.Clear();
                this.Model.ClearLayers();
                if (innerStack.TryGetException(out Exception ex)) {
                    stack.Push(ex);
                }
            }
        }

        public LayerViewModel GetPrevious(LayerViewModel layer) {
            int index = this.layers.IndexOf(layer);
            return index > 0 ? this.layers[index - 1] : null;
        }
    }
}