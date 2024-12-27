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

using FramePFX.AdvancedMenuService;
using FramePFX.CommandSystem;
using FramePFX.Editing.ContextRegistries;
using FramePFX.Editing.Factories;
using FramePFX.Editing.Timelines.Commands;
using FramePFX.Editing.Timelines.Tracks;
using FramePFX.Editing.UI;
using FramePFX.Interactivity.Contexts;
using FramePFX.Interactivity.Formatting;
using FramePFX.PropertyEditing.DataTransfer;
using FramePFX.Services.Messaging;
using FramePFX.Services.VideoEditors;

namespace FramePFX.Plugins.CircleClipPlugin;

public class MyCircleClipPlugin : Plugin {
    public override void OnCreated() {
        
    }

    // Uses the standard clip creation system via AddClipCommand<T>
    private class AddCircleClipCommand : AddClipCommand<MyCirclePluginVideoClip> {
        protected override async Task OnPostAddToTrack(Track track, MyCirclePluginVideoClip clip, bool success, IContextData ctx) {
            MessageBoxInfo info = new MessageBoxInfo("!!!", "This message is being shown from a fully dynamically loaded assembly!!!") {
                Buttons = MessageBoxButton.OKCancel,
                YesOkText = "Wow!!!",
                CancelText = "I Don't Care"
            };

            MessageBoxResult result = await IMessageDialogService.Instance.ShowMessage(info);
            if (result == MessageBoxResult.Cancel) {
                await IMessageDialogService.Instance.ShowMessage("???", "Awe");
            }
        }
    }

    public override void RegisterCommands(CommandManager manager) {
        // Register our command that creates the clip
        manager.Register("commands.mycircleclipplugin.editor.AddCircleClip", new AddCircleClipCommand());
    }

    public override void RegisterServices() {
    }

    public override async Task OnApplicationLoaded() {
        // Register our super cool useless video clip type
        ClipFactory.Instance.RegisterType("vc_plugin_circleclip", typeof(MyCirclePluginVideoClip));

        // TODO: need a better system than this... I mean, I made groups to assist with
        // plugins inserting their own commands into the context registries, but I think
        // we need abstractions around certain regions of the context registries, especially
        // for inserting new clips (and also resources for those context registries), just to
        // make it a bit friendlier to use. But this works so :/
        FixedContextGroup addClipGroup = TrackContextRegistry.TimelineTrackContextRegistry.GetFixedGroup("ModifyAddClips");
        addClipGroup.AddCommand("commands.mycircleclipplugin.editor.AddCircleClip", "Add circle clip (MCCP)", "Add a plugin circlular clip!");

        // Listen to when a video editor window is opened so that we can add our radius parameter slot to its property editor
        IVideoEditorService service = Application.Instance.ServiceManager.GetService<IVideoEditorService>();
        service.VideoEditorCreatedOrShown += OnVideoEditorCreatedOrShown;
    }

    public override void OnApplicationExiting() {
        IVideoEditorService service = Application.Instance.ServiceManager.GetService<IVideoEditorService>();
        service.VideoEditorCreatedOrShown -= OnVideoEditorCreatedOrShown;
    }

    private static void OnVideoEditorCreatedOrShown(IVideoEditorWindow window, bool isBeforeShow) {
        // We don't really an isBeforeShow event for this, because the property editor control listens to
        // the events for add/remove/move, so it would just add in the slot control as per usual.
        // But maybe in the future a pre-post indicator will be needed, so we might as well keep it now
        if (isBeforeShow) {
            window.PropertyEditor.ClipGroup.AddItem(
                new DataParameterFloatPropertyEditorSlot(MyCirclePluginVideoClip.RadiusParameter, typeof(MyCirclePluginVideoClip), "Radius", DragStepProfile.Pixels) {
                    ValueFormatter = SuffixValueFormatter.StandardPixels
                });
        }
    }
}