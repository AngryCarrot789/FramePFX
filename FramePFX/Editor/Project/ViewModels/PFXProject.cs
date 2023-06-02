using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using FramePFX.Core;
using FramePFX.Core.RBC;
using FramePFX.Core.Utils;
using FramePFX.Core.Views.Dialogs;
using FramePFX.Editor.Project.EditorDialogs;
using FramePFX.Editor.Timeline.ViewModels;
using FramePFX.Editor.Timeline.ViewModels.Clips.Resizable;
using FramePFX.Editor.Timeline.ViewModels.Layer;
using FramePFX.ResourceManaging;
using FramePFX.ResourceManaging.Items;
using FramePFX.ResourceManaging.ViewModels;

namespace FramePFX.Editor.Project.ViewModels {
    // Could rename this to something else, but naming it Project might be tricky as that's the namespace name
    public class PFXProject : BaseViewModel {
        private double frameRate;
        private Resolution resolution;
        private string projectPath;
        private bool hasFirstSave;

        /// <summary>
        /// Whether this project is currently being rendered (e.g. to a file or video stream)
        /// </summary>
        public bool IsRendering { get; set; }

        /// <summary>
        /// This project's timeline
        /// </summary>
        public PFXTimeline Timeline { get; }

        /// <summary>
        /// This project's resources
        /// </summary>
        public ResourceManagerViewModel ResourceManager { get; }


        /// <summary>
        /// This project's render resolution (in the viewport display)
        /// </summary>
        public Resolution Resolution {
            get => this.resolution;
            private set => this.RaisePropertyChanged(ref this.resolution, value);
        }

        /// <summary>
        /// This project's render frame rate
        /// </summary>
        public double FrameRate {
            get => this.frameRate;
            private set => this.RaisePropertyChanged(ref this.frameRate, value);
        }

        /// <summary>
        /// The path to the folder of this project
        /// </summary>
        public string ProjectPath {
            get => this.projectPath;
            private set => this.RaisePropertyChanged(ref this.projectPath, value);
        }

        public PFXVideoEditor VideoEditor { get; }

        public ICommand SaveCommand { get; }
        public ICommand SaveAsCommand { get; }
        public ICommand OpenSettingsCommand { get; }

        public PFXProject(PFXVideoEditor videoEditor) {
            this.VideoEditor = videoEditor;
            this.Timeline = new PFXTimeline(this);
            this.ResourceManager = new ResourceManagerViewModel(new ResourceManager());
            this.SaveCommand = new AsyncRelayCommand(this.SaveActionAsync);
            this.SaveAsCommand = new AsyncRelayCommand(this.SaveAsActionAsync);
            this.OpenSettingsCommand = new AsyncRelayCommand(this.OpenSettingsAction);
        }

        public async Task<bool> SaveActionAsync() {
            if (File.Exists(this.ProjectPath) || this.hasFirstSave) {
                await this.SaveActionAsync(this.ProjectPath);
                return true;
            }
            else {
                return await this.SaveAsActionAsync();
            }
        }

        public async Task<bool> SaveAsActionAsync() {
            DialogResult<string> result = IoC.FilePicker.ShowFolderPickerDialog(this.ProjectPath, "Select a folder, in which the project data will be saved into");
            if (result.IsSuccess) {
                this.ProjectPath = result.Value;
                await this.SaveActionAsync(this.ProjectPath);
                return true;
            }
            else {
                return false;
            }
        }

        public async Task SaveActionAsync(string folder) {
            #if DEBUG
            await this.SaveProjectIntoFolder(folder);
            this.hasFirstSave = true;
            #else
            try {
                await this.SaveFile(folder);
                this.hasFirstSave = true;
            }
            catch (Exception e) {
                await CoreIoC.MessageDialogs.ShowMessageAsync("Failed to save", $"Failed to save project to {folder}:\n{e.Message}");
            }
            #endif
        }

