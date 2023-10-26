using SkiaSharp;

namespace Gpic.Core.Services {
    public interface IColourPicker {
        SKColor? PickARGB(SKColor? def = null);
    }
}