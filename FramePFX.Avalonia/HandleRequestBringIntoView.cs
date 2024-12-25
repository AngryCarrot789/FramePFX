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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Reactive;

namespace FramePFX.Avalonia;

public static class HandleRequestBringIntoView {
    public static readonly AttachedProperty<bool> IsEnabledProperty = AvaloniaProperty.RegisterAttached<Control, bool>("IsEnabled", typeof(HandleRequestBringIntoView));

    public static void SetIsEnabled(Control obj, bool value) => obj.SetValue(IsEnabledProperty, value);

    public static bool GetIsEnabled(Control obj) => obj.GetValue(IsEnabledProperty);

    static HandleRequestBringIntoView() {
        Control.RequestBringIntoViewEvent.Raised.Subscribe(new AnonymousObserver<(object, RoutedEventArgs)>(GridOnRequestBringIntoView));
    }

    private static void GridOnRequestBringIntoView((object ctrl, RoutedEventArgs eventArgs) tuple) {
        // Prevent the timeline scrolling when you select a cli
        if (tuple.ctrl is Control control && GetIsEnabled(control)) {
            tuple.eventArgs.Handled = true;
        }
    }
}