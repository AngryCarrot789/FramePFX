//  
// Copyright (c) Xavier CLEMENCE (xavier.clemence@gmail.com). All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information. 
// Ruler Wpf Version 3.0
// 

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FramePFX.Controls.xclemence.RulerWPF.PositionManagers.@base {
    public abstract class HorizontalRulerManager : RulerPositionManager {
        protected HorizontalRulerManager(RulerBase control) : base(control) { }

        public override double GetSize() => this.Control.ActualWidth;
        public override double GetHeight() => this.Control.ActualHeight;

        protected override void OnUpdateFirstStepControl(Canvas control, double stepSize) {
            control.HorizontalAlignment = HorizontalAlignment.Left;
            control.Width = stepSize;
        }

        protected override void OnUpdateStepRepeaterControl(Rectangle control, VisualBrush brush, double stepSize) {
            brush.Viewport = new Rect(0, 0, stepSize, this.GetHeight());
            control.Margin = new Thickness(stepSize, 0, 0, 0);
        }

        protected override bool OnUpdateMakerPosition(Line marker, Point position) {
            if (position.X <= 0 || position.X >= this.GetSize())
                return false;

            marker.X1 = position.X;
            marker.Y1 = 0;

            marker.X2 = position.X;
            marker.Y2 = this.GetHeight();

            return true;
        }
    }
}