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

using PFXToolKitUI.Icons;
using PFXToolKitUI.Themes;
using SkiaSharp;

namespace FramePFX;

public static class AutomationIcons {
    // We really shouldn't use icons to draw the little LEDs... but since I used MVVM to bind
    // an Ellipse's Fill property, and converters don't let you use dynamic resource... issues
    public static readonly Icon IconLED_Active = IconManager.Instance.RegisterEllipseIcon(
        "AutomationLED_Active",
        BrushManager.Instance.GetDynamicThemeBrush("ABrush.PFX.Automation.Active.Fill"),
        BrushManager.Instance.CreateConstant(SKColors.Black),
        3, 3, 1.0);

    public static readonly Icon IconLED_Override = IconManager.Instance.RegisterEllipseIcon(
        "AutomationLED_Override",
        BrushManager.Instance.GetDynamicThemeBrush("ABrush.PFX.Automation.Override.Fill"),
        BrushManager.Instance.CreateConstant(SKColors.Black),
        3, 3, 1.0);
}