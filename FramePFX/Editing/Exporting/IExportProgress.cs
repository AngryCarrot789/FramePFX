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

namespace FramePFX.Editing.Exporting;

// TODO: replace with an abstract class BaseExportProgress
// The ExporterContext creates an instance of the correct export progress model,
// and we use a control registry to create an instance of the corresponding control/window

// Or instead of a window, could we just re-implement the old FramePFX
// notification system from WPF, and show the export progress in there?
// Notifications are smaller, but dialogs require generally more work per exporter to setup I suppose

/// <summary>
/// An interface for the export progress
/// </summary>
public interface IExportProgress {
    bool HasEncodeProgress { get; set; }

    void OnFrameRendered(long frame);

    void OnFrameEncoded(long frame);
}