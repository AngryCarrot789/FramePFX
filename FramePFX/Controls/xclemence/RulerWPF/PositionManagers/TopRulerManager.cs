//  
// Copyright (c) Xavier CLEMENCE (xavier.clemence@gmail.com) and REghZy/AngryCarrot789. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information. 
// Ruler Wpf Version 3.1
// 

using System;
using System.Windows;
using System.Windows.Media;

namespace FramePFX.Controls.xclemence.RulerWPF.PositionManagers {
    public class TopRulerManager : HorizontalRulerManager {
        public TopRulerManager(RulerBase control) : base(control) { }

        public override void DrawMajorLine(DrawingContext dc, double offset) {
            double height = this.Control.ActualHeight;
            double size = Math.Min(this.GetMajorSize(), height);
            Point p1;
            Point p2;
            switch (this.Control.TopRulerLineAlignment) {
                case VerticalAlignment.Top:
                    p1 = new Point(offset, 0);
                    p2 = new Point(offset, size);
                    break;
                case VerticalAlignment.Center:
                    double d = (height - size) / 2d;
                    p1 = new Point(offset, d);
                    p2 = new Point(offset, height - d);
                    break;
                case VerticalAlignment.Bottom:
                    p1 = new Point(offset, height - size);
                    p2 = new Point(offset, height);
                    break;
                case VerticalAlignment.Stretch:
                    p1 = new Point(offset, 0);
                    p2 = new Point(offset, size);
                    break;
                default: throw new ArgumentOutOfRangeException();
            }

            dc.DrawLine(this.Control.MajorStepColourPen, p1, p2);
        }

        public override void DrawMinorLine(DrawingContext dc, double offset) {
            double height = this.Control.ActualHeight;
            double major_size = this.GetMajorSize();
            double size = major_size * (1 - this.Control.MinorStepRatio);

            Point p1;
            Point p2;
            switch (this.Control.TopRulerLineAlignment) {
                case VerticalAlignment.Top:
                    p1 = new Point(offset, 0);
                    p2 = new Point(offset, size);
                    break;
                case VerticalAlignment.Center:
                    double d = (height - size) / 2d;
                    p1 = new Point(offset, d);
                    p2 = new Point(offset, height - d);
                    break;
                case VerticalAlignment.Bottom:
                    p1 = new Point(offset, height - size);
                    p2 = new Point(offset, height);
                    break;
                case VerticalAlignment.Stretch:
                    p1 = new Point(offset, 0);
                    p2 = new Point(offset, size);
                    break;
                default: throw new ArgumentOutOfRangeException();
            }

            dc.DrawLine(this.Control.MinorStepColourPen, p1, p2);
        }

        public override void DrawText(DrawingContext dc, double value, double offset) {
            double height = this.Control.ActualHeight;
            double major_size = this.GetMajorSize();

            FormattedText format = base.GetFormattedText(value);

            double gap = (height - major_size);
            if (gap >= (format.Height / 2d)) {
                dc.DrawText(format, new Point((offset + this.Control.MajorLineThickness) - (format.Width / 2d), gap - format.Height));
            }
            else {
                // draw above major if possible
                dc.DrawText(format, new Point(offset + this.Control.MajorLineThickness + 2d, (height / 2d) - (format.Height / 2d)));
            }

            // draw to right side of major but in center of height
            // dc.DrawText(format, new Point(offset + this.Control.MajorLineThickness + 2d, (this.Control.ActualHeight / 2d) - (format.Height / 2d)));

            /* Draw to right side of major
                double minor_size = this.GetMajorSize() * (1 - this.Control.MinorStepRatio);
                dc.DrawText(format, new Point(offset + this.Control.MajorLineThickness + 2d, this.Control.ActualHeight - minor_size - format.Height));
             */
        }
    }
}