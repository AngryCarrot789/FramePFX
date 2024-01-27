using FramePFX.Actions;
using FramePFX.Editors.Timelines.Tracks;
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
using FramePFX.Editors.Timelines;

namespace FramePFX {
    public partial class App : Application {
        public class NewVideoTrackAction : AnAction {
            public override bool CanExecute(AnActionEventArgs e) {
                return true;
            }

            public override Task ExecuteAsync(AnActionEventArgs e) {
                if (!e.DataContext.TryGetContext(out Timeline timeline))
                    return Task.CompletedTask;

                timeline.AddTrack(new VideoTrack() { DisplayName = "New Video Track" });
                timeline.Tracks[timeline.Tracks.Count - 1].InvalidateRender();
                return Task.CompletedTask;
            }
        }

        private void App_OnStartup(object sender, StartupEventArgs e) {
            // Pre init stuff
            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(int.MaxValue));
            ToolTipService.InitialShowDelayProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(400));

            // App Init

            {
                string[] envArgs = Environment.GetCommandLineArgs();
                if (envArgs.Length > 0 && Path.GetDirectoryName(envArgs[0]) is string dir && dir.Length > 0) {
                    Directory.SetCurrentDirectory(dir);
                }

                ShortcutManager.Instance = new WPFShortcutManager();
                RuntimeHelpers.RunClassConstructor(typeof(UIInputManager).TypeHandle);

                ActionManager.Instance.Register("actions.timeline.NewVideoTrack", new NewVideoTrackAction());

                /*
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
                 */
            }

            // Editor init
            VideoEditor editor = new VideoEditor();
            editor.LoadDefaultProject();

            EditorWindow window = new EditorWindow();
            window.Show();

            this.Dispatcher.InvokeAsync(() => {
                window.Editor = editor;
                // Timeline timeline = editor.CurrentProject.MainTimeline;
                // Task.Run(async () => {
                //     for (int i = timeline.Tracks.Count - 1; i >= 0; i--) {
                //         Track track = timeline.Tracks[i];
                //         for (int j = track.Clips.Count - 1; j >= 0; j--) {
                //             await this.Dispatcher.InvokeAsync(() => track.RemoveClipAt(j));
                //             await Task.Delay(100);
                //         }
                //         await this.Dispatcher.InvokeAsync(() => timeline.RemoveTrackAt(i));
                //         await Task.Delay(100);
                //     }
                //     this.Dispatcher.Invoke(() => timeline.PlayHeadPosition = 250);
                //     await Task.Delay(500);
                //     for (int i = 0; i < 6; i++) {
                //         VideoTrack track = new VideoTrack();
                //         await this.Dispatcher.InvokeAsync(() => timeline.AddTrack(track));
                //         await Task.Delay(50);
                //         for (int j = 0; j < 40; j++) {
                //             await this.Dispatcher.InvokeAsync(() => track.AddClip(new VideoClip() {FrameSpan = new FrameSpan(j * 20L, 16), DisplayName = j.ToString()}));
                //             await Task.Delay(50);
                //         }
                //     }
                // });
            });
        }
    }
}
