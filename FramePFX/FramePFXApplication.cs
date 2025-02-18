using FramePFX.Configurations;
using FramePFX.Configurations.Commands;
using FramePFX.Editing;
using FramePFX.Editing.Commands;
using FramePFX.Editing.Exporting;
using FramePFX.Editing.ResourceManaging.Commands;
using FramePFX.Editing.Timelines;
using FramePFX.Editing.Timelines.Commands;
using FramePFX.Natives;
using PFXToolKitUI;
using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Configurations;
using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.Icons;
using PFXToolKitUI.PropertyEditing.DataTransfer;
using PFXToolKitUI.PropertyEditing.DataTransfer.Enums;
using PFXToolKitUI.Services;
using PFXToolKitUI.Utils.Accessing;
using SkiaSharp;

namespace FramePFX;

public abstract class FramePFXApplication : Application {
    protected override void RegisterServices(ServiceManager manager) {
        base.RegisterServices(manager);
        manager.RegisterConstant(new ResourceDropOnTimelineService());
        manager.RegisterConstant(new TimelineDropManager());
        manager.RegisterConstant(new ExporterRegistry());
        manager.RegisterConstant<IIconPreferences>(new IconPreferencesImpl());
    }

    private class IconPreferencesImpl : IIconPreferences {
        public bool UseAntiAliasing {
            get => EditorConfigurationOptions.Instance.UseIconAntiAliasing;
            set => EditorConfigurationOptions.Instance.UseIconAntiAliasing = value;
        }
    }

    protected override void RegisterCommands(IApplicationStartupProgress progress, CommandManager manager) {
        base.RegisterCommands(progress, manager);

        // timelines, tracks and clips
        manager.Register("commands.editor.CreateVideoTrack", new NewVideoTrackCommand());
        manager.Register("commands.editor.CreateAudioTrack", new NewAudioTrackCommand());
        manager.Register("commands.editor.ToggleTrackAutomationCommand", new ToggleTrackAutomationCommand());
        manager.Register("commands.editor.ToggleClipAutomationCommand", new ToggleClipAutomationCommand());
        manager.Register("commands.editor.TogglePlayCommand", new TogglePlayCommand());
        manager.Register("commands.editor.PlaybackPlayCommand", new PlayCommand());
        manager.Register("commands.editor.PlaybackPauseCommand", new PauseCommand());
        manager.Register("commands.editor.PlaybackStopCommand", new StopCommand());
        manager.Register("commands.editor.DeleteSpecificTrack", new DeleteSpecificTrackCommand());
        manager.Register("commands.editor.DeleteSelectedTracks", new DeleteSelectedTracksCommand());
        manager.Register("commands.editor.SplitClipsCommand", new SplitClipsCommand());
        manager.Register("commands.editor.DeleteClipOwnerTrack", new DeleteClipOwnerTrackCommand());
        manager.Register("commands.editor.RenameClip", new RenameClipCommand());
        manager.Register("commands.editor.RenameTrack", new RenameTrackCommand());
        manager.Register("commands.editor.DeleteClips", new DeleteClipsCommand());
        manager.Register("commands.editor.ToggleLoopTimelineRegion", new ToggleLoopTimelineRegionCommand());
        manager.Register("commands.editor.AutoToggleLoopTimelineRegion", new ToggleLoopTimelineRegionCommand() { CanUpdateRegionToClipSelection = true });

        manager.Register("commands.editor.ToggleClipsEnabled", new ToggleClipsEnabledCommand());
        manager.Register("commands.editor.EnableClips", new EnableClipsCommand());
        manager.Register("commands.editor.DisableClips", new DisableClipsCommand());
        manager.Register("commands.editor.ToggleTracksEnabled", new ToggleTracksEnabledCommand());
        manager.Register("commands.editor.EnableTracks", new EnableTracksCommand());
        manager.Register("commands.editor.DisableTracks", new DisableTracksCommand());

        manager.Register("commands.editor.SelectAllClips", new SelectAllClipsCommand());
        manager.Register("commands.editor.SelectClipsInTracks", new SelectClipsInTracksCommand());
        manager.Register("commands.editor.ChangeClipPlaybackSpeed", new ChangeClipPlaybackSpeedCommand());
        manager.Register("commands.editor.CreateCompositionFromSelection", new CreateCompositionFromSelectionCommand());
        manager.Register("commands.editor.OpenCompositionTimeline", new OpenCompositionTimelineCommand());
        manager.Register("commands.editor.OpenCompositionClipTimeline", new OpenCompositionClipTimelineCommand());

        // Adding clips to tracks
        manager.Register("commands.editor.AddTextClip", new AddTextClipCommand());
        manager.Register("commands.editor.AddTimecodeClip", new AddTimecodeClipCommand());
        manager.Register("commands.editor.AddVideoClipShape", new AddVideoClipShapeCommand());
        manager.Register("commands.editor.AddImageVideoClip", new AddImageVideoClipCommand());
        manager.Register("commands.editor.AddCompositionVideoClip", new AddCompositionVideoClipCommand());

        // resources
        manager.Register("commands.resources.RenameResource", new RenameResourceCommand());
        manager.Register("commands.resources.DeleteResources", new DeleteResourcesCommand());
        manager.Register("commands.resources.AddResourceImage", new AddResourceImageCommand());
        manager.Register("commands.resources.AddResourceColour", new AddResourceColourCommand());
        manager.Register("commands.resources.AddResourceComposition", new AddResourceCompositionCommand());
        manager.Register("commands.resources.GroupResources", new GroupResourcesCommand());
        manager.Register("commands.resources.SetResourcesOnline", new SetResourcesOnlineCommand());
        manager.Register("commands.resources.SetResourcesOffline", new SetResourcesOfflineCommand());
        manager.Register("commands.resources.ToggleOnlineState", new ToggleOnlineStateCommand());
        manager.Register("commands.resources.ChangeResourceColour", new ChangeResourceColourCommand());

        // Editor
        manager.Register("UndoCommand", new UndoCommand());
        manager.Register("RedoCommand", new RedoCommand());
        manager.Register("commands.editor.NewProject", new NewProjectCommand());
        manager.Register("commands.editor.OpenProject", new OpenProjectCommand());
        manager.Register("commands.editor.CloseProject", new CloseProjectCommand());
        manager.Register("commands.editor.SaveProject", new SaveProjectCommand());
        manager.Register("commands.editor.SaveProjectAs", new SaveProjectAsCommand());
        manager.Register("commands.editor.Export", new ExportCommand());

        manager.Register("commands.mainWindow.OpenProjectSettings", new OpenProjectSettingsCommand());
    }

