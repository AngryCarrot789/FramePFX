using System;
using System.Collections.Generic;
using System.Windows;
using FramePFX.Core.Editor.Exporting;
using FramePFX.Views;

namespace FramePFX.Editor.Exporting {
    /// <summary>
    /// Interaction logic for ExportSetupWindow.xaml
    /// </summary>
    public partial class ExportSetupWindow : BaseDialog {
        public ExportSetupWindow() {
            this.InitializeComponent();
            this.DataContextChanged += (s, e) => this.RegenerateExporterPages(((ExportSetupViewModel) e.NewValue)?.SelectedExporter);
        }

        private void RegenerateExporterPages(ExporterViewModel exporter) {
            this.ExportPropertyPageCollection.Items.Clear();
            if (exporter == null) {
                return;
            }

            Type root = typeof(ExporterViewModel);
            List<Type> types = new List<Type>();
            for (Type type = exporter.GetType(); type != null && root.IsAssignableFrom(type); type = type.BaseType) {
                types.Add(type);
            }

            if (types.Count <= 0) {
                return;
            }

            types.Reverse();
            List<FrameworkElement> controls = new List<FrameworkElement>(types.Count);
            foreach (Type type in types) {
                if (ExportPageRegistry.Instance.GenerateControl(type, exporter, out FrameworkElement control)) {
                    control.DataContext = exporter;
                    controls.Add(control);
                }
            }

            if (controls.Count < 1) {
                return;
            }

            // this.ClipPropertyPanelList.Items.Add(controls[0]);
            foreach (FrameworkElement t in controls) {
                this.ExportPropertyPageCollection.Items.Add(t);
            }
        }
    }
}