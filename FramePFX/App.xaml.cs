using System;
using System.Threading.Tasks;
using System.Windows;
using FrameControl.Core;
using FrameControl.Core.Services;
using FrameControl.Services;
using FrameControl.Views.Dialogs.FilePicking;
using FrameControl.Views.Dialogs.Message;
using FrameControl.Views.Dialogs.UserInputs;

namespace FramePFX {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private void Application_Startup(object sender, StartupEventArgs e) {
            IoC.MessageDialogs = new MessageDialogService();
            IoC.Dispatcher = new DispatcherDelegate(this);
            IoC.Clipboard = new ClipboardService();
            IoC.FilePicker = new FilePickDialogService();
            IoC.UserInput = new UserInputDialogService();

            this.MainWindow = new MainWindow();
            this.MainWindow.Show();

            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        protected override void OnExit(ExitEventArgs e) {
            base.OnExit(e);
        }

        private class DispatcherDelegate : IDispatcher {
            private readonly App app;

            public DispatcherDelegate(App app) {
                this.app = app;
            }

            public void Invoke(Action action) {
                this.app.Dispatcher.Invoke(action);
            }

            public T Invoke<T>(Func<T> function) {
                return this.app.Dispatcher.Invoke(function);
            }

            public async Task InvokeAsync(Action action) {
                await this.app.Dispatcher.InvokeAsync(action);
            }

            public async Task<T> InvokeAsync<T>(Func<T> function) {
                return await this.app.Dispatcher.InvokeAsync(function);
            }
        }
    }
}
