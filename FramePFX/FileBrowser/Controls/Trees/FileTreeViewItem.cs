//
// Copyright (c) 2023-2024 REghZy
//
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

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FramePFX.AdvancedMenuService.ContextService.Controls;
using FramePFX.Editors.Contextual;
using FramePFX.Editors.Controls.Bindings;
using FramePFX.Editors.Controls.TreeViews.Controls;
using FramePFX.FileBrowser.FileTree;
using FramePFX.Interactivity.Contexts;
using FramePFX.Utils;

namespace FramePFX.FileBrowser.Controls.Trees
{
    public class FileTreeViewItem : MultiSelectTreeViewItem, IFileTreeControl
    {
        /// <summary>
        /// The resource tree that this node is placed in
        /// </summary>
        public FileTreeView FileTree { get; private set; }

        /// <summary>
        /// The parent node. This may be null, in which case, we are a root item and therefore should use <see cref="FileTree"/> instead
        /// </summary>
        public FileTreeViewItem ParentNode { get; private set; }

        /// <summary>
        /// Gets either our <see cref="ParentNode"/> or <see cref="FileTree"/>
        /// </summary>
        public ItemsControl ParentObject
        {
            get
            {
                if (this.ParentNode != null)
                    return this.ParentNode;
                return this.FileTree;
            }
        }

        /// <summary>
        /// The resource model we are attached to
        /// </summary>
        public VFSFileEntry Resource { get; private set; }

        private bool isDragActive;
        private bool CanExpandNextMouseUp;

        private readonly GetSetAutoEventPropertyBinder<VFSFileEntry> displayNameBinder = new GetSetAutoEventPropertyBinder<VFSFileEntry>(HeaderProperty, nameof(VFSFileEntry.FileNameChanged), b => b.Model.FileName, null);

        public FileTreeViewItem()
        {
            AdvancedContextMenu.SetContextGenerator(this, ResourceContextRegistry.Instance);
        }

        static FileTreeViewItem() => DefaultStyleKeyProperty.OverrideMetadata(typeof(FileTreeViewItem), new FrameworkPropertyMetadata(typeof(FileTreeViewItem)));

        protected override void OnExpandedChanged()
        {
            base.OnExpandedChanged();
            this.Resource?.LoadContent();
        }

        public void OnAdding(FileTreeView fileTree, FileTreeViewItem parentNode, VFSFileEntry resource)
        {
            this.FileTree = fileTree;
            this.ParentNode = parentNode;
            this.Resource = resource;
        }

        public void OnAdded()
        {
            if (this.Resource is VFSFileEntry folder)
            {
                folder.ItemAdded += this.OnResourceAdded;
                folder.ItemRemoved += this.OnResourceRemoved;

                int i = 0;
                foreach (VFSFileEntry item in folder.Items)
                {
                    this.InsertNode(item, i++);
                }
            }

            this.displayNameBinder.Attach(this, this.Resource);
            DataManager.SetContextData(this, new ContextData().Set(DataKeys.FileTreeEntryKey, this.Resource));
        }

        public void OnRemoving()
        {
            if (this.Resource is VFSFileEntry folder)
            {
                folder.ItemAdded -= this.OnResourceAdded;
                folder.ItemRemoved -= this.OnResourceRemoved;
            }

            int count = this.Items.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                this.RemoveNode(i);
            }

            this.displayNameBinder.Detach();
        }

        public void OnRemoved()
        {
            this.FileTree = null;
            this.ParentNode = null;
            this.Resource = null;
            DataManager.ClearContextData(this);
        }

        private void OnResourceAdded(VFSFileEntry parent, VFSFileEntry item, int index) => this.InsertNode(item, index);

        private void OnResourceRemoved(VFSFileEntry parent, VFSFileEntry item, int index) => this.RemoveNode(index);

        public FileTreeViewItem GetNodeAt(int index) => (FileTreeViewItem) this.Items[index];

        public void InsertNode(VFSFileEntry item, int index)
        {
            this.InsertNode(null, item, index);
        }

        public void InsertNode(FileTreeViewItem control, VFSFileEntry resource, int index)
        {
            FileTreeView tree = this.FileTree;
            if (tree == null)
                throw new InvalidOperationException("Cannot add children when we have no resource tree associated");
            if (control == null)
                control = tree.GetCachedItemOrNew();

            control.OnAdding(tree, this, resource);
            this.Items.Insert(index, control);
            control.ApplyTemplate();
            control.OnAdded();
        }

        public void RemoveNode(int index, bool canCache = true)
        {
            FileTreeView tree = this.FileTree;
            if (tree == null)
                throw new InvalidOperationException("Cannot remove children when we have no resource tree associated");

            FileTreeViewItem control = (FileTreeViewItem) this.Items[index];
            control.OnRemoving();
            this.Items.RemoveAt(index);
            control.OnRemoved();
            if (canCache)
                tree.PushCachedItem(control);
        }

        public static bool CanBeginDragDrop()
        {
            return !KeyboardUtils.AreAnyModifiersPressed(ModifierKeys.Control, ModifierKeys.Shift);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                FileTreeView tree = this.FileTree;
                if (tree.SelectedItems.Count < 1 || !this.IsSelected && !KeyboardUtils.AreAnyModifiersPressed(ModifierKeys.Control))
                {
                    tree.ClearSelection();
                    this.IsSelected = true;
                }

                if (e.ClickCount > 1)
                {
                    // this.CanExpandNextMouseUp = true;
                    e.Handled = true;
                }
                else
                {
                    // if (Keyboard.Modifiers == ModifierKeys.None && this.Resource is TreeEntry folder && this.FileTree?.FileTree is ) {
                    //     manager.CurrentFolder = folder;
                    // }

                    if (CanBeginDragDrop() && !e.Handled)
                    {
                        if (this.IsFocused || this.Focus())
                        {
                            this.CaptureMouse();
                            this.isDragActive = true;
                            e.Handled = true;
                            return;
                        }
                    }
                }
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (this.isDragActive && (e.ChangedButton == MouseButton.Left || e.ChangedButton == MouseButton.Right))
            {
                this.isDragActive = false;
                if (this.IsMouseCaptured)
                {
                    this.ReleaseMouseCapture();
                }

                FileTreeView parent = this.FileTree;
                e.Handled = true;
                if (!this.IsSelected)
                {
                    if (e.ChangedButton == MouseButton.Left)
                    {
                        parent?.Selection.Select(this);
                    }
                    else if (!this.IsSelected)
                    {
                        parent?.Selection.Select(this);
                    }
                }
                else if (parent != null && parent.SelectedItems.Count > 1)
                {
                    parent.ClearSelection();
                    this.IsSelected = true;
                }
            }

            if (this.CanExpandNextMouseUp)
            {
                this.CanExpandNextMouseUp = false;
                this.IsExpanded = !this.IsExpanded;
            }

            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                if (ReferenceEquals(e.MouseDevice.Captured, this))
                {
                    this.ReleaseMouseCapture();
                }

                this.isDragActive = false;
            }
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is FileTreeViewItem;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new FileTreeViewItem();
        }
    }
}