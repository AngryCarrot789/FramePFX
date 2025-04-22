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

using PFXToolKitUI.Configurations;
using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.PropertyEditing.DataTransfer;
using PFXToolKitUI.Services.Messaging;
using PFXToolKitUI.Utils.Accessing;

namespace FramePFX.Plugins.AnotherTestPlugin;

public class TestPluginConfigurationPage : PropertyEditorConfigurationPage {
    public static string? GlobalCoolString { get; set; } = "Cool!!!";

    public static readonly DataParameterString CoolStringParameter =
        DataParameter.Register(
            new DataParameterString(
                typeof(TestPluginConfigurationPage),
                nameof(CoolString), GlobalCoolString,
                ValueAccessors.Reflective<string?>(typeof(TestPluginConfigurationPage), nameof(coolString))));

    private string? coolString;

    public string? CoolString {
        get => this.coolString;
        set => DataParameter.SetValueHelper(this, CoolStringParameter, ref this.coolString, value);
    }

    public TestPluginConfigurationPage() {
        this.coolString = CoolStringParameter.GetDefaultValue(this);
        CoolStringParameter.AddValueChangedHandler(this, this.OnStringChanged);
        this.PropertyEditor.Root.AddItem(new DataParameterStringPropertyEditorSlot(CoolStringParameter, typeof(TestPluginConfigurationPage), "Very Cool String") { AnticipatedLineCount = 4 });
    }

    private void OnStringChanged(DataParameter parameter, ITransferableData owner) => this.IsModified = true;
    
    protected override ValueTask OnContextCreated(ConfigurationContext context) {
        this.coolString = GlobalCoolString;
        this.PropertyEditor.Root.SetupHierarchyState([this]);
        return ValueTask.CompletedTask;
    }

    protected override ValueTask OnContextDestroyed(ConfigurationContext context) {
        this.PropertyEditor.Root.ClearHierarchy();
        return ValueTask.CompletedTask;
    }

    public override async ValueTask Apply(List<ApplyChangesFailureEntry>? errors) {
        string? oldCoolString = GlobalCoolString;
        if (oldCoolString != this.coolString) {
            GlobalCoolString = this.coolString;
            await IMessageDialogService.Instance.ShowMessage("Changed cool string", $"Cool! You changed the text from '{oldCoolString ?? ""}' to '{this.coolString ?? ""}'!");
        }
    }
}