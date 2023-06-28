using System;
using FramePFX.Core.Automation;
using FramePFX.Core.Editor.Audio;
using FramePFX.Core.Editor.ResourceManaging;
using FramePFX.Core.Editor.Timeline;
using FramePFX.Core.RBC;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor {
    public class ProjectModel : IRBESerialisable {
        // not a chance anyone's creating more than 9 quintillion clips
        private long nextClipId;
        public volatile bool IsSaving;

        public ProjectSettingsModel Settings { get; }

        public ResourceManager ResourceManager { get; }

        public TimelineModel Timeline { get; }

        /// <summary>
        /// The video editor that this project is currently in
        /// </summary>
        public VideoEditorModel Editor { get; set; }

        public AutomationEngine AutomationEngine { get; }

        public AudioEngine AudioEngine { get; }

        public ProjectModel() {
            this.Settings = new ProjectSettingsModel() {
                Resolution = new Resolution(1920, 1080)
            };

            this.ResourceManager = new ResourceManager(this);
            this.AutomationEngine = new AutomationEngine(this);
            this.AudioEngine = new AudioEngine();
            this.Timeline = new TimelineModel(this) {
                MaxDuration = 10000L
            };
        }

        public long GetNextClipId() {
            return this.nextClipId++;
        }

        public void WriteToRBE(RBEDictionary data) {
            data.SetLong("NextClipId", this.nextClipId);
            this.Settings.WriteToRBE(data.CreateDictionary(nameof(this.Settings)));
            this.ResourceManager.WriteToRBE(data.CreateDictionary(nameof(this.ResourceManager)));
            this.Timeline.WriteToRBE(data.CreateDictionary(nameof(this.Timeline)));
        }

        public void ReadFromRBE(RBEDictionary data) {
            this.nextClipId = data.GetLong("NextClipId");
            if (this.nextClipId < 0) {
                throw new Exception("Invalid next clip id");
            }

            this.Settings.ReadFromRBE(data.GetDictionary(nameof(this.Settings)));
            this.ResourceManager.ReadFromRBE(data.GetDictionary(nameof(this.ResourceManager)));
            this.Timeline.ReadFromRBE(data.GetDictionary(nameof(this.Timeline)));
        }
    }
}