using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FramePFX.Editors.Controls {
    public class GlyphGenerator {
        public static GlyphRun CreateText(string text, double emSize, Control control) {
            return CreateText(text, emSize, control, new Point(0, emSize));
        }

        public static GlyphRun CreateText(string text, double emSize, Control control, Point origin) {
            Typeface typeface = new Typeface(control.FontFamily, control.FontStyle, control.FontWeight, control.FontStretch);
            return CreateText(text, emSize, typeface, origin);
        }

        public static GlyphRun CreateText(string text, double emSize, Typeface typeface, Point origin) {
            if (!typeface.TryGetGlyphTypeface(out GlyphTypeface gtf))
                throw new InvalidOperationException("No glyph typeface found");
            ushort[] indices = new ushort[text.Length];
            double[] advWidths = new double[text.Length];
            for (int i = 0; i < text.Length; i++) {
                ushort index = gtf.CharacterToGlyphMap[text[i]];
                indices[i] = index;
                advWidths[i] = gtf.AdvanceWidths[index] * emSize;
            }

            return new GlyphRun(gtf, 0, false, emSize, indices, origin, advWidths, null, null, null, null, null, null);
        }
    }
}