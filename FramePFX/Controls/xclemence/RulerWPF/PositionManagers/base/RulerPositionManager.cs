//  
// Copyright (c) Xavier CLEMENCE (xavier.clemence@gmail.com). All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information. 
// Ruler Wpf Version 3.0
// 

using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FramePFX.Controls.xclemence.RulerWPF.PositionManagers.@base {
    public enum RulerPosition {
        Top,
        Left
    }

    public abstract class RulerPositionManager {
        protected RulerPositionManager(RulerBase control) => this.Control = control;

        public RulerBase Control { get; private set; }

        public abstract void DrawMajorLine(DrawingContext dc, double offset);
        public abstract void DrawMinorLine(DrawingContext dc, double offset);
        public abstract void DrawText(DrawingContext dc, double value, double offset);

        public abstract Line CreateMajorLine(double offset);
        public abstract Line CreateMinorLine(double offset);
        public abstract TextBlock CreateText(double value, double offset);
        public abstract double GetSize();
        public abstract double GetHeight();

        public bool UpdateMakerPosition(Line marker, Point position) {
            if (marker == null)
                return false;

            return this.OnUpdateMakerPosition(marker, position);
        }


        public void UpdateFirstStepControl(Canvas control, double stepSize) {
            if (control == null)
                return;

            this.OnUpdateFirstStepControl(control, stepSize);
        }

        public void UpdateStepRepeaterControl(Rectangle control, VisualBrush brush, double stepSize) {
            if (control == null)
                return;
            if (brush == null)
                return;

            this.OnUpdateStepRepeaterControl(control, brush, stepSize);
        }

        protected abstract bool OnUpdateMakerPosition(Line marker, Point position);
        protected abstract void OnUpdateStepRepeaterControl(Rectangle control, VisualBrush brush, double stepSize);
        protected abstract void OnUpdateFirstStepControl(Canvas control, double stepSize);

        protected virtual Line GetBaseLine() => new Line {
            Stroke = this.Control.StepColor,
            StrokeThickness = 1,
            Stretch = Stretch.None,
        };

        protected virtual TextBlock GetTextBlock(string text) => new TextBlock {Text = text};

        protected virtual CultureInfo GetTextCulture() => this.Control.TextCulture ?? CultureInfo.CurrentUICulture;
    }
}