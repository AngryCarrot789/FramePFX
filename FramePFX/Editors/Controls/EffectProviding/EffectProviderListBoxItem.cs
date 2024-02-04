using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FramePFX.Editors.EffectSource;
using FramePFX.Utils;

namespace FramePFX.Editors.Controls.EffectProviding {
    public class EffectProviderListBoxItem : ListBoxItem {
        public EffectProviderEntry Model { get; private set; }

        public EffectProviderListBox OwnerList { get; private set; }

        private Point originMousePoint;
        private bool isDragActive;
        private bool isDragDropping;

        public EffectProviderListBoxItem() {
        }

        public void OnAdding(EffectProviderListBox owner, EffectProviderEntry model) {
            this.Model = model;
            this.OwnerList = owner;
        }

        public void OnAdded() {
            this.Content = this.Model.DisplayName;
        }

        public void OnRemoving() {

        }

        public void OnRemoved() {
            this.Model = null;
        }

        #region Drag Dropping

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            if (this.OwnerList != null && !e.Handled && (this.IsFocused || this.Focus())) {
                if (!this.isDragDropping) {
                    this.CaptureMouse();
                    this.originMousePoint = e.GetPosition(this);
                    this.isDragActive = true;
                }
            }

            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
            // weird... this method isn't called when the `DoDragDrop` method
            // returns, even if you release the left mouse button. This means,
            // isDragDropping is always false here
            if (this.isDragActive) {
                this.isDragActive = false;
                if (this.IsMouseCaptured) {
                    this.ReleaseMouseCapture();
                }
            }

            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            if (!this.isDragActive || this.isDragDropping || this.Model == null || this.OwnerList == null) {
                return;
            }

            if (e.LeftButton == MouseButtonState.Pressed) {
                Point posA = e.GetPosition(this);
                Point posB = this.originMousePoint;
                Point change = new Point(Math.Abs(posA.X - posB.X), Math.Abs(posA.X - posB.X));
                if (change.X > 5 || change.Y > 5) {
                    try {
                        this.isDragDropping = true;
                        DragDrop.DoDragDrop(this, new DataObject(EffectProviderListBox.EffectProviderDropType, this.Model), DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link);
                    }
                    catch (Exception ex) {
                        Debugger.Break();
                        Debug.WriteLine("Exception while executing resource item drag drop: " + ex.GetToString());
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

        #endregion
    }
}