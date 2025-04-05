using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.Editing;
using PFXToolKitUI.Configurations;
using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.PropertyEditing.DataTransfer;
using PFXToolKitUI.Utils.Accessing;
using SkiaSharp;

namespace FramePFX.Avalonia.Configs;

public class EditorWindowPropEditorConfigurationPage : PropertyEditorConfigurationPage {
    public static readonly DataParameter<SKColor> TitleBarBrushParameter =
        DataParameter.Register(
            new DataParameter<SKColor>(
                typeof(EditorWindowPropEditorConfigurationPage),
                nameof(TitleBarBrush), default(SKColor),
                ValueAccessors.Reflective<SKColor>(typeof(EditorWindowPropEditorConfigurationPage), nameof(titleBarBrush))));

    private SKColor titleBarBrush;

    public SKColor TitleBarBrush {
        get => this.titleBarBrush;
        set => DataParameter.SetValueHelper(this, TitleBarBrushParameter, ref this.titleBarBrush, value);
    }

    public EditorWindowPropEditorConfigurationPage() {
        this.titleBarBrush = TitleBarBrushParameter.GetDefaultValue(this);
        TitleBarBrushParameter.AddValueChangedHandler(this, this.Handler);

        this.PropertyEditor.Root.AddItem(new DataParameterColourPropertyEditorSlot(TitleBarBrushParameter, typeof(EditorWindowPropEditorConfigurationPage), "Titlebar Brush"));
    }

    private void Handler(DataParameter parameter, ITransferableData owner) => this.MarkModified();

    public override ValueTask OnContextCreated(ConfigurationContext context) {
        EditorConfigurationOptions options = EditorConfigurationOptions.Instance;
        this.titleBarBrush = options.TitleBarBrush;
        this.PropertyEditor.Root.SetupHierarchyState([this]);
        return ValueTask.CompletedTask;
    }

    public override ValueTask OnContextDestroyed(ConfigurationContext context) {
        this.PropertyEditor.Root.ClearHierarchy();
        return ValueTask.CompletedTask;
    }

    public override ValueTask Apply(List<ApplyChangesFailureEntry>? errors) {
        EditorConfigurationOptions options = EditorConfigurationOptions.Instance;
        options.TitleBarBrush = this.titleBarBrush;
        return ValueTask.CompletedTask;

        // await IoC.MessageService.ShowMessage("Change title", "Change window title to: " + this.TitleBar);
    }
}