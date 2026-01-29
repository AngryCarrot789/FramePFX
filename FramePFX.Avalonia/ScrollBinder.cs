// 
// Copyright (c) 2026-2026 REghZy
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
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace FramePFX.Avalonia;

public static class ScrollBinder {
    private static readonly EventHandler<VisualTreeAttachmentEventArgs> s_SenderOnAttachedToVisualTree = SenderOnAttachedToVisualTree;
    private static readonly EventHandler<VisualTreeAttachmentEventArgs> s_ScrollerOnDetachedFromVisualTree = ScrollerOnDetachedFromVisualTree;

    public static readonly AttachedProperty<string?> VerticalBindGroupProperty =
        AvaloniaProperty.RegisterAttached<ScrollViewer, string?>("VerticalBindGroup", typeof(ScrollBinder));

    public static readonly AttachedProperty<string?> HorizontalBindGroupProperty =
        AvaloniaProperty.RegisterAttached<ScrollViewer, string?>("HorizontalBindGroup", typeof(ScrollBinder));

    public static void SetVerticalBindGroup(ScrollViewer obj, string? value) => obj.SetValue(VerticalBindGroupProperty, value);
    public static string? GetVerticalBindGroup(ScrollViewer obj) => obj.GetValue(VerticalBindGroupProperty);

    public static void SetHorizontalBindGroup(ScrollViewer obj, string? value) => obj.SetValue(HorizontalBindGroupProperty, value);
    public static string? GetHorizontalBindGroup(ScrollViewer obj) => obj.GetValue(HorizontalBindGroupProperty);

    private static readonly Dictionary<string, GroupInfo> LiveVerticalBinders = new Dictionary<string, GroupInfo>();
    private static readonly Dictionary<string, GroupInfo> LiveHorizontalBinders = new Dictionary<string, GroupInfo>();

    private sealed class GroupInfo {
        public readonly string GroupName;
        public readonly List<ScrollViewer> ScrollViewers;
        public bool IsUpdating;

        public GroupInfo(string groupName) {
            this.GroupName = groupName;
            this.ScrollViewers = new List<ScrollViewer>();
        }
    }

    static ScrollBinder() {
        VerticalBindGroupProperty.Changed.AddClassHandler<ScrollViewer, string?>((s, e) => OnVerticalBindGroupChanged(s, e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    private static void OnVerticalBindGroupChanged(ScrollViewer sender, string? oldTarget, string? newTarget) {
        if (!string.IsNullOrWhiteSpace(oldTarget)) {
            OnDetachedFromVisualTree(sender);
            sender.AttachedToVisualTree -= s_SenderOnAttachedToVisualTree;
            sender.DetachedFromVisualTree -= s_ScrollerOnDetachedFromVisualTree;
        }

        if (!string.IsNullOrWhiteSpace(newTarget)) {
            if (sender.IsAttachedToVisualTree()) {
                OnAttachedToVisualTree(sender);
            }
            else {
                sender.AttachedToVisualTree += s_SenderOnAttachedToVisualTree;
            }
        }
    }

    private static void SenderOnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e) {
        ScrollViewer scroller = (ScrollViewer) sender!;
        scroller.AttachedToVisualTree -= s_SenderOnAttachedToVisualTree;
        scroller.DetachedFromVisualTree += s_ScrollerOnDetachedFromVisualTree;
        OnAttachedToVisualTree(scroller);
    }

    private static void ScrollerOnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e) {
        ScrollViewer scroller = (ScrollViewer) sender!;
        scroller.DetachedFromVisualTree -= s_ScrollerOnDetachedFromVisualTree;
        scroller.AttachedToVisualTree += s_SenderOnAttachedToVisualTree;
        OnDetachedFromVisualTree(scroller);
    }

    private static void OnDetachedFromVisualTree(ScrollViewer scroller) {
        string? targetV = GetVerticalBindGroup(scroller);
        if (!string.IsNullOrWhiteSpace(targetV) && LiveVerticalBinders.TryGetValue(targetV, out GroupInfo? bindersV)) {
            bindersV.ScrollViewers.Remove(scroller);
            scroller.ScrollChanged -= OnScrollChanged_Vertical;
        }

        string? targetH = GetHorizontalBindGroup(scroller);
        if (!string.IsNullOrWhiteSpace(targetH) && LiveHorizontalBinders.TryGetValue(targetH, out GroupInfo? bindersH)) {
            bindersH.ScrollViewers.Remove(scroller);
            scroller.ScrollChanged -= OnScrollChanged_Horizontal;
        }
    }

    private static void OnAttachedToVisualTree(ScrollViewer scroller) {
        string? targetV = GetVerticalBindGroup(scroller);
        if (!string.IsNullOrWhiteSpace(targetV)) {
            if (!LiveVerticalBinders.TryGetValue(targetV, out GroupInfo? bindersV)) {
                LiveVerticalBinders[targetV] = bindersV = new GroupInfo(targetV);
            }

            bindersV.ScrollViewers.Add(scroller);
            scroller.ScrollChanged += OnScrollChanged_Vertical;
        }

        string? targetH = GetVerticalBindGroup(scroller);
        if (!string.IsNullOrWhiteSpace(targetH)) {
            if (!LiveHorizontalBinders.TryGetValue(targetH, out GroupInfo? bindersH)) {
                LiveHorizontalBinders[targetH] = bindersH = new GroupInfo(targetH);
            }

            bindersH.ScrollViewers.Add(scroller);
            scroller.ScrollChanged += OnScrollChanged_Horizontal;
        }
    }

    private static void OnScrollChanged_Vertical(object? sender, ScrollChangedEventArgs e) {
        ScrollViewer scroller = (ScrollViewer) sender!;
        string? targetV = GetVerticalBindGroup(scroller);
        if (string.IsNullOrWhiteSpace(targetV) || !LiveVerticalBinders.TryGetValue(targetV, out GroupInfo? bindersV)) {
            return;
        }

        if (bindersV.IsUpdating) {
            return;
        }

        try {
            bindersV.IsUpdating = true;
            foreach (ScrollViewer scrollViewer in bindersV.ScrollViewers) {
                if (scrollViewer != scroller) {
                    scrollViewer.Offset = new Vector(scrollViewer.Offset.X, scrollViewer.Offset.Y + e.OffsetDelta.Y);
                    // scrollViewer.UpdateLayout();
                }
            }
        }
        finally {
            bindersV.IsUpdating = false;
        }
    }

    private static void OnScrollChanged_Horizontal(object? sender, ScrollChangedEventArgs e) {
        ScrollViewer scroller = (ScrollViewer) sender!;
        string? targetH = GetHorizontalBindGroup(scroller);
        if (string.IsNullOrWhiteSpace(targetH) || !LiveHorizontalBinders.TryGetValue(targetH, out GroupInfo? bindersH)) {
            return;
        }

        if (bindersH.IsUpdating) {
            return;
        }

        try {
            bindersH.IsUpdating = true;
            foreach (ScrollViewer scrollViewer in bindersH.ScrollViewers) {
                if (scrollViewer != scroller) {
                    scrollViewer.Offset = new Vector(scrollViewer.Offset.X+ e.OffsetDelta.X, scrollViewer.Offset.Y);
                    // scrollViewer.UpdateLayout();
                }
            }
        }
        finally {
            bindersH.IsUpdating = false;
        }
    }
}