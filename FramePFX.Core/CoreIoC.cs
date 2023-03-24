using System;
using FramePFX.Core.ResourceManaging;
using FramePFX.Core.Services;
using FramePFX.Core.Timeline;
using FramePFX.Core.Views.Dialogs.FilePicking;
using FramePFX.Core.Views.Dialogs.Message;
using FramePFX.Core.Views.Dialogs.UserInputs;

namespace FramePFX.Core {
    public static class IoC {
        public static SimpleIoC Instance { get; } = new SimpleIoC();

        public static TimelineViewModel Timeline {
            get => Instance.Provide<TimelineViewModel>();
            set => Instance.Register(value ?? throw new ArgumentNullException(nameof(value), "Value cannot be null"));
        }

        public static IEditor Editor {
            get => Instance.Provide<IEditor>();
            set => Instance.Register(value ?? throw new ArgumentNullException(nameof(value), "Value cannot be null"));
        }

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