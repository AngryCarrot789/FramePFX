using System;
using System.Collections;
using System.Collections.Specialized;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;

namespace FramePFX.Editor.Automation {
    [TemplatePart(Name = nameof(PART_Canvas), Type = typeof(Canvas))]
    public class AutomationSequenceEditor : Control {
        public static readonly DependencyProperty KeyFrameBrushProperty =
            DependencyProperty.Register(
                "KeyFrameBrush",
                typeof(Brush),
                typeof(AutomationSequenceEditor),
                new FrameworkPropertyMetadata(Brushes.OrangeRed, (o, e) => ((AutomationSequenceEditor) o).OnKeyFrameBrushPropertyChanged((Brush) e.OldValue, (Brush) e.NewValue)));

        public static readonly DependencyProperty CurveBrushProperty =
            DependencyProperty.Register(
                "CurveBrush",
                typeof(Brush),
                typeof(AutomationSequenceEditor),
                new FrameworkPropertyMetadata(Brushes.OrangeRed, (o, e) => ((AutomationSequenceEditor) o).OnCurveBrushPropertyChanged((Brush) e.OldValue, (Brush) e.NewValue)));

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                "ItemsSource",
                typeof(IEnumerable),
                typeof(AutomationSequenceEditor),
                new PropertyMetadata(null, (o, e) => ((AutomationSequenceEditor) o).OnItemsSourcePropertyChanged((IEnumerable) e.OldValue, (IEnumerable) e.NewValue)));

        public static readonly DependencyProperty UnitZoomProperty =
            DependencyProperty.Register(
                "UnitZoom",
                typeof(double),
                typeof(AutomationSequenceEditor),
                new PropertyMetadata(1d, (o, e) => ((AutomationSequenceEditor) o).RegenerateAllPoints()));

        public double UnitZoom {
            get => (double) this.GetValue(UnitZoomProperty);
            set => this.SetValue(UnitZoomProperty, value);
        }

        public Brush KeyFrameBrush {
            get => (Brush) this.GetValue(KeyFrameBrushProperty);
            set => this.SetValue(KeyFrameBrushProperty, value);
        }

        public Brush CurveBrush {
            get => (Brush) this.GetValue(CurveBrushProperty);
            set => this.SetValue(CurveBrushProperty, value);
        }

        public IEnumerable ItemsSource {
            get => (IEnumerable) this.GetValue(ItemsSourceProperty);
            set => this.SetValue(ItemsSourceProperty, value);
        }

        private Canvas PART_Canvas;

        public AutomationSequenceEditor() {
            this.Loaded += this.OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {

        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.PART_Canvas = (Canvas) this.GetTemplateChild(nameof(this.PART_Canvas)) ?? throw new Exception("Missing canvas part");
        }

        protected virtual void OnKeyFrameBrushPropertyChanged(Brush oldValue, Brush newValue) {

        }

        protected virtual void OnCurveBrushPropertyChanged(Brush oldValue, Brush newValue) {

        }

        protected virtual void OnItemsSourcePropertyChanged(IEnumerable oldValue, IEnumerable newValue) {
            if (oldValue is INotifyCollectionChanged) {
                ((INotifyCollectionChanged) oldValue).CollectionChanged -= this.OnCollectionChanged;
            }

            if (newValue is INotifyCollectionChanged) {
                ((INotifyCollectionChanged) newValue).CollectionChanged += this.OnCollectionChanged;
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (this.PART_Canvas == null || !this.IsLoaded) {
                return;
            }

            switch (e.Action) {
                case NotifyCollectionChangedAction.Add: break;
                case NotifyCollectionChangedAction.Remove: break;
                case NotifyCollectionChangedAction.Replace: break;
                case NotifyCollectionChangedAction.Move: break;
                case NotifyCollectionChangedAction.Reset:
                    this.RegenerateAllPoints();
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private void RegenerateAllPoints() {
            if (this.PART_Canvas == null) {
                return;
            }

            this.PART_Canvas.Children.Clear();

            IEnumerable items = this.ItemsSource;
            if (items == null) {
                return;
            }

            foreach (object item in items) {

            }
        }

        private void AddPoint() {

        }
    }
}