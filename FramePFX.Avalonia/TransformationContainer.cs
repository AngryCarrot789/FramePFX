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
using FramePFX.Avalonia.AvControls;

namespace FramePFX.Avalonia;

/// <summary>
/// A dummy container control that sits inbetween the <see cref="FreeMoveViewPortV2"/> and
/// its child control, so that the child itself doesn't get render transformed directly
/// </summary>
public class TransformationContainer : Decorator
{
    public TransformationContainer() {
    }
}