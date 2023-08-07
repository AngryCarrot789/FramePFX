//  
// Copyright (c) Xavier CLEMENCE (xavier.clemence@gmail.com) and REghZy/AngryCarrot789. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information. 
// Ruler Wpf Version 3.1
// 

using System.Windows;
using System.Windows.Shapes;

namespace FramePFX.Controls.xclemence.RulerWPF.PositionManagers
{
    public abstract class VerticalRulerManager : RulerPositionManager
    {
        protected VerticalRulerManager(RulerBase control) : base(control) { }

        public override double GetSize() => this.Control.ActualHeight;
        public override double GetHeight() => this.Control.ActualWidth;
        public override double GetMajorSize() => this.Control.MajorLineSize ?? (this.Control.ActualWidth / 2d);

        public override bool OnUpdateMakerPosition(Line marker, Point position)
        {
            if (position.Y <= 0 || position.Y >= this.GetSize())
            {
                return false;
            }

            marker.X1 = 0;
            marker.Y1 = position.Y;

            marker.X2 = this.GetHeight();
            marker.Y2 = position.Y;
            return true;
        }
    }
}