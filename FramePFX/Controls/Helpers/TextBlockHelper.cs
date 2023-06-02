using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace FramePFX.Controls.Helpers {
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