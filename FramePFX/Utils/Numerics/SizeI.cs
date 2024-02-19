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

using System;

namespace FramePFX.Utils.Numerics {
    public struct SizeI : IEquatable<SizeI> {
        public static readonly SizeI Empty = default;

        public int Width;
        public int Height;

        public SizeI(int width, int height) {
            this.Width = width;
            this.Height = height;
        }

        public SizeI(Vector2i vec) {
            this.Width = vec.X;
            this.Height = vec.Y;
        }

        public bool IsEmpty => this == SizeI.Empty;

        public override string ToString() => $"SizeI({this.Width.ToString()}, {this.Height.ToString()})";

        public static SizeI operator +(SizeI a, SizeI b) => new SizeI(a.Width + b.Width, a.Height + b.Height);

        public static SizeI operator -(SizeI a, SizeI b) => new SizeI(a.Width - b.Width, a.Height - b.Height);

        public bool Equals(SizeI obj) => this.Width == obj.Width && this.Height == obj.Height;

        public override bool Equals(object obj) => obj is SizeI skSizeI && this.Equals(skSizeI);

        public static bool operator ==(SizeI left, SizeI right) => left.Equals(right);

        public static bool operator !=(SizeI left, SizeI right) => !left.Equals(right);

        public override int GetHashCode() => (this.Width * 397) ^ this.Height;
    }
}