using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using FramePFX.Actions;
using FramePFX.Automation.Keyframe;
using FramePFX.Editor;
using FramePFX.Editor.Actions;
using FramePFX.Editor.PropertyEditors.Clips;
using FramePFX.Editor.ResourceChecker;
using FramePFX.Editor.ResourceManaging;
using FramePFX.Editor.ResourceManaging.Actions;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.Timelines.Effects.Video;
using FramePFX.Editor.Timelines.Tracks;
using FramePFX.Editor.Timelines.VideoClips;
using FramePFX.Editor.ViewModels;
using FramePFX.History.Actions;
using FramePFX.Shortcuts.Managing;
using FramePFX.Shortcuts.ViewModels;
using FramePFX.Utils;
using FramePFX.WPF.Resources.I18N;
using FramePFX.WPF.Shortcuts;
using FramePFX.WPF.Shortcuts.Converters;
using FramePFX.WPF.Themes;
using FramePFX.WPF.Utils;
using FramePFX.WPF.Views;
using SoundIOSharp;
using FramePFX.WPF.Editor.MainWindow;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.PropertyEditing;
using System.Windows.Controls;
using UndoAction = FramePFX.History.Actions.UndoAction;

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
            // SortedList<long, string> aaaa = null;
            // Dictionary<long, string> aa = null;
            // aa[233] = ";";
            // aaaa[23] = "";
            // aaaa.Remove(23);

            // ICollection<FontFamily> fonts = Fonts.GetFontFamilies(new Uri("pack://application:,,,/Resources/Fonts/Oxanium/#"));
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
            ActionManager.Instance.Register("actions.general.RenameItem", new RenameAction());
            ActionManager.Instance.Register("actions.project.Open", new OpenProjectAction());
            ActionManager.Instance.Register("actions.project.Save", new SaveProjectAction());
            ActionManager.Instance.Register("actions.project.SaveAs", new SaveProjectAsAction());
            ActionManager.Instance.Register("actions.project.history.Undo", new UndoAction());
            ActionManager.Instance.Register("actions.project.history.Redo", new RedoAction());
            ActionManager.Instance.Register("actions.automation.AddKeyFrame", new AddKeyFrameAction());
            ActionManager.Instance.Register("actions.editor.timeline.TogglePlayPause", new TogglePlayPauseAction());
            ActionManager.Instance.Register("actions.editor.timeline.PlayAtLastStopFrame", new PlayAtLastFrameAction());
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

            Services.Application = new ApplicationDelegate(this);
            Services.ServiceManager.Register(Services.Application);
            List<(TypeInfo, ServiceImplementationAttribute)> list_serviceAttributes = new List<(TypeInfo, ServiceImplementationAttribute)>();
            List<(TypeInfo, ActionRegistrationAttribute)> list_actionAttributes = new List<(TypeInfo, ActionRegistrationAttribute)>();

            // Process all attributes in a single scan, instead of multiple scans for services, actions, etc
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (TypeInfo typeInfo in assembly.DefinedTypes) {
                    ServiceImplementationAttribute serviceAttribute = typeInfo.GetCustomAttribute<ServiceImplementationAttribute>();
                    if (serviceAttribute?.Type != null) {
                        list_serviceAttributes.Add((typeInfo, serviceAttribute));
                    }

                    ActionRegistrationAttribute actionAttribute = typeInfo.GetCustomAttribute<ActionRegistrationAttribute>();
                    if (actionAttribute != null) {
                        list_actionAttributes.Add((typeInfo, actionAttribute));
                    }
                }
            }

            foreach ((TypeInfo, ServiceImplementationAttribute) tuple in list_serviceAttributes) {
                object instance;
                try {
                    instance = Activator.CreateInstance(tuple.Item1);
                }
                catch (Exception e) {
                    throw new Exception($"Failed to create implementation of {tuple.Item2.Type} as {tuple.Item1}", e);
                }

                Services.ServiceManager.Register(tuple.Item2.Type, instance);
            }

            await this.SetActivity("Loading localization...");
            LocalizationController.SetLang(LangType.En);

            await this.SetActivity("Loading shortcuts and the action manager...");
            ShortcutManager.Instance = new WPFShortcutManager();
            ShortcutManager.Instance.ShortcutModified += (sender, value) => {
                GlobalUpdateShortcutGestureConverter.BroadcastChange();
            };

            RuntimeHelpers.RunClassConstructor(typeof(UIInputManager).TypeHandle);

            ActionManager.Instance = new ActionManager();
            InputStrokeViewModel.KeyToReadableString = KeyStrokeStringConverter.ToStringFunction;
            InputStrokeViewModel.MouseToReadableString = MouseStrokeStringConverter.ToStringFunction;

            foreach ((TypeInfo type, ActionRegistrationAttribute attribute) in list_actionAttributes.OrderBy(x => x.Item2.RegistrationOrder)) {
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

            // TODO: user modifiable keymap, and also save it to user documents
            // also, use version attribute to check out of date keymap, and offer to
            // overwrite while backing up old file... or just try to convert file

            await this.SetActivity("Loading keymap...");
            string keymapFilePath = Path.GetFullPath(@"Keymap.xml");
            if (File.Exists(keymapFilePath)) {
                try {
                    using (FileStream stream = File.OpenRead(keymapFilePath)) {
                        WPFShortcutManager.WPFInstance.DeserialiseRoot(stream);
                    }
                }
                catch (Exception e) {
                    await Services.DialogService.ShowMessageExAsync("Invalid keymap", "Failed to read keymap file: " + keymapFilePath, e.GetToString());
                }
            }
            else {
                await Services.DialogService.ShowMessageAsync("No keymap available", "Keymap file does not exist: " + keymapFilePath + $".\nCurrent directory: {Directory.GetCurrentDirectory()}\nCommand line args:{string.Join("\n", Environment.GetCommandLineArgs())}");
            }

            await this.SetActivity("Loading FFmpeg...");
            try {
                ffmpeg.avdevice_register_all();
            }
            catch (Exception e) {
                await Services.DialogService.ShowMessageAsync("FFmpeg not found", "The FFmpeg libraries (avcodec-60.dll, avfilter-9, and all other 6 dlls files) must be placed in the project's build folder, e.g. FramePFX/FramePFX.WPF/bin/Debug");
                throw new Exception("FFmpeg Unavailable. Copy FFmpeg DLLs into the same folder as the app's .exe", e);
            }
        }

        private async void Application_Startup(object sender, StartupEventArgs e) {
            // Dialogs may be shown, becoming the main window, possibly causing the
            // app to shutdown when the mode is OnMainWindowClose or OnLastWindowClose

            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            this.MainWindow = this.splash = new AppSplashScreen();
            this.splash.Show();

            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(int.MaxValue));
            ToolTipService.InitialShowDelayProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(400));

            try {
                await this.InitApp();
            }
            catch (Exception ex) {
                if (Services.DialogService != null) {
                    await Services.DialogService.ShowMessageExAsync("App init failed", "Failed to start FramePFX", ex.GetToString());
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
            BindingErrorListener.Listen();
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
        }

        public async Task OnVideoEditorLoaded(VideoEditorViewModel editor) {
            await editor.FileExplorer.LoadDefaultLocation();

            await editor.SetProject(new ProjectViewModel(CreateDemoProject()));

            ClipViewModel demoClip = editor.ActiveProject.Timeline.Tracks[1].Clips[0];
            demoClip.IsSelected = true;
            PFXPropertyEditorRegistry.Instance.OnClipSelectionChanged(CollectionUtils.SingleItem(demoClip));
            VideoClipDataSingleEditorViewModel infoEditor = PFXPropertyEditorRegistry.Instance.ClipInfo.PropertyObjects.OfType<VideoClipDataSingleEditorViewModel>().FirstOrDefault();
            if (infoEditor != null) { // shouldn't be null... but juuuust in case something bad did happen, check anyway
                infoEditor.IsOpacitySelected = true;
                demoClip.Timeline.PlayHeadFrame = 323;
            }

            await ResourceCheckerViewModel.LoadProjectResources(editor.ActiveProject, true);
            ((EditorMainWindow)this.MainWindow)?.VPViewBox.FitContentToCenter();
            await editor.ActiveProject.Timeline.DoAutomationTickAndRenderToPlayback();
        }

        public static void PlaySineWave() {
            SoundIO api = new SoundIO();
            api.ConnectBackend(SoundIOBackend.Wasapi);
            api.FlushEvents();

            SoundIODevice device = api.GetOutputDevice(api.DefaultOutputDeviceIndex);
            if (device == null) {
                return;
            }

            if (device.ProbeError != 0) {
                return;
            }

            SoundIOOutStream outstream = device.CreateOutStream();
            outstream.WriteCallback = (min, max) => write_callback(outstream, min, max);
            outstream.UnderflowCallback = () => underflow_callback(outstream);
            outstream.SampleRate = 4096;
            if (device.SupportsFormat(SoundIODevice.Float32NE)) {
                outstream.Format = SoundIODevice.Float32NE;
                write_sample = write_sample_float32ne;
            }
            else if (device.SupportsFormat(SoundIODevice.Float64NE)) {
                outstream.Format = SoundIODevice.Float64NE;
                write_sample = write_sample_float64ne;
            }
            else if (device.SupportsFormat(SoundIODevice.S32NE)) {
                outstream.Format = SoundIODevice.S32NE;
                write_sample = write_sample_s32ne;
            }
            else if (device.SupportsFormat(SoundIODevice.S16NE)) {
                outstream.Format = SoundIODevice.S16NE;
                write_sample = write_sample_s16ne;
            }
            else {
                return;
            }

            outstream.Open();

            outstream.Start();

            for (; ; ) {
                api.FlushEvents();
            }

            outstream.Dispose();
            device.RemoveReference();
            api.Dispose();
        }

        private static Action<IntPtr, double> write_sample;
        private static double seconds_offset = 0.0;
        private static volatile bool want_pause = false;

        private static void write_callback(SoundIOOutStream outstream, int frame_count_min, int frame_count_max) {
            double dSampleRate = outstream.SampleRate;
            double dSecondsPerFrame = 1.0 / dSampleRate;

            int framesLeft = frame_count_max;
            for (; ; ) {
                int frameCount = framesLeft;
                SoundIOChannelAreas results = outstream.BeginWrite(ref frameCount);

                if (frameCount == 0)
                    break;

                SoundIOChannelLayout layout = outstream.Layout;

                double pitch = 440.0;
                double radians_per_second = pitch * 2.0 * Math.PI;
                for (int frame = 0; frame < frameCount; frame += 1) {
                    double sample = Math.Sin((seconds_offset + frame * dSecondsPerFrame) * radians_per_second);
                    for (int channel = 0; channel < layout.ChannelCount; channel += 1) {
                        SoundIOChannelArea area = results.GetArea(channel);
                        write_sample(area.Pointer, sample);
                        area.Pointer += area.Step;
                    }
                }

                seconds_offset = Math.IEEERemainder(seconds_offset + dSecondsPerFrame * frameCount, 1.0);

                outstream.EndWrite();

                framesLeft -= frameCount;
                if (framesLeft <= 0)
                    break;
            }

            outstream.Pause(want_pause);
        }

        private static int underflow_callback_count = 0;

        private static void underflow_callback(SoundIOOutStream outstream) {
            Console.Error.WriteLine("underflow {0}", underflow_callback_count++);
        }

        private static unsafe void write_sample_s16ne(IntPtr ptr, double sample) {
            short* buf = (short*)ptr;
            double range = (double)short.MaxValue - (double)short.MinValue;
            double val = sample * range / 2.0;
            *buf = (short)val;
        }

        private static unsafe void write_sample_s32ne(IntPtr ptr, double sample) {
            int* buf = (int*)ptr;
            double range = (double)int.MaxValue - (double)int.MinValue;
            double val = sample * range / 2.0;
            *buf = (int)val;
        }

        private static unsafe void write_sample_float32ne(IntPtr ptr, double sample) {
            float* buf = (float*)ptr;
            *buf = (float)sample;
        }

        private static unsafe void write_sample_float64ne(IntPtr ptr, double sample) {
            double* buf = (double*)ptr;
            *buf = sample;
        }

        protected override void OnExit(ExitEventArgs e) {
            base.OnExit(e);
        }

        public static Project CreateDemoProject() {
            BinaryReader reader;

            // Demo project -- projects can be created as entirely models
            Project project = new Project();
            project.Settings.Resolution = new Resolution(1920, 1080);

            ResourceManager manager = project.ResourceManager;
            ulong id_r = manager.RegisterEntry(manager.RootFolder.AddItemAndRet(new ResourceColour(220, 25, 25) {DisplayName = "colour_red"}));
            ulong id_g = manager.RegisterEntry(manager.RootFolder.AddItemAndRet(new ResourceColour(25, 220, 25) {DisplayName = "colour_green"}));
            ulong id_b = manager.RegisterEntry(manager.RootFolder.AddItemAndRet(new ResourceColour(25, 25, 220) {DisplayName = "colour_blue"}));

            ResourceFolder folder = new ResourceFolder("Extra Colours");
            manager.RootFolder.AddItem(folder);
            ulong id_w = manager.RegisterEntry(folder.AddItemAndRet(new ResourceColour(220, 220, 220) {DisplayName = "white colour"}));
            ulong id_d = manager.RegisterEntry(folder.AddItemAndRet(new ResourceColour(50, 100, 220) {DisplayName = "idek"}));

            MotionEffect motion;
            {
                VideoTrack track = new VideoTrack() {
                    DisplayName = "Track 1 with stuff"
                };

                project.Timeline.AddTrack(track);
                track.AutomationData[VideoTrack.OpacityKey].AddKeyFrame(new KeyFrameDouble(0, 0.3d));
                track.AutomationData[VideoTrack.OpacityKey].AddKeyFrame(new KeyFrameDouble(50, 0.5d));
                track.AutomationData[VideoTrack.OpacityKey].AddKeyFrame(new KeyFrameDouble(100, 1d));
                track.AutomationData.ActiveKeyFullId = VideoTrack.OpacityKey.FullId;

                ShapeSquareVideoClip clip1 = new ShapeSquareVideoClip {
                    Width = 200, Height = 200,
                    FrameSpan = new FrameSpan(0, 120),
                    DisplayName = "Clip colour_red"
                };

                clip1.AddEffect(motion = new MotionEffect());
                motion.MediaPosition = new Vector2(0, 0);

                clip1.ResourceHelper.SetTargetResourceId(id_r);
                track.AddClip(clip1);

                ShapeSquareVideoClip clip2 = new ShapeSquareVideoClip {
                    Width = 200, Height = 200,
                    FrameSpan = new FrameSpan(150, 30),
                    DisplayName = "Clip colour_green"
                };

                clip2.AddEffect(motion = new MotionEffect());
                motion.MediaPosition = new Vector2(200, 200);

                clip2.ResourceHelper.SetTargetResourceId(id_g);
                track.AddClip(clip2);
            }
            {
                VideoTrack track = new VideoTrack() {
                    DisplayName = "Track 2"
                };

                project.Timeline.AddTrack(track);
                ShapeSquareVideoClip clip1 = new ShapeSquareVideoClip {
                    Width = 400, Height = 400,
                    FrameSpan = new FrameSpan(300, 90),
                    DisplayName = "Clip colour_blue"
                };

                clip1.AddEffect(motion = new MotionEffect());
                motion.MediaPosition = new Vector2(200, 200);

                clip1.ResourceHelper.SetTargetResourceId(id_b);
                track.AddClip(clip1);
                ShapeSquareVideoClip clip2 = new ShapeSquareVideoClip {
                    Width = 100, Height = 1000,
                    FrameSpan = new FrameSpan(15, 130),
                    DisplayName = "Clip blueish"
                };

                clip2.AddEffect(motion = new MotionEffect());
                motion.AutomationData[MotionEffect.MediaPositionKey].AddKeyFrame(new KeyFrameVector2(10L, Vector2.Zero));
                motion.AutomationData[MotionEffect.MediaPositionKey].AddKeyFrame(new KeyFrameVector2(75L, new Vector2(100, 200)));
                motion.AutomationData[MotionEffect.MediaPositionKey].AddKeyFrame(new KeyFrameVector2(90L, new Vector2(400, 400)));
                motion.AutomationData[MotionEffect.MediaPositionKey].AddKeyFrame(new KeyFrameVector2(115L, new Vector2(100, 700)));
                motion.AutomationData.ActiveKeyFullId = MotionEffect.MediaPositionKey.FullId;
                motion.MediaPosition = new Vector2(400, 400);
                clip2.ResourceHelper.SetTargetResourceId(id_d);
                track.AddClip(clip2);
            }

            project.Timeline.AddTrack(new VideoTrack() {
                DisplayName = "Empty track"
            });

            project.UpdateAutomationBackingStorage();
            return project;
        }

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            AppLogger.WriteLine("Unhandled application exception");
            AppLogger.WriteLine(e.Exception.GetToString());
        }
    }
}