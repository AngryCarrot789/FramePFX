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

using System.Threading.Tasks;
using Avalonia.Controls;
using FramePFX.Editing.Exporting;
using PFXToolKitUI.Avalonia.Services;

namespace FramePFX.Avalonia.Exporting;

public class ExportDialogServiceImpl : IExportDialogService {
    public Task ShowExportDialog(ExportSetup setup) {
        if (IDesktopService.TryGetInstance(out IDesktopService? service) && service.TryGetActiveWindow(out Window? window)) {
            ExportDialog dialog = new ExportDialog(setup) {
                Topmost = true
            };
            
            return dialog.ShowDialog(window);
        }

        return Task.CompletedTask;
    }
}