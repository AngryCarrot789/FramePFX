using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using FramePFX.Core;
using FramePFX.Core.Shortcuts.Managing;
using FramePFX.Render;
using FramePFX.Render.OGL;
using FramePFX.Services;
using FramePFX.Shortcuts;
using FramePFX.Shortcuts.Converters;
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
            CoreIoC.Dispatcher = new DispatcherDelegate(this.Dispatcher);
            CoreIoC.Clipboard = new ClipboardService();
            CoreIoC.FilePicker = new FilePickDialogService();
            CoreIoC.UserInput = new UserInputDialogService();
            CoreIoC.OnShortcutManagedChanged = (x) => {
                if (!string.IsNullOrWhiteSpace(x)) {
                    WPFShortcutManager.Instance.InvalidateShortcutCache();
                    GlobalUpdateShortcutGestureConverter.BroadcastChange();
                    // UpdatePath(this.Resources, x);
                }
            };

            string path = @"F:\VSProjsV2\FramePFX\FramePFX\Keymap.xml";
            if (File.Exists(path)) {
                using (FileStream stream = File.OpenRead(path)) {
                    ShortcutGroup group = WPFKeyMapDeserialiser.Instance.Deserialise(stream);
                    WPFShortcutManager.Instance.SetRoot(group);
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

            IoC.VideoEditor = new VideoEditor();
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
    }
}
