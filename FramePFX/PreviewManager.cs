// 
// Copyright (c) 2026-2026 REghZy
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

using FramePFX.Editing;
using PFXToolKitUI.Utils.RDA;
using SkiaSharp;

namespace FramePFX;

public class PreviewManager {
    private readonly RapidDispatchActionEx rdaInvalidateRender;
    private volatile int suspendRenderCount;
    private bool isSkiaValid;
    private SKPixmap pixmap;
    private SKSurface surface;
    private SKBitmap bitmap;

    public SKImageInfo ImageInfo { get; private set; }

    public VideoEditor VideoEditor { get; }
    
    public PreviewManager(VideoEditor editor) {
        this.VideoEditor = editor;
    }
    
    // private void Reinit(PreviewSettings settings) {
    //     SKImageInfo info = new SKImageInfo(settings.Resolution.Width, settings.Resolution.Height, SKColorType.Bgra8888);
    //     if (this.ImageInfo == info && this.isSkiaValid) {
    //         return;
    //     }
    //     this.DisposeSkiaObjects();
    //     this.ImageInfo = info;
    //     this.bitmap = new SKBitmap(info);
    //     IntPtr ptr = this.bitmap.GetAddress(0, 0);
    //     this.pixmap = new SKPixmap(info, ptr, info.RowBytes);
    //     this.surface = SKSurface.Create(this.pixmap);
    //     this.isSkiaValid = true;
    // }

    /// <summary>
    /// Invalidates the view port's render
    /// </summary>
    public void InvalidateRender() {
    }

    public void Render() {
    }
    
    public SuspendRenderToken SuspendRenderInvalidation() {
        Interlocked.Increment(ref this.suspendRenderCount);
        return new SuspendRenderToken(this);
    }
    
    public struct SuspendRenderToken : IDisposable {
        internal PreviewManager? manager;

        internal SuspendRenderToken(PreviewManager manager) {
            this.manager = manager;
        }

        public void Dispose() {
            if (this.manager != null)
                Interlocked.Decrement(ref this.manager.suspendRenderCount);
            this.manager = null;
        }
    }
}