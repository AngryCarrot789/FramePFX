using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.PropertyEditing;
using FramePFX.Themes;
using FramePFX.Views;
using SkiaSharp;

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

        public EditorWindow() {
            this.InitializeComponent();
            this.Loaded += this.EditorWindow_Loaded;
        }

        private void EditorWindow_Loaded(object sender, RoutedEventArgs e) {
            this.ThePropertyEditor.ApplyTemplate();
            this.ThePropertyEditor.PropertyEditor = VideoEditorPropertyEditor.Instance;
            if (this.ViewPortElement.BeginRender(out SKSurface surface)) {
                using (SKPaint paint = new SKPaint() { Color = SKColors.Black }) {
                    surface.Canvas.DrawRect(0, 0, 1280, 720, paint);
                }

                using (SKPaint paint = new SKPaint() { Color = SKColors.OrangeRed }) {
                    surface.Canvas.DrawRect(0, 0, 90, 30, paint);
                }

                this.ViewPortElement.EndRender();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);
            if (e.Key == Key.LeftAlt) {
                e.Handled = true;
                return;
            }

            if (e.Key == Key.S) {
                Timeline timeline = this.Editor?.Project?.MainTimeline;
                if (timeline == null) {
                    return;
                }

                Clip selected = null;
                foreach (Track track in timeline.Tracks) {
                    if ((selected = track.Clips.FirstOrDefault(x => x.IsSelected)) != null) {
                        break;
                    }
                }

                long playHead = timeline.PlayHeadPosition;
                if (selected != null && selected.IntersectsFrameAt(playHead) && playHead != selected.FrameSpan.Begin && playHead != selected.FrameSpan.EndIndex) {
                    selected.CutAt(playHead - selected.FrameSpan.Begin);
                }
            }
        }

        private void OnEditorChanged(VideoEditor oldEditor, VideoEditor newEditor) {
            if (oldEditor != null) {
                oldEditor.ProjectChanged -= this.OnEditorProjectChanged;
                oldEditor.Playback.PlaybackStateChanged -= this.OnEditorPlaybackStateChanged;
                this.ViewPortElement.VideoEditor = null;
            }

            if (newEditor != null) {
                newEditor.ProjectChanged += this.OnEditorProjectChanged;
                newEditor.Playback.PlaybackStateChanged += this.OnEditorPlaybackStateChanged;
                this.ViewPortElement.VideoEditor = newEditor;
            }

            Project project = newEditor?.Project;
            if (project != null) {
                this.UpdateResourceManager(project.ResourceManager);
                this.UpdateTimeline(project.MainTimeline);
            }
            else {
                this.UpdateResourceManager(null);
                this.UpdateTimeline(null);
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
            this.UpdateResourceManager(newProject?.ResourceManager);
            this.UpdateTimeline(newProject?.MainTimeline);
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
    }
}
