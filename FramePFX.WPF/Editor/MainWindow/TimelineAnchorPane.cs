using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using AvalonDock.Layout;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.WPF.Editor.Timelines;
using FramePFX.WPF.Editor.Timelines.Controls;

namespace FramePFX.WPF.Editor.MainWindow {
    public class TimelineAnchorPane : LayoutAnchorablePane {
        public static readonly DependencyProperty TimelinesProperty =
            DependencyProperty.Register(
                "Timelines",
                typeof(IEnumerable<TimelineViewModel>),
                typeof(TimelineAnchorPane),
                new PropertyMetadata(null, (d, e) => ((TimelineAnchorPane) d).OnTimelinesPropertyChanged((IEnumerable<TimelineViewModel>) e.OldValue, (IEnumerable<TimelineViewModel>) e.NewValue)));

        public static readonly DependencyProperty SelectedTimelineProperty =
            DependencyProperty.Register(
                "SelectedTimeline",
                typeof(TimelineViewModel),
                typeof(TimelineAnchorPane),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) => ((TimelineAnchorPane) d).OnSelectedTimelineChanged((TimelineViewModel) e.OldValue, (TimelineViewModel) e.NewValue)));

        public IEnumerable<TimelineViewModel> Timelines {
            get => (IEnumerable<TimelineViewModel>) this.GetValue(TimelinesProperty);
            set => this.SetValue(TimelinesProperty, value);
        }

        public TimelineViewModel SelectedTimeline {
            get => (TimelineViewModel) this.GetValue(SelectedTimelineProperty);
            set => this.SetValue(SelectedTimelineProperty, value);
        }

        private bool isProcessingTimelineList;

        public TimelineAnchorPane() {
            this.IsVisible = true;
            this.PropertyChanged += this.OnPropertyChanged;
        }

        protected override void OnChildrenCollectionChanged() {
            base.OnChildrenCollectionChanged();
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(this.SelectedContent)) {
                if (this.SelectedContentIndex != -1) {
                    this.GetAnchorAtIndex(this.SelectedContentIndex, out TimelineControl control);
                    this.SelectedTimeline = (TimelineViewModel) control.DataContext;
                }
                else {
                    this.SelectedTimeline = null;
                }
            }
        }

        public void ScheduleArrange(LayoutAnchorable anchorable) {
            // this.Dispatcher.InvokeAsync(() => {
            //     TimelineControl ctrl = ((TimelineControl) anchorable.Content).Timeline;
            //     ctrl.TimelineEditor.InvalidateMeasure();
            // }, DispatcherPriority.Background);
        }

        protected override void OnIsVisibleChanged() {
            base.OnIsVisibleChanged();
            if (!this.IsVisible) {
                this.IsVisible = true;
            }
        }

        private void OnTimelinesPropertyChanged(IEnumerable<TimelineViewModel> oldList, IEnumerable<TimelineViewModel> newList) {
            if (ReferenceEquals(oldList, newList)) {
                return;
            }

            if (oldList is INotifyCollectionChanged)
                ((INotifyCollectionChanged) oldList).CollectionChanged -= this.OnTimelineCollectionChanged;
            if (newList is INotifyCollectionChanged)
                ((INotifyCollectionChanged) newList).CollectionChanged += this.OnTimelineCollectionChanged;
            this.RegenerateCollection();
        }

        private void OnSelectedTimelineChanged(TimelineViewModel oldItem, TimelineViewModel newItem) {
            if (ReferenceEquals(oldItem, newItem)) {
                return;
            }

            this.SelectedContentIndex = newItem == null ? -1 : this.GetAnchorIndexForTimeline(newItem);
        }

        public void ClearCollection() {
            foreach (LayoutAnchorable anchorable in this.Children) {
                this.ItemDeconstruct(anchorable);
            }

            this.Children.Clear();
        }

        public void RegenerateCollection() {
            this.ClearCollection();
            if (this.Timelines is IEnumerable<TimelineViewModel> list) {
                int i = 0;
                foreach (TimelineViewModel timeline in list) {
                    this.GenerateItemForTimeline(i++, timeline);
                }

                if (this.Children.Count > 0) {
                    this.ScheduleArrange(this.SelectedContentIndex != -1 ? this.Children[this.SelectedContentIndex] : this.Children[0]);
                }
            }
        }

        public void GenerateItemForTimeline(int index, TimelineViewModel timeline) {
            LayoutAnchorable anchorable = new LayoutAnchorable {
                Content = new TimelineControl() {
                    DataContext = timeline
                },
                CanClose = false,
                CanDockAsTabbedDocument = false,
                CanHide = false,
                CanFloat = false,
                CanMove = false,
                CanAutoHide = false
            };

            this.ItemConstruct(anchorable, timeline);
            this.Children.Insert(index, anchorable);
            this.ScheduleArrange(anchorable);
        }

        public void ItemConstruct(LayoutAnchorable anchorable, TimelineViewModel timeline) {
            BindingOperations.SetBinding(anchorable, LayoutContent.TitleProperty, new Binding(nameof(timeline.DisplayName)) {
                Source = timeline,
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
        }

        public void ItemDeconstruct(LayoutAnchorable anchorable) {
            BindingOperations.ClearBinding(anchorable, LayoutContent.TitleProperty);
        }

        public void RemoveTimelineAt(int index) {
            LayoutAnchorable anchorable = this.GetAnchorAtIndex(index, out TimelineControl control);
            this.ItemDeconstruct(anchorable);
            this.Children.RemoveAt(index);
        }

        public int GetAnchorIndexForTimeline(TimelineViewModel timeline) {
            for (int i = 0; i < this.Children.Count; i++) {
                if (((TimelineControl) this.Children[i].Content).DataContext == timeline) {
                    return i;
                }
            }

            return -1;
        }

        public LayoutAnchorable GetAnchorForTimeline(TimelineViewModel timeline, out TimelineControl control) {
            foreach (LayoutAnchorable anchorable in this.Children) {
                if ((control = (TimelineControl) anchorable.Content).DataContext == timeline) {
                    return anchorable;
                }
            }

            control = null;
            return null;
        }

        public LayoutAnchorable GetAnchorAtIndex(int index, out TimelineControl control) {
            LayoutAnchorable item = this.Children[index];
            control = (TimelineControl) item.Content;
            return item;
        }

        private void OnTimelineCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            // possible items corruption due to this.isProcessingTimelineList...
            if (this.isProcessingTimelineList) {
                return;
            }

            this.isProcessingTimelineList = true;
            try {
                switch (e.Action) {
                    case NotifyCollectionChangedAction.Add: {
                        int i = e.NewStartingIndex;
                        foreach (object item in e.NewItems) {
                            this.GenerateItemForTimeline(i++, (TimelineViewModel) item ?? throw new Exception("Cannot add null item to collection"));
                        }

                        break;
                    }
                    case NotifyCollectionChangedAction.Remove: {
                        if (e.OldItems.Count != 1)
                            throw new Exception("Cannot remove more than 1 item");
                        this.RemoveTimelineAt(e.OldStartingIndex);
                        break;
                    }
                    case NotifyCollectionChangedAction.Replace: {
                        this.RemoveTimelineAt(e.OldStartingIndex);
                        this.GenerateItemForTimeline(e.OldStartingIndex, (TimelineViewModel) e.NewItems[0] ?? throw new Exception("Cannot add null item to collection"));
                        break;
                    }
                    case NotifyCollectionChangedAction.Move: {
                        this.MoveChild(e.OldStartingIndex, e.NewStartingIndex);
                        break;
                    }
                    case NotifyCollectionChangedAction.Reset: {
                        this.ClearCollection();
                        break;
                    }
                    default: throw new ArgumentOutOfRangeException();
                }
            }
            finally {
                this.isProcessingTimelineList = false;
            }
        }
    }
}