using System.ComponentModel;
using System.Windows.Markup;

namespace FramePFX.WPF.AdvancedContextService.Static
{
    [DefaultProperty("Items")]
    [ContentProperty("Items")]
    public class StaticShortcutMenuItem : StaticBaseMenuItem
    {
        public string ShortcutId { get; set; }
    }
}