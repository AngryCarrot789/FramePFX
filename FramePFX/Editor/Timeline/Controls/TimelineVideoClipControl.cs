using System.Windows.Input;
using FramePFX.Core;
using FramePFX.Core.Editor.ViewModels.Timeline.Clips;
using FramePFX.Editor.Timeline.Layer.Clips;

namespace FramePFX.Editor.Timeline.Controls {
    public class TimelineVideoClipControl : TimelineClipControl {
        public new VideoTimelineLayerControl Layer => (VideoTimelineLayerControl) base.Layer;

        public bool IsMovingControl { get; set; }

        public ClipDragData DragData { get; set; }

        public TimelineVideoClipControl() {
            this.DataContextChanged += (sender, args) => {
                if (args.NewValue is VideoClipViewModel vm) {
                    BaseViewModel.SetInternalData(vm, typeof(IClipHandle), this);
                }
            };
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnPreviewMouseLeftButtonDown(e);
            this.Layer.MakeTopElement(this);
            this.lastLeftClickPoint = e.GetPosition(this);
        }

        /*

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            this.isProcessingMouseAction = true;
            if (!e.Handled) {
                if (this.IsFocused || this.Focus()) {
                    e.Handled = true;
                    this.Layer.OnClipMouseButton(this, e);
                }

                this.isClipDragActivated = true;
            }

            base.OnMouseLeftButtonDown(e);
            this.isProcessingMouseAction = false;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
            this.isProcessingMouseAction = true;
            this.isClipDragActivated = false;
            if (this.IsMouseCaptured) {
                this.ReleaseMouseCapture();
            }

            bool hasDrag = false;
            if (this.Timeline.HasActiveDrag()) {
                this.Timeline.DragData.OnCompleted();
                this.Timeline.DragData = null;
                hasDrag = true;
            }

            if (!e.Handled) {
                e.Handled = true;
                this.Layer.OnClipMouseButton(this, e, hasDrag);
            }

            base.OnMouseLeftButtonUp(e);
            this.isProcessingMouseAction = false;
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            if (this.IsMovingControl || this.isProcessingMouseAction || this.isCancellingDragAction) {
                return;
            }

            if (!this.isClipDragActivated) {
                return;
            }

            if (this.PART_ThumbLeft.IsDragging || this.PART_ThumbRight.IsDragging) {
                return;
            }

            Point mousePoint = e.GetPosition(this);
            if (this.Timeline.HasActiveDrag()) {
                if (this.Timeline.DragData.IsBeingDragged(this)) {
                    if (this.IsMouseCaptured || this.CaptureMouse()) {
                        if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) {
                            this.Timeline.DragData.OnEnterCopyMove();
                        }
                        else if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) {
                            this.Timeline.DragData.OnEnterMoveMode();
                        }

                        this.isProcessingMouseAction = true;
                        // TODO: somehow implement cross-layer drag drop...
                        double diffX = mousePoint.X - this.lastLeftClickPoint.X;
                        if (Math.Abs(diffX) >= 1.0d) {
                            long offset = (long) (diffX / this.UnitZoom);
                            if (offset != 0) {
                                if ((this.FrameBegin + offset) < 0) {
                                    offset = -this.FrameBegin;
                                }

                                if (offset != 0) {
                                    // causes a re-render
                                    this.Timeline.DragData.OnMouseMove(offset);
                                }
                            }
                        }

                        this.isProcessingMouseAction = false;
                    }
                }
                else if (this.DragData != null) {
                    throw new Exception("????????????????????????????????????");
                }
            }
            else if (e.LeftButton == MouseButtonState.Pressed) {
                // handle "drag entry zone"
                if (this.GetMouseDifference(mousePoint.X) > 5d) {
                    if (this.Timeline.DragData == null) {
                        this.Timeline.BeginDragAction();
                    }
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);
            if (e.Handled) {
                return;
            }

            if (e.Key == Key.Escape) {
                this.isCancellingDragAction = true;
                this.isClipDragActivated = false;
                if (this.IsMouseCaptured) {
                    this.ReleaseMouseCapture();
                }

                this.isCancellingDragAction = false;
                if (this.Timeline.HasActiveDrag() && this.Timeline.DragData.IsBeingDragged(this)) {
                    this.Timeline.DragData.OnCancel();
                    this.Timeline.DragData = null;
                    e.Handled = true;
                }
            }
            else {
                if (this.IsMovingControl || this.isProcessingMouseAction || this.isCancellingDragAction) {
                    return;
                }

                if (!this.isClipDragActivated) {
                    return;
                }

                if (this.PART_ThumbLeft.IsDragging || this.PART_ThumbRight.IsDragging) {
                    return;
                }

                if (this.Timeline.HasActiveDrag()) {
                    if (this.Timeline.DragData.IsBeingDragged(this)) {
                        if (this.IsMouseCaptured || this.CaptureMouse()) {
                            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) {
                                this.Timeline.DragData.OnEnterCopyMove();
                            }
                            else if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) {
                                this.Timeline.DragData.OnEnterMoveMode();
                            }
                        }
                    }
                }
            }
        }

        */

        public override string ToString() {
            return $"TimelineClipControl({this.Span})";
        }
    }
}
