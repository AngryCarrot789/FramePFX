﻿//  
// Copyright (c) Xavier CLEMENCE (xavier.clemence@gmail.com) and REghZy/AngryCarrot789. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information. 
// Ruler Wpf Version 3.1
// 

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using FramePFX.Editors.Controls.Rulers.Rulers;
using FramePFX.Utils;
using Rect = System.Windows.Rect;

namespace FramePFX.Editors.Controls.Rulers {
    public class Ruler : RulerBase {
        public const int SubStepNumber = 10;
        private RulerPositionManager positionManager;
        private Rect lastRect;
        private Size lastRenderSize;
        private bool isRenderClean;
        private ScrollViewer scroller;

        public Ruler() {
            this.positionManager = new TopRulerManager(this);
            this.Loaded += this.OnRulerLoaded;
            this.ClipToBounds = true;
        }

        private void OnRulerLoaded(object sender, RoutedEventArgs e) {
            this.Loaded -= this.OnRulerLoaded;
            this.SizeChanged += this.OnRulerSizeChanged;

            // allows high performance rendering, so that we aren't rendering stuff that's offscreen
            this.scroller = VisualTreeUtils.GetParent<ScrollViewer>(this);
            if (this.scroller != null) {
                this.scroller.SizeChanged += this.OnScrollerOnSizeChanged;
                this.scroller.ScrollChanged += this.OnScrollerOnScrollChanged;
            }

            this.isRenderClean = false;
            this.Dispatcher.InvokeAsync(this.InvalidateVisual, DispatcherPriority.Background);
        }

        private void OnScrollerOnSizeChanged(object o, SizeChangedEventArgs e) {
            this.isRenderClean = false;
            this.InvalidateVisual();
        }

        private void OnScrollerOnScrollChanged(object o, ScrollChangedEventArgs e) {
            if (e.HorizontalChange == 0 && e.VerticalChange == 0) {
                return;
            }

            this.isRenderClean = false;
            this.InvalidateVisual();
        }

        protected override Size MeasureOverride(Size availableSize) {
            return base.MeasureOverride(availableSize);
        }

        protected override void UpdateRulerPosition(RulerPosition position) {
            if (position == RulerPosition.Left)
                this.positionManager = new LeftRulerManager(this);
            else
                this.positionManager = new TopRulerManager(this);
        }

        private void OnRulerSizeChanged(object sender, SizeChangedEventArgs e) => this.InvalidateVisual();

        private bool CanDrawRuler() => this.ValidateSize() && (this.CanDrawSlaveMode() || this.CanDrawMasterMode());

        private bool ValidateSize() => this.ActualWidth > 0 && this.ActualHeight > 0;

        private bool CanDrawSlaveMode() => this.SlaveStepProperties != null;

        private bool CanDrawMasterMode() => (this.MajorStepValues != null && !double.IsNaN(this.MaxValue) && this.MaxValue > 0);

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
            base.OnRenderSizeChanged(sizeInfo);
            this.isRenderClean = false;
        }

        protected override void OnRender(DrawingContext dc) {
            base.OnRender(dc);
            if (!this.CanDrawRuler()) {
                return;
            }

            // Size oldSize = this.RenderSize;
            // if (this.isRenderClean && oldSize == this.lastRenderSize) {
            //     return;
            // }
            // this.lastRenderSize = this.RenderSize;
            // this.isRenderClean = true;

            // When zooming, OnRender gets called 3 times... this is why zooming is laggy
            // calculate visible pixel bounds
            Rect rect = UIUtils.GetVisibleRect(this.scroller, this);
            if (rect.Width < 0.01D && rect.Height < 0.01D) {
                return;
            }

            if (this.Background is Brush bg) {
                dc.DrawRectangle(bg, null, rect);
            }

            double pixelStep;
            double valueStep;
            if (this.SlaveStepProperties == null) {
                (pixelStep, valueStep) = this.GetMajorStep();
                this.StepProperties = new RulerStepProperties {PixelSize = pixelStep, Value = valueStep};
            }
            else {
                (pixelStep, valueStep) = this.SlaveStepProperties;
            }

            if (this.ValueStepTransform != null) {
                valueStep = this.ValueStepTransform(valueStep);
            }

            // double major_line_pos = this.DisplayZeroLine ? 0 : pixelStep;

            int steps = Math.Min((int) Math.Floor(valueStep), SubStepNumber);
            double subpixel_size = pixelStep / steps;

            // pxA = bound begin, pxB = bound end
            double pxA, pxB;
            if (this.RulerPosition == RulerPosition.Top) {
                pxA = rect.Left;
                pxB = rect.Right + pixelStep;
            }
            else {
                pxA = rect.Top;
                pxB = rect.Bottom + pixelStep;
            }

            // calculate an initial offset instead of looping until we get into a visible region
            // Flooring may result in us drawing things partially offscreen to the left, which is kinda required
            int i = (int) Math.Floor(pxA / pixelStep);
            int j = (int) Math.Ceiling(pxB / pixelStep);
            do {
                double pixel = i * pixelStep;
                if (i > j) {
                    break;
                }

                // TODO: optimise smaller/minor lines, maybe using skia?
                for (int y = 1; y < steps; ++y) {
                    double sub_pixel = pixel + y * subpixel_size;
                    // calculates p1 and p2 then calls DrawLine
                    this.positionManager.DrawMinorLine(dc, sub_pixel);
                }

                double text_value = i * valueStep;
                if (Math.Abs(text_value - (int) text_value) < 0.00001d) {
                    // calculates p1 and p2 then calls DrawLine
                    this.positionManager.DrawMajorLine(dc, pixel);

                    // creates formatted text and DrawText
                    this.positionManager.DrawText(dc, text_value, pixel);
                }

                i++;
            } while (true);
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
            // find the minimal position of first major step between 0 and 1
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
    }
}