//  
// Copyright (c) Xavier CLEMENCE (xavier.clemence@gmail.com) and REghZy/AngryCarrot789. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information. 
// Ruler Wpf Version 3.1
// 

using System.Windows;
using System.Windows.Shapes;

namespace FramePFX.WPF.Controls.xclemence.RulerWPF.PositionManagers
{
    public abstract class HorizontalRulerManager : RulerPositionManager
    {
        protected HorizontalRulerManager(RulerBase control) : base(control) { }

        public override double GetSize() => this.Control.ActualWidth;
        public override double GetHeight() => this.Control.ActualHeight;
        public override double GetMajorSize() => this.Control.MajorLineSize ?? (this.Control.ActualHeight / 2d);

        public override bool OnUpdateMakerPosition(Line marker, Point position)
        {
            if (position.X <= 0 || position.X >= this.GetSize())
            {
                return false;
            }

            marker.X1 = position.X;
            marker.Y1 = 0;

            marker.X2 = position.X;
            marker.Y2 = this.GetHeight();

            return true;
        }
    }
}