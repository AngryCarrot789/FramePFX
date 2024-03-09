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
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using FramePFX.Utils;

namespace FramePFX.Editors.Controls
{
    public class CancellableThumb : Thumb
    {
        public static readonly DependencyProperty CanCancelProperty = DependencyProperty.RegisterAttached("CanCancel", typeof(bool), typeof(CancellableThumb), new PropertyMetadata(BoolBox.False, OnCanCancelPropertyChanged));

        public static void SetCanCancel(DependencyObject element, bool value) => element.SetValue(CanCancelProperty, value.Box());

        public static bool GetCanCancel(DependencyObject element) => (bool) element.GetValue(CanCancelProperty);

        private static void OnCanCancelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is Thumb thumb))
            {
                throw new Exception("Property must be applied to a thumb");
            }

            thumb.KeyDown -= ThumbOnKeyDown;
            if ((bool) e.NewValue)
            {
                thumb.KeyDown += ThumbOnKeyDown;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (!e.Handled && e.Key == Key.Escape)
            {
                e.Handled = true;
                this.CancelDrag();
            }
        }

        private static void ThumbOnKeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Handled && e.Key == Key.Escape)
            {
                e.Handled = true;
                ((Thumb) sender).CancelDrag();
            }
        }

        public CancellableThumb()
        {
            SetCanCancel(this, true);
        }
    }
}