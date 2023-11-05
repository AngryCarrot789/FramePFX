using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FramePFX.Editor.ViewModels;
using FramePFX.Logger;
using FramePFX.Utils;
using FramePFX.WPF.Editor.Timelines.Utils;

namespace FramePFX.WPF.Editor.Effects {
    public class EffectProviderTreeViewItem : TreeViewItem {
        public const string ProviderDropType = "PFXEffectProvider_DropType";

        public static readonly DependencyProperty IsDroppableTargetOverProperty = DependencyProperty.Register("IsDroppableTargetOver", typeof(bool), typeof(EffectProviderTreeViewItem), new PropertyMetadata(BoolBox.False));

        public EffectProviderTreeView MyResourceTree {
            get {
                ItemsControl parent = ItemsControlFromItemContainer(this);
                for (; parent != null; parent = ItemsControlFromItemContainer(parent))
                    if (parent is EffectProviderTreeView tree)
                        return tree;
                return null;
            }
        }

        public EffectProviderTreeViewItem MyParentItem => ItemsControlFromItemContainer(this) as EffectProviderTreeViewItem;

        public EffectProviderViewModel EffectProvider => (EffectProviderViewModel) this.DataContext;

        public bool IsDroppableTargetOver {
            get => (bool) this.GetValue(IsDroppableTargetOverProperty);
            set => this.SetValue(IsDroppableTargetOverProperty, value.Box());
        }

        private bool isDragDropping;
        private Point originMousePoint;
        private bool isDragActive;

        public EffectProviderTreeViewItem() {
            this.AllowDrop = true;
        }

        static EffectProviderTreeViewItem() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EffectProviderTreeViewItem), new FrameworkPropertyMetadata(typeof(EffectProviderTreeViewItem)));
        }

        public static bool CanBeginDragDrop() {
            return !KeyboardUtils.AreAnyModifiersPressed(ModifierKeys.Control, ModifierKeys.Shift);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) {
                if (CanBeginDragDrop() && !e.Handled) {
                    if ((this.IsFocused || this.Focus()) && !this.isDragDropping) {
                        this.CaptureMouse();
                        this.originMousePoint = e.GetPosition(this);
                        this.isDragActive = true;
                        e.Handled = true;
                        return;
                    }
                }
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e) {
            if (this.isDragActive && (e.ChangedButton == MouseButton.Left || e.ChangedButton == MouseButton.Right)) {
                this.isDragActive = false;
                if (this.IsMouseCaptured) {
                    this.ReleaseMouseCapture();
                }
            }

            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            if (e.LeftButton != MouseButtonState.Pressed) {
                if (ReferenceEquals(e.MouseDevice.Captured, this)) {
                    this.ReleaseMouseCapture();
                }

                this.isDragActive = false;
                this.originMousePoint = new Point(0, 0);
                return;
            }

            if (!this.isDragActive || this.isDragDropping) {
                return;
            }

            Point posA = e.GetPosition(this);
            Point posB = this.originMousePoint;
            Point change = new Point(Math.Abs(posA.X - posB.X), Math.Abs(posA.X - posB.X));
            if (change.X > 5 || change.Y > 5) {
                if (!(this.DataContext is EffectProviderViewModel provider)) {
                    return;
                }

                try {
                    this.isDragDropping = true;
                    DragDrop.DoDragDrop(this, new DataObject(ProviderDropType, provider), DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link);
                }
                catch (Exception ex) {
                    AppLogger.WriteLine("Exception while executing effect provider tree item drag drop: " + ex.GetToString());
                }
                finally {
                    this.isDragDropping = false;
                }
            }
        }

        protected override bool IsItemItsOwnContainerOverride(object item) {
            return item is EffectProviderTreeViewItem;
        }

        protected override DependencyObject GetContainerForItemOverride() {
            return new EffectProviderTreeViewItem();
        }
    }
}