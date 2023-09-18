using System;
using System.IO;
using FramePFX.Editor.ResourceManaging;
using FramePFX.Editor.Timelines;
using FramePFX.RBC;
using FramePFX.Utils;

namespace FramePFX.Editor {
    public class Project {
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

        public bool IsExporting { get; set; }

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
            this.Timeline = new Timeline() {
                Project = this,
                MaxDuration = 5000L
            };
        }

        public void WriteToRBE(RBEDictionary data) {
            if (!string.IsNullOrEmpty(this.DataFolder)) {
                data.SetString(nameof(this.DataFolder), this.DataFolder);
            }
            else if (!string.IsNullOrEmpty(this.TempDataFolder)) {
                data.SetString(nameof(this.TempDataFolder), this.TempDataFolder);
            }

            this.Settings.WriteToRBE(data.CreateDictionary(nameof(this.Settings)));
            this.ResourceManager.WriteToRBE(data.CreateDictionary(nameof(this.ResourceManager)));
            this.Timeline.WriteToRBE(data.CreateDictionary(nameof(this.Timeline)));
        }

        public void ReadFromRBE(RBEDictionary data) {
            this.DataFolder = data.GetString(nameof(this.DataFolder), null);
            if (string.IsNullOrEmpty(this.DataFolder))
                this.TempDataFolder = data.GetString(nameof(this.TempDataFolder), null);
            this.Settings.ReadFromRBE(data.GetDictionary(nameof(this.Settings)));
            this.ResourceManager.ReadFromRBE(data.GetDictionary(nameof(this.ResourceManager)));
            this.Timeline.ReadFromRBE(data.GetDictionary(nameof(this.Timeline)));
        }

        /// <summary>
        /// Gets the absolute file path from a project path
        /// </summary>
        /// <param name="file">Input relative path</param>
        /// <returns>Output absolute filepath that may exist on the system</returns>
        /// <exception cref="ArgumentException">The path's file path is null or empty... somehow</exception>
        public string GetFilePath(ProjectPath file) {
            if (string.IsNullOrEmpty(file.FilePath)) {
                throw new ArgumentException("File's path cannot be null or empty", nameof(file));
            }

            if (file.IsAbsolute) {
                return Path.GetFullPath(file.FilePath);
            }
            else {
                return Path.Combine(this.GetDirectoryPath(out _), file.FilePath);
            }
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

        /// <summary>
        /// Gets or creates the project's data folder
        /// </summary>
        /// <param name="isTemp">Whether or not the current data folder is in the temp directory</param>
        /// <returns>The directory info for the data folder</returns>
        public DirectoryInfo GetDataFolder(out bool isTemp) {
            // Cleaner and also faster than manual existence check (exists() ? new DirectoryInfo(dir) : create())
            return Directory.CreateDirectory(this.GetDirectoryPath(out isTemp));
        }

        /// <summary>
        /// Gets or creates the project's data folder
        /// </summary>
        /// <returns>The directory info for the data folder</returns>
        public DirectoryInfo GetDataFolder() => this.GetDataFolder(out _);

        /// <summary>
        /// Updates the backing storage of the timeline, all tracks and all clips
        /// </summary>
        public void UpdateAutomationBackingStorage() {
            this.Timeline.UpdateAutomationBackingStorage();
        }
    }
}