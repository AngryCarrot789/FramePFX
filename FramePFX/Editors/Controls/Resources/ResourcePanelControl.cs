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
using FramePFX.Editors.Controls.Resources.Explorers;
using FramePFX.Editors.Controls.Resources.Trees;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Editors.Controls.Resources {
    /// <summary>
    /// The main control which manages the UI for the resource manager and all
    /// of the resources inside of it, such as a resource tree, 'current folder' list, selection, etc.
    /// </summary>
    public class ResourcePanelControl : Control {
        public static readonly DependencyProperty ResourceManagerProperty =
            DependencyProperty.Register(
                "ResourceManager",
                typeof(ResourceManager),
                typeof(ResourcePanelControl),
                new PropertyMetadata(null, (d, e) => ((ResourcePanelControl) d).OnResourceManagerChanged((ResourceManager) e.OldValue, (ResourceManager) e.NewValue)));

        public ResourceManager ResourceManager {
            get => (ResourceManager) this.GetValue(ResourceManagerProperty);
            set => this.SetValue(ResourceManagerProperty, value);
        }

        public ResourceExplorerListControl ResourceExplorerList { get; private set; }

        public ResourceTreeView ResourceTreeView { get; private set; }

        public ResourcePanelControl() {
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            if (!(this.GetTemplateChild("PART_ResourceList") is ResourceExplorerListControl listBox))
                throw new Exception("Missing PART_ResourceList");
            if (!(this.GetTemplateChild("PART_ResourceTree") is ResourceTreeView treeView))
                throw new Exception("Missing PART_ResourceTree");
            this.ResourceExplorerList = listBox;
            this.ResourceTreeView = treeView;
        }

        // Assuming OnApplyTemplate is called before this method, which appears the be every time
        private void OnResourceManagerChanged(ResourceManager oldManager, ResourceManager newManager) {
            this.ResourceExplorerList.ResourceManager = newManager;
            this.ResourceTreeView.ResourceManager = newManager;
            if (newManager != null) {
                DataManager.SetContextData(this, new ContextData().Set(DataKeys.ResourceManagerKey, newManager));
            }
            else {
                DataManager.ClearContextData(this);
            }
        }
    }
}