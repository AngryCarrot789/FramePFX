using System;
using FramePFX.Core.Actions;
using FramePFX.Core.Services;
using FramePFX.Core.Shortcuts.Dialogs;
using FramePFX.Core.Shortcuts.Managing;
using FramePFX.Core.Views.Dialogs.FilePicking;
using FramePFX.Core.Views.Dialogs.Message;
using FramePFX.Core.Views.Dialogs.UserInputs;

namespace FramePFX.Core {
    public static class CoreIoC {
        public static SimpleIoC Instance { get; } = new SimpleIoC();

        public static ActionManager ActionManager { get; } = new ActionManager();
        public static Action<string> OnShortcutManagedChanged { get; set; }
        public static Action<string> BroadcastShortcutActivity { get; set; }

        public static IDispatcher Dispatcher { get; set; }
        public static IClipboardService Clipboard { get; set; }
        public static IMessageDialogService MessageDialogs { get; set; }
        public static IFilePickDialogService FilePicker { get; set; }
        public static IUserInputDialogService UserInput { get; set; }
        public static IKeyboardDialogService KeyboardDialogs { get; set; }
        public static IMouseDialogService MouseDialogs { get; set; }
        public static ShortcutManager ShortcutManager { get; set; }

        public static Action<string> BroadcastShortcutChanged { get; set; }
    }
}