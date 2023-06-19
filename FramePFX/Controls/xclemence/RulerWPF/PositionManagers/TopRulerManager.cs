//  
// Copyright (c) Xavier CLEMENCE (xavier.clemence@gmail.com). All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information. 
// Ruler Wpf Version 3.0
// 

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using FramePFX.Controls.xclemence.RulerWPF.PositionManagers.@base;

namespace FramePFX.Controls.xclemence.RulerWPF.PositionManagers {
    public class TopRulerManager : HorizontalRulerManager {
        public TopRulerManager(RulerBase control) : base(control) { }

        public override void DrawMajorLine(DrawingContext dc, double offset) {
            dc.DrawLine(this.Control.StepColorPen, new Point(offset, 0), new Point(offset, this.GetHeight()));
        }

        public override void DrawMinorLine(DrawingContext dc, double offset) {
            dc.DrawLine(this.Control.StepColorPen,
                new Point(offset, this.GetHeight() * (1 - this.Control.MinorStepRatio)),
                new Point(offset, this.GetHeight()));
        }

        public override void DrawText(DrawingContext dc, double value, double offset) {
            string text = value.ToString(this.Control.TextFormat, this.GetTextCulture());
            FormattedText formattedText = new FormattedText(text, this.GetTextCulture(), FlowDirection.LeftToRight, new Typeface("Consolas"), 12, Brushes.Beige, 96);
            dc.DrawText(formattedText, new Point(offset, 0));
        }

        public override Line CreateMajorLine(double offset) {
            Line line = this.GetBaseLine();

            line.X1 = offset;
            line.Y1 = 0;

            line.X2 = offset;
            line.Y2 = this.GetHeight();

            return line;
        }

        public override Line CreateMinorLine(double offset) {
            Line line = this.GetBaseLine();

            line.X1 = offset;
            line.Y1 = this.GetHeight() * (1 - this.Control.MinorStepRatio);

            line.X2 = offset;
            line.Y2 = this.GetHeight();

            return line;
        }

        public override TextBlock CreateText(double value, double offset) {
            TextBlock text = this.GetTextBlock(value.ToString(this.Control.TextFormat, this.GetTextCulture()));

            text.SetValue(Canvas.LeftProperty, offset);

            return text;
        }
    }
}