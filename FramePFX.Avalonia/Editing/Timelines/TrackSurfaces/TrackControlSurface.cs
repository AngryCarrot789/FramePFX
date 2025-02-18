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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using PFXToolKitUI.Avalonia.Bindings;
using PFXToolKitUI.Avalonia.Converters;
using PFXToolKitUI.Avalonia.Utils;
using FramePFX.Editing.Automation.Keyframes;
using FramePFX.Editing.Automation.Params;
using FramePFX.Editing.Timelines.Tracks;
using PFXToolKitUI.Utils;
using SkiaSharp;
using Track = FramePFX.Editing.Timelines.Tracks.Track;

namespace FramePFX.Avalonia.Editing.Timelines.TrackSurfaces;

/// <summary>
/// A control that represents the content inside a <see cref="TrackControlSurfaceItem"/>
/// </summary>
public class TrackControlSurface : TemplatedControl {
    public static readonly ModelControlRegistry<Track, TrackControlSurface> Registry = new();
    public static readonly StyledProperty<string?> DisplayNameProperty = AvaloniaProperty.Register<TrackControlSurface, string?>(nameof(DisplayName));
    public static readonly DirectProperty<TrackControlSurface, ISolidColorBrush?> TrackColourBrushProperty = AvaloniaProperty.RegisterDirect<TrackControlSurface, ISolidColorBrush?>(nameof(TrackColourBrush), o => o.TrackColourBrush);
    public static readonly DirectProperty<TrackControlSurface, ISolidColorBrush?> TrackColourForegroundBrushProperty = AvaloniaProperty.RegisterDirect<TrackControlSurface, ISolidColorBrush?>(nameof(TrackColourForegroundBrush), o => o.TrackColourForegroundBrush);

    private ISolidColorBrush? myTrackColourBrush;
    private ISolidColorBrush? _trackColourForegroundBrush;

    public ISolidColorBrush? TrackColourBrush {
        get => this.myTrackColourBrush;
        private set => this.SetAndRaise(TrackColourBrushProperty, ref this.myTrackColourBrush, value);
    }

    public ISolidColorBrush? TrackColourForegroundBrush {
        get => this._trackColourForegroundBrush;
        private set => this.SetAndRaise(TrackColourForegroundBrushProperty, ref this._trackColourForegroundBrush, value);
    }

    public string? DisplayName {
        get => this.GetValue(DisplayNameProperty);
        set => this.SetValue(DisplayNameProperty, value);
    }

    public TrackControlSurfaceItem? Owner { get; private set; }

    public ToggleButton? ToggleExpandTrackButton { get; private set; }

    public ComboBox? ParameterComboBox { get; private set; }

    public Button? InsertKeyFrameButton { get; private set; }

    public ToggleButton? ToggleOverrideButton { get; private set; }

    public StackPanel? AutomationPanel { get; private set; }

    private double trackHeightBeforeCollapse;
    private bool ignoreTrackHeightChanged;
    private bool ignoreExpandTrackEvent;
    private readonly List<Button> actionButtons;

    private bool isProcessingParameterSelectionChanged;
    private readonly ObservableCollection<TrackListItemParameterViewModel> parameterList;
    private TrackListItemParameterViewModel? selectedParameter;
    private ComboBox? myComboBox;

    private readonly GetSetAutoUpdateAndEventPropertyBinder<Track> displayNameBinder = new GetSetAutoUpdateAndEventPropertyBinder<Track>(DisplayNameProperty, nameof(Track.DisplayNameChanged), b => b.Model.DisplayName, (b, v) => b.Model.DisplayName = (string) v);

    private readonly AutoUpdateAndEventPropertyBinder<Track> trackColourBinder = new AutoUpdateAndEventPropertyBinder<Track>(TrackColourBrushProperty, nameof(Track.ColourChanged), binder => {
        TrackControlSurface element = (TrackControlSurface) binder.Control;
        SKColor c = element.Owner!.Track?.Colour ?? SKColors.Black;
        ((SolidColorBrush) element.TrackColourBrush!).Color = Color.FromArgb(c.Alpha, c.Red, c.Green, c.Blue);
        element.UpdateForegroundColour();
    }, binder => {
        TrackControlSurface element = (TrackControlSurface) binder.Control;
        Color c = ((SolidColorBrush) element.TrackColourBrush!).Color;
        element.Owner!.Track!.Colour = new SKColor(c.R, c.G, c.B, c.A);
    });

    public TrackControlSurface() {
        this.TrackColourBrush = new SolidColorBrush(Colors.Black);
        this.UpdateForegroundColour();
        this.displayNameBinder.AttachControl(this);
        this.trackColourBinder.AttachControl(this);

        this.actionButtons = new List<Button>();
        this.parameterList = new ObservableCollection<TrackListItemParameterViewModel>();
    }

