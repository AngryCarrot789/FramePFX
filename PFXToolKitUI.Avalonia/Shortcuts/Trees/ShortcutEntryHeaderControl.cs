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
using Avalonia.Controls.Documents;
using Avalonia.Data;
using Avalonia.Media;
using PFXToolKitUI.Shortcuts;
using PFXToolKitUI.Utils.Destroying;

namespace PFXToolKitUI.Avalonia.Shortcuts.Trees;

public class ShortcutEntryHeaderControl : TextBlock {
    public static readonly StyledProperty<IKeyMapEntry?> KeyMapEntryProperty = AvaloniaProperty.Register<ShortcutEntryHeaderControl, IKeyMapEntry?>(nameof(KeyMapEntry));
    public static readonly StyledProperty<IBrush?> DisplayNameForegroundProperty = AvaloniaProperty.Register<ShortcutEntryHeaderControl, IBrush?>(nameof(DisplayNameForeground));
    public static readonly StyledProperty<IBrush?> RawNameForegroundProperty = AvaloniaProperty.Register<ShortcutEntryHeaderControl, IBrush?>(nameof(RawNameForeground));

    public IKeyMapEntry? KeyMapEntry {
        get => this.GetValue(KeyMapEntryProperty);
        set => this.SetValue(KeyMapEntryProperty, value);
    }

    public IBrush? DisplayNameForeground {
        get => this.GetValue(DisplayNameForegroundProperty);
        set => this.SetValue(DisplayNameForegroundProperty, value);
    }

    public IBrush? RawNameForeground {
        get => this.GetValue(RawNameForegroundProperty);
        set => this.SetValue(RawNameForegroundProperty, value);
    }

    private List<IDisposable>? bindings;

    public ShortcutEntryHeaderControl() {
    }

    public override string ToString() {
        return "WHY IS IT RENDERING TOSTRING!!!";
    }

    static ShortcutEntryHeaderControl() {
        KeyMapEntryProperty.Changed.AddClassHandler<ShortcutEntryHeaderControl, IKeyMapEntry?>((d, e) => d.OnKeyMapEntryChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    private void OnKeyMapEntryChanged(IKeyMapEntry? oldEntry, IKeyMapEntry? newEntry) {
        DisposableUtils.DisposeMany(null, this.bindings);
        if (newEntry == null) {
            this.Inlines?.Clear();
        }
        else {
            if (this.Inlines == null) {
                this.Inlines = new InlineCollection();
            }
            else {
                this.Inlines.Clear();
            }

            string rawName = newEntry.Name ?? "<root>";
            if (newEntry.DisplayName is string displayName && !string.IsNullOrWhiteSpace(displayName)) {
                this.Inlines.Add(this.BindRunForeground(new Run(displayName), DisplayNameForegroundProperty));
                this.Inlines.Add(" ");
                this.Inlines.Add(this.BindRunForeground(new Run($"({rawName})"), RawNameForegroundProperty));
            }
            else {
                this.Inlines.Add(this.BindRunForeground(new Run(rawName), DisplayNameForegroundProperty));
            }
        }
    }

    private Run BindRunForeground(Run run, StyledProperty<IBrush?> property) {
        (this.bindings ??= new List<IDisposable>()).Add(run.Bind(TextElement.ForegroundProperty, new Binding(property.Name, BindingMode.TwoWay) { Source = this }));
        return run;
    }
}