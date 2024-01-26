using System.Windows;
using System.Windows.Media;
using FramePFX.Editors.Rendering;
using FramePFX.Editors.Timelines;
using SkiaSharp;

namespace FramePFX.Editors.Controls.Viewports {
    /// <summary>
    /// Extends <see cref="SKAsyncViewPort"/> to implement further timeline rendering things, like selected clips
    /// </summary>
    public class VideoEditorViewPortPreview : SKAsyncViewPort {
        private const double thickness = 2.5d;
        private const double half_thickness = thickness / 2d;
        public static readonly DependencyProperty VideoEditorProperty = DependencyProperty.Register("VideoEditor", typeof(VideoEditor), typeof(VideoEditorViewPortPreview), new PropertyMetadata(null, OnVideoEditorChanged));

        public VideoEditor VideoEditor {
            get => (VideoEditor) this.GetValue(VideoEditorProperty);
            set => this.SetValue(VideoEditorProperty, value);
        }

        private readonly Pen OutlinePen = new Pen(Brushes.Orange, 2.5f);

        private Project activeProject;

        public VideoEditorViewPortPreview() {

        }

        private static void OnVideoEditorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            VideoEditorViewPortPreview control = (VideoEditorViewPortPreview) d;
            if (e.OldValue is VideoEditor oldEditor) {
                oldEditor.ProjectChanged -= control.OnProjectChanged;
            }

            if (e.NewValue is VideoEditor newEditor) {
                newEditor.ProjectChanged += control.OnProjectChanged;
                control.SetProject(newEditor.Project);
            }
        }

        private void OnProjectChanged(VideoEditor editor, Project oldproject, Project newproject) {
            this.SetProject(newproject);
        }

        private void SetProject(Project project) {
            Project oldProject = this.activeProject;
            if (oldProject != null) {
                oldProject.MainTimeline.PlayHeadChanged -= OnTimelineSeeked;
                oldProject.RenderManager.FrameRendered -= this.OnFrameAvailable;
            }

            this.activeProject = project;
            if (project != null) {
                project.MainTimeline.PlayHeadChanged += OnTimelineSeeked;
                project.RenderManager.FrameRendered += this.OnFrameAvailable;
            }
        }

        private static void OnTimelineSeeked(Timeline timeline, long oldFrame, long frame) {
            timeline.Project.RenderManager.InvalidateRender();
        }

        private void OnFrameAvailable(RenderManager manager) {
            if (!this.BeginRender(out SKSurface surface)) {
                return;
            }

            try {
                surface.Canvas.Clear(SKColors.Black);
                manager.Draw(surface);
            }
            finally {
                this.EndRender();
            }
        }

        protected override void OnRender(DrawingContext dc) {
            base.OnRender(dc);
            // if (this.Timeline is Timeline timeline) {
            //     foreach (Track track in timeline.Tracks) {
            //         foreach (Clip clip in track.GetSelectedClipsAtFrame(timeline.PlayHeadPosition)) {
            //             if (!(clip is VideoClip) || !(((VideoClip)clip.Model).GetFrameSize() is Vector2 frameSize)) {
            //                 continue;
            //             }
            //
            //             SKRect rect = ((VideoClip)clip.Model).TransformationMatrix.MapRect(frameSize.ToRectAsSize(0, 0));
            //             Point pos = new Point(Math.Floor(rect.Left) - half_thickness, Math.Floor(rect.Top) - half_thickness);
            //             Size size = new Size(Math.Ceiling(rect.Width) + thickness, Math.Ceiling(rect.Height) + thickness);
            //             dc.DrawRectangle(null, this.OutlinePen, new Rect(pos, size));
            //         }
            //     }
            // }
        }
    }
}
