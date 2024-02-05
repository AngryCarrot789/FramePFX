using FramePFX.Editors.Views;
using FramePFX.Editors;
using FramePFX.Shortcuts.Managing;
using FramePFX.Shortcuts.WPF;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using FFmpeg.AutoGen;
using FramePFX.Logger;
using FramePFX.Shortcuts.WPF.Converters;
using FramePFX.Utils;
using FramePFX.Views;

namespace FramePFX {
    public partial class App : Application {
        private AppSplashScreen splash;

        private async void App_OnStartup(object sender, StartupEventArgs args) {
            // Pre init stuff
            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(int.MaxValue));
            ToolTipService.InitialShowDelayProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(400));

            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            this.MainWindow = this.splash = new AppSplashScreen();
            this.splash.Show();

            try {
                AppLogger.Instance.PushHeader("FramePFX initialisation");
                await this.InitWPFApp();
            }
            catch (Exception ex) {
                MessageBox.Show("Failed to start FramePFX: " + ex.GetToString(), "Fatal App init failure");
                this.Dispatcher.Invoke(() => this.Shutdown(0), DispatcherPriority.Background);
                return;
            }
            finally {
                AppLogger.Instance.PopHeader();
            }

            await this.splash.SetAction("Loading FramePFX main window...", null);

            // Editor init
            VideoEditor editor = new VideoEditor();

            EditorWindow window = new EditorWindow();
            window.Show();
            this.splash.Close();
            this.MainWindow = window;
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
            window.Editor = editor;
            await ApplicationCore.Instance.OnEditorLoaded(editor, args.Args);
            // this.Dispatcher.InvokeAsync(() => window.Editor = editor, DispatcherPriority.Loaded);
        }

        public async Task InitWPFApp() {
            await this.splash.SetAction("Loading services...", null);
            string[] envArgs = Environment.GetCommandLineArgs();
            if (envArgs.Length > 0 && Path.GetDirectoryName(envArgs[0]) is string dir && dir.Length > 0) {
                Directory.SetCurrentDirectory(dir);
            }

            ShortcutManager.Instance = new WPFShortcutManager();
            ShortcutManager.Instance.ShortcutModified += (sender, value) => {
                GlobalUpdateShortcutGestureConverter.BroadcastChange();
            };

            RuntimeHelpers.RunClassConstructor(typeof(UIInputManager).TypeHandle);

            // This is where services are registered
            await ApplicationCore.InternalSetupNewInstance(this.splash);
            // Most if not all services are available below here

            await AppLogger.Instance.FlushEntries();
            await this.splash.SetAction("Loading shortcuts and actions...", null);

            ApplicationCore.InternalRegisterActions();

            // TODO: user modifiable keymap, and also save it to user documents
            // also, use version attribute to check out of date keymap, and offer to
            // overwrite while backing up old file... or just try to convert file

            await this.splash.SetAction("Loading keymap...", null);
            string keymapFilePath = Path.GetFullPath(@"Keymap.xml");
            if (File.Exists(keymapFilePath)) {
                try {
                    using (FileStream stream = File.OpenRead(keymapFilePath)) {
                        WPFShortcutManager.WPFInstance.DeserialiseRoot(stream);
                    }
                }
                catch (Exception ex) {
                    IoC.MessageService.ShowMessage("Keymap", "Failed to read keymap file" + keymapFilePath + ":" + ex.GetToString());
                }
            }
            else {
                IoC.MessageService.ShowMessage("Keymap", "Keymap file does not exist at " + keymapFilePath);
            }

            await this.splash.SetAction("Loading FFmpeg...", null);

            try {
                ffmpeg.avdevice_register_all();
            }
            catch (Exception e) {
                IoC.MessageService.ShowMessage("FFmpeg registration failed", "The FFmpeg libraries (avcodec-60.dll, avfilter-9, and all other 6 dlls files) must be placed in the build folder which is where the EXE is, e.g. /FramePFX/bin/x64/Debug", e.GetToString());
            }
        }
    }
}
