using System.Windows;
using System.Windows.Controls;

namespace FramePFX.WPF.Utils
{
    public class ResourceDictionaryDataTemplateSelector : DataTemplateSelector
    {
        public ResourceDictionary ResourceDictionary { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item != null && this.ResourceDictionary != null)
            {
                DataTemplateKey key = new DataTemplateKey(item.GetType());
                object value;
                if (this.ResourceDictionary.Contains(key) && (value = this.ResourceDictionary[key]) is DataTemplate)
                {
                    return (DataTemplate) value;
                }
            }

            return base.SelectTemplate(item, container);
        }
    }
}