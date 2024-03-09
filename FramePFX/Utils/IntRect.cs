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

namespace FramePFX.Utils
{
    public readonly struct IntRect
    {
        public readonly int X1;
        public readonly int Y1;
        public readonly int Width;
        public readonly int Height;

        public int X2 => this.X1 + this.Width;

        public int Y2 => this.Y1 + this.Height;

        public IntRect(int x1, int y1, int width, int height)
        {
            this.X1 = x1;
            this.Y1 = y1;
            this.Width = width;
            this.Height = height;
        }

        public static IntRect FromAABB(int x1, int y1, int x2, int y2)
        {
            return new IntRect(x1, y1, x2 - x1, y2 - y1);
        }
    }
}