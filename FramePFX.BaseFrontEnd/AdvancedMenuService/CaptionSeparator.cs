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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using FramePFX.AdvancedMenuService;

namespace FramePFX.BaseFrontEnd.AdvancedMenuService;

public class CaptionSeparator : TemplatedControl, IAdvancedEntryConnection {
    public static readonly StyledProperty<string?> TextProperty = AvaloniaProperty.Register<CaptionSeparator, string?>(nameof(Text));

    public string? Text {
        get => this.GetValue(TextProperty);
        set => this.SetValue(TextProperty, value);
    }

    public CaptionEntry? Entry { get; private set; }

    IContextObject? IAdvancedEntryConnection.Entry => this.Entry;

    public CaptionSeparator() {
    }

    public void OnAdding(IAdvancedContainer container, ItemsControl parent, IContextObject entry) {
        this.Entry = (CaptionEntry) entry;
    }

    public void OnAdded() {
        this.Entry!.CaptionChanged += this.OnCaptionChanged;
        this.OnCaptionChanged(this.Entry);
    }

    public void OnRemoving() {
        this.Entry!.CaptionChanged -= this.OnCaptionChanged;
    }

    public void OnRemoved() {
        this.Entry = null;
    }

    private void OnCaptionChanged(CaptionEntry sender) {
        this.Text = sender.Caption;
    }
}