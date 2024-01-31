using FramePFX.Editors.Views;
using FramePFX.Editors;
using FramePFX.Shortcuts.Managing;
using FramePFX.Shortcuts.WPF;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using FFmpeg.AutoGen;
using FramePFX.Actions;
using FramePFX.Editors.Actions;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.ResourceManaging.Actions;
using FramePFX.Editors.ResourceManaging.Autoloading;
using FramePFX.Editors.ResourceManaging.Autoloading.Controls;
using FramePFX.Editors.ResourceManaging.Resources;
using FramePFX.Utils;

namespace FramePFX {
    public partial class App : Application {
        private void RegisterActions(ActionManager manager) {
            // timelines, tracks and clips
            manager.Register("actions.timeline.NewVideoTrack", new NewVideoTrackAction());
            manager.Register("actions.timeline.MoveTrackUpAction", new MoveTrackUpAction());
            manager.Register("actions.timeline.MoveTrackDownAction", new MoveTrackDownAction());
            manager.Register("actions.timeline.MoveTrackToTopAction", new MoveTrackToTopAction());
            manager.Register("actions.timeline.MoveTrackToBottomAction", new MoveTrackToBottomAction());
            manager.Register("actions.timeline.ToggleTrackAutomationAction", new ToggleTrackAutomationAction());
            manager.Register("actions.timeline.ToggleClipAutomationAction", new ToggleClipAutomationAction());
            manager.Register("actions.timeline.TogglePlayAction", new TogglePlayAction());
            manager.Register("actions.timeline.SliceClipsAction", new SliceClipsAction());
            manager.Register("actions.timeline.DeleteSelectedClips", new DeleteClipsAction());
            manager.Register("actions.timeline.DeleteSelectedTracks", new DeleteTracksAction());
            manager.Register("actions.timeline.SelectAllClipsInTimelineAction", new SelectAllClipsInTimelineAction());
            manager.Register("actions.timeline.SelectAllClipsInTrackAction", new SelectAllClipsInTrackAction());
            manager.Register("actions.timeline.SelectAllTracksAction", new SelectAllTracksAction());

            // resources
            manager.Register("actions.resources.EnableResourcesAction", new EnableResourcesAction());
            manager.Register("actions.resources.DeleteResourcesAction", new DeleteResourcesAction());
            manager.Register("actions.resources.DisableResourcesAction", new DisableResourcesAction());
        }

        private void App_OnStartup(object sender, StartupEventArgs args) {
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

                string keymapFilePath = Path.GetFullPath(@"Keymap.xml");
                if (File.Exists(keymapFilePath)) {
                    try {
                        using (FileStream stream = File.OpenRead(keymapFilePath)) {
                            WPFShortcutManager.WPFInstance.DeserialiseRoot(stream);
                        }
                    }
                    catch (Exception ex) {
                        MessageBox.Show("Failed to read keymap file" + keymapFilePath + ":" + ex.GetToString());
                        // AppLogger.Instance.WriteLine("Failed to read keymap file" + keymapFilePath + ":" + ex.GetToString());
                        // await IoC.DialogService.ShowMessageExAsync("Invalid keymap", "Failed to read keymap file: " + keymapFilePath, e.GetToString());
                    }
                }
                else {
                    MessageBox.Show("Keymap file does not exist at " + keymapFilePath);
                    // AppLogger.Instance.WriteLine("Keymap file does not exist at " + keymapFilePath);
                    // await IoC.DialogService.ShowMessageAsync("No keymap available", "Keymap file does not exist: " + keymapFilePath + $".\nCurrent directory: {Directory.GetCurrentDirectory()}\nCommand line args:{string.Join("\n", Environment.GetCommandLineArgs())}");
                }
            }

            this.RegisterActions(ActionManager.Instance);

            // Editor init
            VideoEditor.Instance.LoadDefaultProject();

            EditorWindow window = new EditorWindow();
            window.Show();

            this.Dispatcher.InvokeAsync(() => {
                window.Editor = VideoEditor.Instance;

                // ResourceManager manager = VideoEditor.Instance.Project.ResourceManager;
                // ResourceImage img1 = new ResourceImage() { FilePath = "path 1"};
                // ResourceImage img2 = new ResourceImage() { FilePath = "path 2"};
                // manager.RootContainer.AddItem(img1);
                // manager.RootContainer.AddItem(img2);
                // 
                // ResourceLoader loader = new ResourceLoader();
                // img1.Enable(loader);
                // img2.Enable(loader);
                // if (!ResourceLoaderDialog.TryLoadResources(loader)) {
                //     return;
                // }

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
