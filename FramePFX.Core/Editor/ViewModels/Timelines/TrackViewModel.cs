using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Core.Automation;
using FramePFX.Core.Automation.ViewModels;
using FramePFX.Core.Editor.History;
using FramePFX.Core.Editor.Registries;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Editor.Timelines;
using FramePFX.Core.History;
using FramePFX.Core.History.Tasks;
using FramePFX.Core.History.ViewModels;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ViewModels.Timelines {
    /// <summary>
    /// The base view model for a timeline track. This could be a video or audio track (or others...)
    /// </summary>
    public abstract class TrackViewModel : BaseViewModel, IHistoryHolder, IDisplayName, IResourceItemDropHandler, IAutomatableViewModel {
        protected readonly HistoryBuffer<HistoryTrackDisplayName> displayNameHistory = new HistoryBuffer<HistoryTrackDisplayName>();

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
                    if (!this.displayNameHistory.TryGetAction(out HistoryTrackDisplayName action))
                        this.displayNameHistory.PushAction(this.HistoryManager, action = new HistoryTrackDisplayName(this), "Edit media duration");
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
        public string TrackColour {
            get => this.Model.TrackColour;
            set {
                this.Model.TrackColour = value;
                this.RaisePropertyChanged();
            }
        }

        public bool IsHistoryChanging { get; set; }

        public bool IsAutomationRefreshInProgress { get; set; }

        public AsyncRelayCommand RenameTrackCommand { get; }

        public AsyncRelayCommand RemoveSelectedClipsCommand { get; }

        public TimelineViewModel Timeline { get; }

        public ProjectViewModel Project => this.Timeline?.Project;

        public VideoEditorViewModel Editor => this.Timeline?.Project.Editor;

        public HistoryManagerViewModel HistoryManager => this.Timeline?.Project.Editor.HistoryManager;

        public AutomationDataViewModel AutomationData { get; }

        public Track Model { get; }

        IAutomatable IAutomatableViewModel.AutomationModel => this.Model;

        public AutomationEngineViewModel AutomationEngine => this.Project?.AutomationEngine;

        protected TrackViewModel(TimelineViewModel timeline, Track model) {
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
            this.Timeline = timeline ?? throw new ArgumentNullException(nameof(timeline));
            if (!ReferenceEquals(timeline.Model, model.Timeline))
                throw new ArgumentException("The timeline's model and then given track model's timeline do not match");
            this.AutomationData = new AutomationDataViewModel(this, model.AutomationData);
            this.clips = new ObservableCollectionEx<ClipViewModel>();
            this.Clips = new ReadOnlyObservableCollection<ClipViewModel>(this.clips);
            this.SelectedClips = new ObservableCollectionEx<ClipViewModel>();
            this.SelectedClips.CollectionChanged += (sender, args) => {
                this.RemoveSelectedClipsCommand.RaiseCanExecuteChanged();
                this.Timeline.Project.Editor?.View.UpdateClipSelection();
            };
            this.RemoveSelectedClipsCommand = new AsyncRelayCommand(this.RemoveSelectedClipsAction, () => this.SelectedClips.Count > 0);
            this.RenameTrackCommand = new AsyncRelayCommand(async () => {
                string result = await IoC.UserInput.ShowSingleInputDialogAsync("Change track name", "Input a new track name:", this.DisplayName ?? "", this.Timeline.TrackNameValidator);
                if (result != null) {
                    this.DisplayName = result;
                }
            });

            for (int i = 0; i < model.Clips.Count; i++) {
                this.AddClipToTrack(i, ClipRegistry.Instance.CreateViewModelFromModel(model.Clips[i]), false);
            }
        }

        public virtual void OnProjectModified() {
            if (this.Timeline != null) {
                this.Timeline.OnProjectModified();
            }
        }

        public ClipViewModel CreateClip(Clip model, bool addToModel = true) {
            ClipViewModel vm = ClipRegistry.Instance.CreateViewModelFromModel(model);
            this.AddClipToTrack(vm, addToModel);
            return vm;
        }

        public bool RemoveClipFromTrack(ClipViewModel clip, bool removeFromModel = true) {
            int index = this.clips.IndexOf(clip);
            if (index < 0) {
                return false;
            }

            this.RemoveClipFromTrack(index, removeFromModel);
            return true;
        }

        public void RemoveClipFromTrack(int index, bool removeFromModel = true) {
            ClipViewModel clip = this.clips[index];
            if (!ReferenceEquals(this, clip.Track))
                throw new Exception($"Clip track does not match the current instance: {clip.Track} != {this}");
            if (!ReferenceEquals(this.Model.Clips[index], clip.Model))
                throw new Exception($"Clip model clip list desynchronized");

            if (removeFromModel) {
                this.Model.RemoveClipAt(index, false);
            }

            this.clips.RemoveAt(index);
            ClipViewModel.SetTrack(clip, null);

            clip.RaisePropertyChanged(nameof(clip.Track));
        }

        public void AddClipToTrack(int index, ClipViewModel clip, bool addToModel = true) {
            if (index < 0 || index > this.clips.Count)
                throw new IndexOutOfRangeException($"Index < 0 || Index > Count. Index = {index}, Count = {this.clips.Count}");
            if (ReferenceEquals(this, clip.Track))
                throw new InvalidOperationException("Attempted to add clip to a track it was already in");
            if (!this.IsClipTypeAcceptable(clip))
                throw new Exception("Invalid clip for this layer");

            if (addToModel) {
                this.Model.InsertClip(index, clip.Model, false);
            }

            this.clips.Insert(index, clip);
            ClipViewModel.SetTrack(clip, this);
            clip.RaisePropertyChanged(nameof(clip.Track));
        }

        public void AddClipToTrack(ClipViewModel clip, bool addToModel = true) {
            this.AddClipToTrack(this.clips.Count, clip, addToModel);
        }

        public Task RemoveSelectedClipsAction() {
            return this.RemoveSelectedClipsAction(true);
        }

        public async Task RemoveSelectedClipsAction(bool confirm) {
            IList<ClipViewModel> list = this.SelectedClips;
            if (list.Count < 1) {
                return;
            }

            if (confirm && !await IoC.MessageDialogs.ShowYesNoDialogAsync($"Delete clip{Lang.S(list.Count)}?", $"Are you sure you want to delete {(list.Count == 1 ? "1 clip" : $"{list.Count} clips")}?")) {
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
                    this.RemoveClipFromTrack(index);
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
            using (ExceptionStack stack = new ExceptionStack("Exception disposing track")) {
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
                        this.RemoveClipFromTrack(i);
                    }
                    catch (Exception e) {
                        innerStack.Add(new Exception("Failed to remove clip from track", e));
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

            this.Model.Clips.MoveItem(index, endIndex);
            this.clips.Move(index, endIndex);
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

            Clip cloned = clip.Model.Clone();
            cloned.FrameSpan = FrameSpan.FromIndex(frame, span.EndIndex);
            cloned.MediaFrameOffset = frame - span.Begin;
            clip.FrameSpan = span.WithEndIndex(frame);
            this.CreateClip(cloned);
            return Task.CompletedTask;
        }

        public bool IsClipTypeAcceptable(ClipViewModel clip) {
            return this.Model.IsClipTypeAcceptable(clip.Model);
        }

        protected virtual void OnAutomationPropertyUpdated(string propertyName, in RefreshAutomationValueEventArgs e) {
            base.RaisePropertyChanged(propertyName);
        }
    }
}