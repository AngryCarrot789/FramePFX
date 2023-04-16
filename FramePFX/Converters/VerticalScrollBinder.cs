using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using FramePFX.Shortcuts;

namespace FramePFX.Converters {
    public static class VerticalScrollBinder {
        public static readonly DependencyProperty BindGroupProperty =
            DependencyProperty.RegisterAttached(
                "BindGroup",
                typeof(string),
                typeof(VerticalScrollBinder),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PropertyChangedCallback));

        public static void SetBindGroup(ScrollViewer element, string value) {
            element.SetValue(BindGroupProperty, value);
        }

        public static string GetBindGroup(ScrollViewer element) {
            return (string) element.GetValue(BindGroupProperty);
        }

        private static bool IsUpdatingScroll;
        private static readonly Dictionary<string, List<ScrollViewer>> RegisteredScrollers = new Dictionary<string, List<ScrollViewer>>();

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is ScrollViewer scroller) {
                if (e.OldValue is string oldGroup) {
                    if (RegisteredScrollers.TryGetValue(oldGroup, out var list)) {
                        list.Remove(scroller);
                    }
                }

                scroller.ScrollChanged -= OnScrollChanged;
                if (e.NewValue is string newGroup) {
                    scroller.ScrollChanged += OnScrollChanged;
                    if (!RegisteredScrollers.TryGetValue(newGroup, out List<ScrollViewer> list)) {
                        RegisteredScrollers[newGroup] = list = new List<ScrollViewer>();
                    }

                    list.Add(scroller);
                }
            }
        }

        private static void OnScrollChanged(object sender, ScrollChangedEventArgs e) {
            if (IsUpdatingScroll) {
                return;
            }

            ScrollViewer viewer = (ScrollViewer) sender;
            string group = GetBindGroup(viewer);
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
    }
}