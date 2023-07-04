//  
// Copyright (c) Xavier CLEMENCE (xavier.clemence@gmail.com) and REghZy/AngryCarrot789. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information. 
// Ruler Wpf Version 3.1
// 

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using FramePFX.Controls.xclemence.RulerWPF.PositionManagers;
using FramePFX.Utils;
using Rect = System.Windows.Rect;

namespace FramePFX.Controls.xclemence.RulerWPF {
    public class Ruler : RulerBase, IDisposable {
        public const int SubStepNumber = 10;
        private bool disposedValue;
        private RulerPositionManager positionManager;
        private Line marker;
        private bool isLoadedInternal;

        public Ruler() {
            this.positionManager = new TopRulerManager(this);
            this.Loaded += this.OnRulerLoaded;
        }

        private Line Marker => this.marker ?? (this.marker = this.Template.FindName("marker", this) as Line);

        private ScrollViewer scroller;

        private void OnRulerLoaded(object sender, RoutedEventArgs e) {
            this.Loaded -= this.OnRulerLoaded;
            this.SizeChanged += this.OnRulerSizeChanged;
            this.Unloaded += this.OnRulerUnloaded;
            this.isLoadedInternal = true;

            // allows high performance rendering, so that we aren't rendering stuff that's offscreen
            this.scroller = VisualTreeUtils.FindVisualParent<ScrollViewer>(this);
            if (this.scroller != null) {
                this.scroller.SizeChanged += this.OnScrollerOnSizeChanged;
                this.scroller.ScrollChanged += this.OnScrollerOnScrollChanged;
            }

            this.Dispatcher.InvokeAsync(this.InvalidateVisual, DispatcherPriority.Background);
        }

        private void OnScrollerOnSizeChanged(object o, SizeChangedEventArgs e) {
            this.InvalidateVisual();
        }

        private void OnScrollerOnScrollChanged(object o, ScrollChangedEventArgs e) {
            this.InvalidateVisual();
        }

        private void OnRulerUnloaded(object sender, RoutedEventArgs e) => this.UnloadControl();

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            Point mousePosition = e.GetPosition(this);
            this.UpdateMarkerPosition(mousePosition);
        }

        private void OnExternalMouseMouve(object sender, MouseEventArgs e) {
            Point mousePosition = e.GetPosition(this);
            this.UpdateMarkerPosition(mousePosition);
        }

        protected override void UpdateRulerPosition(RulerPosition position) {
            if (position == RulerPosition.Left)
                this.positionManager = new LeftRulerManager(this);
            else
                this.positionManager = new TopRulerManager(this);
        }

