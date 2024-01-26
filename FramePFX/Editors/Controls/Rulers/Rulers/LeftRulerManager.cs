﻿//  
// Copyright (c) Xavier CLEMENCE (xavier.clemence@gmail.com) and REghZy/AngryCarrot789. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information. 
// Ruler Wpf Version 3.1
// 

using System.Windows;
using System.Windows.Media;

namespace FramePFX.Editors.Controls.Rulers.Rulers {
    public class LeftRulerManager : VerticalRulerManager {
        public LeftRulerManager(RulerBase control) : base(control) { }

        public override void DrawMajorLine(DrawingContext dc, double offset) {
            dc.DrawLine(this.Control.MajorStepColourPen, new Point(0, offset), new Point(this.GetMajorSize(), offset));
        }

        public override void DrawMinorLine(DrawingContext dc, double offset) {
            double size = this.GetMajorSize();
            double pos_x = size * (1 - this.Control.MinorStepRatio);
            dc.DrawLine(this.Control.MinorStepColourPen, new Point(pos_x, offset), new Point(size, offset));
        }

        public override void DrawText(DrawingContext dc, double value, double offset) {
            FormattedText format = base.GetFormattedText(value);
            dc.DrawText(format, new Point(0, offset));
        }
    }
}