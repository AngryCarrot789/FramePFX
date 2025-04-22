using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.Editing;
using PFXToolKitUI.Configurations;
using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.PropertyEditing.DataTransfer;
using PFXToolKitUI.Utils.Accessing;

namespace FramePFX.Avalonia.Configs;

public class StartupPropEditorConfigurationPage : PropertyEditorConfigurationPage {
    public static readonly DataParameter<EnumStartupBehaviour> StartupBehaviourParameter = DataParameter.Register(new DataParameter<EnumStartupBehaviour>(typeof(StartupPropEditorConfigurationPage), nameof(StartupBehaviour), default, ValueAccessors.Reflective<EnumStartupBehaviour>(typeof(StartupPropEditorConfigurationPage), nameof(startupBehaviour))));
    public static readonly DataParameterString StartupThemeParameter = DataParameter.Register(new DataParameterString(typeof(StartupPropEditorConfigurationPage), nameof(StartupTheme), "Dark", ValueAccessors.Reflective<string?>(typeof(StartupPropEditorConfigurationPage), nameof(startupTheme))));

    private EnumStartupBehaviour startupBehaviour;
    private string? startupTheme;

    public EnumStartupBehaviour StartupBehaviour {
        get => this.startupBehaviour;
        set => DataParameter.SetValueHelper(this, StartupBehaviourParameter, ref this.startupBehaviour, value);
    }

    public string? StartupTheme {
        get => this.startupTheme;
        set => DataParameter.SetValueHelper(this, StartupThemeParameter, ref this.startupTheme, value);
    }

    public StartupPropEditorConfigurationPage() {
        this.startupBehaviour = StartupBehaviourParameter.GetDefaultValue(this);
        this.startupTheme = StartupThemeParameter.GetDefaultValue(this);

        this.PropertyEditor.Root.AddItem(new DataParameterStartupBehaviourPropertyEditorSlot(StartupBehaviourParameter, typeof(StartupPropEditorConfigurationPage), "Behaviour"));
        this.PropertyEditor.Root.AddItem(new DataParameterStringPropertyEditorSlot(StartupThemeParameter, typeof(StartupPropEditorConfigurationPage), "Startup Theme"));
    }

    static StartupPropEditorConfigurationPage() {
        AffectsModifiedState(StartupBehaviourParameter, StartupThemeParameter);
    }

    protected override ValueTask OnContextCreated(ConfigurationContext context) {
        StartupConfigurationOptions options = StartupConfigurationOptions.Instance;
        this.startupBehaviour = options.StartupBehaviour;
        this.startupTheme = options.StartupTheme;
        this.PropertyEditor.Root.SetupHierarchyState([this]);
        return ValueTask.CompletedTask;
    }

    protected override ValueTask OnContextDestroyed(ConfigurationContext context) {
        this.PropertyEditor.Root.ClearHierarchy();
        return ValueTask.CompletedTask;
    }

    public override ValueTask Apply(List<ApplyChangesFailureEntry>? errors) {
        StartupConfigurationOptions options = StartupConfigurationOptions.Instance;
        options.StartupBehaviour = this.startupBehaviour;
        options.StartupTheme = this.startupTheme ?? "";
        options.ApplyTheme();
        return ValueTask.CompletedTask;
        // await IoC.MessageService.ShowMessage("Change title", "Change window title to: " + this.TitleBar);
    }
}