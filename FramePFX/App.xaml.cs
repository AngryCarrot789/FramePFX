using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using FramePFX.Converters;
using FramePFX.Core;
using FramePFX.Core.Services;
using FramePFX.Core.Shortcuts.Managing;
using FramePFX.Render;
using FramePFX.Services;
using FramePFX.Shortcuts;
using FramePFX.Views.Dialogs.FilePicking;
using FramePFX.Views.Dialogs.Message;
using FramePFX.Views.Dialogs.UserInputs;
using FramePFX.Views.Main;

namespace FramePFX {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private static void UpdateShortcutResourcesRecursive(ResourceDictionary dictionary, ShortcutGroup group) {
            foreach (ShortcutGroup innerGroup in group.Groups) {
                UpdateShortcutResourcesRecursive(dictionary, innerGroup);
            }

            foreach (ManagedShortcut shortcut in group.Shortcuts) {
                UpdatePath(dictionary, shortcut.Path);
            }
        }

        private static void UpdatePath(ResourceDictionary dictionary, string shortcut) {
            string resourcePath = "ShortcutPaths." + shortcut;
            if (dictionary.Contains(resourcePath)) {
                dictionary[resourcePath] = ShortcutPathToInputGestureTextConverter.ShortcutToInputGestureText(shortcut);
            }
        }

        private void Application_Startup(object sender, StartupEventArgs e) {
            CoreIoC.MessageDialogs = new MessageDialogService();
            CoreIoC.Dispatcher = new DispatcherDelegate(this);
            CoreIoC.Clipboard = new ClipboardService();
            CoreIoC.FilePicker = new FilePickDialogService();
            CoreIoC.UserInput = new UserInputDialogService();
            CoreIoC.BroadcastShortcutChanged = (x) => {
                if (!string.IsNullOrWhiteSpace(x)) {
                    UpdatePath(this.Resources, x);
                }
            };

            string path = "F:\\VSProjsV2\\FramePFX\\FramePFX\\Keymap.xml";
            if (File.Exists(path)) {
                AppShortcutManager.Instance.Root = null;
                using (FileStream stream = File.OpenRead(path)) {
                    ShortcutGroup group = WPFKeyMapDeserialiser.Instance.Deserialise(stream);
                    AppShortcutManager.Instance.Root = group;
                }
            }
            else {
                MessageBox.Show("Keymap file does not exist: " + path);
            }

            OGLUtils.SetupOGLThread();
            OGLUtils.WaitForContextCompletion();
            // OpenGLMainThread.Setup();
            // OpenGLMainThread.Instance.Start();
            // while (true) {
            //     Thread.Sleep(50);
            //     if (OpenGLMainThread.Instance.Thread.IsRunning && OpenGLMainThread.GlobalContext != null) {
            //         break;
            //     }
            // }

            IoC.VideoEditor = new VideoEditorViewModel();
            this.MainWindow = new MainWindow {
                DataContext = IoC.VideoEditor
            };

            this.MainWindow.Show();
            IViewPort port = ((MainWindow) this.MainWindow).GLViewport.ViewPort;
            IoC.VideoEditor.PlaybackView.ViewPortHandle = port;
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;

            IoC.VideoEditor.NewProjectAction();
            this.Dispatcher.Invoke(() => {
                IoC.ActiveProject.RenderTimeline();
                // Loaded, to allow ICG to generate content and assign the handles
            }, DispatcherPriority.Loaded);

            sayHello();
        }

        public async void sayHello() {
            await OGLUtils.OGLThread.InvokeAsync(() => {
                Console.Write("ok! 1");
            });

            await Task.Delay(1000);

            await OGLUtils.OGLThread.InvokeAsync(() => {
                Console.Write("ok! 2");
            });
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
