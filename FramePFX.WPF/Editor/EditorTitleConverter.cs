using System;
using System.Globalization;
using System.Threading;
using System.Windows.Data;
using FramePFX.Editor.ViewModels;

namespace FramePFX.WPF.Editor {
    public class EditorTitleConverter : IMultiValueConverter {
        public string DefaultTitle { get; set; } = "FramePFX";

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values[0] is ProjectViewModel project) {
                return Modifiable(this.DefaultTitle + $" | {project.ProjectName} [{project.ProjectFilePath}]", project.HasUnsavedChanges);
            }
            else {
                return this.DefaultTitle;
            }
        }

        private static string Modifiable(string text, bool isModified) {
            return isModified ? (text + "*") : text;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            Lazy<string> lazyName = new Lazy<string>(() => "lol!", LazyThreadSafetyMode.ExecutionAndPublication);
            Console.WriteLine(lazyName.Value);
            throw new NotImplementedException();
        }
    }
}