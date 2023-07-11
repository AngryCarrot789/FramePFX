using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using FramePFX.Core.Utils;

namespace FramePFX.Controls {
    public class CancellableThumb : Thumb {
        public static readonly DependencyProperty CanCancelProperty = DependencyProperty.RegisterAttached("CanCancel", typeof(bool), typeof(CancellableThumb), new PropertyMetadata(BoolBox.False, OnCanCancelPropertyChanged));

        public static void SetCanCancel(DependencyObject element, bool value) => element.SetValue(CanCancelProperty, value);

        public static bool GetCanCancel(DependencyObject element) => (bool) element.GetValue(CanCancelProperty);

        private static void OnCanCancelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (!(d is Thumb thumb)) {
                throw new Exception("Property must be applied to a thumb");
            }

            thumb.KeyDown -= ThumbOnKeyDown;
            if ((bool) e.NewValue) {
                thumb.KeyDown += ThumbOnKeyDown;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);
            if (!e.Handled && e.Key == Key.Escape) {
                e.Handled = true;
                this.CancelDrag();
            }
        }

        private static void ThumbOnKeyDown(object sender, KeyEventArgs e) {
            if (!e.Handled && e.Key == Key.Escape) {
                e.Handled = true;
                ((Thumb) sender).CancelDrag();
            }
        }
    }
}