    protected override async Task Initialise(IApplicationStartupProgress progress) {
        await base.Initialise(progress);

        ApplicationConfigurationManager appConfig = ApplicationConfigurationManager.Instance;
        appConfig.RootEntry.AddEntry(new ConfigurationEntry() {
            DisplayName = "Startup", Id = "config.startup", Page = new StartupPropEditorConfigurationPage()
        });

        appConfig.RootEntry.AddEntry(new ConfigurationEntry() {
            DisplayName = "Editor", Id = "config.editor", Page = new EditorWindowConfigurationPage(),
            Items = [
                new ConfigurationEntry() {
                    DisplayName = "Colours", Id = "config.editor.colours", Page = new EditorWindowPropEditorConfigurationPage()
                }
            ]
        });
    }

    protected override void OnExiting(int exitCode) {
        base.OnExiting(exitCode);
        PFXNative.ShutdownLibrary();
    }

    protected override string? GetSolutionFileName() {
        return "FramePFX.sln";
    }

    public override string GetApplicationName() {
        return "FramePFX";
    }

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

    public class StartupPropEditorConfigurationPage : PropertyEditorConfigurationPage {
        public static readonly DataParameter<StartupConfigurationOptions.EnumStartupBehaviour> StartupBehaviourParameter = DataParameter.Register(new DataParameter<StartupConfigurationOptions.EnumStartupBehaviour>(typeof(StartupPropEditorConfigurationPage), nameof(StartupBehaviour), default, ValueAccessors.Reflective<StartupConfigurationOptions.EnumStartupBehaviour>(typeof(StartupPropEditorConfigurationPage), nameof(startupBehaviour))));
        public static readonly DataParameterString StartupThemeParameter = DataParameter.Register(new DataParameterString(typeof(StartupPropEditorConfigurationPage), nameof(StartupTheme), "Dark", ValueAccessors.Reflective<string?>(typeof(StartupPropEditorConfigurationPage), nameof(startupTheme))));

        private StartupConfigurationOptions.EnumStartupBehaviour startupBehaviour;
        private string? startupTheme;

        public StartupConfigurationOptions.EnumStartupBehaviour StartupBehaviour {
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

        public override ValueTask OnContextCreated(ConfigurationContext context) {
            StartupConfigurationOptions options = StartupConfigurationOptions.Instance;
            this.startupBehaviour = options.StartupBehaviour;
            this.startupTheme = options.StartupTheme;
            this.PropertyEditor.Root.SetupHierarchyState([this]);
            return ValueTask.CompletedTask;
        }

        public override ValueTask OnContextDestroyed(ConfigurationContext context) {
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

    public class DataParameterStartupBehaviourPropertyEditorSlot : DataParameterEnumPropertyEditorSlot<StartupConfigurationOptions.EnumStartupBehaviour> {
        public static DataParameterEnumInfo<StartupConfigurationOptions.EnumStartupBehaviour> CodedIdEnumInfo { get; }

        public DataParameterStartupBehaviourPropertyEditorSlot(DataParameter<StartupConfigurationOptions.EnumStartupBehaviour> parameter, Type applicableType, string? displayName = null) : base(parameter, applicableType, displayName ?? "Codec ID", DataParameterEnumInfo<StartupConfigurationOptions.EnumStartupBehaviour>.EnumValuesOrderedByName, CodedIdEnumInfo) { }

        static DataParameterStartupBehaviourPropertyEditorSlot() {
            CodedIdEnumInfo = DataParameterEnumInfo<StartupConfigurationOptions.EnumStartupBehaviour>.All(new Dictionary<StartupConfigurationOptions.EnumStartupBehaviour, string> {
                [StartupConfigurationOptions.EnumStartupBehaviour.OpenStartupWindow] = "Open startup window",
                [StartupConfigurationOptions.EnumStartupBehaviour.OpenDemoProject] = "Open a demo project",
                [StartupConfigurationOptions.EnumStartupBehaviour.OpenEmptyProject] = "Open a new empty project",
            });
        }
    }
}