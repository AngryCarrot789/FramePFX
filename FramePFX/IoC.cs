using System;
using FramePFX.Core;
using FramePFX.Project;

namespace FramePFX {
    public class IoC {
        public static SimpleIoC Instance => CoreIoC.Instance;

        /// <summary>
        /// The application video editor
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public static VideoEditorViewModel VideoEditor {
            get => Instance.Provide<VideoEditorViewModel>();
            set => Instance.Register(value ?? throw new ArgumentNullException(nameof(value), "Value cannot be null"));
        }

        public static ProjectViewModel ActiveProject {
            get => VideoEditor?.ActiveProject;
        }
    }
}