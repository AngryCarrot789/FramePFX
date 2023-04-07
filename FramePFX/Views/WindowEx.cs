using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace FocusGroupHotkeys.Views {
    /// <summary>
    /// An extended window which adds support for a few of the things in the dark theme I made (e.g. Titlebar brush)
    /// </summary>
    public class WindowEx : Window {
        public static readonly DependencyProperty TitlebarBrushProperty = DependencyProperty.Register("TitlebarBrush", typeof(Brush), typeof(WindowEx), new PropertyMetadata());

        [Category("Brush")]
        public Brush TitlebarBrush {
            get => (Brush) this.GetValue(TitlebarBrushProperty);
            set => this.SetValue(TitlebarBrushProperty, value);
        }

        public WindowEx() {

        }
    }
}