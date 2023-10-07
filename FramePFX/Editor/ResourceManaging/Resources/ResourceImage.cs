using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FramePFX.Logger;
using FramePFX.RBC;
using FramePFX.Utils;
using SkiaSharp;

namespace FramePFX.Editor.ResourceManaging.Resources {
    public class ResourceImage : ResourceItem {
        public string FilePath { get; set; }

        public bool IsRawBitmapMode { get; set; }

        public SKBitmap bitmap;
        public SKImage image;

        public ResourceImage() {
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
                AppLogger.WriteLine("Did not expect bitmap or image to still be valid while deserialising resource image...");
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
                            AppLogger.WriteLine("Failed to create SKBitmap from array\n" + e.GetToString());
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

        public async Task LoadImageAsync(string file, bool setOnline = true) {
            SKBitmap loadedBitmap = await LoadBitmapAsync(file);
            if (this.bitmap != null || this.image != null) {
                this.bitmap?.Dispose();
                this.bitmap = null;
                this.image?.Dispose();
                this.image = null;
            }

            this.bitmap = loadedBitmap;
            this.image = await Task.Run(() => SKImage.FromBitmap(loadedBitmap));
            if (setOnline && !this.IsOnline) {
                this.IsOnline = true;
                this.OnIsOnlineStateChanged();
            }
        }

        public override void Dispose() {
            base.Dispose();
            this.bitmap?.Dispose();
            this.bitmap = null;
            this.image?.Dispose();
            this.image = null;
        }

        public static async Task<SKBitmap> LoadBitmapAsync(string path) {
            using (BufferedStream stream = new BufferedStream(File.OpenRead(path), 16384)) {
                return await Task.Run(() => SKBitmap.Decode(stream));
            }
        }
    }
}