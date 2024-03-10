//
// Copyright (c) 2023-2024 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.Windows;
using System.Windows.Input;
using FramePFX.Views;

namespace FramePFX.Editors.ProjectProps
{
    /// <summary>
    /// Interaction logic for ProjectSettingsWindow.xaml
    /// </summary>
    public partial class ProjectPropertiesDialog : WindowEx
    {
        public static readonly DependencyProperty SettingsProperty = DependencyProperty.Register("Settings", typeof(ProjectSettings), typeof(ProjectPropertiesDialog), new PropertyMetadata(null, (d, e) => ((ProjectPropertiesDialog) d).OnSettingsPropertyChanged((ProjectSettings) e.OldValue, (ProjectSettings) e.NewValue)));

        public ProjectSettings Settings
        {
            get => (ProjectSettings) this.GetValue(SettingsProperty);
            set => this.SetValue(SettingsProperty, value);
        }

        public ProjectPropertiesDialog()
        {
            this.InitializeComponent();

            this.PART_WidthDragger.ValueChanged += (s, e) => this.Settings.Resolution = this.Settings.Resolution.WithX((int) Math.Round(e.NewValue));
            this.PART_HeightDragger.ValueChanged += (s, e) => this.Settings.Resolution = this.Settings.Resolution.WithY((int) Math.Round(e.NewValue));
            this.PART_FrameRateDragger.ValueChanged += (s, e) => this.Settings.FrameRate = Rational.FromDouble(e.NewValue);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (!e.Handled && (e.Key == Key.Enter || e.Key == Key.Escape))
            {
                this.DialogResult = e.Key == Key.Enter;
                this.Close();
            }
        }

        /// <summary>
        /// Shows a new dialog, and if the user wants to save the details, applies our details to the project's settings
        /// </summary>
        /// <param name="project"></param>
        /// <returns>True if the user clicked OK, false if they cancelled the operation</returns>
        public static bool ShowNewDialog(Project project)
        {
            if (project.Editor != null)
            {
                if (project.Editor.Playback.PlayState == PlayState.Play)
                {
                    project.Editor.Playback.Pause();
                }
            }

            ProjectPropertiesDialog dialog = new ProjectPropertiesDialog();
            ProjectSettings cloned = new ProjectSettings(project);
            project.Settings.WriteInto(cloned);
            dialog.Settings = cloned;

            if (dialog.ShowDialog() == true)
            {
                dialog.Settings.WriteInto(project.Settings);
                return true;
            }

            return false;
        }

        private void OnSettingsPropertyChanged(ProjectSettings oldSettings, ProjectSettings newSettings)
        {
            // Not attaching any event handlers as the assumption is that the settings
            // don't change externally while the dialog is open... hopefully

            if (newSettings != null)
            {
                this.PART_WidthDragger.Value = newSettings.Width;
                this.PART_HeightDragger.Value = newSettings.Height;
                this.PART_FrameRateDragger.Value = newSettings.FrameRate.AsDouble;
            }
        }

        private void ApplyAndCloseClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void CancelAndCloseClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}