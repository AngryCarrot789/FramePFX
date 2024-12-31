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
using FramePFX.Editing.Timelines;
using FramePFX.Interactivity.Contexts;
using FramePFX.Toolbars;
using FramePFX.Utils.Collections.Observable;

namespace FramePFX.Editing.Toolbars;

/// <summary>
/// Manages the toolbar that is below the timeline itself
/// </summary>
public sealed class TimelineToolBarManager : BaseToolBarManager {
    /// <summary>
    /// Gets the toolbar buttons that are docked to the west (left) side of the toolbar
    /// </summary>
    public ObservableList<ToolBarButton> WestButtons { get; }

    /// <summary>
    /// Gets the toolbar buttons that are docked to the east (right) side of the toolbar
    /// </summary>
    public ObservableList<ToolBarButton> EastButtons { get; }

    public TimelineToolBarManager() {
        this.WestButtons = new ObservableList<ToolBarButton>();
        this.EastButtons = new ObservableList<ToolBarButton>();
        
        // Setup standard buttons
        this.WestButtons.Add(new TogglePlayStateButtonImpl() { Button = { ToolTip = "Play or pause playback" } });
        this.WestButtons.Add(new SetPlayStateButtonImpl(PlayState.Play) { Button = { ToolTip = "Start playback" } });
        this.WestButtons.Add(new SetPlayStateButtonImpl(PlayState.Pause) { Button = { ToolTip = "Pause playback, keeping the play head at the current frame" } });
        this.WestButtons.Add(new SetPlayStateButtonImpl(PlayState.Stop) { Button = { ToolTip = "Stop playback, returning the play head to the stop head location" } });
        this.WestButtons.Add(new ToggleLoopToolBarButton() { Button = { ToolTip = "Toggles if looping is enabled. When enabled and the play head is inside the loop region, the play head returns back to the start of the loop when it reaches the end" } });
    }

    public static TimelineToolBarManager GetInstance(VideoEditor editor) {
        return editor.ServiceManager.GetService<TimelineToolBarManager>();
    }

    #region Button implementations
    
    public class TogglePlayStateButtonImpl : ToolBarButton {
        private VideoEditor? editor;

        public TogglePlayStateButtonImpl() : base(ToolbarButtonFactory.Instance.CreateButton()) {
        }

        public override Executability CanExecute() {
            return this.editor != null ? Executability.Valid : Executability.Invalid;
        }

        protected override Task OnClickedAsync() {
            if (this.editor != null) {
                if (this.editor.Playback.PlayState == PlayState.Play) {
                    this.editor.Playback.Pause();
                }
                else {
                    this.editor.Playback.Play();
                }
            }

            return Task.CompletedTask;
        }

        protected override void OnContextChanged() {
            base.OnContextChanged();
            if (this.editor != null) {
                this.editor.Playback.PlaybackStateChanged -= this.OnStateChanged;
                this.editor = null;
            }

            if (DataKeys.VideoEditorKey.TryGetContext(this.ContextData, out VideoEditor? theEditor)) {
                this.editor = theEditor;
                this.editor.Playback.PlaybackStateChanged += this.OnStateChanged;
            }
        }

        protected override void OnUpdateCanExecute() {
            base.OnUpdateCanExecute();
            this.UpdateIcon(this.editor?.Playback.PlayState);
        }

        private void OnStateChanged(PlaybackManager sender, PlayState state, long frame) {
            this.UpdateIcon(state);
        }

        private void UpdateIcon(PlayState? state) {
            this.Icon = state == PlayState.Play ? SimpleIcons.PauseIcon : SimpleIcons.PlayPauseIcon;
        }
    }

    public class SetPlayStateButtonImpl : ToolBarButton {
        private VideoEditor? myEditor;

        public PlayState TargetState { get; }

        public SetPlayStateButtonImpl(PlayState target) : base(ToolbarButtonFactory.Instance.CreateButton()) {
            this.TargetState = target;
            switch (target) {
                case PlayState.Play:  this.Icon = SimpleIcons.PlayIcon; break;
                case PlayState.Pause: this.Icon = SimpleIcons.PauseIcon; break;
                case PlayState.Stop:  this.Icon = SimpleIcons.StopIcon; break;
                default:              throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }
        }

        public override Executability CanExecute() {
            if (this.myEditor == null) {
                return Executability.Invalid;
            }

            return this.myEditor.Playback.CanSetPlayStateTo(this.TargetState) ? Executability.Valid : Executability.ValidButCannotExecute;
        }

        protected override Task OnClickedAsync() {
            if (this.myEditor != null && this.myEditor.Playback.CanSetPlayStateTo(this.TargetState)) {
                switch (this.TargetState) {
                    case PlayState.Play:  this.myEditor.Playback.Play(); break;
                    case PlayState.Pause: this.myEditor.Playback.Pause(); break;
                    case PlayState.Stop:  this.myEditor.Playback.Stop(); break;
                }
            }

            return Task.CompletedTask;
        }

        protected override void OnContextChanged() {
            base.OnContextChanged();
            if (this.myEditor != null) {
                this.myEditor.Playback.PlaybackStateChanged -= this.OnStateChanged;
                this.myEditor = null;
            }

            if (DataKeys.VideoEditorKey.TryGetContext(this.ContextData, out VideoEditor? theEditor)) {
                this.myEditor = theEditor;
                this.myEditor.Playback.PlaybackStateChanged += this.OnStateChanged;
            }
        }

        private void OnStateChanged(PlaybackManager sender, PlayState state, long frame) {
            this.UpdateCanExecuteLater();
        }
    }

    private class ToggleLoopToolBarButton : SimpleCommandToolBarButton {
        private Timeline? myTimeline;

        public new IToggleButtonElement Button => (IToggleButtonElement) base.Button;

        public ToggleLoopToolBarButton() : base("commands.editor.ToggleLoopTimelineRegion", ToolbarButtonFactory.Instance.CreateToggleButton(default)) {
            this.Icon = SimpleIcons.LoopIcon;
        }

        protected override void OnContextChanged() {
            base.OnContextChanged();

            if (this.myTimeline != null) {
                this.myTimeline.IsLoopRegionEnabledChanged -= this.OnIsLoopRegionEnabledChanged;
                this.myTimeline = null;
            }

            if (DataKeys.TimelineKey.TryGetContext(this.ContextData, out this.myTimeline)) {
                this.myTimeline.IsLoopRegionEnabledChanged += this.OnIsLoopRegionEnabledChanged;
                this.UpdateIsChecked();
            }
        }

        private void OnIsLoopRegionEnabledChanged(Timeline timeline) {
            this.UpdateIsChecked();
        }

        private void UpdateIsChecked() {
            if (this.myTimeline == null) {
                this.Button.IsThreeState = true;
                this.Button.IsChecked = null;
            }
            else {
                this.Button.IsThreeState = false;
                this.Button.IsChecked = this.myTimeline.IsLoopRegionEnabled;
            }
        }
    }
    
    #endregion
}
