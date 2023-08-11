using System;

namespace FramePFX.Core.Interactivity {
    [Flags]
    public enum FileDropType {
        None,
        Copy,
        Move,
        All = Copy | Move
    }
}