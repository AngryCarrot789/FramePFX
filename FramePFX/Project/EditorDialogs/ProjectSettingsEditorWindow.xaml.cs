using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using FramePFX.Views;

namespace FramePFX.Project.EditorDialogs {
    /// <summary>
    /// Interaction logic for ProjectSettingsEditorWindow.xaml
    /// </summary>
    public partial class ProjectSettingsEditorWindow : WindowEx {
        public ProjectSettingsEditorWindow() {
            InitializeComponent();
            this.DataContext = new ProjectEditorViewModel();
        }
    }
}
