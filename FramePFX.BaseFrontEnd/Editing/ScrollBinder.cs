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
using Avalonia.Data;
using Avalonia.Reactive;

namespace FramePFX.BaseFrontEnd.Editing;

public static class ScrollBinder {
    public static readonly AttachedProperty<string?> VerticalBindGroupProperty = AvaloniaProperty.RegisterAttached<ScrollViewer, string?>("VerticalBindGroup", typeof(ScrollBinder), defaultBindingMode: BindingMode.TwoWay);
    public static readonly AttachedProperty<string?> HorizontalBindGroupProperty = AvaloniaProperty.RegisterAttached<ScrollViewer, string?>("HorizontalBindGroup", typeof(ScrollBinder), defaultBindingMode: BindingMode.TwoWay);

    public static void SetVerticalBindGroup(ScrollViewer obj, string? value) => obj.SetValue(VerticalBindGroupProperty, value);
    public static string? GetVerticalBindGroup(ScrollViewer obj) => obj.GetValue(VerticalBindGroupProperty);

    public static void SetHorizontalBindGroup(ScrollViewer obj, string? value) => obj.SetValue(HorizontalBindGroupProperty, value);
    public static string? GetHorizontalBindGroup(ScrollViewer obj) => obj.GetValue(HorizontalBindGroupProperty);

    private static bool IsUpdatingScroll;
    private static readonly Dictionary<string, List<ScrollViewer>> RegisteredScrollers = new Dictionary<string, List<ScrollViewer>>();

    static ScrollBinder() {
        VerticalBindGroupProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<string?>>(OnVerticalBindGroupChanged));
        HorizontalBindGroupProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<string?>>(OnHorizontalBindGroupChanged));
    }

    private static void OnVerticalBindGroupChanged(AvaloniaPropertyChangedEventArgs<string?> e) {
        if (e.Sender is ScrollViewer scroller) {
            if (e.OldValue.GetValueOrDefault() is string oldGroup) {
                if (RegisteredScrollers.TryGetValue(oldGroup, out List<ScrollViewer>? list)) {
                    list.Remove(scroller);
                }
            }

            scroller.ScrollChanged -= OnVerticalScrollChanged;
            if (e.NewValue.GetValueOrDefault() is string newGroup) {
                scroller.ScrollChanged += OnVerticalScrollChanged;
                if (!RegisteredScrollers.TryGetValue(newGroup, out List<ScrollViewer>? list)) {
                    RegisteredScrollers[newGroup] = list = new List<ScrollViewer>();
                }

                list.Add(scroller);
            }
        }
    }

    private static void OnHorizontalBindGroupChanged(AvaloniaPropertyChangedEventArgs<string?> e) {
        if (e.Sender is ScrollViewer scroller) {
            if (e.OldValue.GetValueOrDefault() is string oldGroup) {
                if (RegisteredScrollers.TryGetValue(oldGroup, out List<ScrollViewer>? list)) {
                    list.Remove(scroller);
                }
            }

            scroller.ScrollChanged -= OnHorizontalScrollChanged;
            if (e.NewValue.GetValueOrDefault() is string newGroup) {
                scroller.ScrollChanged += OnHorizontalScrollChanged;
                if (!RegisteredScrollers.TryGetValue(newGroup, out List<ScrollViewer>? list)) {
                    RegisteredScrollers[newGroup] = list = new List<ScrollViewer>();
                }

                list.Add(scroller);
            }
        }
    }

    private static void OnVerticalScrollChanged(object? sender, ScrollChangedEventArgs e) {
        if (IsUpdatingScroll) {
            return;
        }

        ScrollViewer viewer = (ScrollViewer) sender!;
        string? group = GetVerticalBindGroup(viewer);
        if (group == null) {
            return;
        }

        if (RegisteredScrollers.TryGetValue(group, out List<ScrollViewer>? list)) {
            IsUpdatingScroll = true;
            try {
                foreach (ScrollViewer scrollViewer in list) {
                    if (scrollViewer != viewer) {
                        scrollViewer.Offset = new Vector(scrollViewer.Offset.X, scrollViewer.Offset.Y + e.OffsetDelta.Y);
                        scrollViewer.UpdateLayout();
                    }
                }
            }
            finally {
                IsUpdatingScroll = false;
            }
        }
    }

    private static void OnHorizontalScrollChanged(object? sender, ScrollChangedEventArgs e) {
        if (IsUpdatingScroll) {
            return;
        }

        ScrollViewer viewer = (ScrollViewer) sender!;
        string? group = GetHorizontalBindGroup(viewer);
        if (group == null) {
            return;
        }

        if (RegisteredScrollers.TryGetValue(group, out List<ScrollViewer>? list)) {
            IsUpdatingScroll = true;
            try {
                foreach (ScrollViewer scrollViewer in list) {
                    if (scrollViewer != viewer) {
                        scrollViewer.Offset = new Vector(scrollViewer.Offset.X + e.OffsetDelta.X, scrollViewer.Offset.Y);
                    }
                }
            }
            finally {
                IsUpdatingScroll = false;
            }
        }
    }
}