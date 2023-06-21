using System;
using System.Collections.Generic;
using System.Numerics;

namespace FramePFX.Core.Utils {
    public readonly struct Resolution : IComparable<Resolution>, IEqualityComparer<Resolution> {
        /// <summary>
        /// A resolution whose width and height is zero
        /// </summary>
        public static readonly Resolution Empty = new Resolution(0, 0);

        /// <summary>
        /// The resolution's width
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// The resolution's height
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Whether this resolution's width and height is zero or not
        /// </summary>
        public bool IsEmpty => this.Width == 0 && this.Height == 0;

        public Resolution(int width, int height) {
            this.Width = width;
            this.Height = height;
        }

        public bool Equals(Resolution other) {
            return this.Width == other.Width && this.Height == other.Height;
        }

        public override bool Equals(object obj) {
            return obj is Resolution other && this.Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                return (this.Width * 397) ^ this.Height;
            }
        }

        public int CompareTo(Resolution other) {
            int cmp = this.Width.CompareTo(other.Width);
            if (cmp == 0)
                cmp = this.Height.CompareTo(other.Height);
            return cmp;
        }

        public bool Equals(Resolution x, Resolution y) {
            return x.Equals(y);
        }

        public int GetHashCode(Resolution obj) {
            return obj.GetHashCode();
        }

        public static implicit operator Vector2(in Resolution res) {
            return new Vector2(res.Width, res.Height);
        }

        public static explicit operator Resolution(in Vector2 res) {
            return new Resolution((int) Math.Floor(res.X), (int) Math.Floor(res.Y));
        }

        public static implicit operator Resolution(ulong resolution) => new Resolution((int) (resolution & 0xFFFFFFFF), (int) ((resolution >> 32) & 0xFFFFFFFF));

        public static explicit operator ulong(Resolution color) => (ulong) color.Width + ((ulong) color.Height << 32);
    }
}