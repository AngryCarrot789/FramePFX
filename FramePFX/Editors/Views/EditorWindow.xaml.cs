using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using FramePFX.Editors.Exporting;
using FramePFX.Editors.Exporting.Controls;
using FramePFX.Editors.Rendering;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.Timelines;
using FramePFX.Interactivity.DataContexts;
using FramePFX.PropertyEditing;
using FramePFX.Shortcuts.WPF;
using FramePFX.Themes;
using FramePFX.Utils;
using FramePFX.Views;

namespace FramePFX.Editors.Views {
    /// <summary>
    /// Interaction logic for EditorWindow.xaml
    /// </summary>
    public partial class EditorWindow : WindowEx {
        public static readonly DependencyProperty EditorProperty = DependencyProperty.Register("Editor", typeof(VideoEditor), typeof(EditorWindow), new PropertyMetadata(null, (o, e) => ((EditorWindow) o).OnEditorChanged((VideoEditor)e.OldValue, (VideoEditor)e.NewValue)));

        public VideoEditor Editor {
            get => (VideoEditor) this.GetValue(EditorProperty);
            set => this.SetValue(EditorProperty, value);
        }

        private readonly DataContext actionSystemDataContext;
        private readonly DispatcherTimer updateRenderIntervalTimer;

        private readonly NumberAverager renderTimeAverager;

        public EditorWindow() {
            this.renderTimeAverager = new NumberAverager(10); // average 5 samples. Will take a second to catch up at 5 fps but meh
            this.actionSystemDataContext = new DataContext();
            this.InitializeComponent();
            this.Loaded += this.EditorWindow_Loaded;
            UIInputManager.SetActionSystemDataContext(this, this.actionSystemDataContext);
        }

        protected override Task<bool> OnClosingAsync() {
            // Close the project (which also destroys it) so that we can safely close and destroy all
            // used objects (e.g. images, file locks, video files, etc.) even thought it may not be
            // strictly necessary, still seems like a good idea to do so
            if (this.Editor is VideoEditor editor) {
                if (editor.Project != null) {
                    editor.CloseProject();
                }

                this.Editor = null;
            }

            return Task.FromResult(true);
        }

        private void UpdateFrameRenderInterval(RenderManager manager) {
            this.renderTimeAverager.PushValue(manager.AverageRenderTimeMillis);

            double averageMillis = this.renderTimeAverager.GetAverage();
            this.PART_AvgRenderTimeBlock.Text = $"{Math.Round(averageMillis, 2).ToString(),5} ms ({((int) Math.Round(1000.0 / averageMillis)).ToString(),3} FPS)";
        }

        private void EditorWindow_Loaded(object sender, RoutedEventArgs e) {
            this.ThePropertyEditor.ApplyTemplate();
            this.ThePropertyEditor.PropertyEditor = VideoEditorPropertyEditor.Instance;
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);
            if (e.Key == Key.LeftAlt) {
                e.Handled = true;
            }
        }

        private void OnEditorChanged(VideoEditor oldEditor, VideoEditor newEditor) {
            if (oldEditor != null) {
                oldEditor.ProjectChanged -= this.OnEditorProjectChanged;
                oldEditor.Playback.PlaybackStateChanged -= this.OnEditorPlaybackStateChanged;
                if (oldEditor.Project != null) {
                    this.OnProjectChanged(oldEditor.Project, null);
                }

                this.PART_ViewPort.VideoEditor = null;
            }

            if (newEditor != null) {
                newEditor.ProjectChanged += this.OnEditorProjectChanged;
                newEditor.Playback.PlaybackStateChanged += this.OnEditorPlaybackStateChanged;
                this.PART_ViewPort.VideoEditor = newEditor;
            }

            this.actionSystemDataContext.Set(DataKeys.EditorKey, newEditor);
            Project project = newEditor?.Project;
            this.actionSystemDataContext.Set(DataKeys.ProjectKey, project);
            if (project != null) {
                this.OnProjectChanged(null, project);
            }

            this.UpdatePlayBackButtons(newEditor?.Playback);
        }

        private void OnEditorPlaybackStateChanged(PlaybackManager sender, PlayState state, long frame) {
            this.UpdatePlayBackButtons(sender);
        }

