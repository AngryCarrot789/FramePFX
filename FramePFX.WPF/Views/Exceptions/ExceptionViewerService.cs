using System;
using FramePFX.Exceptions;
using FramePFX.Utils;
using FramePFX.Views.Windows;

namespace FramePFX.WPF.Views.Exceptions {
    public class ExceptionViewerService {
        // singleton to soon support IoC
        public static ExceptionViewerService Instance { get; } = new ExceptionViewerService();

        public IWindow ShowExceptionStack(ErrorList list) {
            ExceptionViewerWindow window = new ExceptionViewerWindow();
            ExceptionStackViewModel vm = new ExceptionStackViewModel(list);
            window.DataContext = vm;
            window.Show();
            return window;
        }

        public IWindow ShowException(Exception exception) {
            ExceptionViewerWindow window = new ExceptionViewerWindow();
            using (ErrorList list = new ErrorList(null, true, true)) {
                list.Add(exception);

                ExceptionStackViewModel vm = new ExceptionStackViewModel(list);
                window.DataContext = vm;
                window.Show();
                return window;
            }
        }
    }
}