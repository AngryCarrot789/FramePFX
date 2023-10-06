using SkiaSharp;

namespace FramePFX.Interactivity {
    /// <summary>
    /// An interface for a native data object, that is, an object with a specific string-representable type
    /// </summary>
    public interface IDataObjekt {
        object GetData(string format);

        object GetData(string format, bool autoConvert);

        bool GetDataPresent(string format);

        bool GetDataPresent(string format, bool autoConvert);

        string[] GetFormats();

        string[] GetFormats(bool autoConvert);

        void SetData(object data);

        void SetData(string format, object data);

        void SetData(string format, object data, bool autoConvert);

        bool GetBitmap(out SKBitmap bitmap);
    }
}