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

using Avalonia.Controls.Primitives;
using FramePFX.BaseFrontEnd.Utils;

namespace FramePFX.BaseFrontEnd.AvControls;

public static class IconButtonHelper {
    public static void ApplyTemplate(IIconButton obj, TemplateAppliedEventArgs e, ref IconControl? iconControl) {
        iconControl = e.NameScope.GetTemplateChild<IconControl>("PART_IconControl");
        if (obj.IconMaxWidth.HasValue)
            iconControl.MaxWidth = obj.IconMaxWidth.Value;
        if (obj.IconMaxHeight.HasValue)
            iconControl.MaxHeight = obj.IconMaxHeight.Value;
    }

    public static void SetMaxWidth(IconControl? iconControl, double? value) {
        if (iconControl != null) {
            iconControl.MaxWidth = value ?? double.NaN;
        }
    }
    
    public static void SetMaxHeight(IconControl? iconControl, double? value) {
        if (iconControl != null) {
            iconControl.MaxHeight = value ?? double.NaN;
        }
    }
}