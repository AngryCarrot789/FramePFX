using System;
using SkiaSharp;

namespace FramePFX.Editor
{
    /// <summary>
    /// A quality factor for when drawing
    /// </summary>
    public enum EnumRenderQuality : byte
    {
        /// <summary>
        /// Unspecified quality settings; SkiaSharp default
        /// </summary>
        Unspecified,
        /// <summary>
        /// Low quality but fast render times
        /// </summary>
        Low,
        /// <summary>
        /// Medium quality looks good but takes a bit longer to draw
        /// </summary>
        Medium,
        /// <summary>
        /// Highest quality at the cost of possibly long render times
        /// </summary>
        High
    }

    public static class EnumRenderQualityExtensions
    {
        public static SKFilterQuality ToFilterQuality(this EnumRenderQuality quality)
        {
            switch (quality)
            {
                case EnumRenderQuality.Unspecified: return SKFilterQuality.None;
                case EnumRenderQuality.Low: return SKFilterQuality.Low;
                case EnumRenderQuality.Medium: return SKFilterQuality.Medium;
                case EnumRenderQuality.High: return SKFilterQuality.High;
                default: throw new ArgumentOutOfRangeException(nameof(quality), quality, null);
            }
        }
    }
}