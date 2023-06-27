using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;

namespace FramePFX.Editor.Resources {
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
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            ResourceListControl list = this.ParentList;
            if (list != null && !e.Handled && (this.IsFocused || this.Focus())) {
                if (!this.isDragDropping) {
                    this.CaptureMouse();
                    this.originMousePoint = e.GetPosition(this);
                    this.isDragActive = true;
                }

                e.Handled = true;
                if (ResourceListControl.AreModifiersPressed(ModifierKeys.Control)) {
                }
                else if (ResourceListControl.AreModifiersPressed(ModifierKeys.Shift) && list.lastSelectedItem != null && list.SelectedItems.Count > 0) {
                    list.MakeRangedSelection(list.lastSelectedItem, this);
                }
                else if (!this.IsSelected) {
                    list.MakePrimarySelection(this);
                }
            }

            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
            // weird... this method isn't called when the `DoDragDrop` method
            // returns, even if you release the left mouse button. This means,
            // isDragDropping is always false here

            ResourceListControl list = this.ParentList;
            if (this.isDragActive) {
                this.isDragActive = false;
                if (this.IsMouseCaptured) {
                    this.ReleaseMouseCapture();
                }

                e.Handled = true;
            }

            if (list != null) {
                if (this.IsSelected) {
                    if (!ResourceListControl.AreModifiersPressed(ModifierKeys.Shift)) {
                        if (!ResourceListControl.AreModifiersPressed(ModifierKeys.Control)) {
                            list.MakePrimarySelection(this);
                        }
                        else {
                            list.SetItemSelectedProperty(this, false);
                        }
                    }
                }
                else {
                    if (list.SelectedItems.Count > 1) {
                        if (ResourceListControl.AreModifiersPressed(ModifierKeys.Control)) {
                            list.SetItemSelectedProperty(this, true);
                        }
                        else {
                            list.MakePrimarySelection(this);
                        }
                    }
                    else {
                        if (ResourceListControl.AreModifiersPressed(ModifierKeys.Control)) {
                            list.SetItemSelectedProperty(this, true);
                        }
                    }
                }
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
                    if (!(this.DataContext is BaseResourceObjectViewModel resource)) {
                        return;
                    }

                    try {
                        this.isDragDropping = true;
                        DragDrop.DoDragDrop(this, new DataObject(nameof(BaseResourceObjectViewModel), resource), DragDropEffects.Copy | DragDropEffects.Move);
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