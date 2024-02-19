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

using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace FramePFX.AttachedProperties {
    public static class TextBlockHelper {
        public static readonly DependencyProperty BindableInlinesProperty =
            DependencyProperty.RegisterAttached(
                "BindableInlines",
                typeof(IEnumerable<Inline>),
                typeof(TextBlockHelper),
                new PropertyMetadata(null, OnBindableInlinesChanged));

        public static IEnumerable<Inline> GetBindableInlines(DependencyObject o) {
            return (IEnumerable<Inline>) o.GetValue(BindableInlinesProperty);
        }

        public static void SetBindableInlines(DependencyObject o, IEnumerable<Inline> value) {
            o.SetValue(BindableInlinesProperty, value);
        }

        private static void OnBindableInlinesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is TextBlock target && e.NewValue is IEnumerable enumerable) {
                target.Inlines.Clear();
                target.Inlines.AddRange(enumerable);
            }
        }
    }
}