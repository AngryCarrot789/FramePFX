// 
// Copyright (c) 2023-2024 REghZy
// 
// This file is part of FramePFX.
// 
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

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
                if (!gtf.CharacterToGlyphMap.TryGetValue(text[i], out ushort index)) {
                    if (!gtf.CharacterToGlyphMap.TryGetValue('?', out index)) {
                        index = 0;
                    }
                }

                indices[i] = index;
                advWidths[i] = gtf.AdvanceWidths[index] * emSize;
            }

            return new GlyphRun(gtf, 0, false, emSize, indices, origin, advWidths, null, null, null, null, null, null);
        }
    }
}