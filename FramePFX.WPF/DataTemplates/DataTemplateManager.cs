using System.Windows;

namespace FramePFX.WPF.DataTemplates {
    /// <summary>
    /// A class that helps manage a mapping between data type and data templated
    /// </summary>
    public class DataTemplateManager {
        public ResourceDictionary ResourceDictionary { get; }

        public DataTemplateManager() {
            this.ResourceDictionary = new ResourceDictionary();
        }

        public void Register<T>(DataTemplate template) {
            ResourceDictionary dictionary = new ResourceDictionary();
            dictionary[new DataTemplateKey(typeof(T))] = template;
            this.ResourceDictionary.MergedDictionaries.Add(dictionary);
        }
    }
}