﻿using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
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
using FramePFX.WPF.Utils;
using FramePFX.WPF.Views;
using SoundIOSharp;
using FramePFX.WPF.Editor.MainWindow;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.PropertyEditing;
using System.Windows.Controls;
using System.Windows.Media;
using FramePFX.App;
using FramePFX.Editor.Actions.Clips;
using FramePFX.Editor.Actions.Tracks;
using FramePFX.Editor.Timelines;
using FramePFX.Logger;
using FramePFX.RBC;
using FramePFX.ServiceManaging;
using FramePFX.TaskSystem;
using FramePFX.WPF.App;
using FontFamily = System.Windows.Media.FontFamily;
using UndoAction = FramePFX.History.Actions.UndoAction;

namespace FramePFX.WPF {
    /// <summary>
    /// The WPF application class
    /// </summary>
    public partial class AppWPF : Application {
        private AttributeProcessor processor;
        private AppSplashScreen splash;
        private readonly RateLimitedExecutor monitor;
        private DateTime lastInput;

        // Used for things
        public static List<FontFamily> FontFamilies { get; } = new List<FontFamily>();

        private readonly ApplicationModel application;

        static AppWPF() {
        }

        public struct MinMax
        {
            internal double minWidth;
            internal double maxWidth;
            internal double minHeight;
            internal double maxHeight;

            internal MinMax(double eMinWidth,double eMaxWidth,double eMinHeight,double eMaxHeight,double eWidth, double eHeight)
            {
                this.maxHeight = eMaxHeight;
                this.minHeight = eMinHeight;
                double height = eHeight;
                this.maxHeight = Math.Max(Math.Min(double.IsNaN(height) ? double.PositiveInfinity : height, this.maxHeight), this.minHeight);
                this.minHeight = Math.Max(Math.Min(this.maxHeight, double.IsNaN(height) ? 0.0 : height), this.minHeight);
                this.maxWidth = eMaxWidth;
                this.minWidth = eMinWidth;
                double width = eWidth;
                this.maxWidth = Math.Max(Math.Min(double.IsNaN(width) ? double.PositiveInfinity : width, this.maxWidth), this.minWidth);
                this.minWidth = Math.Max(Math.Min(this.maxWidth, double.IsNaN(width) ? 0.0 : width), this.minWidth);
            }
        }

        public AppWPF() {
            //MinMax minMax = new MinMax(0, double.PositiveInfinity, 0, double.PositiveInfinity, 0, 0);


            IoC.Application = this.application = new ApplicationModel(this);
            this.processor = new AttributeProcessor();
            this.application.RegisterService(IoC.Application.Dispatcher);
            AppLogger.WriteLine("Application model setup and ready");

            // fonts must be loaded here, as they are used in some of the files in AppWPF.xaml
            foreach (FontFamily fontFamily in Fonts.SystemFontFamilies) {
                FontFamilies.Add(fontFamily);
            }

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

        private async void Application_Startup(object sender, StartupEventArgs e) {
            // Dialogs may be shown, becoming the main window, possibly causing the
            // app to shutdown when the mode is OnMainWindowClose or OnLastWindowClose

            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            this.MainWindow = this.splash = new AppSplashScreen();
            this.splash.Show();

            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(int.MaxValue));
            ToolTipService.InitialShowDelayProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(400));

            try {
                AppLogger.PushHeader("FramePFX initialisation");
                await this.InitApp();
            }
            catch (Exception ex) {
                if (IoC.DialogService != null) {
                    await IoC.DialogService.ShowMessageExAsync("App init failed", "Failed to start FramePFX", ex.GetToString());
                }
                else {
                    MessageBox.Show("Failed to start FramePFX:\n\n" + ex, "Fatal App init failure");
                }

                this.Dispatcher.Invoke(() => this.Shutdown(0), DispatcherPriority.Background);
                return;
            }
            finally {
                AppLogger.PopHeader();
            }

            this.processor.Clear();
            this.processor = null;

            await this.SetActivity("Loading FramePFX main window...");
            BindingErrorListener.Listen();
            EditorMainWindow window = new EditorMainWindow();

            this.splash.Close();
            this.MainWindow = window;
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
            window.Show();
            await this.Dispatcher.Invoke(() => this.OnVideoEditorLoaded(window.Editor, e.Args), DispatcherPriority.Loaded);

