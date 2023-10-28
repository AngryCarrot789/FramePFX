using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using FramePFX.Actions.Contexts;
using FramePFX.Editor.Registries;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Editor.Timelines.Effects.Video;
using FramePFX.Editor.Timelines.VideoClips;
using FramePFX.Editor.ViewModels;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.Interactivity;
using FramePFX.Logger;
using FramePFX.PropertyEditing;
using FramePFX.Utils;
using FramePFX.WPF.Editor.Resources;
using FramePFX.WPF.Editor.Timeline.Track;
using FramePFX.WPF.Editor.Timeline.Utils;
using FramePFX.WPF.Interactivity;
using SkiaSharp;

namespace FramePFX.WPF.Editor.Timeline.Controls {
    [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(TimelineClipControl))]
    public sealed class TimelineTrackControl : MultiSelector {
        /// <summary>
        /// The timeline that contains this track
        /// </summary>
        public TimelineEditorControl Timeline => ItemsControlFromItemContainer(this) as TimelineEditorControl;

        /// <summary>
        /// The zoom level of the associated timeline, or 1, if no timeline is present
        /// </summary>
        public double UnitZoom => this.Timeline?.UnitZoom ?? 1D;

        public TrackViewModel ViewModel => this.DataContext as TrackViewModel;

        private bool isProcessingAsyncDrop;
        public TimelineClipControl lastSelectedItem;

        public TimelineTrackControl() {
            this.CanSelectMultipleItems = true;
            this.AllowDrop = true;
            this.Drop += this.OnDrop;
            this.DragEnter += this.OnDragDropEnter;
            this.DragOver += this.OnDragOver;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e) {
            if (e.Key == Key.System && (e.SystemKey == Key.LeftAlt || e.SystemKey == Key.RightAlt) && e.OriginalSource is TimelineClipControl) {
                e.Handled = true;
                return;
            }

            base.OnPreviewKeyDown(e);
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);
            ResourceManagerViewModel manager;
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0 && e.Key == Key.V && this.DataContext is TrackViewModel track && (manager = track.Project?.ResourceManager) != null) {
                IDataObject dataObject = Clipboard.GetDataObject();
                if (dataObject == null || !dataObject.GetDataPresent(NativeDropTypes.Bitmap)) {
                    return;
                }

                IDataObjekt objekt = new DataObjectWrapper(dataObject);
                if (!objekt.GetBitmap(out SKBitmap bitmap, out int error)) {
                    AppLogger.WriteLine($"Failed to get bitmap from clipboard: {(error == 2 ? "invalid image format" : "invalid object")}");
                    return;
                }

                ResourceImage image = new ResourceImage();
                image.DisplayName = "NewImage_" + RandomUtils.RandomLetters(6);
                bitmap.SetImmutable();
                image.SetBitmapImage(bitmap);
                manager.CurrentFolder.Model.AddItem(image);
                ulong id = manager.Model.RegisterEntry(image);
                ResourceItemViewModel resource = (ResourceItemViewModel) manager.CurrentFolder.LastItem;

                ImageVideoClip imageClip = new ImageVideoClip();
                imageClip.DisplayName = "an image!!! for " + image.DisplayName;
                imageClip.FrameSpan = FramePFX.Editor.Timelines.Track.GetSpanUntilClipOrFuckIt(track.Model, track.Timeline.PlayHeadFrame, maximumDurationToClip: 300);
                imageClip.ResourceImageKey.SetTargetResourceId(id);
                imageClip.AddEffect(new MotionEffect());
                track.Model.AddClip(imageClip);
                track.LastClip.IsSelected = true;
                PFXPropertyEditorRegistry.Instance.OnClipSelectionChanged(new List<ClipViewModel>() {track.LastClip});

                Services.Application.Invoke(async () => {
                    await resource.LoadResourceAsync();
                    VideoEditorViewModel editor = track.Editor;
                    if (editor?.SelectedTimeline != null) {
                        await track.Editor.DoDrawRenderFrame(editor.SelectedTimeline);
                    }
                });
            }
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnPreviewMouseLeftButtonDown(e);
            if (this.DataContext is TrackViewModel track && track.Timeline != null) {
                if (!KeyboardUtils.AreModifiersPressed(ModifierKeys.Control)) {
                    if (track.Timeline.PrimarySelectedTrack != track) {
                        track.Timeline.PrimarySelectedTrack = track;
                        PFXPropertyEditorRegistry.Instance.OnTrackSelectionChanged(track.Timeline.SelectedTracks.ToList());
                    }
                }
            }
        }

        private bool canSetPlayHead;

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e) {
            base.OnPreviewMouseLeftButtonUp(e);
            if (this.canSetPlayHead && !e.Handled && this.DataContext is TrackViewModel track && track.Timeline != null) {
                Point point = e.GetPosition(this);
                track.Timeline.PlayHeadFrame = TimelineUtils.PixelToFrame(point.X, this.Timeline.UnitZoom, true);
            }

            this.canSetPlayHead = false;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);
            // this.canSetPlayHead = true;
            if (this.Timeline.GetSelectedClipContainers().Any(clip => ReferenceEquals(clip.Track, this) && clip.IsMouseOver)) {
                return;
            }

            foreach (TimelineTrackControl trackElement in this.Timeline.GetTrackContainers()) {
                if (trackElement.SelectedItems.Count > 0) {
                    trackElement.UnselectAll();
                }
            }

            this.Focus();
            this.OnSelectionOperationCompleted();
        }

        public IEnumerable<TimelineClipControl> GetClipContainers() {
            return this.GetClipContainers(this.Items);
        }

        public IEnumerable<TimelineClipControl> GetSelectedClipContainers() {
            return this.GetClipContainers(this.SelectedItems);
        }

        public IEnumerable<TimelineClipControl> GetClipContainers(IEnumerable items, bool canUseIcgIndex = true) {
            int i = 0;
            foreach (object item in items) {
                if (item is TimelineClipControl a) {
                    yield return a;
                }
                else if (canUseIcgIndex) {
                    if (this.ItemContainerGenerator.ContainerFromIndex(i) is TimelineClipControl b) {
                        yield return b;
                    }
                }
                else {
                    if (this.ItemContainerGenerator.ContainerFromItem(item) is TimelineClipControl b) {
                        yield return b;
                    }
                }

                i++;
            }
        }

        public IEnumerable<TimelineClipControl> GetClipsThatIntersect(FrameSpan span) => this.GetClipContainers().Where(x => x.Span.Intersects(span));

        private void OnDragDropEnter(object sender, DragEventArgs e) => this.OnDragOver(sender, e);

        private void OnDragOver(object sender, DragEventArgs e) {
            e.Handled = true;
            if (this.isProcessingAsyncDrop || !(this.DataContext is TrackViewModel track)) {
                e.Effects = DragDropEffects.None;
                return;
            }

            EnumDropType inputEffects = DropUtils.GetDropAction((int) e.KeyStates, (EnumDropType) e.Effects);
            if (inputEffects == EnumDropType.None) {
                e.Effects = DragDropEffects.None;
                return;
            }

            EnumDropType outputEffects = EnumDropType.None;
            if (e.Data.GetData(ResourceListControl.ResourceDropType) is List<BaseResourceViewModel> resources) {
                if (resources.Count == 1 && resources[0] is ResourceItemViewModel) {
                    outputEffects = TrackViewModel.DropRegistry.CanDrop(track, resources[0], inputEffects, this.GetDropDataContext(e));
                }
            }
            else {
                outputEffects = TrackViewModel.DropRegistry.CanDropNative(track, new DataObjectWrapper(e.Data), inputEffects, this.GetDropDataContext(e));
            }

            e.Effects = (DragDropEffects) outputEffects;
        }

        private async void OnDrop(object sender, DragEventArgs e) {
            e.Handled = true;
            if (this.isProcessingAsyncDrop || !(this.DataContext is TrackViewModel track)) {
                return;
            }

            EnumDropType effects = DropUtils.GetDropAction((int) e.KeyStates, (EnumDropType) e.Effects);
            if (e.Effects == DragDropEffects.None) {
                return;
            }

            try {
                this.isProcessingAsyncDrop = true;
                if (e.Data.GetData(ResourceListControl.ResourceDropType) is List<BaseResourceViewModel> items) {
                    if (items.Count == 1 && items[0] is ResourceItemViewModel) {
                        await TrackViewModel.DropRegistry.OnDropped(track, items[0], effects, this.GetDropDataContext(e));
                    }
                }
                // TODO: Track effects >:)
                // else if (e.Data.GetData(EffectProviderTreeViewItem.ProviderDropType) is EffectProviderViewModel provider) {
                //     await TrackViewModel.DropRegistry.OnDropped(track, provider, effects, this.GetDropDataContext(e.GetPosition(this)));
                // }
                else {
                    await TrackViewModel.DropRegistry.OnDroppedNative(track, new DataObjectWrapper(e.Data), effects, this.GetDropDataContext(e));
                }
            }
            finally {
                this.isProcessingAsyncDrop = false;
            }
        }

        public IDataContext GetDropDataContext(DragEventArgs e) => this.GetDropDataContext(e.GetPosition(this));

        public IDataContext GetDropDataContext(Point mPos) {
            long frame = TimelineUtils.PixelToFrame(mPos.X, this.UnitZoom);
            frame = Maths.Clamp(frame, 0, this.Timeline?.MaxDuration ?? 0);
            return new DataContext {[TrackViewModel.DroppedFrameKey] = frame};
        }

        public void OnUnitZoomChanged() {
            foreach (TimelineClipControl element in this.GetClipContainers()) {
                element.OnUnitZoomChanged();
            }
        }

        /// <summary>
        /// Creates an index map of this track's clips. The order of this Track's Items changes when a clip
        /// is selected, and also when clips are dragged around. This functions will create a sorted list of the items,
        /// and cache the unordered and ordered indices for fast access in dictionaries
        /// </summary>
        /// <returns></returns>
        public IndexMap<TimelineClipControl> CreateIndexMap() {
            // Dictionary<TimelineClipControl, long> clipToFrame = new Dictionary<TimelineClipControl, long>(this.Items.Count);
            // Dictionary<long, TimelineClipControl> frameToClip = new Dictionary<long, TimelineClipControl>(this.Items.Count);
            int count = this.Items.Count;
            Dictionary<TimelineClipControl, int> clipToRealIndex = new Dictionary<TimelineClipControl, int>(count);
            Dictionary<int, TimelineClipControl> realIndexToClip = new Dictionary<int, TimelineClipControl>(count);
            Dictionary<TimelineClipControl, int> clipToFakeIndex = new Dictionary<TimelineClipControl, int>(count);
            // only named it clipToFakeIndex because it lines up with the other names :3 it should be clipToOrderedIndex
            List<TimelineClipControl> clips = new List<TimelineClipControl>(count);
            IndexMap<TimelineClipControl> map = new IndexMap<TimelineClipControl>(clipToRealIndex, realIndexToClip, clipToFakeIndex, clips);
            int i = 0;

            foreach (TimelineClipControl clip in this.GetClipContainers()) {
                clipToRealIndex[clip] = i;
                realIndexToClip[i++] = clip;
                clips.Add(clip);
            }

            clips.Sort((a, b) => a.FrameBegin.CompareTo(b.FrameBegin));
            for (int j = 0; j < clips.Count; j++)
                clipToFakeIndex[clips[j]] = j;
            return map;
        }

        public void MakeRangedSelection(TimelineClipControl a, TimelineClipControl b) {
            if (a == b) {
                this.MakeSingleSelection(a);
            }
            else {
                IndexMap<TimelineClipControl> map = this.CreateIndexMap();
                int indexA = map.OrderedIndexOf(a);
                if (indexA == -1)
                    return;
                int indexB = map.OrderedIndexOf(b);
                if (indexB == -1)
                    return;

                if (indexA < indexB) {
                    this.UnselectAll();
                    for (int i = indexA; i <= indexB; i++) {
                        int index = map.OrderedIndexToRealIndex(i);
                        if (index != -1) {
                            // using RealIndexToClip may be faster than using ItemContainerGenerator indexing... maybe
                            this.SetItemSelectedProperty(map.RealIndexToValue[index], true, index);
                            // this.SetItemSelectedPropertyAtIndex(index, true);
                        }
                    }
                }
                else if (indexA > indexB) {
                    this.UnselectAll();
                    for (int i = indexB; i <= indexA; i++) {
                        int index = map.OrderedIndexToRealIndex(i);
                        if (index != -1) {
                            this.SetItemSelectedProperty(map.RealIndexToValue[index], true, index);
                            // this.SetItemSelectedPropertyAtIndex(index, true);
                        }
                    }
                }
                else {
                    this.MakeSingleSelection(a);
                }
            }

            this.OnSelectionOperationCompleted();
        }

        public void MakeSingleSelection(TimelineClipControl container) {
            this.UnselectAll();
            this.SetItemSelectedProperty(container, true);
            this.lastSelectedItem = container;
        }

        public void SetItemSelectedProperty(TimelineClipControl container, bool selected, int index = -1) {
            container.IsSelected = selected;
            object x = this.ItemContainerGenerator.ItemFromContainer(container);
            if (x == null || x == DependencyProperty.UnsetValue) {
                x = container;
            }

            if (index == -1)
                index = this.SelectedItems.IndexOf(x);
            if (selected) {
                if (index == -1) {
                    this.SelectedItems.Add(x);
                }
            }
            else {
                if (index != -1) {
                    this.SelectedItems.RemoveAt(index);
                }
            }
        }

        public void OnSelectionOperationCompleted() => this.Timeline.OnSelectionOperationCompleted();

        protected override Size MeasureOverride(Size constraint) {
            Size size = base.MeasureOverride(constraint);
            return this.Timeline is TimelineEditorControl timeline ? new Size(timeline.MaxDuration * this.UnitZoom, constraint.Height) : size;
        }

        protected override DependencyObject GetContainerForItemOverride() => new TimelineClipControl();

        protected override bool IsItemItsOwnContainerOverride(object item) => item is TimelineClipControl;
    }
}