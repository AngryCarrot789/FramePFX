//  
// Copyright (c) Xavier CLEMENCE (xavier.clemence@gmail.com). All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information. 
// Ruler Wpf Version 3.0
// 

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using FramePFX.Controls.xclemence.RulerWPF.PositionManagers;
using FramePFX.Controls.xclemence.RulerWPF.PositionManagers.@base;
using FramePFX.Shortcuts;

namespace FramePFX.Controls.xclemence.RulerWPF {
    [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "updateSubject", Justification = "Managed by unload method")]
    [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "updateSubcription", Justification = "Managed by unload method")]
    public class Ruler : RulerBase, IDisposable {
        private const int SubStepNumber = 10;

        private bool disposedValue;

        private RulerPositionManager rulerPostionControl;

        private Line marker;

        private bool isLoadedInternal;

        public Ruler() {
            this.UpdateRulerPosition(RulerPosition.Top);
            this.Loaded += this.OnRulerLoaded;
        }

        private Line Marker => this.marker ?? (this.marker = this.Template.FindName("marker", this) as Line);

        private ScrollViewer scroller;

        private void OnRulerLoaded(object sender, RoutedEventArgs e) {
            this.Loaded -= this.OnRulerLoaded;
            this.SizeChanged += this.OnRulerSizeChanged;
            this.Unloaded += this.OnRulerUnloaded;
            this.isLoadedInternal = true;

            this.scroller = VisualTreeUtils.FindVisualParent<ScrollViewer>(this);
            if (this.scroller == null) {
                return;
            }

            this.scroller.ScrollChanged += this.OnScrollerOnScrollChanged;
            this.scroller.SizeChanged += this.OnScrollerOnSizeChanged;

            this.RefreshRuler();
        }

        private void OnScrollerOnSizeChanged(object o, SizeChangedEventArgs e) {
            this.ScheduleRender();
        }

        private void OnScrollerOnScrollChanged(object o, ScrollChangedEventArgs e) {
            this.ScheduleRender();
        }

        private void ScheduleRender() {
            this.Dispatcher.Invoke(this.InvalidateVisual);
        }

        /// <summary>
        /// Gets the amount of drawing space this ruler should use to draw
        /// </summary>
        /// <param name="availableSize">
        /// An out parameter for the amount of actual space that was available.
        /// By default, it defaults to the width/height of the return value, however, if
        /// this ruler is placed somewhere in a scroll viewer, then it returns the total amount of space available by the scrollviewer</param>
        /// <returns></returns>
        public Rect GetDrawingBounds(out Size availableSize) {
            availableSize = this.RenderSize;
            if (this.scroller != null) {
                double offsetX = this.scroller.HorizontalOffset, offsetY = this.scroller.VerticalOffset;
                // availableSize = new Size(this.scroller.ExtentWidth, this.scroller.ExtentHeight);
                return new Rect(offsetX, offsetY, Math.Min(this.scroller.ViewportWidth, this.ActualWidth), Math.Min(this.scroller.ViewportHeight, this.ActualHeight));
            }
            else {
                return new Rect(new Point(), this.RenderSize);
            }
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
                this.rulerPostionControl = new LeftRulerManager(this);
            else
                this.rulerPostionControl = new TopRulerManager(this);
        }

        private void UpdateMarkerPosition(Point point) {
            if (this.Marker == null || this.rulerPostionControl == null)
                return;

            bool positionUpdated = this.rulerPostionControl.UpdateMakerPosition(this.Marker, point);

            this.Marker.Visibility = positionUpdated ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnRulerSizeChanged(object sender, SizeChangedEventArgs e) => this.RefreshRuler();

        public override void RefreshRuler() { }

        private bool CanDrawRuler() => this.ValidateSize() && (this.CanDrawSlaveMode() || this.CanDrawMasterMode());

        private bool ValidateSize() => this.ActualWidth > 0 && this.ActualHeight > 0;

        private bool CanDrawSlaveMode() => this.SlaveStepProperties != null;
        private bool CanDrawMasterMode() => (this.MajorStepValues != null && !double.IsNaN(this.MaxValue) && this.MaxValue > 0);

        protected override void OnRender(DrawingContext dc) {
            base.OnRender(dc);
            if (!this.CanDrawRuler()) {
                return;
            }

            int Ceil1(int value, int multiple) {
                int mod = value % multiple;
                return mod == 0 ? value : value + (multiple - mod);
            }

            double Ceil2(double value, int multiple) {
                double mod = value % multiple;
                return mod == 0 ? value : value + (multiple - mod);
            }

            var rect = this.GetDrawingBounds(out var size);

            double actualWidth = this.rulerPostionControl.GetSize();
            (double pixelStep, double valueStep) = this.GetStepProperties();
            double stepNumber = Math.Ceiling(actualWidth / pixelStep);
            double subPixelSize = pixelStep / SubStepNumber;

            {
                for (int y = 1; y < SubStepNumber; ++y) {
                    double subOffset = 0 + y * subPixelSize;
                    if (subOffset < rect.Left || subOffset > actualWidth) {
                        continue;
                    }

                    this.rulerPostionControl.DrawMinorLine(dc, subOffset);
                }
            }

            double majorLinePosition = this.DisplayZeroLine ? 0 : pixelStep;
            this.rulerPostionControl.DrawMajorLine(dc, majorLinePosition);

            RulerTextOverflow overflow = this.TextOverflow;
            for (int i = 0; i < stepNumber; ++i) {
                double offset = pixelStep * i;
                double offsetToCheckDisplay = overflow == RulerTextOverflow.Hidden ? (offset + pixelStep - subPixelSize) : offset;
                if (offsetToCheckDisplay >= rect.Left && offsetToCheckDisplay <= actualWidth) {
                    this.rulerPostionControl.DrawText(dc, i * valueStep, offset);
                    // this.LabelsControl.Children.Add(this.rulerPostionControl.CreateText(i * valueStep, offset));
                }
            }
        }

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
            double normalizeMinSize = this.MinPixelSize * SubStepNumber / this.rulerPostionControl.GetSize();

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
            double pixelStep = this.rulerPostionControl.GetSize() * realStepValue / this.MaxValue;

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