//
// Copyright (c) 2023-2024 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

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
        public static readonly DependencyProperty SelectionOutlineBrushProperty = DependencyProperty.Register("SelectionOutlineBrush", typeof(Brush), typeof(VideoEditorViewPortPreview), new PropertyMetadata(Brushes.Orange, InvalidateSelectionPen));

        private static void InvalidateSelectionPen(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((VideoEditorViewPortPreview) d).OutlinePen = null;
        }

        public Brush SelectionOutlineBrush {
            get => (Brush) this.GetValue(SelectionOutlineBrushProperty);
            set => this.SetValue(SelectionOutlineBrushProperty, value);
        }

        public VideoEditor VideoEditor {
            get => (VideoEditor) this.GetValue(VideoEditorProperty);
            set => this.SetValue(VideoEditorProperty, value);
        }

        public bool DrawSelectedElements {
            get => (bool) this.GetValue(DrawSelectedElementsProperty);
            set => this.SetValue(DrawSelectedElementsProperty, value.Box());
        }

        private Pen OutlinePen;
        private Pen EffectiveDrawAreaOutlinePen;

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
                oldProject.ActiveTimelineChanged -= this.OnProjectActiveTimelineChanged;
                this.UpdateTimelineChanged(oldProject.ActiveTimeline, null);
            }

            this.activeProject = project;
            if (project != null) {
                project.Settings.ResolutionChanged += this.UpdateResolution;
                project.ActiveTimelineChanged += this.OnProjectActiveTimelineChanged;
                this.UpdateTimelineChanged(null, project.ActiveTimeline);
                this.UpdateResolution(project.Settings);
            }
        }

        private void OnProjectActiveTimelineChanged(Project project, Timeline oldTimeline, Timeline newTimeline) {
            this.UpdateTimelineChanged(oldTimeline, newTimeline);
        }

        private void UpdateTimelineChanged(Timeline oldTimeline, Timeline newTimeline) {
            if (oldTimeline != null) {
                oldTimeline.PlayHeadChanged -= OnTimelineSeeked;
                oldTimeline.RenderManager.FrameRendered -= this.OnFrameAvailable;
            }

            if (newTimeline != null) {
                newTimeline.PlayHeadChanged += OnTimelineSeeked;
                newTimeline.RenderManager.FrameRendered += this.OnFrameAvailable;
            }
        }

        private void UpdateResolution(ProjectSettings settings) {
            this.Width = settings.Width;
            this.Height = settings.Height;
        }

        private static void OnTimelineSeeked(Timeline timeline, long oldFrame, long frame) {
            timeline.RenderManager.InvalidateRender();
        }

        private void OnFrameAvailable(RenderManager manager) {
            if (manager.Timeline.Project?.IsExporting ?? false) {
                return;
            }

            if (this.BeginRenderWithSurface(manager.ImageInfo)) {
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
            if (this.DrawSelectedElements && this.VideoEditor?.Project?.ActiveTimeline is Timeline timeline) {
                foreach (Track track in timeline.Tracks) {
                    if (!(track is VideoTrack)) {
                        continue;
                    }

                    // potentially faster than scanning SelectedClips due to track clip chunking
                    IEnumerable<Clip> clips = track.GetClipsAtFrame(timeline.PlayHeadPosition).Where(x => x.IsSelected);
                    foreach (Clip clip in clips) {
                        if (clip is VideoClip videoClip && videoClip.GetRenderSize() is Vector2 frameSize) {
                            Pen pen = this.OutlinePen ?? (this.OutlinePen = new Pen(this.SelectionOutlineBrush ?? Brushes.Transparent, 2.5));
                            DrawClipOutline(videoClip, frameSize, dc, pen);
                        }
                    }
                }

                // SKRect rect = timeline.RenderManager.LastRenderRect;
                // if (rect.Width > 0 && rect.Height > 0) {
                //     Pen pen = this.EffectiveDrawAreaOutlinePen ?? (this.EffectiveDrawAreaOutlinePen = new Pen(Brushes.DarkRed, 2.5));
                //     DrawRectWithPen(dc, pen, rect);
                // }
            }
        }

        private static void DrawClipOutline(VideoClip clip, Vector2 renderSize, DrawingContext ctx, Pen pen) {
            SKRect rect = clip.ClipAndTrackTransformationMatrix.MapRect(renderSize.ToRectWH());
            double realX = Math.Floor(rect.Left);
            double realY = Math.Floor(rect.Top);
            double realW = Math.Ceiling(rect.Width + (rect.Left - realX));
            double realH = Math.Ceiling(rect.Height + (rect.Top - realY));
            Point pos = new Point(realX - half_thickness, realY - half_thickness);
            Size size = new Size(realW + thickness, realH + thickness);
            ctx.DrawRectangle(null, pen, new Rect(pos, size));
        }

        private static void DrawRectWithPen(DrawingContext ctx, Pen pen, SKRect rect) {
            double realX = Math.Floor(rect.Left);
            double realY = Math.Floor(rect.Top);
            double realW = Math.Ceiling(rect.Width + (rect.Left - realX));
            double realH = Math.Ceiling(rect.Height + (rect.Top - realY));
            double thickA = pen.Thickness;
            double thickB = thickA / 2.0;
            Point pos = new Point(realX - thickB, realY - thickB);
            Size size = new Size(realW + thickA, realH + thickA);
            ctx.DrawRectangle(null, pen, new Rect(pos, size));
        }
    }
}