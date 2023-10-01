using System;
using System.Linq;

namespace FramePFX.Shortcuts.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ShortcutTargetAttribute : Attribute
    {
        public string[] ShortcutPaths { get; }

        public ShortcutTargetAttribute(string shortcutPath)
        {
            this.ShortcutPaths = string.IsNullOrWhiteSpace(shortcutPath) ? null : new string[] {shortcutPath};
        }

        public ShortcutTargetAttribute(params string[] shortcutPaths)
        {
            shortcutPaths = shortcutPaths.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            this.ShortcutPaths = shortcutPaths.Length < 1 ? null : shortcutPaths;
        }
    }
}