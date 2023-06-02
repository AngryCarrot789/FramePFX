using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using FFmpeg.AutoGen;
using FramePFX.Core;
using FramePFX.Core.Actions;
using FramePFX.Core.Editor;
using FramePFX.Core.Editor.ViewModels;
using FramePFX.Core.Shortcuts.Managing;
using FramePFX.Core.Shortcuts.ViewModels;
using FramePFX.Core.Utils;
using FramePFX.Shortcuts;
using FramePFX.Shortcuts.Converters;
using FramePFX.Utils;
using FramePFX.Views;
using FramePFX.Views.Main;

namespace FramePFX {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private AppSplashScreen splash;

        public void RegisterActions() {
            ActionManager.SearchAndRegisterActions(ActionManager.Instance);
        }

        private async Task SetActivity(string activity) {
            this.splash.CurrentActivity = activity;
            await this.Dispatcher.InvokeAsync(() => {
            }, DispatcherPriority.ApplicationIdle);
        }

        public async Task InitApp() {
            await this.SetActivity("Loading services...");
            string[] envArgs = Environment.GetCommandLineArgs();
            if (envArgs.Length > 0 && Path.GetDirectoryName(envArgs[0]) is string dir && dir.Length > 0) {
                Directory.SetCurrentDirectory(dir);
            }

            IoC.Dispatcher = new DispatcherDelegate(this.Dispatcher);
            IoC.OnShortcutModified = (x) => {
                if (!string.IsNullOrWhiteSpace(x)) {
                    ShortcutManager.Instance.InvalidateShortcutCache();
                    GlobalUpdateShortcutGestureConverter.BroadcastChange();
                }
            };

            IoC.LoadServicesFromAttributes();

            await this.SetActivity("Loading shortcut and action managers...");
            ShortcutManager.Instance = new WPFShortcutManager();
            ActionManager.Instance = new ActionManager();
            InputStrokeViewModel.KeyToReadableString = KeyStrokeStringConverter.ToStringFunction;
            InputStrokeViewModel.MouseToReadableString = MouseStrokeStringConverter.ToStringFunction;

            this.RegisterActions();

            await this.SetActivity("Loading keymap...");
            string keymapFilePath = Path.GetFullPath(@"Keymap.xml");
            if (File.Exists(keymapFilePath)) {
                using (FileStream stream = File.OpenRead(keymapFilePath)) {
                    ShortcutGroup group = WPFKeyMapSerialiser.Instance.Deserialise(stream);
                    WPFShortcutManager.WPFInstance.SetRoot(group);
                }
            }
            else {
                await IoC.MessageDialogs.ShowMessageAsync("No keymap available", "Keymap file does not exist: " + keymapFilePath + $".\nCurrent directory: {Directory.GetCurrentDirectory()}\nCommand line args:{string.Join("\n", Environment.GetCommandLineArgs())}");
            }

            await this.SetActivity("Loading FFmpeg...");
            ffmpeg.avdevice_register_all();
        }

        private async void Application_Startup(object sender, StartupEventArgs e) {
            // Dialogs may be shown, becoming the main window, possibly causing the
            // app to shutdown when the mode is OnMainWindowClose or OnLastWindowClose
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            this.MainWindow = this.splash = new AppSplashScreen();
            this.splash.Show();

            try {
                await this.InitApp();
            }
            catch (Exception ex) {
                if (IoC.MessageDialogs != null) {
                    await IoC.MessageDialogs.ShowMessageExAsync("App initialisation failed", "Failed to start FramePFX", ex.GetToString());
                }
                else {
                    MessageBox.Show("Failed to start FramePFX:\n\n" + ex, "Fatal App initialisation failure");
                }

                this.Dispatcher.Invoke(() => {
                    this.Shutdown(0);
                }, DispatcherPriority.Background);

                return;
            }

            await this.SetActivity("Loading FramePFX main window...");
            MainWindow window = new MainWindow();
            this.splash.Close();
            this.MainWindow = window;
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
            window.Show();
            this.Dispatcher.Invoke(() => {
                this.OnVideoEditorLoaded((VideoEditorViewModel) window.DataContext);
            }, DispatcherPriority.Loaded);
        }

        public void OnVideoEditorLoaded(VideoEditorViewModel editor) {
            editor.SetProject(new ProjectViewModel(new ProjectModel()) {
                Settings = { FrameRate = 30, Resolution = new Resolution(1920, 1080)}
            });
        }

        protected override void OnExit(ExitEventArgs e) {
            base.OnExit(e);
        }
    }
}
