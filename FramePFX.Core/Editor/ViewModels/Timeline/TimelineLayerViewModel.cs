using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Editor.Timeline;
using FramePFX.Core.Editor.Timeline.Clip;
using FramePFX.Core.Editor.ViewModels.Timeline.Clips;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ViewModels.Timeline {
    /// <summary>
    /// The base view model for a timeline layer. This could be a video or audio layer (or others...)
    /// </summary>
    public abstract class TimelineLayerViewModel : BaseViewModel, IResourceDropNotifier {
        private readonly ObservableCollectionEx<ClipViewModel> clips;
        public ReadOnlyObservableCollection<ClipViewModel> Clips { get; }

        public ObservableCollectionEx<ClipViewModel> SelectedClips { get; }

        private ClipViewModel primarySelectedClip;
        public ClipViewModel PrimarySelectedClip {
            get => this.primarySelectedClip;
            set => this.RaisePropertyChanged(ref this.primarySelectedClip, value);
        }

        public float Opacity {
            get => this.Model.Opacity;
            set {
                this.Model.Opacity = value;
                this.RaisePropertyChanged();
            }
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

        public TimelineLayerModel Model { get; }

        protected TimelineLayerViewModel(TimelineViewModel timeline, TimelineLayerModel model) {
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
            this.Timeline = timeline ?? throw new ArgumentNullException(nameof(timeline));
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

        protected ClipViewModel CreateClip(ClipModel model, bool addToModel = true) {
            ClipViewModel vm = ClipRegistry.Instance.CreateViewModelFromModel(model);
            this.AddClip(vm, addToModel);
            return vm;
        }

        public void AddClip(ClipViewModel clip, bool addToModel = true) {
            clip.Layer = this;
            if (addToModel)
                this.Model.Clips.Add(clip.Model);
            this.clips.Add(clip);
        }

        public void AddClip(ClipViewModel clip, int index, bool addToModel = true) {
            clip.Layer = this;
            if (addToModel)
                this.Model.Clips.Insert(index, clip.Model);
            this.clips.Insert(index, clip);
        }

        public bool RemoveClip(ClipViewModel clip, bool removeFromModel = true) {
            int index = this.clips.IndexOf(clip);
            if (index < 0) {
                return false;
            }

            this.RemoveClip(clip, index, removeFromModel);
            return true;
        }

        public void RemoveClip(ClipViewModel clip, int index, bool removeFromModel = true) {
            if (removeFromModel)
                this.Model.Clips.RemoveAt(index);
            this.clips.RemoveAt(index);
            clip.Layer = null;
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

                    if (clip is IDisposable disposable) {
                        try {
                            disposable.Dispose();
                        }
                        catch (Exception e) {
                            stack.Push(new Exception($"Failed to dispose {clip.GetType()} properly", e));
                        }
                    }

                    if (index >= this.Model.Clips.Count) {
                        Debug.WriteLine($"Warning! {this.GetType()} and {this.Model.GetType()} clip list desynchronized; {this.clips.Count} != {this.Model.Clips.Count}");
                        this.Model.Clips.Remove(clip.Model);
                    }
                    else if (clip.Model != this.Model.Clips[index]) {
                        Debug.WriteLine($"Warning! {this.GetType()} and {this.Model.GetType()} clip list desynchronized; clip's mode != this layer's model's clip");
                        this.Model.Clips.Remove(clip.Model);
                    }
                    else {
                        this.Model.Clips.RemoveAt(index);
                    }

                    this.RemoveClip(clip, index);
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
                foreach (ClipViewModel clip in this.clips) {
                    try {
                        clip.Dispose();
                    }
                    catch (Exception e) {
                        innerStack.Push(e);
                    }

                    clip.Model.Layer = null;
                    clip.Layer = null;
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

        public void MakeTopMost(VideoClipViewModel clip) {
            int endIndex = this.Clips.Count - 1;
            int index = this.Clips.IndexOf(clip);
            if (index == -1 || index == endIndex) {
                return;
            }

            this.clips.Move(index, endIndex);

            ClipModel removedItem = this.Model.Clips[index];
            this.Model.Clips.RemoveAt(index);
            this.Model.Clips.Insert(endIndex, removedItem);
        }

        public async Task OnVideoResourceDropped(ResourceItemViewModel resource, long frameBegin) {
            double fps = this.Timeline.Project.Settings.FrameRate;
            long duration = Math.Min((long) Math.Floor(fps * 5), this.Timeline.MaxDuration - frameBegin);
            if (duration <= 0) {
                return;
            }

            if (resource.Model is ResourceARGB argb) {
                SquareClipModel clip = new SquareClipModel() {
                    FrameSpan = new ClipSpan(frameBegin, duration),
                    Width = 200, Height = 200,
                    DisplayName = argb.UniqueId,
                    ResourceId = argb.UniqueId
                };

                this.CreateClip(clip);
            }
            else if (resource.Model is ResourceImage img) {
                ImageClipModel clip = new ImageClipModel() {
                    FrameSpan = new ClipSpan(frameBegin, duration),
                    DisplayName = img.UniqueId,
                    ResourceId = img.UniqueId
                };

                this.CreateClip(clip);
            }
            // else if (resource.Model is ResourceMedia media) {
            //     media.OpenDecoder();
            //     TimeSpan span = media.GetDuration();
            //     long dur = (long) Math.Floor(span.TotalSeconds * fps);
            //     if (dur < 2) {
            //         // image files are 1
            //         dur = duration;
            //     }
            //     if (dur > 0) {
            //         this.CreateMediaClip(frameBegin, dur, media);
            //     }
            //     else {
            //         await IoC.MessageDialogs.ShowMessageAsync("Invalid media", "This media has a duration of 0 and cannot be added to the timeline");
            //     }
            // }
        }
    }
}