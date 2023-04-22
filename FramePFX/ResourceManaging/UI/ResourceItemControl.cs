using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using FramePFX.ResourceManaging.ViewModels;

namespace FramePFX.ResourceManaging.UI {
    public class ResourceItemControl : ContentControl, IResourceControl {
        public static readonly DependencyProperty UniqueIDHeaderProperty =
            DependencyProperty.Register(
                "UniqueIDHeader",
                typeof(string),
                typeof(ResourceItemControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty HeaderBackgroundProperty =
            DependencyProperty.Register(
                "HeaderBackground",
                typeof(Brush),
                typeof(ResourceItemControl),
                new PropertyMetadata(null));

        public Brush HeaderBackground {
            get => (Brush) this.GetValue(HeaderBackgroundProperty);
            set => this.SetValue(HeaderBackgroundProperty, value);
        }

        public static readonly DependencyProperty IsSelectedProperty =
            Selector.IsSelectedProperty.AddOwner(
                typeof(ResourceItemControl),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d,e) => ((ResourceItemControl) d).OnIsSelectedChanged(e)));

        public bool IsSelected {
            get => (bool) this.GetValue(IsSelectedProperty);
            set => this.SetValue(IsSelectedProperty, value);
        }

        public string UniqueIDHeader {
            get => (string) this.GetValue(UniqueIDHeaderProperty);
            set => this.SetValue(UniqueIDHeaderProperty, value);
        }

        public ResourceListControl ParentList => ItemsControl.ItemsControlFromItemContainer(this) as ResourceListControl;

        public ResourceItemViewModel Resource {
            get => this.DataContext as ResourceItemViewModel;
        }

        private Point originMousePoint;
        private bool isDragActive;
        private bool isDragDropping;

        public ResourceItemControl() {

        }

        private void OnIsSelectedChanged(DependencyPropertyChangedEventArgs e) {
            if (e.NewValue != e.OldValue) {
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            ResourceListControl list = this.ParentList;
            if (list != null && (!e.Handled && this.IsFocused || this.Focus())) {
                if (!this.isDragDropping) {
                    this.CaptureMouse();
                    this.originMousePoint = e.GetPosition(this);
                    this.isDragActive = true;
                }

                e.Handled = true;
                list.OnItemMouseButton(this, e);
            }

            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
            ResourceListControl list = this.ParentList;
            if (this.isDragActive && this.IsMouseCaptured) {
                this.isDragActive = false;
                this.ReleaseMouseCapture();
                e.Handled = true;
            }

            if (list != null && !e.Handled && this.IsFocused) {
                e.Handled = true;
                list.OnItemMouseButton(this, e);
            }

            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            if (!this.isDragActive || this.isDragDropping) {
                return;
            }

            if (e.LeftButton == MouseButtonState.Pressed) {
                Point posA = e.GetPosition(this);
                Point posB = this.originMousePoint;
                Point change = new Point(Math.Abs(posA.X - posB.X), Math.Abs(posA.X - posB.X));
                if (change.X > 5 || change.Y > 5) {
                    ResourceItemViewModel resource = this.Resource;
                    if (resource == null) {
                        return;
                    }

                    this.isDragDropping = true;
                    try {
                        DragDrop.DoDragDrop(this, new DataObject("ResourceItem", resource), DragDropEffects.Copy);
                    }
                    finally {
                        this.isDragDropping = false;
                    }
                }
            }
            else {
                if (ReferenceEquals(e.MouseDevice.Captured, this)) {
                    this.ReleaseMouseCapture();
                }

                this.isDragActive = false;
                this.originMousePoint = new Point(0, 0);
            }
        }
    }
}