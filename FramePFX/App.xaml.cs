using System;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using FFmpeg.AutoGen;
using FramePFX.Core;
using FramePFX.Core.Actions;
using FramePFX.Core.Automation.Keyframe;
using FramePFX.Core.Editor;
using FramePFX.Core.Editor.ResourceManaging;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.Editor.Timelines;
using FramePFX.Core.Editor.Timelines.AudioClips;
using FramePFX.Core.Editor.Timelines.Tracks;
using FramePFX.Core.Editor.Timelines.VideoClips;
using FramePFX.Core.Editor.ViewModels;
using FramePFX.Core.Editor.ViewModels.Timelines;
using FramePFX.Core.RBC;
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

        #if DEBUG
        public static ProjectViewModel DemoProject { get; } = new ProjectViewModel(CreateDemoProject());
        public static TimelineViewModel DemoTimeline { get; } = new TimelineViewModel(DemoProject, new Timeline(DemoProject.Model));
        #endif

        public App() {
        }

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
            #if DEBUG
            await editor.SetProject(new ProjectViewModel(CreateDebugProject()), true);
            #else
            await editor.SetProject(new ProjectViewModel(CreateDemoProject()), true);
            #endif
            ((EditorMainWindow) this.MainWindow)?.VPViewBox.FitContentToCenter();
            editor.ActiveProject.AutomationEngine.TickAndRefreshProject(false);
            await editor.View.Render();
        }

        protected override void OnExit(ExitEventArgs e) {
            base.OnExit(e);
        }

        public static Project CreateDemoProject() {
            // Demo project -- projects can be created as entirely models
            Project project = new Project();
            project.Settings.Resolution = new Resolution(1920, 1080);

            ResourceManager manager = project.ResourceManager;
            ulong id_r = manager.RegisterEntry(manager.RootGroup.Add(new ResourceColour(220, 25, 25) { DisplayName = "colour_red" }));
            ulong id_g = manager.RegisterEntry(manager.RootGroup.Add(new ResourceColour(25, 220, 25) { DisplayName = "colour_green" }));
            ulong id_b = manager.RegisterEntry(manager.RootGroup.Add(new ResourceColour(25, 25, 220) { DisplayName = "colour_blue" }));

            ResourceGroup group = new ResourceGroup("Extra Colours");
            manager.RootGroup.AddItemToList(group);
            ulong id_w = manager.RegisterEntry(group.Add(new ResourceColour(220, 220, 220) { DisplayName = "white colour"}));
            ulong id_d = manager.RegisterEntry(group.Add(new ResourceColour(50, 100, 220) { DisplayName = "idek"}));

            {
                VideoTrack track1 = new VideoTrack(project.Timeline) {
                    DisplayName = "Track 1 with stuff"
                };
                project.Timeline.AddTrack(track1);

                track1.AutomationData[VideoTrack.OpacityKey].AddKeyFrame(new KeyFrameDouble(0, 0.3d));
                track1.AutomationData[VideoTrack.OpacityKey].AddKeyFrame(new KeyFrameDouble(50, 0.5d));
                track1.AutomationData[VideoTrack.OpacityKey].AddKeyFrame(new KeyFrameDouble(100, 1d));
                track1.AutomationData.ActiveKeyFullId = VideoTrack.OpacityKey.FullId;

                ShapeVideoClip clip1 = new ShapeVideoClip {
                    Width = 200, Height = 200,
                    FrameSpan = new FrameSpan(0, 120),
                    DisplayName = "Clip colour_red"
                };

                clip1.MediaPosition = new Vector2(0, 0);
                clip1.SetTargetResourceId(id_r);
                track1.AddClip(clip1);

                ShapeVideoClip clip2 = new ShapeVideoClip {
                    Width = 200, Height = 200,
                    FrameSpan = new FrameSpan(150, 30),
                    DisplayName = "Clip colour_green"
                };
                clip2.MediaPosition = new Vector2(200, 200);
                clip2.SetTargetResourceId(id_g);
                track1.AddClip(clip2);
            }
            {
                VideoTrack track2 = new VideoTrack(project.Timeline) {
                    DisplayName = "Track 2"
                };
                project.Timeline.AddTrack(track2);

                ShapeVideoClip clip1 = new ShapeVideoClip {
                    Width = 400, Height = 400,
                    FrameSpan = new FrameSpan(300, 90),
                    DisplayName = "Clip colour_blue"
                };

                clip1.MediaPosition = new Vector2(200, 200);
                clip1.SetTargetResourceId(id_b);
                track2.AddClip(clip1);
                ShapeVideoClip clip2 = new ShapeVideoClip {
                    Width = 100, Height = 1000,
                    FrameSpan = new FrameSpan(15, 130),
                    DisplayName = "Clip blueish"
                };

                clip2.AutomationData[VideoClip.MediaPositionKey].AddKeyFrame(new KeyFrameVector2(10L, Vector2.Zero));
                clip2.AutomationData[VideoClip.MediaPositionKey].AddKeyFrame(new KeyFrameVector2(75L, new Vector2(100, 200)));
                clip2.AutomationData[VideoClip.MediaPositionKey].AddKeyFrame(new KeyFrameVector2(90L, new Vector2(400, 400)));
                clip2.AutomationData[VideoClip.MediaPositionKey].AddKeyFrame(new KeyFrameVector2(115L, new Vector2(100, 700)));
                clip2.AutomationData.ActiveKeyFullId = VideoClip.MediaPositionKey.FullId;

                clip2.MediaPosition = new Vector2(400, 400);
                clip2.SetTargetResourceId(id_d);
                track2.AddClip(clip2);
            }
            {
                VideoTrack track1 = new VideoTrack(project.Timeline) {
                    DisplayName = "Empty track"
                };
                project.Timeline.AddTrack(track1);
            }

            return project;
        }

         public static Project CreateDebugProject() {
            // Debug project - test a lot of features and make sure they work
            Project project = CreateDemoProject();
            // {
            //     AudioTrack track = new AudioTrack(project.Timeline) {
            //         DisplayName = "Audio Track 1"
            //     };
            //     project.Timeline.AddTrack(track);
            //     SinewaveClip clip = new SinewaveClip() {
            //         FrameSpan = new FrameSpan(300, 90),
            //         DisplayName = "Clip Sine"
            //     };
            //     track.AddClip(clip);
            // }

            return project;
        }
    }
}
