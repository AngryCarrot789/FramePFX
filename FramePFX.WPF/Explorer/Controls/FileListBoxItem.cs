using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FramePFX.FileBrowser;
using FramePFX.FileBrowser.FileTree;
using FramePFX.FileBrowser.FileTree.Physical;
using FramePFX.Logger;
using FramePFX.Utils;
using FramePFX.WPF.Editor.Timeline.Utils;

namespace FramePFX.WPF.Explorer.Controls {
    public class FileListBoxItem : ListBoxItem {
        private Point originMousePoint;
        private bool isDragActive;
        private bool isDragDropping;

        private FileListBox ParentFileListBox => ItemsControl.ItemsControlFromItemContainer(this) as FileListBox;

        public FileListBoxItem() {
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            if (e.ClickCount > 1) {
                if (this.DataContext is TreeEntry entry && entry.FileTree != null) {
                    entry.FileTree.OnNavigate(entry);
                    e.Handled = true;
                    return;
                }
            }

            bool hasToggledSelection = false;
            if (this.ParentFileListBox is FileListBox listBox) {
                if (KeyboardUtils.AreAnyModifiersPressed(ModifierKeys.Control)) {
                    this.IsSelected = !this.IsSelected;
                    hasToggledSelection = true;
                }
                else if (!this.IsSelected) {
                    listBox.SelectedItems.Clear();
                    this.IsSelected = true;
                }
            }

            if (!e.Handled && (this.IsFocused || this.Focus())) {
                if (!this.isDragDropping) {
                    this.CaptureMouse();
                    this.originMousePoint = e.GetPosition(this);
                    this.isDragActive = true;
                    e.Handled = true;
                    if (!hasToggledSelection)
                        this.IsSelected = true;
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
            if (!this.isDragActive || this.isDragDropping) {
                return;
            }

            if (e.LeftButton == MouseButtonState.Pressed) {
                Point posA = e.GetPosition(this);
                Point posB = this.originMousePoint;
                Point change = new Point(Math.Abs(posA.X - posB.X), Math.Abs(posA.X - posB.X));
                if (change.X > 5 || change.Y > 5) {
                    FileListBox parent = this.ParentFileListBox;
                    if (parent == null || !(parent.DataContext is FileExplorerViewModel explorer)) {
                        return;
                    }

                    StringCollection paths = new StringCollection();
                    foreach (TreeEntry entry in explorer.SelectedFiles) {
                        if (!(entry is BasePhysicalVirtualFile file))
                            continue;
                        string path = file.FilePath;
                        if (string.IsNullOrWhiteSpace(path))
                            continue;
                        paths.Add(path);
                    }

                    if (paths.Count < 1) {
                        return;
                    }

                    try {
                        this.isDragDropping = true;
                        DataObject obj = new DataObject();
                        obj.SetFileDropList(paths);
                        DragDrop.DoDragDrop(this, obj, DragDropEffects.Copy | DragDropEffects.Link);
                    }
                    catch (Exception ex) {
                        AppLogger.WriteLine("Exception while executing resource item drag drop: " + ex.GetToString());
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