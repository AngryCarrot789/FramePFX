using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FramePFX.WPF {
    public partial class App : Application {
        public static ThemeType CurrentTheme { get; set; }

        public static ResourceDictionary ThemeDictionary {
            get => Current.Resources.MergedDictionaries[0];
            set => Current.Resources.MergedDictionaries[0] = value;
        }

        public static ResourceDictionary ControlColours {
            get => Current.Resources.MergedDictionaries[1];
            set => Current.Resources.MergedDictionaries[1] = value;
        }

        public static ResourceDictionary I18NText {
            get => Current.Resources.MergedDictionaries[3];
            set => Current.Resources.MergedDictionaries[3] = value;
        }

        private AppSplashScreen splash;

        private readonly InputDrivenTaskExecutor monitor;
        private DateTime lastInput;

        public App() {
            // this.lastInput = DateTime.Now;
            // this.monitor = new InputDrivenTaskExecutor(() => {
            //     DateTime now = DateTime.Now;
            //     Debug.WriteLine($"Tick. Last = {(now - this.lastInput).TotalMilliseconds:F4}");
            //     this.lastInput = now;
            //     return Task.CompletedTask;
            // }, TimeSpan.FromMilliseconds(50));
            // Task.Run(async () => {
            //     while (true) {
            //         this.monitor.OnInput();
            //         await Task.Delay(20);
            //     }
            // });
        }

        public static void RefreshControlsDictionary() {
            ResourceDictionary resource = Current.Resources.MergedDictionaries[2];
            Current.Resources.MergedDictionaries.RemoveAt(2);
            Current.Resources.MergedDictionaries.Insert(2, resource);
        }

        public void RegisterActions() {
            // ActionManager.SearchAndRegisterActions(ActionManager.Instance);
            // TODO: Maybe use an XML file to store this, similar to how intellij registers actions?
            ActionManager.Instance.Register("actions.project.Open", new OpenProjectAction());
            ActionManager.Instance.Register("actions.project.Save", new SaveProjectAction());
            ActionManager.Instance.Register("actions.project.SaveAs", new SaveProjectAsAction());
            ActionManager.Instance.Register("actions.project.history.Undo", new UndoAction());
            ActionManager.Instance.Register("actions.project.history.Redo", new RedoAction());
            ActionManager.Instance.Register("actions.automation.AddKeyFrame", new AddKeyFrameAction());
            ActionManager.Instance.Register("actions.editor.timeline.TogglePlayPause", new TogglePlayPauseAction());
            ActionManager.Instance.Register("actions.resources.DeleteItems", new DeleteResourcesAction());
            ActionManager.Instance.Register("actions.resources.GroupSelection", new GroupSelectedResourcesAction());
            ActionManager.Instance.Register("actions.resources.ToggleOnlineState", new ToggleResourceOnlineStateAction());
            ActionManager.Instance.Register("actions.editor.timeline.DeleteSelectedClips", new DeleteSelectedClips());
            ActionManager.Instance.Register("actions.editor.NewVideoTrack", new NewVideoTrackAction());
            ActionManager.Instance.Register("actions.editor.NewAudioTrack", new NewAudioTrackAction());
            ActionManager.Instance.Register("actions.editor.timeline.SliceClips", new SliceClipsAction());
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

            List<(TypeInfo, ServiceImplementationAttribute)> serviceAttributes = new List<(TypeInfo, ServiceImplementationAttribute)>();
            List<(TypeInfo, ActionRegistrationAttribute)> attributes = new List<(TypeInfo, ActionRegistrationAttribute)>();

            // Process all attributes in a single scan, instead of multiple scans for services, actions, etc
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (TypeInfo typeInfo in assembly.DefinedTypes) {
                    ServiceImplementationAttribute serviceAttribute = typeInfo.GetCustomAttribute<ServiceImplementationAttribute>();
                    if (serviceAttribute?.Type != null) {
                        serviceAttributes.Add((typeInfo, serviceAttribute));
                    }

                    ActionRegistrationAttribute actionAttribute = typeInfo.GetCustomAttribute<ActionRegistrationAttribute>();
                    if (actionAttribute != null) {
                        attributes.Add((typeInfo, actionAttribute));
                    }
                }
            }

            foreach ((TypeInfo, ServiceImplementationAttribute) tuple in serviceAttributes) {
                object instance;
                try {
                    instance = Activator.CreateInstance(tuple.Item1);
                }
                catch (Exception e) {
                    throw new Exception($"Failed to create implementation of {tuple.Item2.Type} as {tuple.Item1}", e);
                }

                IoC.Instance.Register(tuple.Item2.Type, instance);
            }

            await this.SetActivity("Loading localization...");
            LocalizationController.SetLang(LangType.En);

            await this.SetActivity("Loading shortcuts and the action manager...");
            ShortcutManager.Instance = new WPFShortcutManager();
            ActionManager.Instance = new ActionManager();
            InputStrokeViewModel.KeyToReadableString = KeyStrokeStringConverter.ToStringFunction;
            InputStrokeViewModel.MouseToReadableString = MouseStrokeStringConverter.ToStringFunction;

            foreach ((TypeInfo type, ActionRegistrationAttribute attribute) in attributes.OrderBy(x => x.Item2.RegistrationOrder)) {
                AnAction action;
                try {
                    action = (AnAction)Activator.CreateInstance(type, true);
                }
                catch (Exception e) {
                    throw new Exception($"Failed to create an instance of the registered action '{type.FullName}'", e);
                }

                if (attribute.OverrideExisting && ActionManager.Instance.GetAction(attribute.ActionId) != null) {
                    ActionManager.Instance.Unregister(attribute.ActionId);
                }

                ActionManager.Instance.Register(attribute.ActionId, action);
            }

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
            try {
                ffmpeg.avdevice_register_all();
            }
            catch (Exception e) {
                throw new Exception("FFmpeg Unavailable. Copy FFmpeg DLLs into the same folder as the app's .exe", e);
            }
        }

        private async void Application_Startup(object sender, StartupEventArgs e) {
            // Dialogs may be shown, becoming the main window, possibly causing the
            // app to shutdown when the mode is OnMainWindowClose or OnLastWindowClose

#if false
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
            this.MainWindow = new PropertyPageDemoWindow();
            this.MainWindow.Show();
#else
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            this.MainWindow = this.splash = new AppSplashScreen();
            this.splash.Show();

            try {
                await this.InitApp();
            }
            catch (Exception ex) {
                if (IoC.MessageDialogs != null) {
                    await IoC.MessageDialogs.ShowMessageExAsync("App init failed", "Failed to start FramePFX", ex.GetToString());
                }
                else {
                    MessageBox.Show("Failed to start FramePFX:\n\n" + ex, "Fatal App init failure");
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
                // new EditorWindow2() {
                //     DataContext = window.Editor
                // }.Show();
            }, DispatcherPriority.Loaded);
#endif
        }

        public async Task OnVideoEditorLoaded(VideoEditorViewModel editor) {
#if !DEBUG
            await editor.SetProject(new ProjectViewModel(CreateDebugProject()));
#else
            await editor.SetProject(new ProjectViewModel(CreateDemoProject()));
#endif

            await ResourceCheckerViewModel.LoadProjectResources(editor.ActiveProject, true);
            ((EditorMainWindow)this.MainWindow)?.VPViewBox.FitContentToCenter();
            editor.ActiveProject.AutomationEngine.UpdateAndRefresh(true);
            await editor.View.Render();

            // new TestControlPreview().Show();
        }

        protected override void OnExit(ExitEventArgs e) {
            base.OnExit(e);
        }

        public static Project CreateDemoProject() {
            // Demo project -- projects can be created as entirely models
            Project project = new Project();
            project.Settings.Resolution = new Resolution(1920, 1080);

            ResourceManager manager = project.ResourceManager;
            ulong id_r = manager.RegisterEntry(manager.RootGroup.AddItemAndRet(new ResourceColour(220, 25, 25) { DisplayName = "colour_red" }));
            ulong id_g = manager.RegisterEntry(manager.RootGroup.AddItemAndRet(new ResourceColour(25, 220, 25) { DisplayName = "colour_green" }));
            ulong id_b = manager.RegisterEntry(manager.RootGroup.AddItemAndRet(new ResourceColour(25, 25, 220) { DisplayName = "colour_blue" }));

            ResourceGroup group = new ResourceGroup("Extra Colours");
            manager.RootGroup.AddItem(group);
            ulong id_w = manager.RegisterEntry(group.AddItemAndRet(new ResourceColour(220, 220, 220) { DisplayName = "white colour" }));
            ulong id_d = manager.RegisterEntry(group.AddItemAndRet(new ResourceColour(50, 100, 220) { DisplayName = "idek" }));

            {
                VideoTrack track1 = new VideoTrack() {
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
                VideoTrack track2 = new VideoTrack() {
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
                VideoTrack track1 = new VideoTrack() {
                    DisplayName = "Empty track"
                };
                project.Timeline.AddTrack(track1);
            }

            return project;
        }
    }
}
