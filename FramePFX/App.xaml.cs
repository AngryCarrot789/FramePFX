//
// Copyright (c) 2023-2024 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

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
using FramePFX.CommandSystem;
using FramePFX.Logger;
using FramePFX.Utils;
using FramePFX.Views;
using FramePFX.Natives;

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

            string[] envArgs = Environment.GetCommandLineArgs();
            if (envArgs.Length > 0 && Path.GetDirectoryName(envArgs[0]) is string dir && dir.Length > 0) {
                Directory.SetCurrentDirectory(dir);
            }

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
            ApplicationCore.Instance.OnEditorLoaded(editor, args.Args);
        }

        protected override void OnExit(ExitEventArgs e) {
            base.OnExit(e);
            PFXNative.ShutdownLibrary();
        }

        public async Task InitWPFApp() {
            await this.splash.SetAction("Loading services...", null);
            ShortcutManager.Instance = new WPFShortcutManager();
            RuntimeHelpers.RunClassConstructor(typeof(UIInputManager).TypeHandle);

            // This is where services are registered
            ApplicationCore.InternalSetupNewInstance(this.splash);
            // Most if not all services are available below here

            await this.splash.SetAction("Loading PFXCE native library...", null);
            try {
                PFXNative.InitialiseLibrary();
            }
            catch (Exception e) {
                IoC.MessageService.ShowMessage(
                    "Native Library Failure", 
                    "Error loading native engine library. Be sure to built the C++ project. If it built correctly, then one of its" +
                    "library DLL dependencies may be missing. Make sure the FFmpeg and PortAudio DLLs are available (e.g. in the bin folder)." +
                    "\n\nError:\n" + e.GetToString());
                this.Dispatcher.InvokeShutdown();
                return;
            }

            await AppLogger.Instance.FlushEntries();
            await this.splash.SetAction("Loading shortcuts and commands...", null);

            ApplicationCore.Instance.RegisterActions(CommandManager.Instance);

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

            string ffmpegFolderPath = Path.Combine(Path.GetFullPath("."), "\\libraries\\ffmpeg\\bin");
            if (!Directory.Exists(ffmpegFolderPath)) {
                ffmpegFolderPath = Path.GetFullPath("..\\..\\..\\..\\libraries\\ffmpeg\\bin\\");
            }

            if (!Directory.Exists(ffmpegFolderPath)) {
                IoC.MessageService.ShowMessage("FFmpeg not found", "Could not find the FFmpeg folder. Make sure 'ffmpeg' (containing bin, include, lib, etc.) exists in either the solution directory or the same folder as the .exe\n\nThe editor may crash now...");
            }
            else {
                ffmpeg.RootPath = ffmpegFolderPath;

                try {
                    // ffmpeg.RootPath = ""
                    ffmpeg.avdevice_register_all();
                }
                catch (Exception e) {
                    IoC.MessageService.ShowMessage("FFmpeg registration failed", "Failed to register all FFmpeg devices", e.GetToString());
                }
            }
        }
    }
}
