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
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using FramePFX.AttachedProperties;
using FramePFX.Editors.Controls.TreeViews.Controls;
using FramePFX.FileBrowser.FileTree;
using FramePFX.Utils;

namespace FramePFX.FileBrowser.Controls.Trees
{
    public class FileTreeView : MultiSelectTreeView, IFileTreeControl
    {
        public static readonly DependencyProperty FileTreeProperty = DependencyProperty.Register("FileTree", typeof(VFSFileEntry), typeof(FileTreeView), new PropertyMetadata(null, (d, e) => ((FileTreeView) d).OnFileTreeChanged((VFSFileEntry) e.OldValue, (VFSFileEntry) e.NewValue)));

        public VFSFileEntry FileTree {
            get => (VFSFileEntry) this.GetValue(FileTreeProperty);
            set => this.SetValue(FileTreeProperty, value);
        }

        FileTreeView IFileTreeControl.FileTree => this;

        FileTreeViewItem IFileTreeControl.ParentNode => null;

        VFSFileEntry IFileTreeControl.Resource => this.rootFolder;

        internal readonly Stack<FileTreeViewItem> itemCache;
        private VFSFileEntry rootFolder;

        public FileTreeView()
        {
            this.itemCache = new Stack<FileTreeViewItem>();
        }

        static FileTreeView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FileTreeView), new FrameworkPropertyMetadata(typeof(FileTreeView)));
        }

        public FileTreeViewItem GetCachedItemOrNew()
        {
            return this.itemCache.Count > 0 ? this.itemCache.Pop() : new FileTreeViewItem();
        }

        public void PushCachedItem(FileTreeViewItem item)
        {
            if (this.itemCache.Count < 128)
            {
                this.itemCache.Push(item);
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (this.GetTemplateChild("PART_ScrollViewerContent") is FrameworkElement element)
            {
                HandleRequestBringIntoView.SetIsEnabled(element, true);
            }
        }

        private void OnFileTreeChanged(VFSFileEntry oldTree, VFSFileEntry newTree)
        {
            if (oldTree != null)
            {
                this.rootFolder.ItemAdded -= this.OnResourceAdded;
                this.rootFolder.ItemRemoved -= this.OnResourceRemoved;
                for (int i = this.Items.Count - 1; i >= 0; i--)
                {
                    this.RemoveNode(i);
                }

                this.rootFolder = null;
            }

            if (newTree != null)
            {
                this.rootFolder = newTree;
                newTree.ItemAdded += this.OnResourceAdded;
                newTree.ItemRemoved += this.OnResourceRemoved;

                int i = 0;
                newTree.LoadContent();
                foreach (VFSFileEntry resource in newTree.Items)
                {
                    this.InsertNode(resource, i++);
                }
            }
        }

        private void OnResourceAdded(VFSFileEntry parent, VFSFileEntry item, int index) => this.InsertNode(item, index);

        private void OnResourceRemoved(VFSFileEntry parent, VFSFileEntry item, int index) => this.RemoveNode(index);

        public FileTreeViewItem GetNodeAt(int index)
        {
            return (FileTreeViewItem) this.Items[index];
        }

        public void InsertNode(VFSFileEntry item, int index)
        {
            this.InsertNode(this.GetCachedItemOrNew(), item, index);
        }

        public void InsertNode(FileTreeViewItem control, VFSFileEntry resource, int index)
        {
            control.OnAdding(this, null, resource);
            this.Items.Insert(index, control);
            control.ApplyTemplate();
            control.OnAdded();
        }

        public void RemoveNode(int index, bool canCache = true)
        {
            FileTreeViewItem control = (FileTreeViewItem) this.Items[index];
            VFSFileEntry model = control.Resource ?? throw new Exception("Expected node to have a resource");
            control.OnRemoving();
            this.Items.RemoveAt(index);
            control.OnRemoved();
            if (canCache)
                this.PushCachedItem(control);
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