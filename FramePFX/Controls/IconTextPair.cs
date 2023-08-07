using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace FramePFX.Controls
{
    [ContentProperty("Icon")]
    public class IconTextPair : Control
    {
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(object), typeof(IconTextPair), new PropertyMetadata(null));
        public static readonly DependencyProperty IconWidthProperty = DependencyProperty.Register("IconWidth", typeof(double), typeof(IconTextPair), new PropertyMetadata(16d));
        public static readonly DependencyProperty IconHeightProperty = DependencyProperty.Register("IconHeight", typeof(double), typeof(IconTextPair), new PropertyMetadata(16d));
        public static readonly DependencyProperty TextProperty = TextBlock.TextProperty.AddOwner(typeof(IconTextPair));

        public object Icon
        {
            get => this.GetValue(IconProperty);
            set => this.SetValue(IconProperty, value);
        }

        public double IconWidth
        {
            get => (double) this.GetValue(IconWidthProperty);
            set => this.SetValue(IconWidthProperty, value);
        }

        public double IconHeight
        {
            get => (double) this.GetValue(IconHeightProperty);
            set => this.SetValue(IconHeightProperty, value);
        }

        public string Text
        {
            get => (string) this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }

        public IconTextPair()
        {
        }
    }
}