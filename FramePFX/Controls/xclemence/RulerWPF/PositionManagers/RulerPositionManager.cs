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
using System.Windows.Shapes;

namespace FramePFX.Controls.xclemence.RulerWPF.PositionManagers
{
    public abstract class RulerPositionManager
    {
        public static readonly Typeface FallbackTypeFace = new Typeface("Consolas");

        public RulerBase Control { get; }

        protected RulerPositionManager(RulerBase control)
        {
            this.Control = control;
        }

        #region Rendering Functions

        public abstract void DrawMajorLine(DrawingContext dc, double offset);

        public abstract void DrawMinorLine(DrawingContext dc, double offset);

        public abstract void DrawText(DrawingContext dc, double value, double offset);

        protected FormattedText GetFormattedText(double value)
        {
            CultureInfo culture = this.Control.TextCulture ?? CultureInfo.CurrentUICulture;
            string text = value.ToString(this.Control.TextFormat, culture);
            ICollection<Typeface> typefaces = this.Control.FontFamily.GetTypefaces();
            return new FormattedText(text, culture, FlowDirection.LeftToRight, typefaces.FirstOrDefault() ?? FallbackTypeFace, 12, this.Control.Foreground, 96);
        }

        #endregion

        #region Size Functions

        public abstract double GetSize();

        public abstract double GetHeight();

        public abstract double GetMajorSize();

        #endregion

        public abstract bool OnUpdateMakerPosition(Line marker, Point position);
    }
}