using FramePFX.Core;
using FramePFX.Core.Utils;
using FramePFX.ResourceManaging;
using FramePFX.ResourceManaging.Items;
using FramePFX.Timeline.ViewModels;
using FramePFX.Timeline.ViewModels.Clips.Resizable;
using FramePFX.Timeline.ViewModels.Layer;
using Timelining.ViewModels.Clips.Resizable;

namespace FramePFX.Project {
    public class Project : BaseViewModel {
        /// <summary>
        /// This project's timeline
        /// </summary>
        public EditorTimeline Timeline { get; }

        /// <summary>
        /// This project's resources
        /// </summary>
        public ResourceManager ResourceManager { get; }

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

        public VideoEditor VideoEditor { get; }

        public Project(VideoEditor videoEditor) {
            this.VideoEditor = videoEditor;
            this.Timeline = new EditorTimeline(this);
            this.ResourceManager = new ResourceManager(this);
        }

        public void RenderTimeline() {
            this.Timeline.MarkRenderDirty();
        }

        public void SetupDefaultProject() {
            // this.PlaybackResolution = new Resolution(1920, 1080);
            this.PlaybackResolution = new Resolution(1920, 1080);
            this.Timeline.MaxDuration = 10000;
            ResourceColour redColour = new ResourceColour {
                Red = 0.9f,
                Green = 0.1f,
                Blue = 0.1f
            };

            ResourceColour greenColour = new ResourceColour {
                Red = 0.1f,
                Green = 0.9f,
                Blue = 0.1f
            };

            ResourceColour blueColour = new ResourceColour {
                Red = 0.1f,
                Green = 0.1f,
                Blue = 0.9f
            };

            this.ResourceManager.AddResource("Resource_RED", redColour);
            this.ResourceManager.AddResource("Resource_GREEN", greenColour);
            this.ResourceManager.AddResource("Resource_BLUE", blueColour);

            TimelineLayer l1 = this.Timeline.CreateLayer("Layer 1");
            CreateSquare(l1, 0, 50, redColour, 5f, 5f, 100f, 100f, "Red_0");
            CreateSquare(l1, 100, 150, redColour, 105f, 5f, 100f, 100f, "Red_1");
            CreateSquare(l1, 275, 50, greenColour, 210f, 5f, 100f, 100f, "Green_0");

            TimelineLayer l2 = this.Timeline.CreateLayer("Layer 2");
            CreateSquare(l2, 0, 100, greenColour, 5f, 105f, 100f, 100f, "Green_1");
            CreateSquare(l2, 100, 50, blueColour, 105f, 105f, 100f, 100f, "Blue_0");
            CreateSquare(l2, 175, 75, blueColour, 210f, 105f, 100f, 100f, "Blue_1");
        }

        public static void CreateSquare(TimelineLayer timelineLayer, long begin, long duration, ResourceColour colour, float x, float y, float w, float h, string name) {
            ShapeClipViewModel clip = timelineLayer.CreateSquareClip(begin, duration, colour);
            clip.SetShape(x, y, w, h);
            clip.Container.Name = name;
        }
    }
}