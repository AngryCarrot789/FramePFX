using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ViewModels.Timeline {
    /// <summary>
    /// The base view model for a timeline layer. This could be a video or audio layer (or others...)
    /// </summary>
    public abstract class TimelineLayerViewModel : BaseViewModel {
        private readonly ObservableCollectionEx<TimelineClipViewModel> clips;
        public ReadOnlyObservableCollection<TimelineClipViewModel> Clips { get; }

        private IList<TimelineClipViewModel> selectedClips;
        public IList<TimelineClipViewModel> SelectedClips {
            get => this.selectedClips ?? (this.SelectedClips = null);
            set => this.RaisePropertyChanged(ref this.selectedClips, value ?? new List<TimelineClipViewModel>());
        }

        private TimelineClipViewModel primarySelectedClip;
        public TimelineClipViewModel PrimarySelectedClip {
            get => this.primarySelectedClip;
            set => this.RaisePropertyChanged(ref this.primarySelectedClip, value);
        }

        public AsyncRelayCommand RemoveSelectedClipsCommand { get; }

        public TimelineViewModel Timeline { get; }

        protected TimelineLayerViewModel(TimelineViewModel timeline) {
            this.Timeline = timeline ?? throw new ArgumentNullException(nameof(timeline));
            this.clips = new ObservableCollectionEx<TimelineClipViewModel>();
            this.Clips = new ReadOnlyObservableCollection<TimelineClipViewModel>(this.clips);
            this.RemoveSelectedClipsCommand = new AsyncRelayCommand(this.RemoveSelectedClipsAction, () => this.SelectedClips.Count > 0);
        }

        public Task RemoveSelectedClipsAction() {
            return this.RemoveSelectedClipsAction(true);
        }

        public async Task RemoveSelectedClipsAction(bool confirm) {
            IList<TimelineClipViewModel> list = this.SelectedClips;
            if (list.Count < 1) {
                return;
            }

            if (confirm && !await IoC.MessageDialogs.ShowYesNoDialogAsync($"Delete clip{(list.Count == 1 ? "" : "s")}?", $"Are you sure you want to delete {(list.Count == 1 ? "1 clip" : $"{list.Count} clips")}?")) {
                return;
            }

            await this.DisposeAndRemoveItemsAction(list);
        }

        public async Task DisposeAndRemoveItemsAction(IEnumerable<TimelineClipViewModel> list) {
            try {
                this.DisposeAndRemoveItemsUnsafe(list.ToList());
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageExAsync("Error", "An error occurred while removing clips", e.GetToString());
            }
        }

        public void DisposeAndRemoveItemsUnsafe(IList<TimelineClipViewModel> list) {
            using (ExceptionStack stack = new ExceptionStack("Exception disposing clips")) {
                foreach (TimelineClipViewModel item in list) {
                    if (item is IDisposable disposable) {
                        try {
                            disposable.Dispose();
                        }
                        catch (Exception e) {
                            stack.Push(new Exception($"Failed to dispose {item.GetType()} properly", e));
                        }
                    }

                    this.clips.Remove(item);
                }
            }
        }
    }
}