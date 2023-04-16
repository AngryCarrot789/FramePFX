﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FramePFX.Core;
using FramePFX.Core.Utils;
using FramePFX.Core.Views.Dialogs.UserInputs;
using FramePFX.Project;
using FramePFX.Timeline.ViewModels.Clips;
using FramePFX.Timeline.ViewModels.Layer;

namespace FramePFX.Timeline.ViewModels {
    public class EditorTimeline : BaseViewModel {
        private readonly RapidDispatchCallback renderDispatch;
        private readonly ObservableCollection<BaseTimelineLayer> layers;
        private volatile bool ignorePlayHeadPropertyChange;
        public volatile bool isFramePropertyChangeScheduled;

        public EditorProject Project { get; }

        public ViewportPlayback PlaybackViewport => this.Project.VideoEditor.PlaybackView;

        /// <summary>
        /// This timeline's layer collection
        /// </summary>
        public ReadOnlyObservableCollection<BaseTimelineLayer> Layers { get; }

        private BaseTimelineLayer selectedTimelineLayer;
        public BaseTimelineLayer SelectedTimelineLayer {
            get => this.selectedTimelineLayer;
            set => this.RaisePropertyChanged(ref this.selectedTimelineLayer, value);
        }

        private BaseTimelineClip mainSelectedClip;
        public BaseTimelineClip MainSelectedClip {
            get => this.mainSelectedClip;
            set => this.RaisePropertyChanged(ref this.mainSelectedClip, value);
        }

        private BaseTimelineClip clipToModify;
        public BaseTimelineClip ClipToModify {
            get => this.clipToModify;
            set => this.RaisePropertyChanged(ref this.clipToModify, value);
        }

        /// <summary>
        /// A handle to the actual timeline UI control
        /// </summary>
        public ITimelineHandle Handle { get; set; }

        /// <summary>
        /// A handle to the actual play head UI control
        /// </summary>
        public IPlayHeadHandle PlayHeadHandle { get; set; }

        private long maxDuration;
        public long MaxDuration {
            get => this.maxDuration;
            set => this.RaisePropertyChangedIfChanged(ref this.maxDuration, value);
        }

        private long playHeadFrame;
        public long PlayHeadFrame {
            get => this.playHeadFrame;
            set {
                long oldValue = this.playHeadFrame;
                if (oldValue == value) {
                    return;
                }

                if (value >= this.MaxDuration) {
                    value = this.MaxDuration - 1;
                }

                if (value < 0) {
                    value = 0;
                }

                if (this.ignorePlayHeadPropertyChange) {
                    this.playHeadFrame = value;
                }
                else {
                    this.RaisePropertyChanged(ref this.playHeadFrame, value);
                    this.OnPlayHeadMoved(oldValue, value, true);
                }
            }
        }

        public bool IsAtLastFrame => this.PlayHeadFrame >= (this.MaxDuration - 1);

        public bool IsRenderDirty { get; private set; }

        public RelayCommand<bool> DeleteSelectedClipsCommand { get; }

        public ICommand DeleteSelectedLayerCommand { get; }

        public InputValidator LayerNameValidator { get; set; }

        public EditorTimeline(EditorProject project) {
            this.LayerNameValidator = InputValidator.FromFunc((x) => string.IsNullOrEmpty(x) ? "Layer name cannot be empty" : null);
            this.Project = project;
            this.layers = new ObservableCollection<BaseTimelineLayer>();
            this.Layers = new ReadOnlyObservableCollection<BaseTimelineLayer>(this.layers);
            this.renderDispatch = new RapidDispatchCallback(this.RenderViewPortAndMarkNotDirty) {
                InvokeLater = true
            };
            this.DeleteSelectedClipsCommand = new RelayCommand<bool>(async (x) => await this.DeleteSelectedClipsAction(x));
            this.DeleteSelectedLayerCommand = new RelayCommand(this.DeleteSelectedLayerAction);
        }

        public void OnUpdateSelection(IEnumerable<TimelineVideoClip> clips) {
            this.GenerateProperties(clips.ToList());
        }

        public void GenerateProperties(List<TimelineVideoClip> clips) {
            // List<List<PropertyGroupViewModel>> groupTypes = new List<List<PropertyGroupViewModel>>();
            // for (int i = 0; i < clips.Count; i++) {
            //     ClipContainerViewModel clip = clips[i];
            //     if (clip.Content != null) {
            //         groupTypes.Add(new List<PropertyGroupViewModel>());
            //         clip.Content.AccumulatePropertyGroups(groupTypes[i]);
            //     }
            //     else {
            //         groupTypes.Add(null);
            //     }
            // }
            // foreach (List<PropertyGroupViewModel> groups in groupTypes) {
            //     if (groups == null) {
            //         continue;
            //     }
            // }
            // this.GeneratedProperties.Clear();
            // foreach (PropertyGroupViewModel entry in common) {
            //     this.GeneratedProperties.Add(entry);
            // }
        }

