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

using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using FramePFX.BaseFrontEnd.Shortcuts.Trees;
using FramePFX.BaseFrontEnd.Utils;
using FramePFX.Configurations.Shortcuts;
using FramePFX.Shortcuts;

namespace FramePFX.BaseFrontEnd.Configurations.Pages.Basic;

public class ShortcutEditorConfigurationPageControl : BaseConfigurationPageControl {
    private ShortcutTreeView? PART_ShortcutTree;

    public ShortcutEditorConfigurationPageControl() {
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_ShortcutTree = e.NameScope.GetTemplateChild<ShortcutTreeView>("PART_ShortcutTree");
        if (e.NameScope.TryGetTemplateChild("PART_DemoHyperlink", out HyperlinkButton? hyperlink)) {
            hyperlink.Click += this.OnHyperlinkClicked;
        }
    }

    private void OnHyperlinkClicked(object? sender, RoutedEventArgs e) {
        if (((HyperlinkButton) sender!).Content is string text) {
            ShortcutGroupEntry? target = this.PART_ShortcutTree?.RootEntry?.GetGroupByPath(text);
            if (target != null) {
                this.PART_ShortcutTree!.ExpandTo(target);
            }
        }
    }

    public override void OnConnected() {
        base.OnConnected();
        this.PART_ShortcutTree!.RootEntry = ((ShortcutEditorConfigurationPage) this.Page!).RootGroupEntry;
    }

    public override void OnDisconnected() {
        base.OnDisconnected();
        this.PART_ShortcutTree!.RootEntry = null;
    }
}