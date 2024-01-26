//  
// Copyright (c) Xavier CLEMENCE (xavier.clemence@gmail.com) and REghZy/AngryCarrot789. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information. 
// Ruler Wpf Version 3.1
// 

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace FramePFX.Editors.Controls.Rulers.Rulers {
    public abstract class RulerPositionManager {
        public static readonly Typeface FallbackTypeFace = new Typeface("Consolas");

        public RulerBase Control { get; }

        protected RulerPositionManager(RulerBase control) {
            this.Control = control;
        }

        #region Rendering Functions

        public abstract void DrawMajorLine(DrawingContext dc, double offset);

        public abstract void DrawMinorLine(DrawingContext dc, double offset);

        public abstract void DrawText(DrawingContext dc, double value, double offset);

        protected FormattedText GetFormattedText(double value) {
            Typeface typeface = this.Control.CachedTypeFace;
            if (typeface == null) {
                FontFamily font = this.Control.FontFamily ?? (this.Control.FontFamily = new FontFamily("Consolas"));
                ICollection<Typeface> typefaces = font.GetTypefaces();
                this.Control.CachedTypeFace = typeface = typefaces.FirstOrDefault() ?? FallbackTypeFace;
            }

            string text = value.ToString(this.Control.TextFormat, CultureInfo.CurrentUICulture);
            return new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, 12, this.Control.Foreground, 96);
        }

        protected GlyphRun GetTextRun(double value, Point origin) {
            string text = value.ToString(this.Control.TextFormat, CultureInfo.CurrentUICulture);
            FontFamily font = this.Control.FontFamily ?? (this.Control.FontFamily = new FontFamily("Consolas"));
            ICollection<Typeface> typefaces = font.GetTypefaces();
            return GlyphGenerator.CreateText(text, 12, typefaces.FirstOrDefault() ?? FallbackTypeFace, origin);
        }

        #endregion

        #region Size Functions

        public abstract double GetSize();

        public abstract double GetHeight();

        public abstract double GetMajorSize();

        #endregion
    }
}