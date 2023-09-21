using System;
using FramePFX.ServiceManaging;
using FramePFX.Shortcuts.Dialogs;
using FramePFX.Views.Dialogs.FilePicking;
using FramePFX.Views.Dialogs.Message;
using FramePFX.Views.Dialogs.Progression;
using FramePFX.Views.Dialogs.UserInputs;

namespace FramePFX {
    /// <summary>
    /// A class that
    /// </summary>
    public static class Services {
        public static ServiceManager ServiceManager { get; } = new ServiceManager();

        public static IShortcutManagerDialogService ShortcutManagerDialog => GetService<IShortcutManagerDialogService>();
        public static Action<string> OnShortcutModified { get; set; }
        public static Action<string> BroadcastShortcutActivity { get; set; }

        /// <summary>
        /// An interface which wraps the application
        /// </summary>
        public static IApplication Application { get; set; }

        public static IClipboardService Clipboard => GetService<IClipboardService>();
        public static IMessageDialogService DialogService => GetService<IMessageDialogService>();
        public static IProgressionDialogService ProgressionDialogs => GetService<IProgressionDialogService>();
        public static IFilePickDialogService FilePicker => GetService<IFilePickDialogService>();
        public static IUserInputDialogService UserInput => GetService<IUserInputDialogService>();
        public static IExplorerService ExplorerService => GetService<IExplorerService>();
        public static IKeyboardDialogService KeyboardDialogs => GetService<IKeyboardDialogService>();
        public static IMouseDialogService MouseDialogs => GetService<IMouseDialogService>();

        public static ITranslator Translator => GetService<ITranslator>();

        public static T GetService<T>() => ServiceManager.GetService<T>();
    }
}