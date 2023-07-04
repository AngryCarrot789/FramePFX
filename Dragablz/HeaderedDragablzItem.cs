using System.Windows;
using System.Windows.Controls;

namespace Dragablz {
    public class HeaderedDragablzItem : DragablzItem {
        static HeaderedDragablzItem() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HeaderedDragablzItem), new FrameworkPropertyMetadata(typeof(HeaderedDragablzItem)));
        }

        public static readonly DependencyProperty HeaderContentProperty = DependencyProperty.Register(
            "HeaderContent", typeof(object), typeof(HeaderedDragablzItem), new PropertyMetadata(default(object)));

        public object HeaderContent {
            get { return (object) this.GetValue(HeaderContentProperty); }
            set { this.SetValue(HeaderContentProperty, value); }
        }

        public static readonly DependencyProperty HeaderContentStringFormatProperty = DependencyProperty.Register(
            "HeaderContentStringFormat", typeof(string), typeof(HeaderedDragablzItem), new PropertyMetadata(default(string)));

        public string HeaderContentStringFormat {
            get { return (string) this.GetValue(HeaderContentStringFormatProperty); }
            set { this.SetValue(HeaderContentStringFormatProperty, value); }
        }

        public static readonly DependencyProperty HeaderContentTemplateProperty = DependencyProperty.Register(
            "HeaderContentTemplate", typeof(DataTemplate), typeof(HeaderedDragablzItem), new PropertyMetadata(default(DataTemplate)));

        public DataTemplate HeaderContentTemplate {
            get { return (DataTemplate) this.GetValue(HeaderContentTemplateProperty); }
            set { this.SetValue(HeaderContentTemplateProperty, value); }
        }

        public static readonly DependencyProperty HeaderContentTemplateSelectorProperty = DependencyProperty.Register(
            "HeaderContentTemplateSelector", typeof(DataTemplateSelector), typeof(HeaderedDragablzItem), new PropertyMetadata(default(DataTemplateSelector)));

        public DataTemplateSelector HeaderContentTemplateSelector {
            get { return (DataTemplateSelector) this.GetValue(HeaderContentTemplateSelectorProperty); }
            set { this.SetValue(HeaderContentTemplateSelectorProperty, value); }
        }
    }
}