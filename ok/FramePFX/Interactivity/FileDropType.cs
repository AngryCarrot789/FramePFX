using System;

namespace FramePFX.Interactivity {
    [Flags]
    public enum FileDropType {
        None,
        Copy,
        Move,
        All = Copy | Move
    }
}