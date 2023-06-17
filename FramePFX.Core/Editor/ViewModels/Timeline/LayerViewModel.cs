using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FramePFX.Core.Editor.History;
using FramePFX.Core.Editor.Registries;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Editor.Timeline;
using FramePFX.Core.History;
using FramePFX.Core.History.Tasks;
using FramePFX.Core.History.ViewModels;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ViewModels.Timeline {
    /// <summary>
    /// The base view model for a timeline layer. This could be a video or audio layer (or others...)
    /// </summary>
    public abstract class LayerViewModel : BaseViewModel, IModifyProject, IHistoryHolder, IDisplayName, IResourceItemDropHandler {
        protected readonly HistoryBuffer<HistoryLayerDisplayName> displayNameHistory = new HistoryBuffer<HistoryLayerDisplayName>();

        private readonly ObservableCollectionEx<ClipViewModel> clips;
        public ReadOnlyObservableCollection<ClipViewModel> Clips { get; }

        public ObservableCollectionEx<ClipViewModel> SelectedClips { get; }

        private ClipViewModel primarySelectedClip;
        public ClipViewModel PrimarySelectedClip {
            get => this.primarySelectedClip;
            set => this.RaisePropertyChanged(ref this.primarySelectedClip, value);
        }

        public string DisplayName {
            get => this.Model.DisplayName;
            set {
                if (!this.IsHistoryChanging) {
                    if (!this.displayNameHistory.TryGetAction(out HistoryLayerDisplayName action))
                        this.displayNameHistory.PushAction(this.HistoryManager, action = new HistoryLayerDisplayName(this), "Edit media duration");
                    action.DisplayName.SetCurrent(value);
                }

                this.Model.DisplayName = value;
                this.RaisePropertyChanged();
            }
        }

        public double MinHeight {
            get => this.Model.MinHeight;
            set {
                this.Model.MinHeight = value;
                this.RaisePropertyChanged();
            }
        }

        public double MaxHeight {
            get => this.Model.MaxHeight;
            set {
                this.Model.MaxHeight = value;
                this.RaisePropertyChanged();
            }
        }

        public double Height {
            get => this.Model.Height;
            set {
                this.Model.Height = Math.Max(Math.Min(value, this.MaxHeight), this.MinHeight);
                this.RaisePropertyChanged();
            }
        }

        // Feels so wrong having colours here for some reason... should be done in a converter with enums maybe?
        public string LayerColour {
            get => this.Model.LayerColour;
            set {
                this.Model.LayerColour = value;
                this.RaisePropertyChanged();
            }
        }

        public bool IsHistoryChanging { get; set; }

        public AsyncRelayCommand RenameLayerCommand { get; }

        public AsyncRelayCommand RemoveSelectedClipsCommand { get; }

        public TimelineViewModel Timeline { get; }

        public ProjectViewModel Project => this.Timeline?.Project;

        public VideoEditorViewModel Editor => this.Timeline?.Project.Editor;

        public HistoryManagerViewModel HistoryManager => this.Timeline?.Project.Editor.HistoryManager;


        public LayerModel Model { get; }

        public event ProjectModifiedEvent ProjectModified;

        protected LayerViewModel(TimelineViewModel timeline, LayerModel model) {
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
            this.Timeline = timeline ?? throw new ArgumentNullException(nameof(timeline));
            if (!ReferenceEquals(timeline.Model, model.Timeline))
                throw new ArgumentException($"The timeline's model and then given layer model's timeline do not match");
            this.clips = new ObservableCollectionEx<ClipViewModel>();
            this.Clips = new ReadOnlyObservableCollection<ClipViewModel>(this.clips);
            this.SelectedClips = new ObservableCollectionEx<ClipViewModel>();
            this.SelectedClips.CollectionChanged += (sender, args) => {
                this.RemoveSelectedClipsCommand.RaiseCanExecuteChanged();
                this.Timeline.Project.Editor?.View.UpdateSelectionPropertyPages();
            };
            this.RemoveSelectedClipsCommand = new AsyncRelayCommand(this.RemoveSelectedClipsAction, () => this.SelectedClips.Count > 0);
            this.RenameLayerCommand = new AsyncRelayCommand(async () => {
                string result = await IoC.UserInput.ShowSingleInputDialogAsync("Change layer name", "Input a new layer name:", this.DisplayName ?? "", this.Timeline.LayerNameValidator);
                if (result != null) {
                    this.DisplayName = result;
                }
            });

            foreach (ClipModel clip in model.Clips) {
                this.CreateClip(clip, false);
            }
        }

        public virtual void OnProjectModified(object sender, [CallerMemberName] string property = null) {
            this.ProjectModified?.Invoke(sender, property);
        }

        public ClipViewModel CreateClip(ClipModel model, bool addToModel = true) {
            ClipViewModel vm = ClipRegistry.Instance.CreateViewModelFromModel(model);
            this.AddClipToLayer(vm, addToModel);
            return vm;
        }

        public bool RemoveClipFromLayer(ClipViewModel clip, bool removeFromModel = true) {
            int index = this.clips.IndexOf(clip);
            if (index < 0) {
                return false;
            }

            this.RemoveClipFromLayer(index, removeFromModel);
            return true;
        }

        public void RemoveClipFromLayer(int index, bool removeFromModel = true) {
            ClipViewModel clip = this.clips[index];
            if (!ReferenceEquals(this, clip.Layer))
                throw new Exception($"Clip layer does not match the current instance: {clip.Layer} != {this}");
            if (!ReferenceEquals(this.Model.Clips[index], clip.Model))
                throw new Exception($"Layer model clip list desynchronized");

            if (removeFromModel) {
                this.Model.RemoveClipAt(index, false);
            }

            this.clips.RemoveAt(index);
            ClipViewModel.SetLayer(clip, null);

            clip.RaisePropertyChanged(nameof(clip.Layer));
        }

        public void AddClipToLayer(int index, ClipViewModel clip, bool addToModel = true) {
            if (index < 0 || index > this.clips.Count)
                throw new IndexOutOfRangeException($"Index < 0 || Index > Count. Index = {index}, Count = {this.clips.Count}");
            if (ReferenceEquals(this, clip.Layer))
                throw new InvalidOperationException("Attempted to add clip to a layer it was already in");

            if (addToModel) {
                this.Model.InsertClip(index, clip.Model, false);
            }

            this.clips.Insert(index, clip);
            ClipViewModel.SetLayer(clip, this);
            clip.RaisePropertyChanged(nameof(clip.Layer));
        }

        public void AddClipToLayer(ClipViewModel clip, bool addToModel = true) {
            this.AddClipToLayer(this.clips.Count, clip, addToModel);
        }

        public Task RemoveSelectedClipsAction() {
            return this.RemoveSelectedClipsAction(true);
        }

        public async Task RemoveSelectedClipsAction(bool confirm) {
            IList<ClipViewModel> list = this.SelectedClips;
            if (list.Count < 1) {
                return;
            }

            if (confirm && !await IoC.MessageDialogs.ShowYesNoDialogAsync($"Delete clip{(list.Count == 1 ? "" : "s")}?", $"Are you sure you want to delete {(list.Count == 1 ? "1 clip" : $"{list.Count} clips")}?")) {
                return;
            }

            await this.DisposeAndRemoveItemsAction(list);
        }

        public async Task DisposeAndRemoveItemsAction(IEnumerable<ClipViewModel> list) {
            try {
                this.DisposeAndRemoveItemsUnsafe(list.ToList());
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageExAsync("Error", "An error occurred while removing clips", e.GetToString());
            }
        }

        public void DisposeAndRemoveItemsUnsafe(IList<ClipViewModel> list) {
            using (ExceptionStack stack = new ExceptionStack("Exception disposing clips")) {
                foreach (ClipViewModel clip in list) {
                    int index = this.clips.IndexOf(clip);
                    if (index < 0) {
                        continue;
                    }

                    Validate.Exception(index < this.Model.Clips.Count, "Model-ViewModel list desynchronized");
                    Validate.Exception(ReferenceEquals(clip.Model, this.Model.Clips[index]), "Model-ViewModel list desynchronized");
                    this.RemoveClipFromLayer(index);
                    if (clip is IDisposable disposable) {
                        try {
                            disposable.Dispose();
                        }
                        catch (Exception e) {
                            stack.Add(new Exception($"Failed to dispose {clip.GetType()} properly", e));
                        }
                    }
                }
            }
        }

        public void Dispose() {
            using (ExceptionStack stack = new ExceptionStack("Exception disposing layer")) {
                try {
                    this.DisposeCore(stack);
                }
                catch (Exception e) {
                    stack.Add(new Exception(nameof(this.DisposeCore) + " method unexpectedly threw", e));
                }
            }
        }

        protected virtual void DisposeCore(ExceptionStack stack) {
            using (ExceptionStack innerStack = new ExceptionStack("Exception disposing a clip", false)) {
                for (int i = this.clips.Count - 1; i >= 0; i--) {
                    ClipViewModel clip = this.clips[i];

                    try {
                        this.RemoveClipFromLayer(i);
                    }
                    catch (Exception e) {
                        innerStack.Add(new Exception("Failed to remove clip from layer", e));
                    }

                    try {
                        clip.Dispose();
                    }
                    catch (Exception e) {
                        innerStack.Add(new Exception($"Failed to dispose clip: {clip}", e));
                    }
                }

                this.clips.Clear();
                this.Model.Clips.Clear();
                if (innerStack.TryGetException(out Exception ex)) {
                    stack.Add(ex);
                }
            }
        }

        public IEnumerable<ClipViewModel> GetClipsAtFrame(long frame) {
            return this.Clips.Where(clip => clip.IntersectsFrameAt(frame));
        }

        public void MakeTopMost(ClipViewModel clip) {
            int endIndex = this.Clips.Count - 1;
            int index = this.Clips.IndexOf(clip);
            if (index == -1 || index == endIndex) {
                return;
            }

            this.clips.Move(index, endIndex);
            this.Model.Clips.MoveItem(index, endIndex);
        }

        public abstract bool CanDropResource(ResourceItemViewModel resource);

        public abstract Task OnResourceDropped(ResourceItemViewModel resource, long frameBegin);

        /// <summary>
        /// Slices the given clip at the given frame
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        public Task SliceClipAction(ClipViewModel clip, long frame) {
            FrameSpan span = clip.Model.FrameSpan;
            if (frame <= span.Begin || frame >= span.EndIndex) {
                return Task.CompletedTask;
            }

            ClipModel cloned = clip.Model.Clone();
            cloned.FrameSpan = FrameSpan.FromIndex(frame, span.EndIndex);
            cloned.MediaFrameOffset = frame - span.Begin;
            clip.FrameSpan = span.SetEndIndex(frame);
            this.CreateClip(cloned);
            return Task.CompletedTask;
        }

        public bool CanAccept(ClipViewModel clip) {
            return this.Model.CanAccept(clip.Model);
        }
    }
}