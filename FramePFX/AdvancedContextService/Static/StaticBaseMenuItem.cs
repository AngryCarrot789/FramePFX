using System.ComponentModel;
using System.Windows.Markup;
using System.Windows.Media;

namespace FramePFX.AdvancedContextService.Static
{
    [DefaultProperty("Items")]
    [ContentProperty("Items")]
    public class StaticBaseMenuItem : StaticMenuElement
    {
        private StaticMenuItemCollection items;

        public StaticMenuItemCollection Items
        {
            get => this.items ?? (this.items = new StaticMenuItemCollection());
        }

        public ImageSource Icon { get; set; }
    }
}