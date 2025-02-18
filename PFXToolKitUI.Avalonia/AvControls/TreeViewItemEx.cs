// This file is part of FramePFX.
// 
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Threading;

namespace PFXToolKitUI.Avalonia.AvControls;

public abstract class TreeViewItemEx : TreeViewItem {
    private bool isReallyVisible;

    public bool IsReallyVisible => this.isReallyVisible;

    protected TreeViewItemEx() {
    }

    static TreeViewItemEx() {
        IsExpandedProperty.Changed.AddClassHandler<TreeViewItemEx, bool>((d, e) => d.OnIsExpandedChanged());
    }

    private void OnIsExpandedChanged() {
        Dispatcher.UIThread.Post(() => this.PropagateIsVisible(false), DispatcherPriority.Loaded);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
        base.OnPropertyChanged(change);
        if (change.Property == IsVisibleProperty) {
            bool isVisible = this.IsEffectivelyVisible;
            if (this.isReallyVisible != isVisible) {
                this.PropagateIsVisible(true);
            }
        }
        // else if (change.Property == IsExpandedProperty) {
        // }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
        base.OnAttachedToVisualTree(e);
        this.CheckIsEffectivelyVisible();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e) {
        base.OnDetachedFromVisualTree(e);
        this.CheckIsEffectivelyVisible();
    }

    private void CheckIsEffectivelyVisible() {
        if (this.isReallyVisible != this.IsEffectivelyVisible) {
            this.isReallyVisible = this.IsEffectivelyVisible;
            this.OnIsReallyVisibleChanged();
        }
    }

    private void PropagateIsVisible(bool self) {
        if (self) {
            if (!(this.GetLogicalParent() is TreeViewItemEx ex)) {
                return;
            }

            bool isVisible = this.IsEffectivelyVisible && ex.IsReallyVisible && ex.IsExpanded;
            if (this.isReallyVisible == isVisible) {
                return;
            }

            this.isReallyVisible = isVisible;
            this.OnIsReallyVisibleChanged();
        }

        foreach (ILogical item in this.LogicalChildren) {
            (item as TreeViewItemEx)?.PropagateIsVisible(true);
        }
    }

    /// <summary>
    /// Invoked when this tree view's <see cref="Visual.IsEffectivelyVisible"/> property changes
    /// </summary>
    protected virtual void OnIsReallyVisibleChanged() {
    }
}