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
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using FramePFX.Editors.Timelines;
using FramePFX.Utils;
using FramePFX.Utils.Visuals;
using Rect = System.Windows.Rect;

namespace FramePFX.Editors.Controls.Timelines
{
    public class TimelineRuler : FrameworkElement
    {
        private static readonly int[] Steps = new[] {1, 2, 5, 10};
        private const double MinRender = 0.01D;
        private const double MajorLineThickness = 1.0;
        private const double MinorStepRatio = 0.5;
        public static readonly Typeface FallbackTypeFace = new Typeface("Consolas");
        public static readonly DependencyProperty TimelineProperty = DependencyProperty.Register("Timeline", typeof(Timeline), typeof(TimelineRuler), new PropertyMetadata(null, (d, e) => ((TimelineRuler) d).OnTimelineChanged((Timeline) e.OldValue, (Timeline) e.NewValue)));
        public static readonly DependencyProperty BackgroundProperty = Panel.BackgroundProperty.AddOwner(typeof(TimelineRuler), new FrameworkPropertyMetadata(Panel.BackgroundProperty.DefaultMetadata.DefaultValue, FrameworkPropertyMetadataOptions.None));
        public static readonly DependencyProperty FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(typeof(TimelineRuler), new FrameworkPropertyMetadata(SystemFonts.MessageFontFamily, (d, e) => ((TimelineRuler) d).CachedTypeFace = null));
        public static readonly DependencyProperty ForegroundProperty = TextElement.ForegroundProperty.AddOwner(typeof(TimelineRuler), new FrameworkPropertyMetadata(SystemColors.ControlTextBrush));
        public static readonly DependencyProperty StepColorProperty = DependencyProperty.Register(nameof(StepColor), typeof(Brush), typeof(TimelineRuler), new FrameworkPropertyMetadata(Brushes.DimGray, FrameworkPropertyMetadataOptions.AffectsRender, (d, e) => ((TimelineRuler) d).majorLineStepColourPen = null));

        public Timeline Timeline
        {
            get => (Timeline) this.GetValue(TimelineProperty);
            set => this.SetValue(TimelineProperty, value);
        }

        public Brush Background
        {
            get => (Brush) this.GetValue(BackgroundProperty);
            set => this.SetValue(BackgroundProperty, value);
        }

        public FontFamily FontFamily
        {
            get => (FontFamily) this.GetValue(FontFamilyProperty);
            set => this.SetValue(FontFamilyProperty, value);
        }

        public Brush Foreground
        {
            get => (Brush) this.GetValue(ForegroundProperty);
            set => this.SetValue(ForegroundProperty, value);
        }

        public Brush StepColor
        {
            get => (Brush) this.GetValue(StepColorProperty);
            set => this.SetValue(StepColorProperty, value);
        }

        private Pen MajorStepColourPen => this.majorLineStepColourPen ?? (this.StepColor is Brush brush ? this.majorLineStepColourPen = new Pen(brush, MajorLineThickness) : null);
        private Pen MinorStepColourPen => this.minorLineStepColourPen ?? (this.StepColor is Brush brush ? this.minorLineStepColourPen = new Pen(brush, 0.5) : null);

        private Typeface CachedTypeFace;
        private long timelineMaxDuration;
        private ScrollViewer scroller;
        private Pen majorLineStepColourPen;
        private Pen minorLineStepColourPen;

        public TimelineRuler()
        {
            this.Loaded += this.OnRulerLoaded;
            this.ClipToBounds = true;
        }

        private void OnTimelineChanged(Timeline oldTimeline, Timeline newTimeline)
        {
            if (oldTimeline != null)
                oldTimeline.MaxDurationChanged -= this.OnTimelineMaxDurationChanged;

            if (newTimeline != null)
            {
                newTimeline.MaxDurationChanged += this.OnTimelineMaxDurationChanged;
                this.OnTimelineMaxDurationChanged(newTimeline);
            }
        }

        private void OnTimelineMaxDurationChanged(Timeline timeline)
        {
            this.timelineMaxDuration = timeline.MaxDuration;
            this.InvalidateVisual();
        }

        private void OnRulerLoaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= this.OnRulerLoaded;

            // allows high performance rendering, so that we aren't rendering stuff that's offscreen
            this.scroller = VisualTreeUtils.GetParent<ScrollViewer>(this);
            if (this.scroller != null)
            {
                this.scroller.SizeChanged += this.OnScrollerOnSizeChanged;
                this.scroller.ScrollChanged += this.OnScrollerOnScrollChanged;
            }

