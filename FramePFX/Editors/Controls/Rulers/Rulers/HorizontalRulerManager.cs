//  
// Copyright (c) Xavier CLEMENCE (xavier.clemence@gmail.com) and REghZy/AngryCarrot789. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information. 
// Ruler Wpf Version 3.1
// 

namespace FramePFX.Editors.Controls.Rulers.Rulers {
    public abstract class HorizontalRulerManager : RulerPositionManager {
        protected HorizontalRulerManager(RulerBase control) : base(control) { }

        public override double GetSize() => this.Control.ActualWidth;

        public override double GetHeight() => this.Control.ActualHeight;

        public override double GetMajorSize() => this.Control.MajorLineSize ?? (this.Control.ActualHeight / 2d);
    }
}