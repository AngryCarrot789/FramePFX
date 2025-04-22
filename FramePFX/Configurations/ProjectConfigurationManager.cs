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

using Fractions;
using FramePFX.Editing;
using FramePFX.Editing.UI;
using PFXToolKitUI.Configurations;
using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.Interactivity.Formatting;
using PFXToolKitUI.PropertyEditing.DataTransfer;
using PFXToolKitUI.Utils;
using PFXToolKitUI.Utils.Accessing;
using PFXToolKitUI.Utils.Destroying;
using SkiaSharp;

namespace FramePFX.Configurations;

public delegate void SetupProjectConfigurationEventHandler(ProjectConfigurationManager configuration);

/// <summary>
/// The configuration manager for a FramePFX project
/// </summary>
public class ProjectConfigurationManager : ConfigurationManager, IDestroy {
    public Project Project { get; }

    public IVideoEditorWindow VideoEditor { get; }

    /// <summary>
    /// A global event fired whenever an instance of <see cref="ProjectConfigurationManager"/> is
    /// created for usage in the project's settings dialog.
    /// Since this is static, care must be taken to remove handlers when no longer needed
    /// </summary>
    public static event SetupProjectConfigurationEventHandler? SetupProjectConfiguration;

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
        if (!project.ServiceManager.TryGetService(out ProjectConfigurationManager? manager)) {
            project.ServiceManager.RegisterConstant(manager = new ProjectConfigurationManager(project, videoEditor));
            SetupProjectConfiguration?.Invoke(manager);
        }

        return manager;
    }
}

public class ProjectVideoPropertyEditorConfigurationPage : PropertyEditorConfigurationPage {
    public static readonly DataParameterNumber<long> WidthParameter =
        DataParameter.Register(
            new DataParameterNumber<long>(
                typeof(ProjectVideoPropertyEditorConfigurationPage),
                nameof(Width), 500, 128, int.MaxValue,
                ValueAccessors.Reflective<long>(typeof(ProjectVideoPropertyEditorConfigurationPage), nameof(width))));

    public static readonly DataParameterNumber<long> HeightParameter =
        DataParameter.Register(
            new DataParameterNumber<long>(
                typeof(ProjectVideoPropertyEditorConfigurationPage),
                nameof(Height), 500, 128, int.MaxValue,
                ValueAccessors.Reflective<long>(typeof(ProjectVideoPropertyEditorConfigurationPage), nameof(height))));

    public static readonly DataParameterNumber<double> FrameRateParameter =
        DataParameter.Register(
            new DataParameterNumber<double>(
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

    public ProjectVideoPropertyEditorConfigurationPage(ProjectConfigurationManager manager) {
        this.manager = manager;
        this.width = WidthParameter.GetDefaultValue(this);
        this.height = HeightParameter.GetDefaultValue(this);
        this.frameRate = FrameRateParameter.GetDefaultValue(this);

        this.PropertyEditor.Root.AddItem(new DataParameterNumberPropertyEditorSlot<long>(WidthParameter, WidthParameter.OwnerType, "Width", DragStepProfile.Pixels) { ValueFormatter = SuffixValueFormatter.StandardPixels });
        this.PropertyEditor.Root.AddItem(new DataParameterNumberPropertyEditorSlot<long>(HeightParameter, HeightParameter.OwnerType, "Height", DragStepProfile.Pixels) { ValueFormatter = SuffixValueFormatter.StandardPixels });
        this.PropertyEditor.Root.AddItem(new DataParameterNumberPropertyEditorSlot<double>(FrameRateParameter, FrameRateParameter.OwnerType, "Frame Rate", DragStepProfile.FramesPerSeconds));
    }

    static ProjectVideoPropertyEditorConfigurationPage() {
        DataParameter.AddMultipleHandlers(MarkModified, WidthParameter, HeightParameter, FrameRateParameter);
    }

    private static void MarkModified(DataParameter parameter, ITransferableData owner) {
        ((ProjectVideoPropertyEditorConfigurationPage) owner).IsModified = true;
    }

    protected override ValueTask OnContextCreated(ConfigurationContext context) {
        ProjectSettings settings = this.manager.Project.Settings;
        this.width = settings.Width;
        this.height = settings.Height;
        this.frameRate = settings.FrameRateDouble;
        this.PropertyEditor.Root.SetupHierarchyState([this]);
        return ValueTask.CompletedTask;
    }

    protected override ValueTask OnContextDestroyed(ConfigurationContext context) {
        this.PropertyEditor.Root.ClearHierarchy();
        return ValueTask.CompletedTask;
    }

    public override ValueTask Apply(List<ApplyChangesFailureEntry>? errors) {
        ProjectSettings settings = this.manager.Project.Settings;
        settings.Resolution = new SKSizeI((int) this.Width, (int) this.Height);
        if (DoubleUtils.IsValid(this.FrameRate)) {
            settings.FrameRate = Fraction.FromDouble(this.FrameRate);
        }

        this.manager.VideoEditor.CenterViewPort();
        return ValueTask.CompletedTask;
    }
}