        public void DeleteSelectedLayerAction() {
            if (this.selectedTimelineLayer != null) {
                this.layers.Remove(this.selectedTimelineLayer);
            }
        }

        public async Task DeleteSelectedClipsAction(bool skipDialog) {
            List<TimelineVideoClip> list = this.Handle.GetSelectedClips().ToList();
            switch (list.Count) {
                case 0: return;
                case 1:
                    list[0].Layer.RemoveClip(list[0]);
                    return;
                default: {
                    if (skipDialog || await CoreIoC.MessageDialogs.ShowYesNoDialogAsync("Delete clips", "Do you want to delete these " + list.Count + " clips?")) {
                        foreach (TimelineVideoClip clip in list) {
                            clip.Layer.RemoveClip(clip);
                        }
                    }

                    return;
                }
            }
        }

        public bool CanRender() {
            return this.Project.VideoEditor.PlaybackView.IsReadyForRender();
        }

        public void MarkRenderDirty() {
            this.IsRenderDirty = true;
            if (this.CanRender()) {
                this.ScheduleRender(false);
            }
        }

        public void OnPlayHeadMoved(long oldFrame, long frame, bool render) {
            if (oldFrame == frame) {
                return;
            }

            if (render && this.CanRender()) {
                this.RenderViewPortAndMarkNotDirty();
            }
            else {
                this.IsRenderDirty = true;
            }
        }

        public void ScheduleRender(bool useCurrentThread = true) {
            if (useCurrentThread) {
                this.RenderViewPortAndMarkNotDirty();
            }
            else {
                this.renderDispatch.Invoke();
            }
        }

        private void RenderViewPortAndMarkNotDirty() {
            this.IsRenderDirty = false;
            IoC.VideoEditor.PlaybackView.RenderTimeline(this);
        }

        // TODO: Could optimise this, maybe create "chunks" of clips that span 10 frame sections across the entire timeline
        public IEnumerable<TimelineVideoClip> GetVideoClipsIntersectingFrame() {
            return this.GetVideoClipsIntersectingFrame(this.playHeadFrame);
        }

        public IEnumerable<TimelineVideoClip> GetVideoClipsIntersectingFrame(long frame) {
            foreach (BaseTimelineLayer layer in this.layers) {
                foreach (BaseTimelineClip clip in layer.Clips) {
                    if (clip is TimelineVideoClip vc && vc.IntersectsFrameAt(frame)) {
                        yield return vc;
                    }
                }
            }
        }

        public VideoTimelineLayer CreateVideoLayer(string name = null) {
            VideoTimelineLayer timelineLayer = new VideoTimelineLayer(this) {
                Name = name ?? $"Layer {this.Layers.Count + 1}"
            };

            this.layers.Add(timelineLayer);
            return timelineLayer;
        }

        public BaseTimelineLayer GetPrevious(BaseTimelineLayer timelineLayer) {
            int index = this.Layers.IndexOf(timelineLayer);
            return index < 1 ? null : this.Layers[index - 1];
        }

        public static long WrapIndex(long index, long endIndex) {
            // only works properly if index is less than (endIndex * 2)
            // e.g. if index is 2005 and endIndex is 1000, this function will return 1005, not 5
            // Assume that will never be the case though...
            return index >= endIndex ? (index - endIndex) : index;
        }

        /// <summary>
        /// Steps the play head for the next frame (typically from the playback thread)
        /// </summary>
        public void StepFrame(long change = 1L) {
            this.ignorePlayHeadPropertyChange = true;
            long duration = this.maxDuration;
            long oldFrame = this.playHeadFrame;
            // Clamp between 0 and max duration. also clamp change in safe duration range
            long newFrame = Math.Max(this.playHeadFrame + Maths.Clamp(change, -duration, duration), 0);
            this.playHeadFrame = WrapIndex(newFrame, duration);
            this.OnPlayHeadMoved(oldFrame, this.playHeadFrame, false);
            if (!this.isFramePropertyChangeScheduled) {
                this.isFramePropertyChangeScheduled = true;
                CoreIoC.Dispatcher.Invoke(() => {
                    this.RaisePropertyChanged(nameof(this.PlayHeadFrame));
                    this.isFramePropertyChangeScheduled = false;
                });
            }

            this.ignorePlayHeadPropertyChange = false;
        }

        public void OnPlayBegin() {
            foreach (BaseTimelineLayer layer in this.layers) {
                foreach (BaseTimelineClip clip in layer.Clips) {
                    clip.OnTimelinePlayBegin();
                }
            }
        }

        public void OnPlayEnd() {
            foreach (BaseTimelineLayer layer in this.layers) {
                foreach (BaseTimelineClip clip in layer.Clips) {
                    clip.OnTimelinePlayEnd();
                }
            }
        }
    }
}
