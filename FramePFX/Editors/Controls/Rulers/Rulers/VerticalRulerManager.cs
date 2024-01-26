//  
// Copyright (c) Xavier CLEMENCE (xavier.clemence@gmail.com) and REghZy/AngryCarrot789. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information. 
// Ruler Wpf Version 3.1
// 

namespace FramePFX.Editors.Controls.Rulers.Rulers {
    public abstract class VerticalRulerManager : RulerPositionManager {
        protected VerticalRulerManager(RulerBase control) : base(control) { }

        public override double GetSize() => this.Control.ActualHeight;

        public override double GetHeight() => this.Control.ActualWidth;

        public override double GetMajorSize() => this.Control.MajorLineSize ?? (this.Control.ActualWidth / 2d);
    }
}