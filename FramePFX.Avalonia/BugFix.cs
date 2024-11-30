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

using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.VisualTree;

namespace FramePFX.Avalonia;

public static class BugFix {
    public static void TextBox_FocusSelectAll(TextBox textBox) {
        textBox.Focus();
        textBox.SelectAll();
        
        // Fixes an issue with the TextPresenter being rendered before the
        // SelectionStart/SelectionEnd properties update via the TemplateBinding
        // in the TextBox's ControlTheme
        TextPresenter? presenter = textBox.FindDescendantOfType<TextPresenter>(false);
        if (presenter != null) {
            presenter.CoerceValue(TextPresenter.SelectionStartProperty);
            presenter.CoerceValue(TextPresenter.SelectionEndProperty);
        }
    }
}