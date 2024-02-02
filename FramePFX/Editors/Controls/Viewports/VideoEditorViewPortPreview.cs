using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using FramePFX.Editors.Rendering;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Editors.Utils;
using FramePFX.Utils;
using SkiaSharp;
using Vector2 = System.Numerics.Vector2;

namespace FramePFX.Editors.Controls.Viewports {
    /// <summary>
    /// Extends <see cref="SKAsyncViewPort"/> to implement further timeline rendering things, like selected clips
    /// </summary>
    public class VideoEditorViewPortPreview : SKAsyncViewPort {
        private const double thickness = 2.5d;
        private const double half_thickness = thickness / 2d;
        public static readonly DependencyProperty VideoEditorProperty = DependencyProperty.Register("VideoEditor", typeof(VideoEditor), typeof(VideoEditorViewPortPreview), new PropertyMetadata(null, OnVideoEditorChanged));
        public static readonly DependencyProperty DrawSelectedElementsProperty = DependencyProperty.Register("DrawSelectedElements", typeof(bool), typeof(VideoEditorViewPortPreview), new FrameworkPropertyMetadata(BoolBox.True, FrameworkPropertyMetadataOptions.AffectsRender));

        public VideoEditor VideoEditor {
            get => (VideoEditor) this.GetValue(VideoEditorProperty);
            set => this.SetValue(VideoEditorProperty, value);
        }

        public bool DrawSelectedElements {
            get => (bool) this.GetValue(DrawSelectedElementsProperty);
            set => this.SetValue(DrawSelectedElementsProperty, value.Box());
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
                oldProject.Settings.ResolutionChanged -= this.UpdateResolution;
                oldProject.MainTimeline.PlayHeadChanged -= OnTimelineSeeked;
                oldProject.RenderManager.FrameRendered -= this.OnFrameAvailable;
            }

            this.activeProject = project;
            if (project != null) {
                project.Settings.ResolutionChanged += this.UpdateResolution;
                project.MainTimeline.PlayHeadChanged += OnTimelineSeeked;
                project.RenderManager.FrameRendered += this.OnFrameAvailable;
                this.UpdateResolution(project.Settings);
            }
        }

        private void UpdateResolution(ProjectSettings settings) {
            this.Width = settings.Width;
            this.Height = settings.Height;
        }

        private static void OnTimelineSeeked(Timeline timeline, long oldFrame, long frame) {
            timeline.Project.RenderManager.InvalidateRender();
        }

        private void OnFrameAvailable(RenderManager manager) {
            if (manager.Project.IsExporting) {
                return;
            }

            if (this.BeginRenderWithSurface(manager.surface)) {
                this.EndRenderWithSurface(manager.surface);
            }

            // if (!this.BeginRender(out SKSurface surface)) {
            //     return;
            // }
            // try {
            //     surface.Canvas.Clear(SKColors.Black);
            //     long start = Time.GetSystemTicks();
            //     manager.Draw(surface);
            //     long duration = Time.GetSystemTicks() - start;
            //     System.Diagnostics.Debug.WriteLine((duration / Time.TICK_PER_MILLIS_D).ToString());
            // }
            // finally {
            //     this.EndRender();
            // }
        }

        protected override void OnRender(DrawingContext dc) {
            base.OnRender(dc);
            if (this.DrawSelectedElements && this.VideoEditor?.Project?.MainTimeline is Timeline timeline) {
                foreach (Track track in timeline.Tracks) {
                    if (!(track is VideoTrack)) {
                        continue;
                    }

                    // potentially faster than scanning SelectedClips due to track clip chunking
                    IEnumerable<Clip> clips = track.GetClipsAtFrame(timeline.PlayHeadPosition).Where(x => x.IsSelected);
                    foreach (Clip clip in clips) {
                        if (clip is VideoClip videoClip && videoClip.GetRenderSize() is Vector2 frameSize) {
                            SKRect rect = videoClip.TransformationMatrix.MapRect(frameSize.ToSkiaAsSize(0, 0));
                            Point pos = new Point(Math.Floor(rect.Left) - half_thickness, Math.Floor(rect.Top) - half_thickness);
                            Size size = new Size(Math.Ceiling(rect.Width) + thickness, Math.Ceiling(rect.Height) + thickness);
                            dc.DrawRectangle(null, this.OutlinePen, new Rect(pos, size));
                        }
                    }
                }
            }
        }
    }
}
