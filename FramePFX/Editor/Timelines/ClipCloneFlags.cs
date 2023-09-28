using System;

namespace FramePFX.Editor.Timelines {
    [Flags]
    public enum ClipCloneFlags {
        None = 0,
        Effects = 2,
        AutomationData = 4,
        DefaultFlags = Effects | AutomationData
    }
}