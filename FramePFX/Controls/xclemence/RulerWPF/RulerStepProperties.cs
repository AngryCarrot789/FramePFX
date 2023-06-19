//  
// Copyright (c) Xavier CLEMENCE (xavier.clemence@gmail.com). All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information. 
// Ruler Wpf Version 3.0
// 

namespace FramePFX.Controls.xclemence.RulerWPF {
    public class RulerStepProperties {
        public double PixelSize { get; set; }
        public double Value { get; set; }

        public void Deconstruct(out double pixelSize, out double value) {
            pixelSize = this.PixelSize;
            value = this.Value;
        }
    }
}