        private void UpdateMarkerPosition(Point point) {
            if (this.Marker == null || this.positionManager == null) {
                return;
            }

            bool positionUpdated = this.positionManager.OnUpdateMakerPosition(this.Marker, point);
            this.Marker.Visibility = positionUpdated ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnRulerSizeChanged(object sender, SizeChangedEventArgs e) => this.InvalidateVisual();

        private bool CanDrawRuler() => this.ValidateSize() && (this.CanDrawSlaveMode() || this.CanDrawMasterMode());

        private bool ValidateSize() => this.ActualWidth > 0 && this.ActualHeight > 0;

        private bool CanDrawSlaveMode() => this.SlaveStepProperties != null;
        private bool CanDrawMasterMode() => (this.MajorStepValues != null && !double.IsNaN(this.MaxValue) && this.MaxValue > 0);

        protected override void OnRender(DrawingContext dc) {
            base.OnRender(dc);
            if (this.CanDrawRuler()) {
                Rect db = this.scroller == null ? new Rect(new Point(), this.RenderSize) : new Rect(this.scroller.HorizontalOffset, this.scroller.VerticalOffset, this.scroller.ViewportWidth, this.scroller.ViewportHeight);
                (double pixel_step, double value_step) = this.GetStepProperties();
                double major_line_pos = this.DisplayZeroLine ? 0 : pixel_step;
                double subpixel_size = pixel_step / SubStepNumber;

                // calculate visible pixel bounds
                double pixel_bound_begin = this.Position == RulerPosition.Top ? db.Left : db.Top;
                double pixel_bound_end = this.Position == RulerPosition.Top ? db.Right : db.Bottom;

                // calculate an initial offset instead of looping until we get into a visible region
                // Flooring may result in us drawing things partially offscreen to the left, which is kinda required
                int initial_offset = (int) Math.Floor(pixel_bound_begin / pixel_step);
                for (int i = initial_offset; true; i++) {
                    double pixel = i * pixel_step;
                    if (pixel <= pixel_bound_end) {
                        for (int y = 1; y < SubStepNumber; ++y) {
                            double sub_pixel = pixel + y * subpixel_size;
                            this.positionManager.DrawMinorLine(dc, sub_pixel);
                        }

                        double text_value = i * value_step;
                        this.positionManager.DrawMajorLine(dc, pixel + major_line_pos);
                        this.positionManager.DrawText(dc, text_value, pixel);
                    }
                    else {
                        break;
                    }
                }
            }
            else {
                return;
            }
        }

        // public static void Draw(DrawingContext dc, Rect render_area, Size control_size, double pixel_offset) {
        //     double cycles = Math.Ceiling(rect.Left - 0) / subPixelSize;
        //     double pixel_start = cycles * subPixelSize;
        //     double pixel = pixel_start;
        //     while (pixel >= rect.Left && pixel <= ) {
        //     }
        // }

        private (double pixelStep, double valueStep) GetStepProperties() {
            double pixelStep;
            double valueStep;

            if (this.SlaveStepProperties == null) {
                (pixelStep, valueStep) = this.GetMajorStep();
                this.StepProperties = new RulerStepProperties {PixelSize = pixelStep, Value = valueStep};
            }
            else {
                (pixelStep, valueStep) = this.SlaveStepProperties;
            }

            if (this.ValueStepTransform != null)
                valueStep = this.ValueStepTransform(valueStep);

            return (pixelStep, valueStep);
        }

        private (double pixelStep, double valueStep) GetMajorStep() {
            // find thes minimal position of first major step between 0 and 1
            double normalizeMinSize = this.MinPixelSize * SubStepNumber / this.positionManager.GetSize();

            // calculate the real value of this step (min step value)
            double minStepValue = normalizeMinSize * this.MaxValue;

            // calculate magnetude of min step value (power of ten)
            int minStepValueMagnitude = (int) Math.Floor(Math.Log10(minStepValue));

            // normalise min step value between 0 and 10 (according to Major step value scale)
            double normalizeMinStepValue = minStepValue / Math.Pow(10, minStepValueMagnitude);

            // select best step according values defined by customer
            int normalizeRealStepValue = this.MajorStepValues.Union(new int[] {10}).First(x => x > normalizeMinStepValue);

            // apply magnitude to return inside  initial value scale
            double realStepValue = normalizeRealStepValue * Math.Pow(10, minStepValueMagnitude);

            // find size of real value (pixel)
            double pixelStep = this.positionManager.GetSize() * realStepValue / this.MaxValue;

            return (pixelStep, valueStep: realStepValue);
        }

        protected override void UpdateMarkerControlReference(UIElement oldElement, UIElement newElement) {
            if (oldElement != null)
                oldElement.MouseMove -= this.OnExternalMouseMouve;
            if (newElement != null)
                newElement.MouseMove += this.OnExternalMouseMouve;
        }

        private void UnloadControl() {
            if (this.isLoadedInternal) {
                if (this.MarkerControlReference != null)
                    this.MarkerControlReference.MouseMove -= this.OnExternalMouseMouve;
                this.isLoadedInternal = false;
            }
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing) {
            if (!this.disposedValue) {
                if (disposing)
                    this.UnloadControl();

                this.disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}