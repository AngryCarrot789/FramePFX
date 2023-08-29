using System;

namespace FramePFX.Editor.ResourceManaging {
    [Flags]
    public enum ResourceRemoveFlags {
        None,
        RemoveFromModel = 1,
        UnregisterHierarchy = 2,
        DisposeObject = 4,
    }
}