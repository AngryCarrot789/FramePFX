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

using FramePFX.DataTransfer;
using FramePFX.Editing;
using FramePFX.Editing.UI;
using FramePFX.Interactivity.Formatting;
using FramePFX.PropertyEditing.DataTransfer;
using FramePFX.Utils;
using FramePFX.Utils.Accessing;
using FramePFX.Utils.Destroying;
using SkiaSharp;

namespace FramePFX.Configurations;

/// <summary>
/// The configuration manager for a FramePFX project
/// </summary>
public class ProjectConfigurationManager : ConfigurationManager, IDestroy {
    public Project Project { get; }

    public IVideoEditorWindow VideoEditor { get; }

    public ProjectConfigurationManager(Project project, IVideoEditorWindow editorUi) {
        this.Project = project;
        this.VideoEditor = editorUi;
        this.RootEntry.AddEntry(new ConfigurationEntry() {
            DisplayName = "Video", Id = "config.project.video", Page = new ProjectVideoPropertyEditorConfigurationPage(this)
        });
    }

    public void Destroy() {
        
    }

    public static ProjectConfigurationManager GetInstance(Project project, IVideoEditorWindow videoEditor) {
        if (!project.ServiceManager.TryGetService(out ProjectConfigurationManager? manager))
            project.ServiceManager.RegisterConstant(manager = new ProjectConfigurationManager(project, videoEditor));
        return manager;
    }
}

public class ProjectVideoPropertyEditorConfigurationPage : PropertyEditorConfigurationPage, ITransferableData {
    public static readonly DataParameterLong WidthParameter =
        DataParameter.Register(
            new DataParameterLong(
                typeof(ProjectVideoPropertyEditorConfigurationPage),
                nameof(Width), 500, 128, int.MaxValue,
                ValueAccessors.Reflective<long>(typeof(ProjectVideoPropertyEditorConfigurationPage), nameof(width))));

    public static readonly DataParameterLong HeightParameter =
        DataParameter.Register(
            new DataParameterLong(
                typeof(ProjectVideoPropertyEditorConfigurationPage),
                nameof(Height), 500, 128, int.MaxValue,
                ValueAccessors.Reflective<long>(typeof(ProjectVideoPropertyEditorConfigurationPage), nameof(height))));

    public static readonly DataParameterDouble FrameRateParameter =
        DataParameter.Register(
            new DataParameterDouble(
                typeof(ProjectVideoPropertyEditorConfigurationPage),
                nameof(FrameRate), 30.0, 12.0, 240.0,
                ValueAccessors.Reflective<double>(typeof(ProjectVideoPropertyEditorConfigurationPage), nameof(frameRate))));

    private long width;
    private long height;
    private double frameRate;
    private readonly ProjectConfigurationManager manager;

    public long Width {
        get => this.width;
        set => DataParameter.SetValueHelper(this, WidthParameter, ref this.width, value);
    }

    public long Height {
        get => this.height;
        set => DataParameter.SetValueHelper(this, HeightParameter, ref this.height, value);
    }

    public double FrameRate {
        get => this.frameRate;
        set => DataParameter.SetValueHelper(this, FrameRateParameter, ref this.frameRate, value);
    }

    public TransferableData TransferableData { get; }

    public ProjectVideoPropertyEditorConfigurationPage(ProjectConfigurationManager manager) {
        this.manager = manager;
        this.TransferableData = new TransferableData(this);
        this.width = WidthParameter.GetDefaultValue(this);
        this.height = HeightParameter.GetDefaultValue(this);
        this.frameRate = FrameRateParameter.GetDefaultValue(this);

        this.PropertyEditor.Root.AddItem(new DataParameterLongPropertyEditorSlot(WidthParameter, WidthParameter.OwnerType, "Width", DragStepProfile.Pixels) { ValueFormatter = SuffixValueFormatter.StandardPixels });
        this.PropertyEditor.Root.AddItem(new DataParameterLongPropertyEditorSlot(HeightParameter, HeightParameter.OwnerType, "Height", DragStepProfile.Pixels) { ValueFormatter = SuffixValueFormatter.StandardPixels });
        this.PropertyEditor.Root.AddItem(new DataParameterDoublePropertyEditorSlot(FrameRateParameter, FrameRateParameter.OwnerType, "Frame Rate", DragStepProfile.FramesPerSeconds));
    }

    static ProjectVideoPropertyEditorConfigurationPage() {
        DataParameter.AddMultipleHandlers(MarkModified, WidthParameter, HeightParameter, FrameRateParameter);
    }

    private static void MarkModified(DataParameter parameter, ITransferableData owner) {
        ((ProjectVideoPropertyEditorConfigurationPage) owner).MarkModified();
    }

    public override async ValueTask OnContextCreated(ConfigurationContext context) {
        ProjectSettings settings = this.manager.Project.Settings;
        this.width = settings.Width;
        this.height = settings.Height;
        this.frameRate = settings.FrameRate.AsDouble;
        this.PropertyEditor.Root.SetupHierarchyState([this]);
    }

    public override ValueTask OnContextDestroyed(ConfigurationContext context) {
        this.PropertyEditor.Root.ClearHierarchy();
        return ValueTask.CompletedTask;
    }

    public override async ValueTask Apply() {
        ProjectSettings settings = this.manager.Project.Settings;
        settings.Resolution = new SKSizeI((int) this.Width, (int) this.Height);
        if (DoubleUtils.IsValid(this.FrameRate)) {
            settings.FrameRate = Rational.FromDouble(this.FrameRate);
        }

        this.manager.VideoEditor.CenterViewPort();
    }
}