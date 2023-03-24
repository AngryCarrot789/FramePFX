using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using FramePFX.Core;
using FramePFX.Core.Services;
using FramePFX.Render;
using FramePFX.Services;
using FramePFX.Views.Dialogs.FilePicking;
using FramePFX.Views.Dialogs.Message;
using FramePFX.Views.Dialogs.UserInputs;
using FramePFX.Views.Main;

namespace FramePFX {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private void Application_Startup(object sender, StartupEventArgs e) {
            CoreIoC.MessageDialogs = new MessageDialogService();
            CoreIoC.Dispatcher = new DispatcherDelegate(this);
            CoreIoC.Clipboard = new ClipboardService();
            CoreIoC.FilePicker = new FilePickDialogService();
            CoreIoC.UserInput = new UserInputDialogService();

            OpenGLMainThread.Setup();
            OpenGLMainThread.Instance.Start();
            while (true) {
                Thread.Sleep(50);
                if (OpenGLMainThread.Instance.Thread.IsRunning && OpenGLMainThread.GlobalContext != null) {
                    break;
                }
            }

            IoC.VideoEditor = new VideoEditorViewModel();
            this.MainWindow = new MainWindow {
                DataContext = IoC.VideoEditor
            };

            this.MainWindow.Show();
            IoC.VideoEditor.Viewport.ViewPortHandle = ((MainWindow) this.MainWindow).oglPort;
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;

            IoC.VideoEditor.NewProjectAction();
            this.Dispatcher.Invoke(() => {
                IoC.ActiveProject.RenderTimeline();
                // Loaded, to allow ICG to generate content and assign the handles
            }, DispatcherPriority.Loaded);
        }

        protected override void OnExit(ExitEventArgs e) {
            base.OnExit(e);
        }

        private class DispatcherDelegate : IDispatcher {
            private readonly App app;

            public DispatcherDelegate(App app) {
                this.app = app;
            }

            public void InvokeLater(Action action) {
                this.app.Dispatcher.Invoke(action, DispatcherPriority.Normal);
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