    private void UpdateForegroundColour() {
        this.TrackColourForegroundBrush = PerceivedForegroundConverter.GetBrush(this.TrackColourBrush!);
    }

    static TrackControlSurface() {
        Registry.RegisterType<VideoTrack>(() => new TrackControlSurfaceVideo());
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        e.NameScope.GetTemplateChild("PART_ExpandTrackButton", out ToggleButton expandButton);
        e.NameScope.GetTemplateChild("PART_InsertKeyFrameButton", out Button insertKeyFrameButton);
        e.NameScope.GetTemplateChild("PART_OverrideButton", out ToggleButton toggleOverrideButton);
        e.NameScope.GetTemplateChild("PART_ParameterComboBox", out this.myComboBox);
        e.NameScope.GetTemplateChild("PART_AutomationPanel", out StackPanel automationPanel);

        this.AutomationPanel = automationPanel;

        this.ToggleExpandTrackButton = expandButton;
        expandButton.IsThreeState = false;
        expandButton.IsCheckedChanged += this.ExpandTrackCheckedChanged;

        this.InsertKeyFrameButton = this.CreateBasicButtonAction(insertKeyFrameButton, () => {
            Parameter? parameter = this.selectedParameter?.Parameter;
            if (parameter != null) {
                Track? track = this.Owner!.Track;
                if (track != null && track.GetRelativePlayHead(out long playHead)) {
                    AutomationSequence seq = track.AutomationData[parameter];
                    object value = parameter.GetCurrentObjectValue(track);
                    seq.AddNewKeyFrame(playHead, out KeyFrame keyFrame);
                    keyFrame.SetValueFromObject(value);
                    seq.UpdateValue(playHead);
                }
            }
        });

        this.ToggleOverrideButton = toggleOverrideButton;
        this.CreateBasicButtonAction(toggleOverrideButton, () => {
            if (this.selectedParameter != null) {
                AutomationSequence seq = this.Owner!.Track!.AutomationData[this.selectedParameter.Parameter];
                seq.IsOverrideEnabled = !seq.IsOverrideEnabled;
            }
        });

        this.myComboBox.ItemsSource = this.parameterList;
        this.myComboBox.SelectionChanged += this.OnParameterSelectionChanged;
        this.ParameterComboBox = this.myComboBox;
        this.UpdateForSelectedParameter(null, null);
    }

    protected Button CreateBasicButtonAction(Button button, Action action) {
        this.actionButtons.Add(button);
        button.Click += (sender, args) => {
            if (this.Owner != null)
                action();
        };

        return button;
    }

