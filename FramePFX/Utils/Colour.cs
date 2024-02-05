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