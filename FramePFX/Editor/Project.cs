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
        /// This project's data folder, which is where file-based data is stored (that isn't stored using an absolute path).
        /// <para>
        /// This will return null if a data folder has not been generated, in which case,
        /// calling <see cref="GetDataFolderPath"/> or any similar function will generate it
        /// </para>
        /// </summary>
        public string DataFolder { get; private set; }

        /// <summary>
        /// Whether or not <see cref="DataFolder"/> is located in the tmp folder
        /// </summary>
        public bool IsTempDataFolder { get; private set; }

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
            this.Settings.WriteToRBE(data.CreateDictionary(nameof(this.Settings)));
            this.ResourceManager.WriteToRBE(data.CreateDictionary(nameof(this.ResourceManager)));
            this.Timeline.WriteToRBE(data.CreateDictionary(nameof(this.Timeline)));
        }

        public void ReadFromRBE(RBEDictionary data, string dataFolder) {
            if (this.DataFolder != null) {
                throw new Exception("Our data folder is not invalid; cannot replace it");
            }

            this.DataFolder = dataFolder;
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
                throw new ArgumentException("File's path cannot be null or empty (corrupted project path)", nameof(file));
            }

            if (file.IsAbsolute) {
                return Path.GetFullPath(file.FilePath);
            }
            else {
                return Path.Combine(this.GetDataFolderPath(out _), file.FilePath);
            }
        }

        public string GetDataFolderPath(out bool isTemp) {
            if (string.IsNullOrEmpty(this.DataFolder)) {
                this.IsTempDataFolder = isTemp = true;
                this.DataFolder = RandomUtils.RandomLettersWhere(Path.GetTempPath(), 16, x => !Directory.Exists(x));
            }
            else {
                isTemp = false;
            }

            return this.DataFolder;
        }

        /// <summary>
        /// Gets or creates the project's data folder
        /// </summary>
        /// <param name="isTemp">Whether or not the current data folder is in the temp directory</param>
        /// <returns>The directory info for the data folder</returns>
        public DirectoryInfo GetDataFolder(out bool isTemp) {
            // Cleaner and also faster than manual existence check (exists() ? new DirectoryInfo(dir) : create())
            return Directory.CreateDirectory(this.GetDataFolderPath(out isTemp));
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

        public static void SetDataFolder(Project project, string value) {
            project.DataFolder = value;
        }
    }
}