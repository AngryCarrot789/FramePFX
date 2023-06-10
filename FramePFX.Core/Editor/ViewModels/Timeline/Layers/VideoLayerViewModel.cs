using System;
using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.Editor.Timeline;
using FramePFX.Core.Editor.Timeline.Clip;
using FramePFX.Core.Editor.Timeline.Layers;
using FramePFX.Core.Editor.ViewModels.Timeline.Clips;
using FramePFX.Core.Editor.ViewModels.Timeline.Removals;
using FramePFX.Core.Utils;
using FramePFX.Core.Views.Dialogs.Message;

namespace FramePFX.Core.Editor.ViewModels.Timeline.Layers {
    public class VideoLayerViewModel : LayerViewModel {
        private static readonly MessageDialog SliceCloneTextResourceDialog;

        static VideoLayerViewModel() {
            SliceCloneTextResourceDialog = new MessageDialog("reference") {
                ShowAlwaysUseNextResultOption = true,
                Header = "Reference or copy text resource?",
                Message = "Do you want to reference the same text resource (shared text, font, etc), or clone it (creating a new resource)?"
            };
            SliceCloneTextResourceDialog.AddButton("Reference", "reference", true);
            SliceCloneTextResourceDialog.AddButton("Copy", "copy", true);
            SliceCloneTextResourceDialog.AddButton("Cancel", "cancel", true);
        }

        public new VideoLayerModel Model => (VideoLayerModel) base.Model;

        public float Opacity {
            get => this.Model.Opacity;
            set {
                this.Model.Opacity = value;
                this.RaisePropertyChanged();
                this.Timeline.DoRender(true);
            }
        }

        public VideoLayerViewModel(TimelineViewModel timeline, VideoLayerModel model) : base(timeline, model) {

        }

        public override async Task SliceClipAction(ClipViewModel clip, long frame) {
            // assert clip.Layer == this.Model

            string imageCloneResult = null;
            ResourceText resourceText = null;
            if (clip.Model is TextClipModel txt1 && txt1.ResourcePath != null && txt1.ResourcePath.TryGetResource(out resourceText)) {
                imageCloneResult = await SliceCloneTextResourceDialog.ShowAsync();
                if (imageCloneResult == null || imageCloneResult == "cancel") {
                    return;
                }
            }

            ClipModel cloned = clip.Model.Clone();
            FrameSpan oldSpan = clip.Model.FrameSpan;
            cloned.FrameSpan = FrameSpan.FromIndex(frame, oldSpan.EndIndex);
            clip.Model.FrameSpan = oldSpan.SetEndIndex(frame);
            if (imageCloneResult != null && imageCloneResult == "copy") {
                string path = TextIncrement.GetNextNumber(resourceText.UniqueId);
                ResourceText textRes = new ResourceText(resourceText.Manager) {
                    Border = resourceText.Border,
                    Foreground = resourceText.Foreground,
                    Text = resourceText.Text,
                    FontFamily = resourceText.FontFamily,
                    SkewX = resourceText.SkewX,
                    FontSize = resourceText.FontSize
                };

                clip.Project.ResourceManager.AddModel(textRes, path);
                ((TextClipModel) cloned).SetTargetResourceId(path);
            }

            this.CreateClip(cloned);
        }

        public VideoClipRangeRemoval GetRangeRemoval(long spanBegin, long spanDuration) {
            if (spanDuration < 0)
                throw new ArgumentOutOfRangeException(nameof(spanDuration), "Span duration cannot be negative");
            long spanEnd = spanBegin + spanDuration;
            VideoClipRangeRemoval range = new VideoClipRangeRemoval();
            foreach (ClipViewModel clipViewModel in this.Clips) {
                if (clipViewModel is VideoClipViewModel clip) {
                    long clipBegin = clip.FrameBegin;
                    long clipDuration = clip.FrameDuration;
                    long clipEnd = clipBegin + clipDuration;
                    if (clipEnd <= spanBegin && clipBegin >= spanEnd) {
                        continue; // not intersecting
                    }
                    if (spanBegin <= clipBegin) { // cut the left part away
                        if (spanEnd >= clipEnd) {
                            // remove clip entirely
                            range.AddRemovedClip(clip);
                        }
                        else if (spanEnd <= clipBegin) { // not intersecting
                            continue;
                        }
                        else {
                            range.AddSplitClip(clip, null, FrameSpan.FromIndex(spanEnd, clipEnd));
                        }
                    }
                    else if (spanEnd >= clipEnd) { // cut the right part away
                        if (spanBegin >= clipEnd) { // not intersecting
                            continue;
                        }
                        else {
                            range.AddSplitClip(clip, FrameSpan.FromIndex(clipBegin, spanBegin), null);
                        }
                    }
                    else { // fully intersecting; double split
                        range.AddSplitClip(clip, FrameSpan.FromIndex(clipBegin, spanBegin), FrameSpan.FromIndex(spanEnd, clipEnd));
                    }
                }
            }
            return range;
        }
    }
}