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

using FramePFX.Editing.UI;
using FramePFX.Interactivity.Contexts;
using FramePFX.Toolbars;
using FramePFX.Utils.Collections.Observable;

namespace FramePFX.Editing.Toolbars;

public class ViewPortToolBarManager : BaseToolBarManager {
    /// <summary>
    /// Gets the toolbar buttons that are docked to the west (left) side of the toolbar
    /// </summary>
    public ObservableList<ToolBarButton> WestButtons { get; }

    /// <summary>
    /// Gets the toolbar buttons that are docked in the middle of the toolbar
    /// </summary>
    public ObservableList<ToolBarButton> CenterButtons { get; }

    /// <summary>
    /// Gets the toolbar buttons that are docked to the east (right) side of the toolbar
    /// </summary>
    public ObservableList<ToolBarButton> EastButtons { get; }

    public ViewPortToolBarManager() {
        this.WestButtons = new ObservableList<ToolBarButton>();
        this.CenterButtons = new ObservableList<ToolBarButton>();
        this.EastButtons = new ObservableList<ToolBarButton>();

        this.WestButtons.Add(new ToggleZoomToCursorToolBarButton() { Button = { ToolTip = "If enabled, pans towards your cursor when zooming in and pans away when zooming out. When false, zooms into the center of the screen" } });
        
        this.CenterButtons.Add(new TimelineToolBarManager.TogglePlayStateButtonImpl() { Button = { ToolTip = "Play or pause playback" } });
        this.CenterButtons.Add(new TimelineToolBarManager.SetPlayStateButtonImpl(PlayState.Play) { Button = { ToolTip = "Start playback" } });
        this.CenterButtons.Add(new TimelineToolBarManager.SetPlayStateButtonImpl(PlayState.Pause) { Button = { ToolTip = "Pause playback, keeping the play head at the current frame" } });
        this.CenterButtons.Add(new TimelineToolBarManager.SetPlayStateButtonImpl(PlayState.Stop) { Button = { ToolTip = "Stop playback, returning the play head to the stop head location" } });
        
        this.EastButtons.Add(new ToggleUseCheckerboardToolBarButton() { Button = { ToolTip = "Toggles whether to use a checkerboard or black background", } });
    }

    public static ViewPortToolBarManager GetInstance(VideoEditor editor) {
        return editor.ServiceManager.GetService<ViewPortToolBarManager>();
    }

    public class ToggleZoomToCursorToolBarButton : ToolBarButton {
        private IVideoEditorWindow? editor;

        public new IToggleButtonElement Button => (IToggleButtonElement) base.Button;

        public ToggleZoomToCursorToolBarButton() : base(ToolbarButtonFactory.Instance.CreateToggleButton(ToggleButtonStyle.Button)) {
            this.Button.Text = "Zoom To Cursor";
        }

        protected override Task OnClickedAsync() {
            if (this.editor != null) {
                this.editor.ViewPort.PanToCursorOnUserZoom = !this.editor.ViewPort.PanToCursorOnUserZoom;
            }
            
            return Task.CompletedTask;
        }

        protected override void OnContextChanged() {
            base.OnContextChanged();
            if (!DataKeys.VideoEditorUIKey.TryGetContext(this.ContextData, out IVideoEditorWindow? newEditor)) {
                this.ClearEditor();
                this.UpdateCheckBox();
            }
            else if (!ReferenceEquals(newEditor, this.editor)) {
                this.ClearEditor();

                this.editor = newEditor;
                this.editor.ViewPort.PanToCursorOnUserZoomChanged += this.OnPanToCursorChanged;
                base.Button.IsEnabled = true;
                this.UpdateCheckBox();
            }
        }

        private void ClearEditor() {
            if (this.editor != null) {
                this.editor.ViewPort.PanToCursorOnUserZoomChanged -= this.OnPanToCursorChanged;
                this.editor = null;
                base.Button.IsEnabled = false;
            }
        }

        private void OnPanToCursorChanged(IViewPortElement element) {
            this.UpdateCheckBox();
        }

        private void UpdateCheckBox() {
            if (this.editor != null) {
                this.Button.IsChecked = this.editor.ViewPort.PanToCursorOnUserZoom;
            }
        }
    }
    
    public class ToggleUseCheckerboardToolBarButton : ToolBarButton {
        private IVideoEditorWindow? editor;

        public new IToggleButtonElement Button => (IToggleButtonElement) base.Button;

        public ToggleUseCheckerboardToolBarButton() : base(ToolbarButtonFactory.Instance.CreateToggleButton(ToggleButtonStyle.Button)) {
        }

        protected override Task OnClickedAsync() {
            if (this.editor != null) {
                this.editor.ViewPort.UseTransparentCheckerBoardBackground = !this.editor.ViewPort.UseTransparentCheckerBoardBackground;
            }
            
            return Task.CompletedTask;
        }

        protected override void OnContextChanged() {
            base.OnContextChanged();
            if (!DataKeys.VideoEditorUIKey.TryGetContext(this.ContextData, out IVideoEditorWindow? newEditor)) {
                this.ClearEditor();
                this.UpdateCheckBox();
            }
            else if (!ReferenceEquals(newEditor, this.editor)) {
                this.ClearEditor();

                this.editor = newEditor;
                this.editor.ViewPort.UseTransparentCheckerBoardBackgroundChanged += this.OnPanToCursorChanged;
                base.Button.IsEnabled = true;
                this.UpdateCheckBox();
            }
        }

        private void ClearEditor() {
            if (this.editor != null) {
                this.editor.ViewPort.UseTransparentCheckerBoardBackgroundChanged -= this.OnPanToCursorChanged;
                this.editor = null;
                base.Button.IsEnabled = false;
            }
        }

        private void OnPanToCursorChanged(IViewPortElement element) {
            this.UpdateCheckBox();
        }

        private void UpdateCheckBox() {
            if (this.editor != null) {
                this.Button.IsChecked = this.editor.ViewPort.UseTransparentCheckerBoardBackground;
                this.Button.Text = this.editor.ViewPort.UseTransparentCheckerBoardBackground ? "Use Black BG" : "Use Transparent BG";
            }
            else {
                this.Button.Text = "<invalid>";
            }
        }
    }
}