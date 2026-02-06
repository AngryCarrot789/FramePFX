// 
// Copyright (c) 2026-2026 REghZy
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

using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media.Immutable;
using FramePFX.Editing.ViewStates;
using PFXToolKitUI.Avalonia.AvControls.ListBoxes;
using PFXToolKitUI.Avalonia.Bindings;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Interactivity.Windowing;
using Track = FramePFX.Editing.Track;

namespace FramePFX.Avalonia.Editor.TrackSettings;

public class TrackSettingsListBoxItem : ModelBasedListBoxItem<Track> {
    private readonly IBinder<TrackViewState> heightBinder =
        new EventUpdateBinder<TrackViewState>(
            nameof(TrackViewState.HeightChanged),
            b => ((TrackSettingsListBoxItem) b.Control).Height = b.Model.Height /* + 1.0 /* bottom border thickness */);
    
    private readonly IBinder<Track> colourStripBinder =
        new EventUpdateBinder<Track>(
            nameof(Track.ColourChanged),
            b => ((Border) b.Control).Background = new ImmutableSolidColorBrush((uint) b.Model.Colour));

    private TrackSettingsControl? settingsControl;

    public TopLevelIdentifier TopLevelIdentifier => ((TrackSettingsListBox) this.ListBox!).Timeline!.TopLevelIdentifier;

    public TrackSettingsListBoxItem() {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.colourStripBinder.AttachControl(e.NameScope.GetTemplateChild<Border>("PART_TrackColourStrip"));
        this.SetDragSourceControl(this.colourStripBinder.Control);
    }

    protected override void OnAddingToList() {
        this.colourStripBinder.AttachModel(this.Model!);
        
        this.Content = this.settingsControl = TrackSettingsControl.Create(this.Model!);
        this.settingsControl.InternalOnConnecting(this, TrackViewState.GetInstance(this.Model!, this.TopLevelIdentifier));
    }

    protected override void OnAddedToList() {
        this.heightBinder.Attach(this, TrackViewState.GetInstance(this.Model!, this.TopLevelIdentifier));
        this.settingsControl!.InternalOnConnected();
    }

    protected override void OnRemovingFromList() {
        this.heightBinder.Detach();
        this.settingsControl!.InternalOnDisconnecting();
    }

    protected override void OnRemovedFromList() {
        this.colourStripBinder.DetachModel();
        
        this.settingsControl!.InternalOnDisconnected();
        this.Content = this.settingsControl = null;
    }
}