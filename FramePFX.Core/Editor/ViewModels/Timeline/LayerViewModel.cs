using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceManaging;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.Editor.Timeline;
using FramePFX.Core.Editor.Timeline.Clip;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ViewModels.Timeline {
    /// <summary>
    /// The base view model for a timeline layer. This could be a video or audio layer (or others...)
    /// </summary>
    public abstract class LayerViewModel : BaseViewModel, ILayerDropable {
        private readonly ObservableCollectionEx<ClipViewModel> clips;
        public ReadOnlyObservableCollection<ClipViewModel> Clips { get; }

        public ObservableCollectionEx<ClipViewModel> SelectedClips { get; }

        private ClipViewModel primarySelectedClip;
        public ClipViewModel PrimarySelectedClip {
            get => this.primarySelectedClip;
            set => this.RaisePropertyChanged(ref this.primarySelectedClip, value);
        }

        public string Name {
            get => this.Model.Name;
            set {
                this.Model.Name = value;
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

        public AsyncRelayCommand RenameLayerCommand { get; }

        public AsyncRelayCommand RemoveSelectedClipsCommand { get; }

        public TimelineViewModel Timeline { get; }

        public LayerModel Model { get; }

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
                string result = await IoC.UserInput.ShowSingleInputDialogAsync("Change layer name", "Input a new layer name:", this.Name ?? "", this.Timeline.LayerNameValidator);
                if (result != null) {
                    this.Name = result;
                }
            });

            foreach (ClipModel clip in model.Clips) {
                this.CreateClip(clip, false);
            }
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
            if (!ReferenceEquals(this, clip.Layer)) {
                throw new Exception($"Clip layer does not match the current instance: {clip.Layer} != {this}");
            }

            if (!ReferenceEquals(this.Model.Clips[index], clip.Model)) {
                throw new Exception($"Layer model clip list desynchronized");
            }

            using (ExceptionStack stack = new ExceptionStack()) {
                if (removeFromModel) {
                    this.Model.RemoveClipAt(index, false);
                }

                this.clips.RemoveAt(index);
                ClipViewModel.SetLayer(clip, null);

                try {
                    clip.RaisePropertyChanged(nameof(clip.Layer));
                }
                catch (Exception e) {
                    stack.Push(new Exception($"Failed to raise clip's property changed event for layer", e));
                }
            }
        }

        public void AddClipToLayer(int index, ClipViewModel clip, bool addToModel = true) {
            if (index < 0 || index > this.clips.Count) {
                throw new IndexOutOfRangeException($"Index < 0 || Index > Count. Index = {index}, Count = {this.clips.Count}");
            }

            Validate.Exception(!ReferenceEquals(this, clip.Layer), "Attempted to add clip to a layer it was already in");
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
                            stack.Push(new Exception($"Failed to dispose {clip.GetType()} properly", e));
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
                    stack.Push(new Exception(nameof(this.DisposeCore) + " method unexpectedly threw", e));
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
                        innerStack.Push(new Exception("Failed to remove clip from layer", e));
                    }

                    try {
                        clip.Dispose();
                    }
                    catch (Exception e) {
                        innerStack.Push(new Exception($"Failed to dispose clip: {clip}", e));
                    }
                }

                this.clips.Clear();
                this.Model.Clips.Clear();
                if (innerStack.TryGetException(out Exception ex)) {
                    stack.Push(ex);
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

        public async Task OnResourceDropped(ResourceItem resource, long frameBegin) {
            Validate.Exception(!string.IsNullOrEmpty(resource.UniqueId), "Expected valid resource UniqueId");
            Validate.Exception(resource.IsRegistered, "Expected resource to be registered");

            double fps = this.Timeline.Project.Settings.FrameRate;
            long defaultDuration = (long) (fps * 5);

            ClipModel newClip = null;
            if (resource is ResourceMedia media) {
                media.OpenMediaFromFile();
                TimeSpan span = media.GetDuration();
                long dur = (long) Math.Floor(span.TotalSeconds * fps);
                if (dur < 2) {
                    // image files are 1
                    dur = defaultDuration;
                }

                if (dur > 0) {
                    long newProjectDuration = frameBegin + dur + 600;
                    if (newProjectDuration > this.Timeline.MaxDuration) {
                        this.Timeline.MaxDuration = newProjectDuration;
                    }

                    MediaClipModel clip = new MediaClipModel() {
                        FrameSpan = new ClipSpan(frameBegin, dur),
                        DisplayName = media.UniqueId
                    };

                    clip.SetTargetResourceId(media.UniqueId);
                    newClip = clip;
                }
                else {
                    await IoC.MessageDialogs.ShowMessageAsync("Invalid media", "This media has a duration of 0 and cannot be added to the timeline");
                    return;
                }
            }
            else {
                if (resource is ResourceColour argb) {
                    ShapeClipModel clip = new ShapeClipModel() {
                        FrameSpan = new ClipSpan(frameBegin, defaultDuration),
                        Width = 200, Height = 200,
                        DisplayName = argb.UniqueId
                    };

                    clip.SetTargetResourceId(argb.UniqueId);
                    newClip = clip;
                }
                else if (resource is ResourceImage img) {
                    ImageClipModel clip = new ImageClipModel() {
                        FrameSpan = new ClipSpan(frameBegin, defaultDuration),
                        DisplayName = img.UniqueId
                    };

                    clip.SetTargetResourceId(img.UniqueId);
                    newClip = clip;
                }
                else if (resource is ResourceText text) {
                    TextClipModel clip = new TextClipModel() {
                        FrameSpan = new ClipSpan(frameBegin, defaultDuration),
                        DisplayName = text.UniqueId
                    };

                    clip.SetTargetResourceId(text.UniqueId);
                    newClip = clip;
                }
                else {
                    return;
                }
            }

            this.CreateClip(newClip);
            if (newClip is VideoClipModel videoClipModel) {
                videoClipModel.InvalidateRender();
            }
        }

        /// <summary>
        /// Slices the given clip at the given frame. The given frame will always be above the clip's start frame and less than the clip's end frame; it is guaranteed to intersect
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        public abstract Task SliceClipAction(ClipViewModel clip, long frame);
    }
}