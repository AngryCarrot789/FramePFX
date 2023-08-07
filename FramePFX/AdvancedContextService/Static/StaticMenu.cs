using System.ComponentModel;
using System.Windows.Markup;

namespace FramePFX.AdvancedContextService.Static
{
    [DefaultProperty("Items")]
    [ContentProperty("Items")]
    public class StaticMenu
    {
        private StaticMenuItemCollection items;

        public StaticMenuItemCollection Items
        {
            get => this.items ?? (this.items = new StaticMenuItemCollection());
        }
    }
}