using FramePFX.Core.Editor.ResourceManaging;
using FramePFX.Core.Editor.Timeline;
using FramePFX.Core.RBC;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor {
    public class ProjectModel : IRBESerialisable {
        public ProjectSettingsModel Settings { get; }

        public string ProjectDir { get; set; }

        public volatile bool IsSaving;

        /// <summary>
        /// The video editor that this project is currently in
        /// </summary>
        public VideoEditorModel Editor { get; set; }

        public TimelineModel Timeline { get; }

        public ResourceManager ResourceManager { get; }

        // not a chance anyone's creating more than 9 quintillion clips
        public long CurrentClipId { get; private set; }

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

        public long GetNextClipId() {
            return this.CurrentClipId++;
        }

        public void WriteToRBE(RBEDictionary data) {

        }

        public void ReadFromRBE(RBEDictionary data) {

        }
    }
}