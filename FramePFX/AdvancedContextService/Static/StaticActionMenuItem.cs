using System.ComponentModel;
using System.Windows.Markup;

namespace FrameControlEx.AdvancedContextService.Static {
    [DefaultProperty("Items")]
    [ContentProperty("Items")]
    public class StaticActionMenuItem : StaticBaseMenuItem {
        public string ActionID { get; set; }
    }
}