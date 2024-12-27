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

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using FramePFX.BaseFrontEnd.ResourceManaging;
using FramePFX.BaseFrontEnd.Utils;
using FramePFX.Editing.ResourceManaging;
using FramePFX.Editing.ResourceManaging.Resources;
using SkiaSharp;

namespace FramePFX.Avalonia.Editing.ResourceManaging.Lists.ContentItems;

public class RELIC_Image : ResourceExplorerListItemContent {
    public new ResourceImage? Resource => (ResourceImage?) base.Resource;

    private Image PART_Image;
    private WriteableBitmap? bitmap;

    public RELIC_Image() {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_Image = e.NameScope.GetTemplateChild<Image>(nameof(this.PART_Image));
    }

    protected override void OnConnected() {
        base.OnConnected();
        this.Resource!.ImageChanged += this.ResourceOnImageChanged;
        this.TryLoadImage(this.Resource);
    }

    protected override void OnDisconnected() {
        base.OnDisconnected();
        this.Resource!.ImageChanged -= this.ResourceOnImageChanged;
        this.ClearImage();
    }

    private void ResourceOnImageChanged(BaseResource resource) {
        this.TryLoadImage((ResourceImage) resource);
    }

    private unsafe void TryLoadImage(ResourceImage imgRes) {
        if (imgRes.bitmap != null) {
            SKBitmap bmp = imgRes.bitmap;
            if (this.bitmap == null || this.bitmap.PixelSize.Width != bmp.Width || this.bitmap.PixelSize.Height != bmp.Height) {
                this.bitmap?.Dispose();
                this.bitmap = new WriteableBitmap(new PixelSize(bmp.Width, bmp.Height), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Premul);
            }

            // Lock the WriteableBitmap for writing
            using (ILockedFramebuffer lockedFramebuffer = this.bitmap.Lock()) {
                IntPtr bmpPixels = bmp.GetPixels();
                if (bmpPixels == IntPtr.Zero)
                    throw new InvalidOperationException("Could not access SKBitmap pixels");

                int byteCount = bmp.Height * bmp.RowBytes;
                Buffer.MemoryCopy(bmpPixels.ToPointer(), lockedFramebuffer.Address.ToPointer(), byteCount, byteCount);
            }

            this.PART_Image.Source = this.bitmap;
        }
        else {
            this.ClearImage();
        }
    }

    private void ClearImage() {
        this.bitmap?.Dispose();
        this.bitmap = null;
        this.PART_Image.Source = null;
    }
}