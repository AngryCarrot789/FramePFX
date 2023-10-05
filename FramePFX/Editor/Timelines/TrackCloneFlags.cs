using System;

namespace FramePFX.Editor.Timelines {
    [Flags]
    public enum TrackCloneFlags {
        None = 0,
        Clips = 1,
        Effects = 2,
        AutomationData = 4,
        DefaultFlags = Clips | Effects | AutomationData
    }
}