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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using FramePFX.Editors.Automation.Keyframes;
using FramePFX.Editors.Automation.Params;
using FramePFX.Editors.Controls.Bindings;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Utils;
using SkiaSharp;
using Track = FramePFX.Editors.Timelines.Tracks.Track;

namespace FramePFX.Editors.Controls.Timelines.Tracks.Surfaces {
    /// <summary>
    /// A control which represents the contents of a <see cref="TrackControlSurfaceListBox"/>
    /// </summary>
    public class TrackControlSurface : Control {
        private static readonly Dictionary<Type, Func<TrackControlSurface>> Constructors = new Dictionary<Type, Func<TrackControlSurface>>();
        private static readonly DependencyPropertyKey TrackColourBrushPropertyKey = DependencyProperty.RegisterReadOnly("TrackColourBrush", typeof(Brush), typeof(TrackControlSurface), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty TrackColourBrushProperty = TrackColourBrushPropertyKey.DependencyProperty;
        public static readonly DependencyProperty DisplayNameProperty = DependencyProperty.Register("DisplayName", typeof(string), typeof(TrackControlSurface), new PropertyMetadata(null));

        public TrackControlSurfaceListBoxItem Owner { get; private set; }

        public Brush TrackColourBrush {
            get => (Brush) this.GetValue(TrackColourBrushProperty);
            private set => this.SetValue(TrackColourBrushPropertyKey, value);
        }

        public string DisplayName {
            get => (string) this.GetValue(DisplayNameProperty);
            set => this.SetValue(DisplayNameProperty, value);
        }

        private readonly GetSetAutoEventPropertyBinder<Track> displayNameBinder = new GetSetAutoEventPropertyBinder<Track>(DisplayNameProperty, nameof(Track.DisplayNameChanged), b => b.Model.DisplayName, (b, v) => b.Model.DisplayName = (string) v);

        private readonly UpdaterAutoEventPropertyBinder<Track> trackColourBinder = new UpdaterAutoEventPropertyBinder<Track>(TrackColourBrushProperty, nameof(Track.ColourChanged), binder => {
            TrackControlSurface element = (TrackControlSurface) binder.Control;
            SKColor c = element.Owner.Track?.Colour ?? SKColors.Black;
            ((SolidColorBrush) element.TrackColourBrush).Color = Color.FromArgb(c.Alpha, c.Red, c.Green, c.Blue);
        }, binder => {
            TrackControlSurface element = (TrackControlSurface) binder.Control;
            Color c = ((SolidColorBrush) element.TrackColourBrush).Color;
            element.Owner.Track.Colour = new SKColor(c.R, c.G, c.B, c.A);
        });

        public ToggleButton ToggleExpandTrackButton { get; private set; }

        public ComboBox ParameterComboBox { get; private set; }

        public Button InsertKeyFrameButton { get; private set; }

        public Button ToggleOverrideButton { get; private set; }
        public FrameworkElement AutomationPanel { get; private set; }

        private double trackHeightBeforeCollapse;
        private bool ignoreTrackHeightChanged;
        private bool ignoreExpandTrackEvent;
        private readonly List<Button> actionButtons;

        private bool isProcessingParameterSelectionChanged;
        private readonly ObservableCollection<TrackListItemParameterViewModel> parameterList;
        private TrackListItemParameterViewModel selectedParameter;

        public TrackControlSurface() {
            this.TrackColourBrush = new SolidColorBrush(Colors.Black);
            this.actionButtons = new List<Button>();
            this.parameterList = new ObservableCollection<TrackListItemParameterViewModel>();
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.GetTemplateChild("PART_ExpandTrackButton", out ToggleButton expandButton);
            this.GetTemplateChild("PART_InsertKeyFrameButton", out Button insertKeyFrameButton);
            this.GetTemplateChild("PART_OverrideButton", out Button toggleOverrideButton);
            this.GetTemplateChild("PART_ParameterComboBox", out ComboBox paramComboBox);
            this.GetTemplateChild("PART_AutomationPanel", out FrameworkElement automationPanel);

            this.AutomationPanel = automationPanel;

            this.ToggleExpandTrackButton = expandButton;
            expandButton.IsThreeState = false;
            expandButton.Checked += this.ExpandTrackCheckedChanged;
            expandButton.Unchecked += this.ExpandTrackCheckedChanged;

            this.InsertKeyFrameButton = insertKeyFrameButton;
            this.CreateBasicButtonAction(insertKeyFrameButton, () => {
                Parameter parameter = this.selectedParameter?.Parameter;
                if (parameter != null) {
                    Track track = this.Owner.Track;
                    if (track.GetRelativePlayHead(out long playHead)) {
                        AutomationSequence seq = track.AutomationData[parameter];
                        object value = parameter.GetCurrentObjectValue(track);
                        seq.AddNewKeyFrame(playHead, out KeyFrame keyFrame);
                        keyFrame.SetValueFromObject(value);
                        seq.UpdateValue(playHead);
                    }
                }
            });

            this.ToggleOverrideButton = insertKeyFrameButton;
            this.CreateBasicButtonAction(toggleOverrideButton, () => {
                if (this.selectedParameter != null) {
                    AutomationSequence seq = this.Owner.Track.AutomationData[this.selectedParameter.Parameter];
                    seq.IsOverrideEnabled = !seq.IsOverrideEnabled;
                }
            });

            paramComboBox.ItemsSource = this.parameterList;
            paramComboBox.SelectionChanged += this.OnParameterSelectionChanged;
            this.ParameterComboBox = paramComboBox;
            this.UpdateForSelectedParameter(this.selectedParameter);
        }

        protected void GetTemplateChild<T>(string name, out T value) where T : DependencyObject {
            if ((value = this.GetTemplateChild(name) as T) == null)
                throw new Exception("Missing part: " + name);
        }

        public static void RegisterType<T>(Type trackType, Func<T> func) where T : TrackControlSurface {
            Constructors[trackType] = func;
        }

        static TrackControlSurface() {
            RegisterType(typeof(VideoTrack), () => new TrackControlSurfaceVideo());
            RegisterType(typeof(AudioTrack), () => new TrackControlSurfaceAudio());
        }

        public static TrackControlSurface NewInstance(Type trackType) {
            if (trackType == null) {
                throw new ArgumentNullException(nameof(trackType));
            }

            // Just try to find a base control type. It should be found first try unless I forgot to register a new control type
            bool hasLogged = false;
            for (Type type = trackType; type != null; type = type.BaseType) {
                if (Constructors.TryGetValue(type, out Func<TrackControlSurface> func)) {
                    return func();
                }

                if (!hasLogged) {
                    hasLogged = true;
                    Debugger.Break();
                    Debug.WriteLine("Could not find control for track type on first try. Scanning base types");
                }
            }

            throw new Exception("No such content control for track type: " + trackType.Name);
        }

        private void OnParameterSelectionChanged(object sender, SelectionChangedEventArgs e) {
            this.isProcessingParameterSelectionChanged = true;
            this.selectedParameter = this.ParameterComboBox.SelectedItem as TrackListItemParameterViewModel;
            this.UpdateForSelectedParameter(this.selectedParameter);
            this.isProcessingParameterSelectionChanged = false;
        }

        private void UpdateForSelectedParameter(TrackListItemParameterViewModel selected) {
            this.InsertKeyFrameButton.IsEnabled = selected != null;
            this.ToggleOverrideButton.IsEnabled = selected != null;
            if (this.Owner?.TrackList?.TimelineControl is TimelineControl control) {
                Track trackModel = this.Owner.Track;
                TimelineTrackControl track = control.GetTimelineControlFromTrack(trackModel);
                if (track?.AutomationEditor != null) {
                    track.AutomationEditor.Sequence = selected != null ? trackModel.AutomationData[selected.Parameter] : null;
                }
            }
        }

        private void ExpandTrackCheckedChanged(object sender, RoutedEventArgs e) {
            if (this.ignoreExpandTrackEvent)
                return;

            bool isExpanded = this.ToggleExpandTrackButton.IsChecked ?? false;
            Track track = this.Owner.Track;
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
            if (Maths.Equals(this.Owner.Track.Height, Track.MinimumHeight)) {
                this.trackHeightBeforeCollapse = Track.DefaultHeight;
                this.ToggleExpandTrackButton.IsChecked = false;
            }
            else {
                this.ToggleExpandTrackButton.IsChecked = true;
            }

            this.ignoreExpandTrackEvent = false;
        }

        public void Connect(TrackControlSurfaceListBoxItem owner) {
            this.Owner = owner;
            this.OnConnected();
        }

        public void Disconnect() {
            this.OnDisconnected();
            this.Owner = null;
        }

        protected virtual void OnConnected() {
            Track track = this.Owner.Track;
            this.displayNameBinder.Attach(this, track);
            this.trackColourBinder.Attach(this, track);
            track.HeightChanged += this.OnTrackHeightChanged;
            this.UpdateTrackHeightExpander();

            foreach (Parameter parameter in Parameter.GetApplicableParameters(track.GetType())) {
                this.parameterList.Add(new TrackListItemParameterViewModel(this, parameter));
            }

            foreach (Button actionButton in this.actionButtons) {
                actionButton.IsEnabled = true;
            }
        }

        protected virtual void OnDisconnected() {
            this.displayNameBinder.Detatch();
            this.trackColourBinder.Detatch();
            this.Owner.Track.HeightChanged -= this.OnTrackHeightChanged;

            foreach (TrackListItemParameterViewModel item in this.parameterList)
                item.Disconnect();
            this.parameterList.Clear();

            foreach (Button actionButton in this.actionButtons) {
                actionButton.IsEnabled = false;
            }
        }

        protected Button CreateBasicButtonAction(Button button, Action action) {
            this.actionButtons.Add(button);
            button.Click += (sender, args) => {
                if (this.Owner != null)
                    action();
            };

            return button;
        }

        public void SetAutomationVisibility(bool visibility) {
            if (this.AutomationPanel != null)
                this.AutomationPanel.Visibility = visibility ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public class TrackListItemParameterViewModel : INotifyPropertyChanged {
        public Parameter Parameter { get; }

        public AutomationSequence Sequence { get; }

        public string Name => this.Parameter.Key.Name;

        public string FullName => this.Parameter.Key.ToString();

        public bool IsAutomated { get; private set; }

        public bool IsOverrideEnabled { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public TrackListItemParameterViewModel() {
        }

        public TrackListItemParameterViewModel(TrackControlSurface owner, Parameter parameter) {
            this.Parameter = parameter;
            this.Sequence = owner.Owner.Track.AutomationData[parameter];
            this.Sequence.OverrideStateChanged += this.SequenceOverrideStateChanged;
            this.Sequence.KeyFrameAdded += this.SequenceKeyFrameCollectionChanged;
            this.Sequence.KeyFrameRemoved += this.SequenceKeyFrameCollectionChanged;
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
}