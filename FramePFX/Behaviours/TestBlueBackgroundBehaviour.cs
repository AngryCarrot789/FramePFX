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
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FramePFX.Behaviours {
    public class TestRedBackgroundBehaviour1 : Behaviour<Control> {
        protected override void OnAttached() {
            this.AttachedElement.Background = Brushes.DarkRed;
        }

        protected override void OnDetatched() {
            this.AttachedElement.ClearValue(Control.BackgroundProperty);
        }
    }

    public class TestBlueBackgroundBehaviour2 : Behaviour<Control> {
        protected override void OnAttached() {
            this.AttachedElement.BorderBrush = Brushes.Red;
        }

        protected override void OnDetatched() {
            this.AttachedElement.ClearValue(Control.BorderBrushProperty);
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent) {
            base.OnVisualParentChanged(oldParent);
        }
    }
}