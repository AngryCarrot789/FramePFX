using System;
using System.Windows;
using FramePFX.Core.Interactivity;

namespace FramePFX.Interactivity {
    public static class FileDropAttachments {
        public static readonly DependencyProperty FileDropNotifierProperty =
            DependencyProperty.RegisterAttached(
                "FileDropNotifier",
                typeof(IFileDropNotifier),
                typeof(FileDropAttachments),
                new FrameworkPropertyMetadata(null, (o, args) => OnFileDropNotifierPropertyChanged(o, args, false)));

        public static readonly DependencyProperty PreviewFileDropNotifierProperty =
            DependencyProperty.RegisterAttached(
                "PreviewFileDropNotifier",
                typeof(IFileDropNotifier),
                typeof(FileDropAttachments),
                new FrameworkPropertyMetadata(null, (o, args) => OnFileDropNotifierPropertyChanged(o, args, true)));

        public static readonly DependencyPropertyKey IsProcessingDragDropEntryProperty = DependencyProperty.RegisterAttachedReadOnly("IsProcessingDragDropEntry", typeof(bool), typeof(FileDropAttachments), new PropertyMetadata(false));
        public static readonly DependencyPropertyKey IsProcessingDragDropProcessProperty = DependencyProperty.RegisterAttachedReadOnly("IsProcessingDragDropProcess", typeof(bool), typeof(FileDropAttachments), new PropertyMetadata(false));

        public static void SetFileDropNotifier(FrameworkElement element, IFileDropNotifier value) {
            element.SetValue(FileDropNotifierProperty, value);
        }

        public static IFileDropNotifier GetFileDropNotifier(FrameworkElement element) {
            return (IFileDropNotifier) element.GetValue(FileDropNotifierProperty);
        }

        public static void SetPreviewFileDropNotifier(FrameworkElement element, IFileDropNotifier value) {
            element.SetValue(PreviewFileDropNotifierProperty, value);
        }

        public static IFileDropNotifier GetPreviewFileDropNotifier(FrameworkElement element) {
            return (IFileDropNotifier) element.GetValue(PreviewFileDropNotifierProperty);
        }

        private static void SetIsProcessingDragDropEntry(FrameworkElement element, bool value) {
            element.SetValue(IsProcessingDragDropEntryProperty, value);
        }

        public static bool GetIsProcessingDragDropEntry(FrameworkElement element) {
            return (bool) element.GetValue(IsProcessingDragDropEntryProperty.DependencyProperty);
        }

        private static void SetIsProcessingDragDropProcess(FrameworkElement element, bool value) {
            element.SetValue(IsProcessingDragDropProcessProperty, value);
        }

        public static bool GetIsProcessingDragDropProcess(FrameworkElement element) {
            return (bool) element.GetValue(IsProcessingDragDropProcessProperty.DependencyProperty);
        }

        private static void OnFileDropNotifierPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e, bool preview) {
            if (d is FrameworkElement element) {
                // element.PreviewDragEnter -= OnElementDragEnter;
                // element.DragEnter -= OnElementDragEnter;
                element.PreviewDrop -= OnElementDrop;
                element.Drop -= OnElementDrop;

                if (e.NewValue != null) {
                    element.AllowDrop = true;
                    if (preview) {
                        element.PreviewDragOver += OnElementDragEnter;
                        element.PreviewDrop += OnElementDrop;
                    }
                    else {
                        element.DragOver += OnElementDragEnter;
                        element.Drop += OnElementDrop;
                    }
                }
            }
        }

        private static async void OnElementDragEnter(object sender, DragEventArgs e) {
            FrameworkElement element = (FrameworkElement) sender ?? throw new Exception("Expected FrameworkElement");
            if (GetIsProcessingDragDropEntry(element) || GetIsProcessingDragDropProcess(element)) {
                return;
            }

            IFileDropNotifier notifier = GetFileDropNotifier(element) ?? GetPreviewFileDropNotifier(element);
            if (notifier == null) {
                return;
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop) && e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0) {
                FileDropType type = (FileDropType) ((int) e.Effects & (int) FileDropType.All);
                SetIsProcessingDragDropEntry(element, true);
                try {
                    if (!await notifier.CanDrop(files, type)) {
                        e.Effects = DragDropEffects.None; // does nothing
                    }

                    e.Handled = true;
                }
                finally {
                    SetIsProcessingDragDropEntry(element, false);
                }
            }
        }

        private static async void OnElementDrop(object sender, DragEventArgs e) {
            FrameworkElement element = ((FrameworkElement) sender) ?? throw new Exception("Expected FrameworkElement");
            if (GetIsProcessingDragDropProcess(element)) {
                return;
            }

            IFileDropNotifier notifier = GetFileDropNotifier(element) ?? GetPreviewFileDropNotifier(element);
            if (notifier == null) {
                return;
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop) && e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0) {
                FileDropType type = (FileDropType) ((int) e.Effects & (int) FileDropType.All);
                SetIsProcessingDragDropProcess(element, true);
                try {
                    if (await notifier.CanDrop(files, type)) {
                        e.Effects = (DragDropEffects) await notifier.OnFilesDropped(files, type);
                    }
                    else {
                        e.Effects = DragDropEffects.None;
                        e.Handled = true;
                    }
                }
                finally {
                    SetIsProcessingDragDropProcess(element, false);
                }
            }
        }
    }
}