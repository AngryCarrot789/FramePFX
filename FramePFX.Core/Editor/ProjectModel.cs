using FramePFX.Core.Editor.ResourceManaging;
using FramePFX.Core.Editor.Timeline;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor {
    public class ProjectModel {
        public ProjectSettingsModel Settings { get; }

        public string ProjectDir { get; set; }

        public volatile bool IsSaving;

        /// <summary>
        /// The video editor that this project is currently in
        /// </summary>
        public VideoEditorModel Editor { get; set; }

        public TimelineModel Timeline { get; }

        public ResourceManager ResourceManager { get; }

        public ProjectModel() {
            this.Settings = new ProjectSettingsModel() {
                Resolution = new Resolution(1920, 1080),
                FrameRate = 30
            };

            this.ResourceManager = new ResourceManager(this);
            this.Timeline = new TimelineModel(this) {
                MaxDuration = 10000
            };
        }

        public void SaveProject(string file) {

        }

        public void LoadProject(string file) {

        }
    }
}