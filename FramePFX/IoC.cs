using System;
using FramePFX.Core;

namespace FramePFX {
    public static class IoC {
        public static SimpleIoC Instance => CoreIoC.Instance;

        /// <summary>
        /// The application video editor
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public static VideoEditor VideoEditor { get; set; }

        public static Project.EditorProject ActiveProject {
            get => VideoEditor?.ActiveProject;
        }
    }
}