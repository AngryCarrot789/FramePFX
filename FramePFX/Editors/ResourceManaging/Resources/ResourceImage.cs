using System;
using System.IO;
using System.Runtime.InteropServices;
using FramePFX.Editors.ResourceManaging.Autoloading;
using FramePFX.Editors.ResourceManaging.Events;
using FramePFX.RBC;
using SkiaSharp;

namespace FramePFX.Editors.ResourceManaging.Resources {
    public class ResourceImage : ResourceItem {
        private string filePath;

        public string FilePath {
            get => this.filePath;
            set {
                this.filePath = value;
            }
        }

        public bool IsRawBitmapMode { get; set; }

        public SKBitmap bitmap;
        public SKImage image;

        public event BaseResourceEventHandler ImageChanged;

        public ResourceImage() {
        }

        public void SetBitmapImage(SKBitmap skBitmap) {
            this.bitmap = skBitmap;
            this.image = SKImage.FromBitmap(skBitmap);
            this.IsRawBitmapMode = true;
            this.TryAutoEnable(null);
            this.ImageChanged?.Invoke(this);
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            if (this.IsRawBitmapMode) {
                if (this.bitmap != null) {
                    IntPtr pixels = this.bitmap.GetPixels(out IntPtr length);
                    if (pixels != IntPtr.Zero && length != IntPtr.Zero) {
                        byte[] array = new byte[length.ToInt32()];
                        Marshal.Copy(pixels, array, 0, array.Length);
                        data.SetBool(nameof(this.IsRawBitmapMode), true);
                        data.SetByteArray(nameof(this.bitmap), array);
                        data.SetStruct("BitmapImageFormat", new UnmanagedImageFormat(this.bitmap.Info));
                    }
                }
            }
            else if (!string.IsNullOrEmpty(this.FilePath)) {
                data.SetString(nameof(this.FilePath), this.FilePath);
            }
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            // juuust in case...
            if (this.bitmap != null || this.image != null) {
                this.bitmap?.Dispose();
                this.image?.Dispose();
            }

            this.IsRawBitmapMode = data.GetBool(nameof(this.IsRawBitmapMode), false);
            if (this.IsRawBitmapMode) {
                UnmanagedImageFormat format = data.GetStruct<UnmanagedImageFormat>("BitmapImageFormat");
                if (format.Width > 0 && format.Height > 0) {
                    unsafe {
                        try {
                            byte[] array = data.GetByteArray(nameof(this.bitmap));
                            this.bitmap = new SKBitmap();
                            fixed (byte* ptr = array) {
                                this.bitmap.InstallPixels(format.ImageInfo, (IntPtr) ptr);
                            }

                            this.image = this.bitmap != null ? SKImage.FromBitmap(this.bitmap) : null;
                        }
                        catch (Exception e) {
                            this.bitmap?.Dispose();
                            this.image?.Dispose();
                        }
                    }
                }
            }
            else if (data.TryGetString(nameof(this.FilePath), out string filePath)) {
                this.FilePath = filePath;
            }
        }

        protected override bool OnTryAutoEnable(ResourceLoader loader) {
            if (string.IsNullOrEmpty(this.FilePath) || this.image != null) {
                return true;
            }

            try {
                this.LoadImageAsync(this.FilePath);
                return true;
            }
            catch (Exception e) {
                loader?.AddEntry(new InvalidImagePathEntry(this) {FilePath = this.FilePath});
                return false;
            }
        }

        public override bool TryEnableForLoaderEntry(InvalidResourceEntry entry) {
            InvalidImagePathEntry imgEntry = (InvalidImagePathEntry) entry;
            try {
                this.LoadImageAsync(imgEntry.FilePath);
                return base.TryEnableForLoaderEntry(imgEntry);
            }
            catch (Exception e) {
                return false;
            }
        }

        public void LoadImageAsync(string file) {
            SKBitmap bmp = null;
            SKImage img = null;
            try {
                using (BufferedStream stream = new BufferedStream(File.OpenRead(file), 32768)) {
                    bmp = SKBitmap.Decode(stream);
                    img = SKImage.FromBitmap(bmp);
                }
            }
            catch {
                bmp?.Dispose();
                img?.Dispose();
                throw;
            }

            if (this.bitmap != null || this.image != null) {
                this.bitmap?.Dispose();
                this.bitmap = null;
                this.image?.Dispose();
                this.image = null;
            }

            this.image = img;
            this.bitmap = bmp;
            this.ImageChanged?.Invoke(this);
        }

        public override void Destroy() {
            base.Destroy();
            this.bitmap?.Dispose();
            this.bitmap = null;
            this.image?.Dispose();
            this.image = null;
            this.ImageChanged?.Invoke(this);
        }

        private readonly struct UnmanagedImageFormat {
            public readonly int Width;
            public readonly int Height;
            public readonly SKColorType ColorType;
            public readonly SKAlphaType AlphaType;

            public SKImageInfo ImageInfo => new SKImageInfo(this.Width, this.Height, this.ColorType, this.AlphaType);

            public UnmanagedImageFormat(int width, int height, SKColorType colorType, SKAlphaType alphaType) {
                this.Width = width;
                this.Height = height;
                this.ColorType = colorType;
                this.AlphaType = alphaType;
            }

            public UnmanagedImageFormat(SKImageInfo info) {
                this.Width = info.Width;
                this.Height = info.Height;
                this.ColorType = info.ColorType;
                this.AlphaType = info.AlphaType;
            }
        }
    }
}