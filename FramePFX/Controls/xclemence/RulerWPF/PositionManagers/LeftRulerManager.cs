//  
// Copyright (c) Xavier CLEMENCE (xavier.clemence@gmail.com). All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information. 
// Ruler Wpf Version 3.0
// 

using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using FramePFX.Controls.xclemence.RulerWPF.PositionManagers.@base;

namespace FramePFX.Controls.xclemence.RulerWPF.PositionManagers {
    public class LeftRulerManager : VerticalRulerManager {
        public LeftRulerManager(RulerBase control) : base(control) { }

        public override void DrawMajorLine(DrawingContext dc, double offset) {
        }

        public override void DrawMinorLine(DrawingContext dc, double offset) {

        }

        public override void DrawText(DrawingContext dc, double value, double offset) {

        }

        public override Line CreateMajorLine(double offset) {
            Line line = this.GetBaseLine();

            line.X1 = 0;
            line.Y1 = offset;

            line.X2 = this.GetHeight();
            line.Y2 = offset;

            return line;
        }

        public override Line CreateMinorLine(double offset) {
            double height = this.GetHeight();
            Line line = this.GetBaseLine();

            line.X1 = height * (1 - this.Control.MinorStepRatio);
            line.Y1 = offset;

            line.X2 = height;
            line.Y2 = offset;

            return line;
        }

        public override TextBlock CreateText(double value, double offset) {
            string text = value.ToString(this.Control.TextFormat, this.GetTextCulture()).Select(x => x.ToString(CultureInfo.InvariantCulture)).Where(x => !string.IsNullOrWhiteSpace(x)).Aggregate((x, y) => $"{x}{Environment.NewLine}{y}");


            TextBlock textBlock = this.GetTextBlock(text);

            textBlock.SetValue(Canvas.TopProperty, offset);

            return textBlock;
        }
    }
}