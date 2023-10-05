namespace FramePFX.Shortcuts.Events {
    public delegate void ShortcutModifiedEventHandler<in T>(T sender, IShortcut oldValue);
}