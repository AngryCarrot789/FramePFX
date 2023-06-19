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
    public abstract class VerticalRulerManager : RulerPositionManager {
        protected VerticalRulerManager(RulerBase control) : base(control) { }

        public override double GetSize() => this.Control.ActualHeight;
        public override double GetHeight() => this.Control.ActualWidth;

        protected override void OnUpdateFirstStepControl(Canvas control, double stepSize) {
            control.VerticalAlignment = VerticalAlignment.Top;
            control.Height = stepSize;
        }

        protected override void OnUpdateStepRepeaterControl(Rectangle control, VisualBrush brush, double stepSize) {
            brush.Viewport = new Rect(0, 0, this.GetHeight(), stepSize);
            control.Margin = new Thickness(0, stepSize, 0, 0);
        }

        protected override bool OnUpdateMakerPosition(Line marker, Point position) {
            if (position.Y <= 0 || position.Y >= this.GetSize())
                return false;

            marker.X1 = 0;
            marker.Y1 = position.Y;

            marker.X2 = this.GetHeight();
            marker.Y2 = position.Y;

            return true;
        }
    }
}