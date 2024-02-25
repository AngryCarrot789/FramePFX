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

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace FramePFX.AttachedProperties {
    public static class ScrollBinder {
        public static readonly DependencyProperty VerticalBindGroupProperty =
            DependencyProperty.RegisterAttached(
                "VerticalBindGroup",
                typeof(string),
                typeof(ScrollBinder),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnVerticalBindGroupPropertyChanged));

        public static readonly DependencyProperty HorizontalBindGroupProperty =
            DependencyProperty.RegisterAttached(
                "HorizontalBindGroup",
                typeof(string),
                typeof(ScrollBinder),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnHorizontalBindGroupPropertyChanged));

        public static void SetVerticalBindGroup(DependencyObject element, string value) => element.SetValue(VerticalBindGroupProperty, value);
        public static string GetVerticalBindGroup(DependencyObject element) => (string) element.GetValue(VerticalBindGroupProperty);

        public static void SetHorizontalBindGroup(DependencyObject element, string value) => element.SetValue(HorizontalBindGroupProperty, value);
        public static string GetHorizontalBindGroup(DependencyObject element) => (string) element.GetValue(HorizontalBindGroupProperty);

        private static bool IsUpdatingScroll;
        private static readonly Dictionary<string, List<ScrollViewer>> RegisteredScrollers = new Dictionary<string, List<ScrollViewer>>();

        private static void OnVerticalBindGroupPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is ScrollViewer scroller) {
                if (e.OldValue is string oldGroup) {
                    if (RegisteredScrollers.TryGetValue(oldGroup, out var list)) {
                        list.Remove(scroller);
                    }
                }

                scroller.ScrollChanged -= OnVerticalScrollChanged;
                if (e.NewValue is string newGroup) {
                    scroller.ScrollChanged += OnVerticalScrollChanged;
                    if (!RegisteredScrollers.TryGetValue(newGroup, out List<ScrollViewer> list)) {
                        RegisteredScrollers[newGroup] = list = new List<ScrollViewer>();
                    }

                    list.Add(scroller);
                }
            }
        }

        private static void OnVerticalScrollChanged(object sender, ScrollChangedEventArgs e) {
            if (IsUpdatingScroll) {
                return;
            }

            ScrollViewer viewer = (ScrollViewer) sender;
            string group = GetVerticalBindGroup(viewer);
            if (group == null) {
                return;
            }

            if (RegisteredScrollers.TryGetValue(group, out var list)) {
                IsUpdatingScroll = true;
                try {
                    foreach (ScrollViewer scrollViewer in list) {
                        if (scrollViewer != viewer) {
                            scrollViewer.ScrollToVerticalOffset(e.VerticalOffset);
                        }
                    }
                }
                finally {
                    IsUpdatingScroll = false;
                }
            }
        }

        private static void OnHorizontalBindGroupPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is ScrollViewer scroller) {
                if (e.OldValue is string oldGroup) {
                    if (RegisteredScrollers.TryGetValue(oldGroup, out var list)) {
                        list.Remove(scroller);
                    }
                }

                scroller.ScrollChanged -= OnHorizontalScrollChanged;
                if (e.NewValue is string newGroup) {
                    scroller.ScrollChanged += OnHorizontalScrollChanged;
                    if (!RegisteredScrollers.TryGetValue(newGroup, out List<ScrollViewer> list)) {
                        RegisteredScrollers[newGroup] = list = new List<ScrollViewer>();
                    }

                    list.Add(scroller);
                }
            }
        }

        private static void OnHorizontalScrollChanged(object sender, ScrollChangedEventArgs e) {
            if (IsUpdatingScroll) {
                return;
            }

            ScrollViewer viewer = (ScrollViewer) sender;
            string group = GetHorizontalBindGroup(viewer);
            if (group == null) {
                return;
            }

            if (RegisteredScrollers.TryGetValue(group, out var list)) {
                IsUpdatingScroll = true;
                try {
                    foreach (ScrollViewer scrollViewer in list) {
                        if (scrollViewer != viewer) {
                            scrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
                        }
                    }
                }
                finally {
                    IsUpdatingScroll = false;
                }
            }
        }
    }
}