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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using FramePFX.Editing.Audio;
using FramePFX.Editing.Scratch;
using FramePFX.Editing.Video;
using FramePFX.Editing.ViewStates;
using PFXToolKitUI.Avalonia.Bindings;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Interactivity.Windowing;
using Track = FramePFX.Editing.Track;

namespace FramePFX.Avalonia.Editor.TrackSettings;

/// <summary>
/// A control that represents the content inside a <see cref="TrackSettingsListBoxItem"/>.
/// <para>
/// This templated control should contain sliders, buttons, etc., that allow the user to change track settings
/// </para>
/// </summary>
[TemplatePart(Name = "PART_TrackNameTextBlock", Type = typeof(TextBlock), IsRequired = true)]
public class TrackSettingsControl : TemplatedControl {
    public static readonly ModelControlRegistry<Track, TrackSettingsControl> Registry = new();
    public static readonly DirectProperty<TrackSettingsControl, TrackViewState?> TrackViewStateProperty = AvaloniaProperty.RegisterDirect<TrackSettingsControl, TrackViewState?>(nameof(TrackViewState), o => o.TrackViewState);
    public static readonly DirectProperty<TrackSettingsControl, TrackSettingsListBoxItem?> ListBoxItemProperty = AvaloniaProperty.RegisterDirect<TrackSettingsControl, TrackSettingsListBoxItem?>(nameof(TrackSettingsListBoxItem), o => o.ListBoxItem);

    public TrackViewState? TrackViewState {
        get => field;
        private set => this.SetAndRaise(TrackViewStateProperty, ref field, value);
    }

    public TrackSettingsListBoxItem? ListBoxItem {
        get => field;
        private set => this.SetAndRaise(ListBoxItemProperty, ref field, value);
    }

    public TopLevelIdentifier TopLevelIdentifier { get; private set; }

    private readonly IBinder<Track> trackNameBinder = new EventUpdateBinder<Track>(nameof(Track.DisplayNameChanged), (b) => ((TextBlock) b.Control).Text = b.Model.DisplayName);
    
    public TrackSettingsControl() {
    }

    static TrackSettingsControl() {
        Registry.RegisterType<VideoTrack>(() => new TrackSettingsControlVideo());
        Registry.RegisterType<AudioTrack>(() => new TrackSettingsControlAudio());
        Registry.RegisterType<ScratchTrack>(() => new TrackSettingsControlScratch());
    }
    
    public static TrackSettingsControl Create(Track track) => Registry.NewInstance(track);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.trackNameBinder.AttachControl(e.NameScope.GetTemplateChild<TextBlock>("PART_TrackNameTextBlock"));
    }

    internal void InternalOnConnecting(TrackSettingsListBoxItem item, TrackViewState track) {
        this.ListBoxItem = item;
        this.TrackViewState = track;
        this.TopLevelIdentifier = item.TopLevelIdentifier;
        this.trackNameBinder.AttachModel(track.Track);
        this.OnConnecting();
    }

    internal void InternalOnConnected() {
        this.OnConnected();
    }

    internal void InternalOnDisconnecting() {
        this.trackNameBinder.DetachModel();
        this.OnDisconnecting();
    }

    internal void InternalOnDisconnected() {
        this.OnDisconnected();
        this.ListBoxItem = null;
        this.TrackViewState = null;
        this.TopLevelIdentifier = default;
    }

    protected virtual void OnConnecting() {
    }

    protected virtual void OnConnected() {
    }

    protected virtual void OnDisconnecting() {
    }

    protected virtual void OnDisconnected() {
    }
}