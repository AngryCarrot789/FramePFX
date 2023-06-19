//  
// Copyright (c) Xavier CLEMENCE (xavier.clemence@gmail.com) and REghZy/AngryCarrot789. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information. 
// Ruler Wpf Version 3.0
// 

using System.Windows;
using System.Windows.Media;

namespace FramePFX.Controls.xclemence.RulerWPF.PositionManagers {
    public class TopRulerManager : HorizontalRulerManager {
        public TopRulerManager(RulerBase control) : base(control) { }

        public override void DrawMajorLine(DrawingContext dc, double offset) {
            dc.DrawLine(this.Control.StepColorPen, new Point(offset, 0), new Point(offset, this.GetMajorSize()));
        }

        public override void DrawMinorLine(DrawingContext dc, double offset) {
            double size = this.GetMajorSize();
            double pos_y = size * (1 - this.Control.MinorStepRatio);
            dc.DrawLine(this.Control.StepColorPen, new Point(offset, pos_y), new Point(offset, size));
        }

        public override void DrawText(DrawingContext dc, double value, double offset) {
            FormattedText format = base.GetFormattedText(value);
            dc.DrawText(format, new Point(offset, 0));
        }
    }
}