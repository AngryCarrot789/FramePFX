// 
// Copyright (c) 2024-2024 REghZy
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

using FramePFX.CommandSystem;
using FramePFX.Configurations;
using FramePFX.Editing.Exporting;

namespace FramePFX.Plugins.AnotherTestPlugin;

public class TestPlugin : Plugin {
    public override void OnCreated() {
        
    }

    public override void RegisterCommands(CommandManager manager) {
    }

    public override void RegisterServices() {
        
    }

    public override async Task OnApplicationLoaded() {
        // Register a test exporter
        ExporterRegistry.Instance.RegisterExporter(new ExporterKey("testplugin.TestExporter", "Test Exporter (do not use)"), new TestExporterInfo());
        
        // Register a test configuration page in the editor section
        ApplicationConfigurationManager.Instance.RootEntry.AddEntry(new ConfigurationEntry() {
            DisplayName = "Test Plugin Settings", Id = "config.testplugineditorsettings", Page = new TestPluginConfigurationPage()
        });
        
        IConfigurationDialogService e;
        IExportDialogService d;
    }
    
    public override void OnApplicationExiting() {
    }

    private class TestExporterInfo : BaseExporterInfo {
        public override BaseExportContext CreateContext(ExportSetup setup) {
            return new TestExportContext(this, setup);
        }
        
        private class TestExportContext : BaseExportContext {
            public TestExportContext(BaseExporterInfo exporter, ExportSetup setup) : base(exporter, setup) {
                
            }

            public override void Export(IExportProgress progress, CancellationToken cancellation) {
                Thread.Sleep(500);
                progress.OnFrameRendered(this.Span.Duration / 2);
                Thread.Sleep(500);
                progress.OnFrameEncoded(this.Span.Duration / 2);
                Thread.Sleep(500);
                progress.OnFrameRendered(this.Span.Duration);
                Thread.Sleep(500);
                progress.OnFrameEncoded(this.Span.Duration);
            }
        }
    }
}