        private void UpdatePlayBackButtons(PlaybackManager manager) {
            if (manager != null) {
                this.PlayBackButton_Play.IsEnabled = manager.CanSetPlayStateTo(PlayState.Play);
                this.PlayBackButton_Pause.IsEnabled = manager.CanSetPlayStateTo(PlayState.Pause);
                this.PlayBackButton_Stop.IsEnabled = manager.CanSetPlayStateTo(PlayState.Stop);
            }
            else {
                this.PlayBackButton_Play.IsEnabled = false;
                this.PlayBackButton_Pause.IsEnabled = false;
                this.PlayBackButton_Stop.IsEnabled = false;
            }
        }

        private void OnEditorProjectChanged(VideoEditor editor, Project oldProject, Project newProject) {
            this.OnProjectChanged(oldProject, newProject);
            this.actionSystemDataContext.Set(DataKeys.ProjectKey, newProject);
        }

        private void OnProjectChanged(Project oldProject, Project newProject) {
            if (oldProject != null) {
                oldProject.RenderManager.FrameRendered -= this.UpdateFrameRenderInterval;
            }

            if (newProject != null) {
                newProject.RenderManager.FrameRendered += this.UpdateFrameRenderInterval;
            }

            this.UpdateRenderSettings(newProject?.Settings);
            this.UpdateResourceManager(newProject?.ResourceManager);
            this.UpdateTimeline(newProject?.MainTimeline);
        }

        private void UpdateRenderSettings(ProjectSettings settings) {
            if (settings != null) {
                this.PART_ViewPort.Width = settings.Width;
                this.PART_ViewPort.Height = settings.Height;
            }
        }

        private void UpdateResourceManager(ResourceManager manager) {
            this.TheResourcePanel.ResourceManager = manager;
        }

        private void UpdateTimeline(Timeline timeline) {
            this.TheTimeline.Timeline = timeline;
        }

        private void OnFitToContentClicked(object sender, RoutedEventArgs e) {
            this.VPViewBox.FitContentToCenter();
        }

        private void TogglePlayPauseClick(object sender, RoutedEventArgs e) {
            if (this.Editor is VideoEditor editor) {
                if (editor.Playback.PlayState == PlayState.Play) {
                    editor.Playback.Pause();
                }
                else if (editor.Project != null) {
                    editor.Playback.Play(editor.Project.MainTimeline.PlayHeadPosition);
                }
            }
        }

        private void PlayClick(object sender, RoutedEventArgs e) {
            if (this.Editor is VideoEditor editor && editor.Project != null) {
                editor.Playback.Play(editor.Project.MainTimeline.PlayHeadPosition);
            }
        }

        private void PauseClick(object sender, RoutedEventArgs e) {
            if (this.Editor is VideoEditor editor && editor.Project != null) {
                editor.Playback.Pause();
            }
        }

        private void StopClick(object sender, RoutedEventArgs e) {
            if (this.Editor is VideoEditor editor && editor.Project != null) {
                editor.Playback.Stop();
            }
        }

        private void SetThemeClick(object sender, RoutedEventArgs e) {
            ThemeType type;
            switch (((MenuItem)sender).Uid) {
                case "0": type = ThemeType.DeepDark;      break;
                case "1": type = ThemeType.SoftDark;      break;
                case "2": type = ThemeType.DarkGreyTheme; break;
                case "3": type = ThemeType.GreyTheme;     break;
                case "4": type = ThemeType.RedBlackTheme; break;
                case "5": type = ThemeType.LightTheme;    break;
                default: return;
            }

            ThemeController.SetTheme(type);
        }

        private void Export_Click(object sender, RoutedEventArgs e) {
            Project project = this.Editor.Project;
            project.Editor.Playback.Pause();
            ExportDialog dialog = new ExportDialog {
                ExportSetup = new ExportSetup(project) {
                    Properties = {
                        Span = new FrameSpan(0, project.MainTimeline.LargestFrameInUse),
                        FilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "OutputVideo.mp4")
                    }
                }
            };
            dialog.ShowDialog();
        }
    }
}
