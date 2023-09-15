﻿using System.Windows;
using FramePFX.WPF.Views;

namespace FramePFX.WPF.Editor.Project.EditorDialogs {
    /// <summary>
    /// Interaction logic for ProjectSettingsEditorWindow.xaml
    /// </summary>
    public partial class ProjectSettingsEditorWindow : BaseDialog {
        public ProjectSettingsEditorWindow() {
            this.InitializeComponent();
            this.DataContext = new ProjectSettingsEditorViewModel(this);
            this.Loaded += this.ProjectSettingsEditorWindow_Loaded;
        }

        private void ProjectSettingsEditorWindow_Loaded(object sender, RoutedEventArgs e) {
        }
    }
}