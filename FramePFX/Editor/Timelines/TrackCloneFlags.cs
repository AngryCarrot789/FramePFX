using System;

namespace FramePFX.Editor.Timelines {
    [Flags]
    public enum TrackCloneFlags {
        None = 0,
        CloneClips = 1,
        CloneEffects = 2,
        AutomationData = 4,
        DefaultFlags = CloneClips | CloneEffects | AutomationData
    }
}