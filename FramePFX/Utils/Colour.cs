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

namespace FramePFX.Utils {
    /// <summary>
    /// A colour which stores a BGRA8888 colour
    /// </summary>
    public readonly struct Colour {
        private readonly uint value;

        public Colour(uint bgra) {
            this.value = bgra;
        }

        public static Colour FromARGB(uint argb) => new Colour(argb >> 24 & 255 | (argb >> 16 & 255) << 8 | (argb >> 8 & 255) << 16 | (argb & 255) << 24);

        public static Colour FromRGBA(uint argb) => new Colour(argb >> 16 & 255 | (argb >> 8 & 255) << 8 | (argb & 255) << 16 | (argb >> 24 & 255) << 24);

        public uint ToARGB() => (this.value >> 24) & 255 | (((this.value >> 16) & 255) << 8) | (((this.value >> 8) & 255) << 16) | (((this.value >> 0) & 255) << 24);

        public uint ToRGBA() => this.value >> 16 & 255 | (this.value >> 8 & 255) << 8 | (this.value >> 0 & 255) << 16 | (this.value >> 24 & 255) << 24;
    }
}