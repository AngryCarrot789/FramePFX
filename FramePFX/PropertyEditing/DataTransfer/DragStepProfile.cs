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

namespace FramePFX.PropertyEditing.DataTransfer;

/// <summary>
/// Contains information about how a number dragger should step value changes
/// </summary>
public readonly struct DragStepProfile
{
    public static readonly DragStepProfile UnitOne = new DragStepProfile(0.0001, 0.001, 0.0025, 0.01);
    public static readonly DragStepProfile SubPixel = new DragStepProfile(0.00025, 0.0025, 0.025, 0.25);
    public static readonly DragStepProfile Pixels = new DragStepProfile(0.001, 0.01, 0.1, 1.0);
    public static readonly DragStepProfile Percentage = new DragStepProfile(0.001, 0.01, 0.1, 1.0);
    public static readonly DragStepProfile FontSize = new DragStepProfile(0.001, 0.01, 0.1, 1.0);
    public static readonly DragStepProfile Rotation = new DragStepProfile(0.005, 0.05, 0.5, 2);
    public static readonly DragStepProfile InfPixelRange = new DragStepProfile(0.05, 0.1, 1.0, 5);
    public static readonly DragStepProfile SecondsRealtime = new DragStepProfile(0.001, 0.01, 0.05, 0.5);

    /// <summary>
    /// A tiny step change, when holding CTRL+SHIFT
    /// </summary>
    public readonly double TinyStep;

    /// <summary>
    /// A smaller step change, when holding SHIFT
    /// </summary>
    public readonly double SmallStep;

    /// <summary>
    /// A normal step change, when holding no modifier keys
    /// </summary>
    public readonly double NormalStep;

    /// <summary>
    /// A larger step change, when holding CTRL
    /// </summary>
    public readonly double LargeStep;

    public DragStepProfile(double tinyStep, double smallStep, double normalStep, double largeStep)
    {
        this.TinyStep = tinyStep;
        this.SmallStep = smallStep;
        this.NormalStep = normalStep;
        this.LargeStep = largeStep;
    }
}