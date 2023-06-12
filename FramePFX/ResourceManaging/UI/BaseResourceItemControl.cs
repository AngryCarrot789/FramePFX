using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using FramePFX.Core.Editor.ResourceManaging;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;

namespace FramePFX.ResourceManaging.UI {
    public abstract class BaseResourceItemControl : ContentControl {
        public static readonly DependencyProperty HeaderTextProperty =
            DependencyProperty.Register(
                "HeaderText",
                typeof(string),
                typeof(BaseResourceItemControl),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty HeaderBackgroundProperty =
            DependencyProperty.Register(
                "HeaderBackground",
                typeof(Brush),
                typeof(BaseResourceItemControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty ContentBackgroundProperty =
            DependencyProperty.Register(
                "ContentBackground",
                typeof(Brush),
                typeof(BaseResourceItemControl),
                new PropertyMetadata(null));

        public Brush HeaderBackground {
            get => (Brush) this.GetValue(HeaderBackgroundProperty);
            set => this.SetValue(HeaderBackgroundProperty, value);
        }

        public Brush ContentBackground {
            get => (Brush) this.GetValue(ContentBackgroundProperty);
            set => this.SetValue(ContentBackgroundProperty, value);
        }

        public static readonly DependencyProperty IsSelectedProperty =
            Selector.IsSelectedProperty.AddOwner(
                typeof(BaseResourceItemControl),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d,e) => ((BaseResourceItemControl) d).OnIsSelectedChanged(e)));

        public bool IsSelected {
            get => (bool) this.GetValue(IsSelectedProperty);
            set => this.SetValue(IsSelectedProperty, value);
        }

        public string HeaderText {
            get => (string) this.GetValue(HeaderTextProperty);
            set => this.SetValue(HeaderTextProperty, value);
        }

        public ResourceListControl ParentList => ItemsControl.ItemsControlFromItemContainer(this) as ResourceListControl;

        private Point originMousePoint;
        private bool isDragActive;
        private bool isDragDropping;

        public BaseResourceItemControl() {

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
                    if (!(this.DataContext is ResourceItemViewModel resource)) {
                        return;
                    }

                    try {
                        this.isDragDropping = true;
                        DragDrop.DoDragDrop(this, new DataObject(nameof(ResourceItem), resource.Model), DragDropEffects.Copy | DragDropEffects.Move);
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