    private void OnParameterSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        this.isProcessingParameterSelectionChanged = true;
        TrackListItemParameterViewModel? oldSelection = this.selectedParameter;
        this.selectedParameter = this.ParameterComboBox!.SelectedItem as TrackListItemParameterViewModel;
        this.UpdateForSelectedParameter(oldSelection, this.selectedParameter);
        this.isProcessingParameterSelectionChanged = false;
    }

    private void UpdateForSelectedParameter(TrackListItemParameterViewModel? oldSelection, TrackListItemParameterViewModel? newSelected) {
        this.InsertKeyFrameButton!.IsEnabled = newSelected != null;
        this.ToggleOverrideButton!.IsEnabled = newSelected != null;

        if (oldSelection != null)
            oldSelection.Sequence.OverrideStateChanged -= this.OnSelectedSequenceOverrideStateChanged;
        if (newSelected != null)
            newSelected.Sequence.OverrideStateChanged += this.OnSelectedSequenceOverrideStateChanged;

        this.ToggleOverrideButton.IsChecked = newSelected?.IsOverrideEnabled ?? false;
        if (this.Owner?.TrackList?.TimelineControl is TimelineControl control) {
            Track trackModel = this.Owner.Track!;
            TimelineTrackControl? track = control.TrackStorage!.GetTrackByModel(trackModel);
            if (track != null) {
                track.AutomationSequence = newSelected != null ? trackModel.AutomationData[newSelected.Parameter] : null;
            }
        }
    }

    private void OnSelectedSequenceOverrideStateChanged(AutomationSequence sequence) {
        if (this.selectedParameter != null)
            this.ToggleOverrideButton!.IsChecked = sequence.IsOverrideEnabled;
    }

    private void ExpandTrackCheckedChanged(object? sender, RoutedEventArgs e) {
        if (this.ignoreExpandTrackEvent)
            return;

        bool isExpanded = this.ToggleExpandTrackButton!.IsChecked ?? false;
        Track track = this.Owner!.Track!;
        this.ignoreTrackHeightChanged = true;

        if (isExpanded) {
            if (DoubleUtils.AreClose(this.trackHeightBeforeCollapse, Track.MinimumHeight)) {
                this.trackHeightBeforeCollapse = Track.DefaultHeight;
            }

            track.Height = this.trackHeightBeforeCollapse;
        }
        else {
            this.trackHeightBeforeCollapse = track.Height;
            track.Height = Track.MinimumHeight;
        }

        this.ignoreTrackHeightChanged = false;
    }

    private void OnTrackHeightChanged(Track track) {
        if (this.ignoreTrackHeightChanged)
            return;
        this.UpdateTrackHeightExpander();
    }

    private void UpdateTrackHeightExpander() {
        this.ignoreExpandTrackEvent = true;
        if (Maths.Equals(this.Owner!.Track!.Height, Track.MinimumHeight)) {
            this.trackHeightBeforeCollapse = Track.DefaultHeight;
            this.ToggleExpandTrackButton!.IsChecked = false;
        }
        else {
            this.ToggleExpandTrackButton!.IsChecked = true;
        }

        this.ignoreExpandTrackEvent = false;
    }

    public void Connect(TrackControlSurfaceItem trackList) {
        this.Owner = trackList;
        this.OnConnected();
    }

    public void Disconnect() {
        this.OnDisconnected();
        this.Owner = null;
    }

    public virtual void OnConnected() {
        Track track = this.Owner!.Track!;
        this.displayNameBinder.AttachModel(track);
        this.trackColourBinder.AttachModel(track);
        track.HeightChanged += this.OnTrackHeightChanged;
        this.UpdateTrackHeightExpander();

        foreach (Parameter parameter in Parameter.GetApplicableParameters(track.GetType())) {
            this.parameterList.Add(new TrackListItemParameterViewModel(this, parameter));
        }

        if (this.ParameterComboBox != null)
            this.ParameterComboBox.SelectedIndex = 0;

        foreach (Button actionButton in this.actionButtons) {
            actionButton.IsEnabled = true;
        }
    }

    public virtual void OnDisconnected() {
        this.displayNameBinder.DetachModel();
        this.trackColourBinder.DetachModel();
        this.Owner!.Track!.HeightChanged -= this.OnTrackHeightChanged;

        foreach (TrackListItemParameterViewModel item in this.parameterList)
            item.Disconnect();
        this.parameterList.Clear();

        foreach (Button actionButton in this.actionButtons) {
            actionButton.IsEnabled = false;
        }
    }

    public void OnIsAutomationVisibilityChanged(bool isVisible) {
        if (this.AutomationPanel != null)
            this.AutomationPanel.IsVisible = isVisible;
    }
}

public class TrackListItemParameterViewModel : INotifyPropertyChanged {
    public Parameter Parameter { get; }

    public AutomationSequence Sequence { get; }

    public string Name => this.Parameter.Key.Name;

    public string FullName => this.Parameter.Key.ToString();

    public bool IsAutomated { get; private set; }

    public bool IsOverrideEnabled { get; private set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public TrackListItemParameterViewModel(TrackControlSurface owner, Parameter parameter) {
        this.Parameter = parameter;
        this.Sequence = owner.Owner!.Track!.AutomationData[parameter];
        this.Sequence.OverrideStateChanged += this.SequenceOverrideStateChanged;
        this.Sequence.KeyFrameAdded += this.SequenceKeyFrameCollectionChanged;
        this.Sequence.KeyFrameRemoved += this.SequenceKeyFrameCollectionChanged;
        this.IsAutomated = !this.Sequence.IsEmpty;
        this.IsOverrideEnabled = this.Sequence.IsOverrideEnabled;
    }

    private void SequenceOverrideStateChanged(AutomationSequence sequence) {
        this.IsOverrideEnabled = sequence.IsOverrideEnabled;
        this.OnPropertyChanged(nameof(this.IsOverrideEnabled));
    }

    private void SequenceKeyFrameCollectionChanged(AutomationSequence sequence, KeyFrame keyframe, int index) {
        bool isAutomated = !sequence.IsEmpty;
        if (this.IsAutomated != isAutomated) {
            this.IsAutomated = isAutomated;
            this.OnPropertyChanged(nameof(this.IsAutomated));
        }
    }

    public void Disconnect() {
        this.Sequence.OverrideStateChanged -= this.SequenceOverrideStateChanged;
        this.Sequence.KeyFrameAdded -= this.SequenceKeyFrameCollectionChanged;
        this.Sequence.KeyFrameRemoved -= this.SequenceKeyFrameCollectionChanged;
    }

    public override string ToString() {
        return this.Name;
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}