using System;
using System.IO;
using System.Runtime.InteropServices;
using FramePFX.Editors.ResourceManaging.Autoloading;
using FramePFX.Editors.ResourceManaging.Events;
using FramePFX.Logger;
using FramePFX.RBC;
using FramePFX.Utils;
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

        public bool IsRawBitmapMode { get; private set; }

        public SKBitmap bitmap;
        public SKImage image;

        public object Shared0; // a shared WriteableBitmap for ImageClipContent
        public bool Shared1; // arePixelsDirty

        public event ResourceEventHandler ImageChanged;

        public ResourceImage() {
        }

        public void SetBitmapImage(SKBitmap skBitmap, bool enableResource = true) {
            this.bitmap = skBitmap;
            this.image = SKImage.FromBitmap(skBitmap);
            this.IsRawBitmapMode = true;
            if (enableResource) {
                this.TryAutoEnable(null);
            }

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

                        UnmanagedImageFormat imgFormat = new UnmanagedImageFormat(this.bitmap.Info);
                        data.SetStruct("BitmapImageFormat", imgFormat);
                        data.SetInt("BitmapImageFormatHashCode", imgFormat.GetHashCode());
                    }
                }
            }
            else if (!string.IsNullOrEmpty(this.FilePath)) {
                data.SetString(nameof(this.FilePath), this.FilePath);
            }
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.IsRawBitmapMode = data.GetBool(nameof(this.IsRawBitmapMode), false);
            if (this.IsRawBitmapMode) {
                int hashCode = data.GetInt("BitmapImageFormatHashCode");
                UnmanagedImageFormat format = data.GetStruct<UnmanagedImageFormat>("BitmapImageFormat");
                if (format.Width <= 0 || format.Height <= 0) {
                    return;
                }

                // !!!
                if (format.GetHashCode() != hashCode) {
                    return;
                }

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
            else if (data.TryGetString(nameof(this.FilePath), out string filePath)) {
                this.FilePath = filePath;
            }
        }

        protected override void LoadDataIntoClone(BaseResource clone) {
            base.LoadDataIntoClone(clone);
            ResourceImage cloned = (ResourceImage) clone;
            if (this.IsRawBitmapMode) {
                if (this.bitmap != null) {
                    cloned.bitmap = this.bitmap.Copy();
                    if (cloned.bitmap != null)
                        cloned.image = SKImage.FromBitmap(cloned.bitmap);
                }
            }
            else {
                cloned.filePath = this.filePath;
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
            if (entry is InvalidImagePathEntry imgEntry) {
                try {
                    this.LoadImageAsync(imgEntry.FilePath);
                }
                catch (Exception e) {
                    AppLogger.Instance.WriteLine("Failed to autoload load image file: " + e.GetToString());
                    return false;
                }
            }

            return base.TryEnableForLoaderEntry(entry);
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

        protected override void OnDisableCore(bool user) {
            base.OnDisableCore(user);
            this.DisposeImage(false);
        }

        public override void Destroy() {
            base.Destroy();
            this.DisposeImage(true);
        }

        private void DisposeImage(bool canDisposeRawBitmap) {
            if ((this.bitmap != null || this.image != null) && (!this.IsRawBitmapMode || canDisposeRawBitmap)) {
                this.bitmap?.Dispose();
                this.bitmap = null;
                this.image?.Dispose();
                this.image = null;
                this.IsRawBitmapMode = false;
                this.ImageChanged?.Invoke(this);
            }
        }

        public void ClearRawBitmapImage() {
            if (!this.IsRawBitmapMode) {
                throw new InvalidOperationException("Not using a raw bitmap");
            }

            this.DisposeImage(true);
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

            public bool Equals(UnmanagedImageFormat other) {
                return this.Width == other.Width && this.Height == other.Height && this.ColorType == other.ColorType && this.AlphaType == other.AlphaType;
            }

            public override bool Equals(object obj) {
                return obj is UnmanagedImageFormat other && this.Equals(other);
            }

            public override int GetHashCode() {
                unchecked {
                    int hash = this.Width;
                    hash = (hash * 397) ^ this.Height;
                    hash = (hash * 397) ^ (int) this.ColorType;
                    hash = (hash * 397) ^ (int) this.AlphaType;
                    return hash;
                }
            }
        }
    }
}