using System;
using FrameControl.Core.Services;
using FrameControl.Core.Views.Dialogs.FilePicking;
using FrameControl.Core.Views.Dialogs.Message;
using FrameControl.Core.Views.Dialogs.UserInputs;

namespace FrameControl.Core {
    public static class IoC {
        public static SimpleIoC Instance { get; } = new SimpleIoC();

        public static IDispatcher Dispatcher {
            get => Instance.Provide<IDispatcher>();
            set => Instance.Register(value ?? throw new ArgumentNullException(nameof(value), "Value cannot be null"));
        }

        public static IClipboardService Clipboard {
            get => Instance.Provide<IClipboardService>();
            set => Instance.Register(value ?? throw new ArgumentNullException(nameof(value), "Value cannot be null"));
        }

        public static IMessageDialogService MessageDialogs {
            get => Instance.Provide<IMessageDialogService>();
            set => Instance.Register(value ?? throw new ArgumentNullException(nameof(value), "Value cannot be null"));
        }

        public static IFilePickDialogService FilePicker {
            get => Instance.Provide<IFilePickDialogService>();
            set => Instance.Register(value ?? throw new ArgumentNullException(nameof(value), "Value cannot be null"));
        }

        public static IUserInputDialogService UserInput {
            get => Instance.Provide<IUserInputDialogService>();
            set => Instance.Register(value ?? throw new ArgumentNullException(nameof(value), "Value cannot be null"));
        }
    }
}