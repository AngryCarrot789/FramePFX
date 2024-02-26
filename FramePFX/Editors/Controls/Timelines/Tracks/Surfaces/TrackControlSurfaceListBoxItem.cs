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
using System.Windows;
using System.Windows.Controls;
using FramePFX.AdvancedMenuService.ContextService.Controls;
using FramePFX.Editors.Contextual;
using FramePFX.Editors.Controls.Bindings;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Interactivity.Contexts;
using FramePFX.PropertyEditing;
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

        private readonly GetSetAutoEventPropertyBinder<Track> isSelectedBinder = new GetSetAutoEventPropertyBinder<Track>(IsSelectedProperty, nameof(VideoTrack.IsSelectedChanged), b => b.Model.IsSelected.Box(), (b, v) => b.Model.SetIsSelected((bool) v, (bool) v));
        private bool wasFocusedBeforeMoving;

        public TrackControlSurfaceListBoxItem() {
            AdvancedContextMenu.SetContextGenerator(this, TrackContextRegistry.Instance);
            this.Selected += this.OnSelectionChanged;
            this.Unselected += this.OnSelectionChanged;
        }

        private void OnSelectionChanged(object sender, RoutedEventArgs e) {
            VideoEditorPropertyEditor.Instance.UpdateTrackSelectionAsync(this.TrackList.Timeline);
        }

        static TrackControlSurfaceListBoxItem() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TrackControlSurfaceListBoxItem), new FrameworkPropertyMetadata(typeof(TrackControlSurfaceListBoxItem)));
        }

        private void OnTrackHeightChanged(Track track) {
            this.Height = track.Height;
        }

        #region Model Linkage

        public void OnAddingToList(TrackControlSurfaceListBox ownerList, Track track) {
            this.Track = track ?? throw new ArgumentNullException(nameof(track));
            this.TrackList = ownerList;
            this.Track.HeightChanged += this.OnTrackHeightChanged;
            this.Content = ownerList.GetContentObject(track.GetType());
        }

        public void OnAddedToList() {
            ((TrackControlSurface) this.Content).Connect(this);
            this.Height = this.Track.Height;
            this.isSelectedBinder.Attach(this, this.Track);
            DataManager.SetContextData(this, new ContextData().Set(DataKeys.TrackKey, this.Track));
        }

        public void OnRemovingFromList() {
            this.Track.HeightChanged -= this.OnTrackHeightChanged;
            this.isSelectedBinder.Detatch();
            TrackControlSurface content = (TrackControlSurface) this.Content;
            content.Disconnect();
            this.Content = null;
            this.TrackList.ReleaseContentObject(this.Track.GetType(), content);
            DataManager.ClearContextData(this);
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