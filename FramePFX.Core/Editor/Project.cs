using System;
using System.IO;
using FramePFX.Core.Automation;
using FramePFX.Core.Editor.Audio;
using FramePFX.Core.Editor.ResourceManaging;
using FramePFX.Core.Editor.Timelines;
using FramePFX.Core.RBC;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor {
    public class Project : IRBESerialisable {
        public volatile bool IsSaving;

        public ProjectSettings Settings { get; }

        public ResourceManager ResourceManager { get; }

        /// <summary>
        /// This project's main timeline
        /// </summary>
        public Timeline Timeline { get; }

        /// <summary>
        /// The video editor that this project is currently in
        /// </summary>
        public VideoEditor Editor { get; set; }

        /// <summary>
        /// This project's automation engine
        /// </summary>
        public AutomationEngine AutomationEngine { get; }

        public AudioEngine AudioEngine { get; }

        /// <summary>
        /// This project's data folder, which is where file-based data is stored (that isn't stored using an absolute path)
        /// </summary>
        public string DataFolder;

        public string TempDataFolder;

        public Project() {
            this.Settings = new ProjectSettings() {
                Resolution = new Resolution(1920, 1080)
            };

            this.ResourceManager = new ResourceManager(this);
            this.AutomationEngine = new AutomationEngine(this);
            this.AudioEngine = new AudioEngine();
            this.Timeline = new Timeline(this) {
                MaxDuration = 10000L
            };
        }

        public void WriteToRBE(RBEDictionary data) {
            this.DataFolder = data.GetString(nameof(this.DataFolder), null);
            if (string.IsNullOrEmpty(this.DataFolder))
                this.TempDataFolder = data.GetString(nameof(this.TempDataFolder), null);
            this.Settings.WriteToRBE(data.CreateDictionary(nameof(this.Settings)));
            this.ResourceManager.WriteToRBE(data.CreateDictionary(nameof(this.ResourceManager)));
            this.Timeline.WriteToRBE(data.CreateDictionary(nameof(this.Timeline)));
        }

        public void ReadFromRBE(RBEDictionary data) {
            if (!string.IsNullOrEmpty(this.DataFolder)) {
                data.SetString(nameof(this.DataFolder), this.DataFolder);
            }
            else if (!string.IsNullOrEmpty(this.TempDataFolder)) {
                data.SetString(nameof(this.TempDataFolder), this.TempDataFolder);
            }

            this.Settings.ReadFromRBE(data.GetDictionary(nameof(this.Settings)));
            this.ResourceManager.ReadFromRBE(data.GetDictionary(nameof(this.ResourceManager)));
            this.Timeline.ReadFromRBE(data.GetDictionary(nameof(this.Timeline)));
        }

        /// <summary>
        /// Gets the absolute file path from a relative file path (relative to the project data folder directory)
        /// </summary>
        /// <param name="file">Input relative path</param>
        /// <returns>Output absolute filepath that may exist on the system</returns>
        /// <exception cref="ArgumentException">Input file name/path is null or empty</exception>
        public string GetAbsolutePath(string file) {
            if (string.IsNullOrEmpty(file)) {
                throw new ArgumentException("File path cannot be null or empty", nameof(file));
            }

            return Path.Combine(this.GetDirectoryPath(out _), file);
        }

        public string GetDirectoryPath(out bool isTemp) {
            if (string.IsNullOrEmpty(this.DataFolder)) {
                isTemp = true;
                if (string.IsNullOrEmpty(this.TempDataFolder)) {
                    string path = RandomUtils.RandomStringWhere(Path.GetTempPath(), 32, x => !Directory.Exists(x));
                    return this.TempDataFolder = path;
                }
                else {
                    return this.TempDataFolder;
                }
            }
            else {
                isTemp = false;
                return this.DataFolder;
            }
        }

        public DirectoryInfo CreateDir(out bool isTemp) {
            // Cleaner and also faster than manual existence check (exists() ? new DirectoryInfo(dir) : create())
            return Directory.CreateDirectory(this.GetDirectoryPath(out isTemp));
        }

        public DirectoryInfo CreateDir() => this.CreateDir(out _);
    }
}