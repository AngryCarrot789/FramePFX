using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using FramePFX.RBC;
using SkiaSharp;

namespace FramePFX.Editor.ResourceManaging.Resources {
    public class ResourceImage : ResourceItem {
        public string FilePath { get; set; }

        public bool IsRawBitmapMode { get; set; }

        private SKBitmap bitmap;
        public SKImage image;

        public ResourceImage() {
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            if (this.IsRawBitmapMode) {
                if (this.bitmap != null) {
                    using (MemoryStream stream = new MemoryStream(this.bitmap.ByteCount)) {
                        this.bitmap.Encode(stream, SKEncodedImageFormat.Bmp, 5);
                        data.SetBool(nameof(this.IsRawBitmapMode), true);
                        data.SetByteArray(nameof(this.bitmap), stream.ToArray());
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
                if (data.TryGetByteArray(nameof(this.bitmap), out byte[] array)) {
                    this.bitmap = SKBitmap.Decode(array);
                    this.image = SKImage.FromBitmap(this.bitmap);
                }
            }
            else if (data.TryGetString(nameof(this.FilePath), out string filePath)) {
                this.FilePath = filePath;
            }
        }

        public async Task LoadImageAsync(string file, bool setOnline = true) {
            SKBitmap loadedBitmap = await LoadBitmapAsync(file);
            if (this.bitmap != null || this.image != null) {
#if DEBUG
                try {
                    this.bitmap?.Dispose();
                    this.image?.Dispose();
                }
                finally {
                    this.bitmap = null;
                    this.image = null;
                }
#else // lazy
                this.DisposeImageCareless();
#endif
            }

            this.bitmap = loadedBitmap;
            this.image = await Task.Run(() => SKImage.FromBitmap(this.bitmap));
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
            using (BufferedStream stream = new BufferedStream(File.OpenRead(path), 8192)) {
                return await Task.Run(() => SKBitmap.Decode(stream));
            }
        }
    }
}