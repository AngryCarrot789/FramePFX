using System.Windows;
using System.Windows.Interop;
using FramePFX.Interactivity;
using SkiaSharp;

namespace FramePFX.WPF.Interactivity {
    public class DataObjectWrapper : IDataObjekt {
        private readonly IDataObject mObject;

        public DataObjectWrapper(IDataObject mObject) {
            this.mObject = mObject;
        }

        public object GetData(string format) {
            return this.mObject.GetData(format);
        }

        public object GetData(string format, bool autoConvert) {
            return this.mObject.GetData(format, autoConvert);
        }

        public bool GetDataPresent(string format) {
            return this.mObject.GetDataPresent(format);
        }

        public bool GetDataPresent(string format, bool autoConvert) {
            return this.mObject.GetDataPresent(format, autoConvert);
        }

        public string[] GetFormats() {
            return this.mObject.GetFormats();
        }

        public string[] GetFormats(bool autoConvert) {
            return this.mObject.GetFormats(autoConvert);
        }

        public void SetData(object data) {
            this.mObject.SetData(data);
        }

        public void SetData(string format, object data) {
            this.mObject.SetData(format, data);
        }

        public void SetData(string format, object data, bool autoConvert) {
            this.mObject.SetData(format, data, autoConvert);
        }

        public bool GetBitmap(out SKBitmap bitmap, out int error) {
            if (!(this.GetData(NativeDropTypes.Bitmap) is InteropBitmap bmp)) {
                error = 1;
                bitmap = null;
                return false;
            }

            if (bmp.Format.ToString() != "Bgra32") {
                error = 2;
                bitmap = null;
                return false;
            }

            SKImageInfo info = new SKImageInfo(bmp.PixelWidth, bmp.PixelHeight);
            System.Diagnostics.Debug.Assert(info.Width * (bmp.Format.BitsPerPixel / 8) == info.RowBytes);

            bitmap = new SKBitmap(info);
            bmp.CopyPixels(new Int32Rect(0, 0, info.Width, info.Height), bitmap.GetPixels(), info.BytesSize, info.RowBytes);
            error = 0;
            return true;
        }
    }
}