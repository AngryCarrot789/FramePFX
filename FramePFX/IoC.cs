using System;
using FramePFX.App;
using FramePFX.Components;
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
    public static class IoC {
        /// <summary>
        /// Gets or sets the current application. This should only be set once
        /// </summary>
        public static IApplication Application { get; set; }

        public static Action<string> OnShortcutModified { get; set; } = (x) => { };

        public static Action<string> BroadcastShortcutActivity { get; set; } = (x) => { };

        /// <summary>
        /// Gets the application clipboard service
        /// </summary>
        public static IClipboardService Clipboard => GetService<IClipboardService>();

        /// <summary>
        /// Gets the application dialog service
        /// </summary>
        public static IMessageDialogService DialogService => GetService<IMessageDialogService>();

        // TODO: remove this
        public static IProgressionDialogService ProgressionDialogs => GetService<IProgressionDialogService>();


        public static IFilePickDialogService FilePicker => GetService<IFilePickDialogService>();

        public static IUserInputDialogService UserInput => GetService<IUserInputDialogService>();

        public static IExplorerService ExplorerService => GetService<IExplorerService>();

        public static IKeyboardDialogService KeyboardDialogs => GetService<IKeyboardDialogService>();

        public static IMouseDialogService MouseDialogs => GetService<IMouseDialogService>();

        public static ITranslator Translator => GetService<ITranslator>();

        public static T GetService<T>() => Application.GetService<T>();
    }
}