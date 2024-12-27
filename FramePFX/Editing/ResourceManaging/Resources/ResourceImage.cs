//
// Copyright (c) 2023-2024 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System.Runtime.InteropServices;
using FramePFX.Editing.ResourceManaging.Autoloading;
using FramePFX.Editing.ResourceManaging.Events;
using FramePFX.Logging;
using FramePFX.Utils;
using SkiaSharp;

namespace FramePFX.Editing.ResourceManaging.Resources;

public class ResourceImage : ResourceItem {
    private string filePath;

    public string FilePath {
        get => this.filePath;
        set {
            this.filePath = value;
        }
    }

    public bool IsRawBitmapMode { get; private set; }

    public SKBitmap? bitmap;
    public SKImage? image;

    public object Shared0; // a shared WriteableBitmap for ImageClipContent
    public bool Shared1; // arePixelsDirty

    public event ResourceEventHandler? ImageChanged;

    public ResourceImage() {
    }

    static ResourceImage() {
        SerialisationRegistry.Register<ResourceImage>(0, (resource, data, ctx) => {
            ctx.DeserialiseBaseType(data);
            resource.IsRawBitmapMode = data.GetBool(nameof(resource.IsRawBitmapMode), false);
            if (resource.IsRawBitmapMode) {
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
                        byte[]? array = data.GetByteArray(nameof(resource.bitmap));
                        resource.bitmap = new SKBitmap();
                        fixed (byte* ptr = array) {
                            resource.bitmap.InstallPixels(format.ImageInfo, (IntPtr) ptr);
                        }

                        resource.image = resource.bitmap != null ? SKImage.FromBitmap(resource.bitmap) : null;
                    }
                    catch (Exception e) {
                        resource.bitmap?.Dispose();
                        resource.image?.Dispose();
                    }
                }
            }
            else if (data.TryGetString(nameof(resource.FilePath), out string? filePath)) {
                resource.FilePath = filePath;
            }
        }, (resource, data, ctx) => {
            ctx.SerialiseBaseType(data);
            if (resource.IsRawBitmapMode) {
                if (resource.bitmap != null) {
                    IntPtr pixels = resource.bitmap.GetPixels(out IntPtr length);
                    if (pixels != IntPtr.Zero && length != IntPtr.Zero) {
                        byte[] array = new byte[length.ToInt32()];
                        Marshal.Copy(pixels, array, 0, array.Length);
                        data.SetBool(nameof(resource.IsRawBitmapMode), true);
                        data.SetByteArray(nameof(resource.bitmap), array);

                        UnmanagedImageFormat imgFormat = new UnmanagedImageFormat(resource.bitmap.Info);
                        data.SetStruct("BitmapImageFormat", imgFormat);
                        data.SetInt("BitmapImageFormatHashCode", imgFormat.GetHashCode());
                    }
                }
            }
            else if (!string.IsNullOrEmpty(resource.FilePath)) {
                data.SetString(nameof(resource.FilePath), resource.FilePath);
            }
        });
    }

    public void SetBitmapImage(SKBitmap skBitmap, bool enableResource = true) {
        this.bitmap = skBitmap;
        this.image = SKImage.FromBitmap(skBitmap);
        this.IsRawBitmapMode = true;
        if (enableResource && !this.IsOnline) {
            this.Enable();
        }

        this.ImageChanged?.Invoke(this);
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

    protected override async ValueTask<bool> OnTryAutoEnable(ResourceLoader? loader) {
        if (string.IsNullOrEmpty(this.FilePath) || this.image != null) {
            return true;
        }

        try {
            await this.LoadImageAsync(this.FilePath);
            return true;
        }
        catch (Exception e) {
            loader?.AddEntry(new InvalidImagePathEntry(this));
            return false;
        }
    }

    public override async ValueTask<bool> TryEnableForLoaderEntry(InvalidResourceEntry entry) {
        if (entry is InvalidImagePathEntry imgEntry) {
            try {
                await this.LoadImageAsync(imgEntry.FilePath);
            }
            catch (Exception e) {
                AppLogger.Instance.WriteLine("Failed to autoload load image file: " + e.GetToString());
                return false;
            }
        }

        return await base.TryEnableForLoaderEntry(entry);
    }

    public async Task LoadImageAsync(string file) {
        SKBitmap? bmp = null;
        SKImage? img = null;
        try {
            await Task.Run(() => {
                using BufferedStream stream = new BufferedStream(File.OpenRead(file), 32768);
                bmp = SKBitmap.Decode(stream);
                img = SKImage.FromBitmap(bmp);
            });
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

    protected override void OnDisabled() {
        base.OnDisabled();
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