            if (Debugger.IsAttached) {
                TaskManager.Instance.Run(new TaskAction(async (progress) => {
                    progress.HeaderText = "Debug Counting task";
                    progress.IsIndeterminate = false;
                    for (int i = 0; i < 5; i++) {
                        int j = i + 1;
                        progress.FooterText = $"Progress: {j * 20}";
                        progress.CompletionValue = j * 0.2d;
                        await Task.Delay(500);
                    }

                    progress.FooterText = "Completed!";
                    await IoC.Application.Dispatcher.InvokeAsync(() => {
                        window.TestBorder.Width = 20;
                        window.InvalidateArrange();
                    });
                }));
            }
        }

        public async Task InitApp() {
            await this.SetActivity("Loading services...");
            this.processor.RegisterProcessor<ServiceImplementationAttribute>((typeInfo, attribute) => {
                object instance;
                try {
                    instance = Activator.CreateInstance(typeInfo);
                }
                catch (Exception e) {
                    throw new Exception($"Failed to create implementation of {attribute.Type} as {typeInfo}", e);
                }

                this.application.RegisterService(attribute.Type, instance);
            });

            this.processor.RegisterProcessor<ActionRegistrationAttribute>((typeInfo, attribute) => {
                ExecutableAction action;
                try {
                    action = (ExecutableAction) Activator.CreateInstance(typeInfo, true);
                }
                catch (Exception e) {
                    throw new Exception($"Failed to create an instance of the registered action '{typeInfo.FullName}'", e);
                }

                if (attribute.OverrideExisting && ActionManager.Instance.Unregister(attribute.ActionId) != null) {
                    AppLogger.WriteLine("Action registration attribute overwrote action: " + attribute.ActionId);
                }

                ActionManager.Instance.Register(attribute.ActionId, action);
            });

            string[] envArgs = Environment.GetCommandLineArgs();
            if (envArgs.Length > 0 && Path.GetDirectoryName(envArgs[0]) is string dir && dir.Length > 0) {
                Directory.SetCurrentDirectory(dir);
            }

            await AppLogger.FlushEntries();
            this.processor.ScanProcess();
            this.processor.Process(typeof(ServiceImplementationAttribute));

            await this.SetActivity("Loading localization...");
            LocalizationController.SetLang(LangType.En);

            await this.SetActivity("Loading shortcuts and the action manager...");
            this.application.RegisterService(new ActionManager());

            ShortcutManager.Instance = new WPFShortcutManager();
            ShortcutManager.Instance.ShortcutModified += (sender, value) => {
                GlobalUpdateShortcutGestureConverter.BroadcastChange();
            };

            RuntimeHelpers.RunClassConstructor(typeof(UIInputManager).TypeHandle);
            InputStrokeViewModel.KeyToReadableString = KeyStrokeStringConverter.ToStringFunction;
            InputStrokeViewModel.MouseToReadableString = MouseStrokeStringConverter.ToStringFunction;

            this.processor.Process(typeof(ActionRegistrationAttribute));

            {
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
                ActionManager.Instance.Register("actions.resources.GroupSelectionIntoFolder", new GroupSelectedResourcesAction());
                ActionManager.Instance.Register("actions.editor.timeline.DeleteSelectedClips", new DeleteSelectedClips());
                ActionManager.Instance.Register("actions.editor.timeline.DeleteSelectedTracks", new DeleteSelectedTracksAction());
                ActionManager.Instance.Register("actions.editor.NewVideoTrack", new NewVideoTrackAction());
                ActionManager.Instance.Register("actions.editor.NewAudioTrack", new NewAudioTrackAction());
                ActionManager.Instance.Register("actions.editor.timeline.SliceClips", new SliceClipsAction());
            }

            AppLogger.PushHeader($"Registered {ActionManager.Instance.Count} actions", false);
            foreach (KeyValuePair<string, ExecutableAction> pair in ActionManager.Instance.Actions) {
                AppLogger.WriteLine($"{pair.Key}: {pair.Value.GetType()}");
            }

            AppLogger.PopHeader();

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
                    await IoC.DialogService.ShowMessageExAsync("Invalid keymap", "Failed to read keymap file: " + keymapFilePath, e.GetToString());
                }
            }
            else {
                await IoC.DialogService.ShowMessageAsync("No keymap available", "Keymap file does not exist: " + keymapFilePath + $".\nCurrent directory: {Directory.GetCurrentDirectory()}\nCommand line args:{string.Join("\n", Environment.GetCommandLineArgs())}");
            }

            await this.SetActivity("Loading FFmpeg...");

            try {
                ffmpeg.avdevice_register_all();
            }
            catch (Exception e) {
                await IoC.DialogService.ShowMessageAsync("FFmpeg not found", "The FFmpeg libraries (avcodec-60.dll, avfilter-9, and all other 6 dlls files) must be placed in the project's build folder, e.g. FramePFX/FramePFX.WPF/bin/Debug");
                throw new Exception("FFmpeg Unavailable. Copy FFmpeg DLLs into the same folder as the app's .exe", e);
            }

            await this.SetActivity("Performing serialisation tests...");
            AppLogger.PushHeader("Testing serialisation system");
            try {
                Clip clip = new ImageVideoClip();
                RBEDictionary dict = new RBEDictionary();
                Clip.Serialisation.Serialise(clip, dict, new SerialisationContext("9.9.9"));
                Clip.Serialisation.Deserialise(clip, dict, new SerialisationContext("9.9.9"));
            }
            catch (Exception e) {
                string msg = e.GetToString();
                AppLogger.WriteLine(msg);
                AppLogger.PopHeader();
                await IoC.DialogService.ShowMessageExAsync("Test Failed", "Serialisation Test Failed. App cannot start safely", msg);
                throw new Exception("Test failed", e);
            }
        }

        private async Task SetActivity(string activity) {
            AppLogger.WriteLine(activity);
            this.splash.CurrentActivity = activity;
            await this.Dispatcher.InvokeAsync(() => {
            }, DispatcherPriority.ApplicationIdle);
        }

        public async Task OnVideoEditorLoaded(VideoEditorViewModel editor, string[] args) {
            AppLogger.WriteLine($"FramePFX loaded. Command line args:\n{string.Join("\n", args)}");

            await editor.FileExplorer.LoadDefaultLocation();

            if (args.Length > 0 && File.Exists(args[0])) {
                await editor.OpenProjectAtAction(args[0]);
            }
            else {
                await editor.LoadProject(new ProjectViewModel(CreateDemoProject()));
                ClipViewModel demoClip = editor.ActiveProject.Timeline.Tracks[1].Clips[0];
                demoClip.IsSelected = true;
                PFXPropertyEditorRegistry.Instance.OnClipSelectionChanged(CollectionUtils.SingleItem(demoClip));
                VideoClipDataSingleEditorViewModel infoEditor = PFXPropertyEditorRegistry.Instance.ClipInfo.PropertyObjects.OfType<VideoClipDataSingleEditorViewModel>().FirstOrDefault();
                if (infoEditor != null) { // shouldn't be null... but juuuust in case something bad did happen, check anyway
                    infoEditor.IsOpacitySelected = true;
                    demoClip.Timeline.PlayHeadFrame = 323;
                }

                await ResourceCheckerViewModel.LoadProjectResources(editor.ActiveProject, true);
            }

            ((EditorMainWindow) this.MainWindow)?.ViewPortControl.VPViewBox.FitContentToCenter();
            if (editor.ActiveProject != null) {
                await editor.ActiveProject.Timeline.UpdateAndRenderTimelineToEditor();
            }
        }

        public static void RefreshControlsDictionary() {
            ResourceDictionary resource = Current.Resources.MergedDictionaries[2];
            Current.Resources.MergedDictionaries.RemoveAt(2);
            Current.Resources.MergedDictionaries.Insert(2, resource);
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

            for (;;) {
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
            for (;;) {
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
            short* buf = (short*) ptr;
            double range = (double) short.MaxValue - (double) short.MinValue;
            double val = sample * range / 2.0;
            *buf = (short) val;
        }

        private static unsafe void write_sample_s32ne(IntPtr ptr, double sample) {
            int* buf = (int*) ptr;
            double range = (double) int.MaxValue - (double) int.MinValue;
            double val = sample * range / 2.0;
            *buf = (int) val;
        }

        private static unsafe void write_sample_float32ne(IntPtr ptr, double sample) {
            float* buf = (float*) ptr;
            *buf = (float) sample;
        }

        private static unsafe void write_sample_float64ne(IntPtr ptr, double sample) {
            double* buf = (double*) ptr;
            *buf = sample;
        }

        protected override void OnExit(ExitEventArgs e) {
            base.OnExit(e);
        }

        public static Project CreateDemoProject() {
            BinaryReader reader;

            // Demo project -- projects can be created as entirely models
            Project project = new Project();
            project.Settings.Resolution = new Rect2i(1920, 1080);

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
                    FrameSpan = new FrameSpan(0, 120),
                    DisplayName = "Clip colour_red"
                };

                clip1.GetDefaultKeyFrame(ShapeSquareVideoClip.WidthKey).SetFloatValue(200);
                clip1.GetDefaultKeyFrame(ShapeSquareVideoClip.HeightKey).SetFloatValue(200);

                clip1.AddEffect(motion = new MotionEffect());
                motion.MediaPosition = new Vector2(0, 0);

                clip1.ColourKey.SetTargetResourceId(id_r);
                track.AddClip(clip1);

                ShapeSquareVideoClip clip2 = new ShapeSquareVideoClip {
                    FrameSpan = new FrameSpan(150, 30),
                    DisplayName = "Clip colour_green"
                };

                clip2.GetDefaultKeyFrame(ShapeSquareVideoClip.WidthKey).SetFloatValue(200);
                clip2.GetDefaultKeyFrame(ShapeSquareVideoClip.HeightKey).SetFloatValue(200);

                clip2.AddEffect(motion = new MotionEffect());
                motion.MediaPosition = new Vector2(200, 200);
                motion.MediaScaleOrigin = new Vector2(100, 100);

                clip2.ColourKey.SetTargetResourceId(id_g);
                track.AddClip(clip2);
            }
            {
                VideoTrack track = new VideoTrack() {
                    DisplayName = "Track 2"
                };

                project.Timeline.AddTrack(track);
                ShapeSquareVideoClip clip1 = new ShapeSquareVideoClip {
                    FrameSpan = new FrameSpan(300, 90),
                    DisplayName = "Clip colour_blue"
                };

                clip1.GetDefaultKeyFrame(ShapeSquareVideoClip.WidthKey).SetFloatValue(400);
                clip1.GetDefaultKeyFrame(ShapeSquareVideoClip.HeightKey).SetFloatValue(400);

                clip1.AddEffect(motion = new MotionEffect());
                motion.MediaPosition = new Vector2(200, 200);

                clip1.ColourKey.SetTargetResourceId(id_b);
                track.AddClip(clip1);
                ShapeSquareVideoClip clip2 = new ShapeSquareVideoClip {
                    FrameSpan = new FrameSpan(15, 130),
                    DisplayName = "Clip blueish"
                };

                clip2.GetDefaultKeyFrame(ShapeSquareVideoClip.WidthKey).SetFloatValue(100);
                clip2.GetDefaultKeyFrame(ShapeSquareVideoClip.HeightKey).SetFloatValue(1000);
                clip2.AddEffect(motion = new MotionEffect());
                motion.AutomationData[MotionEffect.MediaPositionKey].AddKeyFrame(new KeyFrameVector2(10L, Vector2.Zero));
                motion.AutomationData[MotionEffect.MediaPositionKey].AddKeyFrame(new KeyFrameVector2(75L, new Vector2(100, 200)));
                motion.AutomationData[MotionEffect.MediaPositionKey].AddKeyFrame(new KeyFrameVector2(90L, new Vector2(400, 400)));
                motion.AutomationData[MotionEffect.MediaPositionKey].AddKeyFrame(new KeyFrameVector2(115L, new Vector2(100, 700)));
                motion.AutomationData.ActiveKeyFullId = MotionEffect.MediaPositionKey.FullId;
                motion.MediaPosition = new Vector2(400, 400);
                clip2.ColourKey.SetTargetResourceId(id_d);
                track.AddClip(clip2);
            }

            project.Timeline.AddTrack(new VideoTrack() {
                DisplayName = "Empty track"
            });

            project.UpdateTimelineBackingStorage();
            return project;
        }

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            AppLogger.WriteLine("Unhandled application exception");
            AppLogger.WriteLine(e.Exception.GetToString());
            IoC.DialogService.ShowMessageExAsync("Unhandled Exception", "FramePFX has encountered an expected error... it will crash now :(", e.Exception?.GetToString());
        }
    }
}