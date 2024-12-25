// 
// Copyright (c) 2024-2024 REghZy
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

namespace FramePFX.AdvancedMenuService;

public delegate void GroupCaptionEntryCaptionChangedEventHandler(CaptionEntry sender);

/// <summary>
/// A context entry that is placed above a group of items. This is not necessarily related to <see cref="IContextGroup"/> objects
/// </summary>
public class CaptionEntry : IContextObject {
    private string? caption;

    public string? Caption {
        get => this.caption;
        set {
            if (this.caption == value)
                return;

            this.caption = value;
            this.CaptionChanged?.Invoke(this);
        }
    }

    public event GroupCaptionEntryCaptionChangedEventHandler? CaptionChanged;

    public CaptionEntry(string caption) {
        this.caption = caption;
    }
}