        public async Task SaveProjectIntoFolder(string folder) {
            RBEDictionary map = new RBEDictionary();
            map.SetStruct("Resolution", this.Resolution);
            map.SetDouble("FPS", this.FrameRate);
            RBEUtils.WriteToFile(map, Path.Combine(folder, "Project.pfx"));
        }

        public async Task ReadProjectFromFolder(string folder) {
            RBEDictionary map = RBEUtils.ReadFromFile(Path.Combine(folder, "Project.pfx")) as RBEDictionary;
            if (map == null) {
                throw new Exception($"Failed to read root {nameof(RBEDictionary)} for Project.pfx");
            }

            this.Resolution = map.GetStruct<Resolution>("Resolution");
            this.FrameRate = map.GetDouble("FPS");
            RBEDictionary resources = map.GetOrCreateDictionaryElement("Resources");
            // await this.ResourceManager.SaveResources(resources, folder);
        }

        public async Task OpenSettingsAction() {
            ProjectSettingsViewModel x = await ProjectSettingsDialogService.Instance.EditSettings(this);
            if (x != null) {
                if (x.FrameRate < 1 || this.resolution.Width < 1 || this.resolution.Height < 1) {
                    return;
                }

                this.Resolution = new Resolution(x.Width, x.Height);
                this.FrameRate = x.FrameRate;

                this.VideoEditor.UpdateResolution(this.Resolution);
                this.Timeline.ScheduleRender(false);
            }
        }

        public void RenderTimeline() {
            this.Timeline.ScheduleRender(true);
        }

        public void SetupDefaultProject() {
            // this.PlaybackResolution = new Resolution(1920, 1080);
            this.Resolution = new Resolution(1280, 720);
            this.FrameRate = 25;
            this.Timeline.MaxDuration = 10000;
            ResourceManager manager = this.ResourceManager.Manager;
            ResourceRGBA redColour = new ResourceRGBA(manager) {
                R = 0.9f, G = 0.1f, B = 0.1f
            };

            ResourceRGBA greenColour = new ResourceRGBA(manager) {
                R = 0.1f, G = 0.9f, B = 0.1f
            };

            ResourceRGBA blueColour = new ResourceRGBA(manager) {
                R = 0.1f, G = 0.1f, B = 0.9f
            };

            manager.AddResource("Resource_RED", redColour);
            manager.AddResource("Resource_GREEN", greenColour);
            manager.AddResource("Resource_BLUE", blueColour);

            PFXTimelineLayer l1 = this.Timeline.CreateVideoLayer("Layer 1");
            CreateSquare(l1, 0, 50, redColour, 5f, 5f, 100f, 100f, "Red_0");
            CreateSquare(l1, 100, 150, redColour, 105f, 5f, 100f, 100f, "Red_1");
            CreateSquare(l1, 275, 50, greenColour, 210f, 5f, 100f, 100f, "Green_0");

            PFXTimelineLayer l2 = this.Timeline.CreateVideoLayer("Layer 2");
            CreateSquare(l2, 0, 100, greenColour, 5f, 105f, 100f, 100f, "Green_1");
            CreateSquare(l2, 100, 50, blueColour, 105f, 105f, 100f, 100f, "Blue_0");
            CreateSquare(l2, 175, 75, blueColour, 210f, 105f, 100f, 100f, "Blue_1");
        }

        public static void CreateSquare(PFXTimelineLayer timelineLayer, long begin, long duration, ResourceRGBA colour, float x, float y, float w, float h, string name) {
            PFXShapeClip timelineClip = timelineLayer.CreateSquareClip(begin, duration, colour);
            timelineClip.DisplayName = name;
            timelineClip.SetShape(x, y, w, h);
        }

        public void OnPlayBegin() {
            this.Timeline.OnPlayBegin();
        }

        public void OnPlayEnd() {
            this.Timeline.OnPlayEnd();
        }
    }
}