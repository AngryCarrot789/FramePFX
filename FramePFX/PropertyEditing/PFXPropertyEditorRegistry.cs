using System.Collections.Generic;
using System.Linq;
using FramePFX.Editor.PropertyEditors.Clips;
using FramePFX.Editor.PropertyEditors.Effects;
using FramePFX.Editor.PropertyEditors.Tracks;
using FramePFX.Editor.PropertyEditors.Tracks.Video;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Editor.Timelines.VideoClips;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.Editor.ViewModels.Timelines.Effects.Video;
using FramePFX.Editor.ViewModels.Timelines.VideoClips;
using FramePFX.PropertyEditing.Editors;

namespace FramePFX.PropertyEditing {
    public class PFXPropertyEditorRegistry : PropertyEditorRegistry {
        public static PFXPropertyEditorRegistry Instance { get; } = new PFXPropertyEditorRegistry();

        public FixedPropertyGroupViewModel ClipInfo { get; }

        public FixedPropertyGroupViewModel TrackInfo { get; }

        public DynamicPropertyGroupViewModel EffectInfo { get; }

        public FixedPropertyGroupViewModel ResourceInfo { get; }

        private PFXPropertyEditorRegistry() {
            this.ClipInfo = this.CreateRootGroup(typeof(ClipViewModel), "Clip Info");
            this.ClipInfo.AddPropertyEditor("ClipDataEditor", new ClipDataEditorViewModel());
            this.ClipInfo.AddPropertyEditor("VideoClipDataEditor_Single", new VideoClipDataSingleEditorViewModel());
            this.ClipInfo.AddPropertyEditor("VideoClipDataEditor_Multi", new VideoClipDataMultipleEditorViewModel());

            {
                FixedPropertyGroupViewModel group = this.ClipInfo.CreateFixedSubGroup(typeof(TextVideoClipViewModel), "Text Info");
                group.AddPropertyEditor("TextEditor", new TextClipDataEditorViewModel());
            }

            {
                FixedPropertyGroupViewModel group = this.ClipInfo.CreateFixedSubGroup(typeof(ShapeSquareVideoClipViewModel), "Shape Info");
                group.AddPropertyEditor("Width", AutomatableFloatEditorViewModel.NewInstance<ShapeSquareVideoClipViewModel>(ShapeSquareVideoClip.WidthKey, x => x.Width, (x, y) => x.Width = y));
                group.AddPropertyEditor("Height", AutomatableFloatEditorViewModel.NewInstance<ShapeSquareVideoClipViewModel>(ShapeSquareVideoClip.HeightKey, x => x.Height, (x, y) => x.Height = y));
            }

            this.EffectInfo = new EffectListPropertyGroupViewModel();
            this.ClipInfo.AddSubGroup(this.EffectInfo, "Effects", false);
            this.EffectInfo.RegisterType(typeof(MotionEffectViewModel), "Motion", (single) => {
                EffectPropertyGroupViewModel motion = new EffectPropertyGroupViewModel(typeof(MotionEffectViewModel)) {
                    IsExpanded = true, IsHeaderBold = true, IsSelectable = true
                };

                if (!single.HasValue || single.Value)
                    motion.AddPropertyEditor("MotionEffect_Single", new MotionEffectDataSingleEditorViewModel());
                if (!single.HasValue || !single.Value)
                    motion.AddPropertyEditor("MotionEffect_Multi", new MotionEffectDataMultiEditorViewModel());

                return motion;
            });

            this.Root.AddSeparator(false);

            {
                this.TrackInfo = this.CreateRootGroup(typeof(TrackViewModel), "Track Info");
                this.TrackInfo.AddPropertyEditor("TrackDataEditor", new TrackDataEditorViewModel());
                this.TrackInfo.AddSeparator(true);
                this.TrackInfo.AddPropertyEditor("VideoTrackDataEditor_Single", new VideoTrackDataSingleEditorViewModel());
                this.TrackInfo.AddPropertyEditor("VideoTrackDataEditor_Multi", new VideoTrackDataMultipleEditorViewModel());
            }

            this.Root.AddSeparator(false);

            this.ResourceInfo = this.Root.CreateFixedSubGroup(typeof(BaseResourceViewModel), "Resource Info");
        }

        public void OnTrackSelectionChanged(IReadOnlyList<TrackViewModel> tracks) {
            this.TrackInfo.SetupHierarchyState(tracks);
            this.Root.CleanSeparators();
        }

        public void OnClipSelectionChanged(TimelineViewModel timeline) {
            this.OnClipSelectionChanged(timeline.Tracks.SelectMany(x => x.SelectedClips).ToList());
        }

        public void OnClipSelectionChanged(IReadOnlyList<ClipViewModel> clips) {
            // List<BaseEffectViewModel> effects = clips.SelectMany(clip => clip.Effects).ToList();
            this.ClipInfo.SetupHierarchyState(clips);
            // foreach (IPropertyEditorObject obj in this.ClipInfo.PropertyObjects) {
            //     if (obj is FixedPropertyGroupViewModel group && this.ClipInfo.IsDisconnectedFromHandlerHierarchy(group)) {
            //         group.SetupHierarchyState(clips);
            //     }
            // }

            this.EffectInfo.SetupHierarchyStateExtended(clips.Select(x => x.Effects).ToList());
            this.Root.CleanSeparators();
        }

        public void OnEffectCollectionChanged() {
            IReadOnlyList<object> clips = this.ClipInfo.Handlers;
            if (clips != null && clips.Count > 0) {
                this.EffectInfo.SetupHierarchyStateExtended(clips.Cast<ClipViewModel>().Select(x => x.Effects).ToList());
            }
            else {
                this.EffectInfo.ClearHierarchyState();
            }
        }

        public void OnResourcesSelectionChanged(IReadOnlyList<BaseResourceViewModel> list) {
            this.ResourceInfo.SetupHierarchyState(list);
            this.Root.CleanSeparators();
        }
    }
}