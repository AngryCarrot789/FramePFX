using System;
using FramePFX.Core;
using FramePFX.Project;

namespace FramePFX {
    public static class IoC {
        public static SimpleIoC Instance => CoreIoC.Instance;

        /// <summary>
        /// The application video editor
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public static VideoEditorViewModel VideoEditor { get; set; }

        public static ProjectViewModel ActiveProject {
            get => VideoEditor?.ActiveProject;
        }
    }
}