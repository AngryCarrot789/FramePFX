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

using Avalonia;
using Avalonia.Media;

namespace FramePFX.Avalonia.AvControls;

public static class TiledBrush {
    // TODO: customisable tile brushes maybe?
    private static DrawingBrush? brushTile4, brushTile8;

    public static DrawingBrush TiledTransparencyBrush4 => brushTile4 ??= Generate(4);
    public static DrawingBrush TiledTransparencyBrush8 => brushTile8 ??= Generate(8);

    private static DrawingBrush Generate(int tileSize) {
        int totalSize = tileSize * 2;
        return new DrawingBrush() {
            Drawing = new DrawingGroup() {
                Children = {
                    new GeometryDrawing() {
                        Brush = Brushes.White,
                        Geometry = new GeometryGroup() {
                            Children = {
                                new RectangleGeometry(new Rect(0, 0, tileSize, tileSize)),
                                new RectangleGeometry(new Rect(tileSize, tileSize, tileSize, tileSize)),
                            }
                        }
                    },
                    new GeometryDrawing() {
                        Brush = Brushes.DarkGray,
                        Geometry = new GeometryGroup() {
                            Children = {
                                new RectangleGeometry(new Rect(tileSize, 0, tileSize, tileSize)),
                                new RectangleGeometry(new Rect(0, tileSize, tileSize, tileSize)),
                            }
                        }
                    }
                }
            },
            TileMode = TileMode.Tile,
            // This is important in order for repeating tiles to work
            DestinationRect = new RelativeRect(0, 0, totalSize, totalSize, RelativeUnit.Absolute),
        };
    }
}