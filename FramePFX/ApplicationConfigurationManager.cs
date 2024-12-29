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

using FramePFX.Configurations;
using FramePFX.Configurations.Basic;
using FramePFX.Configurations.Shortcuts;
using FramePFX.DataTransfer;
using FramePFX.Editing;
using FramePFX.PropertyEditing.DataTransfer;
using FramePFX.PropertyEditing.DataTransfer.Enums;
using FramePFX.Shortcuts;
using FramePFX.Themes;
using FramePFX.Utils.Accessing;
using SkiaSharp;

namespace FramePFX;

/// <summary>
/// The configuration manager for the entire FramePFX application
/// </summary>
public class ApplicationConfigurationManager : ConfigurationManager {
    public static readonly ApplicationConfigurationManager Instance = new ApplicationConfigurationManager();

    public ConfigurationEntry EditorConfigurationEntry { get; }
    
    public ConfigurationEntry StartupConfigurationEntry { get; }

    private ApplicationConfigurationManager() {
        if (Instance != null)
            throw new InvalidOperationException("Singleton");

        this.RootEntry.AddEntry(this.EditorConfigurationEntry = new ConfigurationEntry() {
            DisplayName = "Editor", Id = "config.editor", Page = new EditorWindowConfigurationPage(),
            Items = [
                new ConfigurationEntry() {
                    DisplayName = "Colours", Id = "config.editor.colours", Page = new EditorWindowPropEditorConfigurationPage()
                }
            ]
        });

        this.RootEntry.AddEntry(new ConfigurationEntry() {
            DisplayName = "Keymap", Id = "config.keymap", Page = new ShortcutEditorConfigurationPage(ShortcutManager.Instance)
        });
        
        this.RootEntry.AddEntry(new ConfigurationEntry() {
            DisplayName = "Themes", Id = "config.themes", Page = ThemeManager.Instance.ThemeConfigurationPage
        });
        
        this.RootEntry.AddEntry(this.StartupConfigurationEntry = new ConfigurationEntry() {
            DisplayName = "Startup", Id = "config.startup", Page = new StartupPropEditorConfigurationPage()
        });
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

        public override async ValueTask OnContextCreated(ConfigurationContext context) {
            EditorConfigurationOptions options = EditorConfigurationOptions.Instance;
            this.titleBarBrush = options.TitleBarBrush;
            this.PropertyEditor.Root.SetupHierarchyState([this]);
        }

        public override ValueTask OnContextDestroyed(ConfigurationContext context) {
            this.PropertyEditor.Root.ClearHierarchy();
            return ValueTask.CompletedTask;
        }

        public override async ValueTask Apply(List<ApplyChangesFailureEntry>? errors) {
            EditorConfigurationOptions options = EditorConfigurationOptions.Instance;
            options.TitleBarBrush = this.titleBarBrush;

            // await IoC.MessageService.ShowMessage("Change title", "Change window title to: " + this.TitleBar);
        }
    }
    
    public class StartupPropEditorConfigurationPage : PropertyEditorConfigurationPage {
        
        public static readonly DataParameter<StartupConfigurationOptions.EnumStartupBehaviour> StartupBehaviourParameter =
            DataParameter.Register(
                new DataParameter<StartupConfigurationOptions.EnumStartupBehaviour>(
                    typeof(StartupPropEditorConfigurationPage),
                    nameof(StartupBehaviour), default,
                    ValueAccessors.Reflective<StartupConfigurationOptions.EnumStartupBehaviour>(typeof(StartupPropEditorConfigurationPage), nameof(startupBehaviour))));

        private StartupConfigurationOptions.EnumStartupBehaviour startupBehaviour;

        public StartupConfigurationOptions.EnumStartupBehaviour StartupBehaviour {
            get => this.startupBehaviour;
            set => DataParameter.SetValueHelper(this, StartupBehaviourParameter, ref this.startupBehaviour, value);
        }
        
        public StartupPropEditorConfigurationPage() {
            this.startupBehaviour = StartupBehaviourParameter.GetDefaultValue(this);
            StartupBehaviourParameter.AddValueChangedHandler(this, this.MarkModifiedHandler);

            this.PropertyEditor.Root.AddItem(new DataParameterStartupBehaviourPropertyEditorSlot(StartupBehaviourParameter, typeof(StartupPropEditorConfigurationPage), "Behaviour"));
        }

        private void MarkModifiedHandler(DataParameter parameter, ITransferableData owner) => this.MarkModified();

        public override async ValueTask OnContextCreated(ConfigurationContext context) {
            StartupConfigurationOptions options = StartupConfigurationOptions.Instance;
            this.startupBehaviour = options.StartupBehaviour;
            this.PropertyEditor.Root.SetupHierarchyState([this]);
        }

        public override ValueTask OnContextDestroyed(ConfigurationContext context) {
            this.PropertyEditor.Root.ClearHierarchy();
            return ValueTask.CompletedTask;
        }

        public override async ValueTask Apply(List<ApplyChangesFailureEntry>? errors) {
            StartupConfigurationOptions options = StartupConfigurationOptions.Instance;
            options.StartupBehaviour = this.startupBehaviour;

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