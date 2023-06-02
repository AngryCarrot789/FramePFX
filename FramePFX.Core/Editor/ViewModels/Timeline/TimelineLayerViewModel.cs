using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Core.Editor.Timeline;
using FramePFX.Core.Editor.Timeline.Clip;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ViewModels.Timeline {
    /// <summary>
    /// The base view model for a timeline layer. This could be a video or audio layer (or others...)
    /// </summary>
    public abstract class TimelineLayerViewModel : BaseViewModel {
        private readonly ObservableCollectionEx<ClipViewModel> clips;
        public ReadOnlyObservableCollection<ClipViewModel> Clips { get; }

        private IList<ClipViewModel> selectedClips;
        public IList<ClipViewModel> SelectedClips {
            get => this.selectedClips ?? (this.SelectedClips = null);
            set => this.RaisePropertyChanged(ref this.selectedClips, value ?? new List<ClipViewModel>());
        }

        private ClipViewModel primarySelectedClip;
        public ClipViewModel PrimarySelectedClip {
            get => this.primarySelectedClip;
            set => this.RaisePropertyChanged(ref this.primarySelectedClip, value);
        }

        public AsyncRelayCommand RemoveSelectedClipsCommand { get; }

        public TimelineViewModel Timeline { get; }

        public TimelineLayerModel Model { get; }

        protected TimelineLayerViewModel(TimelineViewModel timeline, TimelineLayerModel model) {
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
            this.Timeline = timeline ?? throw new ArgumentNullException(nameof(timeline));
            this.clips = new ObservableCollectionEx<ClipViewModel>();
            this.Clips = new ReadOnlyObservableCollection<ClipViewModel>(this.clips);
            this.RemoveSelectedClipsCommand = new AsyncRelayCommand(this.RemoveSelectedClipsAction, () => this.SelectedClips.Count > 0);
            foreach (ClipModel clip in model.Clips) {
                this.clips.Add(ClipRegistry.Instance.CreateViewModelFromModel(clip));
            }
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

                    this.clips.RemoveAt(index);
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
                }

                this.clips.Clear();
                this.Model.Clips.Clear();
                if (innerStack.TryGetException(out Exception ex)) {
                    stack.Push(ex);
                }
            }
        }
    }
}