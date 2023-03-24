using System;
using FramePFX.Core;
using FramePFX.Core.Utils;
using FramePFX.ResourceManaging;
using FramePFX.ResourceManaging.Items;
using FramePFX.Timeline;
using FramePFX.Timeline.Layer;
using FramePFX.Timeline.Layer.Clips.Resizable;

namespace FramePFX.Project {
    public class ProjectViewModel : BaseViewModel {
        /// <summary>
        /// This project's timeline
        /// </summary>
        public TimelineViewModel Timeline { get; }

        /// <summary>
        /// This project's resources
        /// </summary>
        public ResourceManagerViewModel ResourceManager { get; }

        private Resolution playbackResolution;

        /// <summary>
        /// This project's playback resolution (in the viewport display)
        /// </summary>
        public Resolution PlaybackResolution {
            get => this.playbackResolution;
            set => this.RaisePropertyChanged(ref this.playbackResolution, value);
        }

        private double playbackFPS;
        public double PlaybackFPS {
            get => this.playbackFPS;
            set => this.RaisePropertyChanged(ref this.playbackFPS, value);
        }

        public ProjectViewModel() {
            this.Timeline = new TimelineViewModel(this);
            this.ResourceManager = new ResourceManagerViewModel(this);
        }

        public void RenderTimeline() {
            this.Timeline.MarkRenderDirty();
        }

        public void SetupDefaultProject() {
            this.PlaybackResolution = new Resolution(1920, 1080);
            this.Timeline.MaxDuration = 10000;
            ResourceColourViewModel redColour = new ResourceColourViewModel {
                Red = 0.9f,
                Green = 0.1f,
                Blue = 0.1f
            };

            ResourceColourViewModel greenColour = new ResourceColourViewModel {
                Red = 0.1f,
                Green = 0.9f,
                Blue = 0.1f
            };

            ResourceColourViewModel blueColour = new ResourceColourViewModel {
                Red = 0.1f,
                Green = 0.1f,
                Blue = 0.9f
            };

            this.ResourceManager.AddResource("Resource_RED", redColour);
            this.ResourceManager.AddResource("Resource_GREEN", greenColour);
            this.ResourceManager.AddResource("Resource_BLUE", blueColour);

            LayerViewModel l1 = this.Timeline.CreateLayer("Layer 1");
            CreateSquare(l1, 0, 50, redColour, 5f, 5f, 100f, 100f, "Red_0");
            CreateSquare(l1, 100, 150, redColour, 105f, 5f, 100f, 100f, "Red_1");
            CreateSquare(l1, 275, 50, greenColour, 210f, 5f, 100f, 100f, "Green_0");

            LayerViewModel l2 = this.Timeline.CreateLayer("Layer 2");
            CreateSquare(l2, 0, 100, greenColour, 5f, 105f, 100f, 100f, "Green_1");
            CreateSquare(l2, 100, 50, blueColour, 105f, 105f, 100f, 100f, "Blue_0");
            CreateSquare(l2, 175, 75, blueColour, 210f, 105f, 100f, 100f, "Blue_1");
        }

        public static void CreateSquare(LayerViewModel layer, long begin, long duration, ResourceColourViewModel colour, float x, float y, float w, float h, string name) {
            ColouredShapeClipViewModel clip = layer.CreateSquareClip(begin, duration, colour);
            clip.SetShape(x, y, w, h);
            clip.Container.Name = name;
        }
    }
}