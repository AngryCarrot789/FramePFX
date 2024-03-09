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

using System.Collections.Generic;
using FramePFX.AdvancedMenuService.ContextService;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.FileBrowser.Context
{
    public class ExplorerContextGenerator : IContextGenerator
    {
        public static ExplorerContextGenerator Instance { get; } = new ExplorerContextGenerator();

        public ExplorerContextGenerator()
        {
            // this.OpenInExplorerCommand = new RelayCommand<string>((x) => {
            //     if (!string.IsNullOrEmpty(x))
            //         IoC.ExplorerService.OpenFileInExplorer(x);
            // });
            //
            // this.CopyStringCommand = new RelayCommand<string>((x) => {
            //     if (!string.IsNullOrEmpty(x))
            //         IoC.Clipboard.SetText(x);
            // });
            //
            // this.RemoveFromParentCommand = new RelayCommand<TreeEntry>((x) => {
            //     x.Parent?.RemoveItemCore(x);
            // });
        }

        public void Generate(List<IContextEntry> list, IContextData context)
        {
            // if (!context.TryGetContext(DataKeys.FileTreeEntryKey, out TreeEntry item)) {
            //     return;
            // }
            //
            // FileTreeViewModel explorer = item.FileTree;
            // if (explorer != null || context.TryGetContext(out explorer)) {
            //     if (item.ContainsKey(Win32FileSystem.FilePathKey)) {
            //         list.Add(new CommandContextEntry("Open", explorer.OpenItemCommand, item));
            //     }
            // }
            //
            // if (item.TryGetDataValue(Win32FileSystem.FilePathKey, out string path)) {
            //     list.Add(new CommandContextEntry("Open in Explorer", this.OpenInExplorerCommand, path));
            //     list.Add(new CommandContextEntry("Copy Path", this.CopyStringCommand, path));
            // }
            //
            // if (item.Parent != null && item.Parent.IsRootContainer) {
            //     if (list.Count > 0) {
            //         list.Add(SeparatorEntry.Instance);
            //     }
            //
            //     list.Add(new CommandContextEntry("Remove from tree", this.RemoveFromParentCommand, item));
            // }
        }
    }
}