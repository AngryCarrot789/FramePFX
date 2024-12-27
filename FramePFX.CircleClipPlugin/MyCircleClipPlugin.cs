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
using FramePFX.Editing.ContextRegistries;
using FramePFX.Editing.Factories;
using FramePFX.Editing.Timelines.Commands;
using FramePFX.Editing.UI;
using FramePFX.Interactivity.Formatting;
using FramePFX.Plugins;
using FramePFX.PropertyEditing.DataTransfer;
using FramePFX.Services.VideoEditors;

namespace FramePFX.CircleClipPlugin;

public class MyCircleClipPlugin : Plugin {
    public override void OnCreated() {
        
    }

    private class AddCircleClipCommand : AddClipCommand<MyCirclePluginVideoClip> {
        
    }
    
    public override void RegisterCommands(CommandManager manager) {
        manager.Register("commands.mycircleclipplugin.editor.AddCircleClip", new AddCircleClipCommand());
    }

    public override void RegisterServices() {
        
    }

    public override async Task OnApplicationLoaded() {
        ClipFactory.Instance.RegisterType("vc_plugin_circleclip", typeof(MyCirclePluginVideoClip));
        
        TrackContextRegistry.TimelineTrackContextRegistry.GetFixedGroup("ModifyAddClips").AddCommand("commands.mycircleclipplugin.editor.AddCircleClip", "Add circle clip (MCCP)", "Add a plugin circlular clip!");
        
        IVideoEditorService service = Application.Instance.ServiceManager.GetService<IVideoEditorService>();
        service.VideoEditorCreatedOrShown += OnVideoEditorCreatedOrShown;
    }
    
    public override void OnApplicationExiting() {
        IVideoEditorService service = Application.Instance.ServiceManager.GetService<IVideoEditorService>();
        service.VideoEditorCreatedOrShown -= OnVideoEditorCreatedOrShown;
    }

    private static void OnVideoEditorCreatedOrShown(IVideoEditorWindow window, bool isBeforeShow) {
        if (isBeforeShow) {
            window.PropertyEditor.ClipGroup.AddItem(new DataParameterFloatPropertyEditorSlot(MyCirclePluginVideoClip.RadiusParameter, typeof(MyCirclePluginVideoClip), "Radius", DragStepProfile.Pixels) {ValueFormatter = SuffixValueFormatter.StandardPixels});
        }
    }
}