            this.Dispatcher.InvokeAsync(this.InvalidateVisual, DispatcherPriority.Background);
        }

        private void OnScrollerOnSizeChanged(object o, SizeChangedEventArgs e) => this.InvalidateVisual();

        private void OnScrollerOnScrollChanged(object o, ScrollChangedEventArgs e)
        {
            if (e.HorizontalChange != 0 || e.VerticalChange != 0)
            {
                this.InvalidateVisual();
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            Size size = this.RenderSize;
            if (size.Width <= MinRender || size.Height <= MinRender || this.timelineMaxDuration < 1)
            {
                return;
            }

            // When zooming, OnRender gets called 3 times... this is why zooming is laggy
            // Calculate visible pixel bounds
            Rect rect = UIUtils.GetVisibleRect(this.scroller, this);
            if (rect.Width < MinRender && rect.Height < MinRender)
            {
                return;
            }

            if (this.Background is Brush bg)
            {
                dc.DrawRectangle(bg, null, rect);
            }

            const int SubStepNumber = 10;
            const int MinPixelSize = 4;
            double minPixel = MinPixelSize * SubStepNumber / size.Width;
            double minStep = minPixel * this.timelineMaxDuration;
            double minStepMagPow = Math.Pow(10, Math.Floor(Math.Log10(minStep)));
            double normMinStep = minStep / minStepMagPow;
            int finalStep = Steps.FirstOrDefault(step => step > normMinStep);
            if (finalStep < 1)
            {
                return;
            }

            double valueStep = finalStep * minStepMagPow;
            double pixelSize = size.Width * valueStep / this.timelineMaxDuration;

            int steps = Math.Min((int) Math.Floor(valueStep), SubStepNumber);
            double subpixelSize = pixelSize / steps;

            // calculate an initial offset instead of looping until we get into a visible region
            // Flooring may result in us drawing things partially offscreen to the left, which is kinda required
            int i = (int) Math.Floor(rect.Left / pixelSize);
            int j = (int) Math.Ceiling((rect.Right + pixelSize) / pixelSize);
            do
            {
                double pixel = i * pixelSize;
                if (i > j)
                {
                    break;
                }

                // TODO: optimise smaller/minor lines, maybe using skia?
                for (int y = 1; y < steps; ++y)
                {
                    double subpixel = pixel + y * subpixelSize;
                    this.DrawMinorLine(dc, subpixel, size.Height);
                }

                double text_value = i * valueStep;
                if (Math.Abs(text_value - (int) text_value) < 0.00001d)
                {
                    this.DrawMajorLine(dc, pixel, size.Height);
                    this.DrawText(dc, text_value, pixel);
                }

                i++;
            } while (true);
        }

        public void DrawMajorLine(DrawingContext dc, double offset, double height)
        {
            double size = Math.Min(height / 2d, height);
            dc.DrawLine(this.MajorStepColourPen, new Point(offset, height - size), new Point(offset, height));
        }

        public void DrawMinorLine(DrawingContext dc, double offset, double height)
        {
            double majorSize = height / 2d;
            double size = majorSize * (1 - MinorStepRatio);
            dc.DrawLine(this.MinorStepColourPen, new Point(offset, height - size), new Point(offset, height));
        }

        public void DrawText(DrawingContext dc, double value, double offset)
        {
            double height = this.ActualHeight;
            double majorSize = this.ActualHeight / 2d;

            Point point;
            FormattedText format = this.GetFormattedText(value);
            double gap = (height - majorSize);
            if (gap >= (format.Height / 2d))
            {
                point = new Point((offset + MajorLineThickness) - (format.Width / 2d), gap - format.Height);
            }
            else
            {
                // Draw above major if possible
                point = new Point(offset + MajorLineThickness + 2d, (height / 2d) - (format.Height / 2d));
            }

            dc.DrawText(format, point);
        }

        protected FormattedText GetFormattedText(double value)
        {
            Typeface typeface = this.CachedTypeFace;
            if (typeface == null)
            {
                FontFamily font = this.FontFamily ?? (this.FontFamily = new FontFamily("Consolas"));
                ICollection<Typeface> typefaces = font.GetTypefaces();
                this.CachedTypeFace = typeface = typefaces.FirstOrDefault() ?? FallbackTypeFace;
            }

            string text = value.ToString();
            return new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, 12, this.Foreground, 96);
        }
    }
}