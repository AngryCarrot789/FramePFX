//  
// Copyright (c) Xavier CLEMENCE (xavier.clemence@gmail.com) and REghZy/AngryCarrot789. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information. 
// Ruler Wpf Version 3.1
// 

namespace FramePFX.Editors.Controls.Rulers {
    public class RulerStepProperties {
        public double PixelSize { get; set; }
        public double Value { get; set; }

        public void Deconstruct(out double pixelSize, out double value) {
            pixelSize = this.PixelSize;
            value = this.Value;
        }
    }
}