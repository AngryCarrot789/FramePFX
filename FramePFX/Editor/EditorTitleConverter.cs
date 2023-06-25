using System;
using System.Globalization;
using System.Windows.Data;
using FramePFX.Core.Editor.ViewModels;

namespace FramePFX.Editor {
    public class EditorTitleConverter : IMultiValueConverter {
        public string DefaultTitle { get; set; } = "FramePFX";

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values[0] is ProjectViewModel project) {
                if (string.IsNullOrEmpty(project.ProjectDirectory)) {
                    return Modifiable(this.DefaultTitle, project.HasUnsavedChanges);
                }
                else {
                    return Modifiable(this.DefaultTitle + $" [{project.ProjectDirectory}]", project.HasUnsavedChanges);
                }
            }
            else {
                return this.DefaultTitle;
            }
        }

        private static string Modifiable(string text, bool isModified) {
            return isModified ? (text + "*") : text;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}