using System;

namespace FramePFX.Editors.ResourceManaging {
    [Flags]
    public enum ResourceRemoveFlags {
        None,
        RemoveFromModel = 1,
        UnregisterHierarchy = 2,
        DisposeObject = 4,
    }
}