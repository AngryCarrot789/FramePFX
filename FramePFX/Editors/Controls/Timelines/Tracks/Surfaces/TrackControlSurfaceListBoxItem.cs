using System;
using System.Windows;
using System.Windows.Controls;
using FramePFX.AdvancedContextService.WPF;
using FramePFX.Editors.Contextual;
using FramePFX.Editors.Controls.Binders;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Interactivity.DataContexts;
using FramePFX.Shortcuts.WPF;
using FramePFX.Utils;

namespace FramePFX.Editors.Controls.Timelines.Tracks.Surfaces {
    /// <summary>
    /// A track item for the track list box. This control represents the track control surface,
    /// which lets you modify things like volume, opacity, selected automation parameter, etc.
    /// <para>
    /// This control surface is stored in our <see cref="ContentControl.Content"/> property, which
    /// is created dynamically based on the type of track that is connected (via <see cref="OnAddingToList"/>)
    /// </para>
    /// </summary>
    public class TrackControlSurfaceListBoxItem : ListBoxItem {
        /// <summary>
        /// Gets this track item's associated track model
        /// </summary>
        public Track Track { get; private set; }

        /// <summary>
        /// Gets our owner list box
        /// </summary>
        public TrackControlSurfaceListBox TrackList { get; private set; }

        private readonly GetSetAutoPropertyBinder<Track> isSelectedBinder = new GetSetAutoPropertyBinder<Track>(IsSelectedProperty, nameof(VideoTrack.IsSelectedChanged), b => b.Model.IsSelected.Box(), (b, v) => b.Model.SetIsSelected((bool) v, (bool) v));
        private bool wasFocusedBeforeMoving;

        public TrackControlSurfaceListBoxItem() {
            AdvancedContextMenu.SetContextGenerator(this, TrackContextRegistry.Instance);
        }

        static TrackControlSurfaceListBoxItem() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (TrackControlSurfaceListBoxItem), new FrameworkPropertyMetadata(typeof(TrackControlSurfaceListBoxItem)));
        }

        private void OnTrackHeightChanged(Track track) {
            this.Height = track.Height;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
            base.OnPropertyChanged(e);
            this.isSelectedBinder?.OnPropertyChanged(e);
        }

        #region Model Linkage

        public void OnAddingToList(TrackControlSurfaceListBox ownerList, Track track) {
            this.Track = track ?? throw new ArgumentNullException(nameof(track));
            this.TrackList = ownerList;
            this.Track.HeightChanged += this.OnTrackHeightChanged;
            this.Content = ownerList.GetContentObject(track.GetType());
            UIInputManager.SetActionSystemDataContext(this, new DataContext().Set(DataKeys.TrackKey, track));
        }

        public void OnAddedToList() {
            ((TrackControlSurface) this.Content).Connect(this);
            this.Height = this.Track.Height;
            this.isSelectedBinder.Attach(this, this.Track);
        }

        public void OnRemovingFromList() {
            this.Track.HeightChanged -= this.OnTrackHeightChanged;
            this.isSelectedBinder.Detatch();
            TrackControlSurface content = (TrackControlSurface) this.Content;
            content.Disconnect();
            this.Content = null;
            this.TrackList.ReleaseContentObject(this.Track.GetType(), content);
            UIInputManager.ClearActionSystemDataContext(this);
        }

        public void OnRemovedFromList() {
            this.TrackList = null;
            this.Track = null;
        }

        public void OnIndexMoving(int oldIndex, int newIndex) {
            this.isSelectedBinder.Detatch();
            this.wasFocusedBeforeMoving = this.IsFocused;
        }

        public void OnIndexMoved(int oldIndex, int newIndex) {
            this.isSelectedBinder.Attach(this, this.Track);
            this.Height = this.Track.Height;
            if (this.wasFocusedBeforeMoving) {
                this.wasFocusedBeforeMoving = false;
                this.Focus();
            }
        }

        #endregion
    }
}