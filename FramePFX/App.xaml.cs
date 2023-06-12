using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using FFmpeg.AutoGen;
using FramePFX.Core;
using FramePFX.Core.Actions;
using FramePFX.Core.Editor;
using FramePFX.Core.Editor.ResourceManaging;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.Editor.Timeline.Clip;
using FramePFX.Core.Editor.Timeline.Layers;
using FramePFX.Core.Editor.ViewModels;
using FramePFX.Core.Editor.ViewModels.Timeline.Layers;
using FramePFX.Core.Shortcuts.Managing;
using FramePFX.Core.Shortcuts.ViewModels;
using FramePFX.Core.Utils;
using FramePFX.Editor;
using FramePFX.Shortcuts;
using FramePFX.Shortcuts.Converters;
using FramePFX.Utils;
using FramePFX.Views;

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
            EditorMainWindow window = new EditorMainWindow();
            this.splash.Close();
            this.MainWindow = window;
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
            window.Show();
            await this.Dispatcher.Invoke(async () => {
                await this.OnVideoEditorLoaded(window.Editor);
            }, DispatcherPriority.Loaded);
        }

        public async Task OnVideoEditorLoaded(VideoEditorViewModel editor) {
            // Demo project -- projects can be created as entirely models

            ProjectModel project = new ProjectModel();
            project.Settings.Resolution = new Resolution(1920, 1080);

            {
                ResourceManager manager = project.ResourceManager;
                manager.RegisterEntry("colour_red", manager.RootGroup.Add(new ResourceColour() {R = 0.9f, G = 0.1f, B = 0.1f}));
                manager.RegisterEntry("colour_green", manager.RootGroup.Add(new ResourceColour() {R = 0.1f, G = 0.9f, B = 0.1f}));
                manager.RegisterEntry("colour_blue", manager.RootGroup.Add(new ResourceColour() {R = 0.1f, G = 0.1f, B = 0.9f}));

                ResourceGroup group = new ResourceGroup();
                manager.RootGroup.AddItemToList(group);
                manager.RegisterEntry("white colour", group.Add(new ResourceColour() {R = 0.9f, G = 0.9f, B = 0.9f}));
                manager.RegisterEntry("idek", group.Add(new ResourceColour() {R = 0.2f, G = 0.4f, B = 0.9f}));
            }

            {
                VideoLayerModel layer1 = new VideoLayerModel(project.Timeline) {
                    DisplayName = "Layer 1 with stuff"
                };
                project.Timeline.AddLayer(layer1);

                ShapeClipModel clip1 = new ShapeClipModel {
                    MediaPosition = new Vector2(0, 0),
                    Width = 200, Height = 200,
                    FrameSpan = new FrameSpan(0, 120),
                    DisplayName = "Clip colour_red"
                };
                clip1.SetTargetResourceId("idek");
                layer1.AddClip(clip1);

                ShapeClipModel clip2 = new ShapeClipModel {
                    MediaPosition = new Vector2(200, 200),
                    Width = 200, Height = 200,
                    FrameSpan = new FrameSpan(150, 30),
                    DisplayName = "Clip colour_green"
                };
                clip2.SetTargetResourceId("colour_green");
                layer1.AddClip(clip2);
            }
            {
                VideoLayerModel layer2 = new VideoLayerModel(project.Timeline) {
                    DisplayName = "Layer 2"
                };
                project.Timeline.AddLayer(layer2);

                ShapeClipModel clip1 = new ShapeClipModel {
                    MediaPosition = new Vector2(200, 400),
                    Width = 400, Height = 400,
                    FrameSpan = new FrameSpan(300, 90),
                    DisplayName = "Clip colour_blue"
                };

                clip1.SetTargetResourceId("colour_blue");
                layer2.AddClip(clip1);
                ShapeClipModel clip2 = new ShapeClipModel {
                    MediaPosition = new Vector2(400, 400),
                    Width = 100, Height = 1000,
                    FrameSpan = new FrameSpan(15, 25),
                    DisplayName = "Clip colour_green"
                };

                clip2.SetTargetResourceId("colour_green");
                layer2.AddClip(clip2);
            }
            {
                VideoLayerModel layer1 = new VideoLayerModel(project.Timeline) {
                    DisplayName = "Empty layer"
                };
                project.Timeline.AddLayer(layer1);
            }

            ProjectViewModel project1 = new ProjectViewModel(project);

            await editor.SetProject(project1);
            ((EditorMainWindow) this.MainWindow)?.VPViewBox.FitContentToCenter();
            editor.View.RenderViewPort(false);
        }

        protected override void OnExit(ExitEventArgs e) {
            base.OnExit(e);
        }
    }
}
