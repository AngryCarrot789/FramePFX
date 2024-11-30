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
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using FramePFX.Avalonia.AvControls;
using FramePFX.Avalonia.Bindings;
using FramePFX.Avalonia.Utils;
using FramePFX.Editing.Rendering;
using FramePFX.Editing.ResourceManaging;
using FramePFX.Editing.ResourceManaging.Events;
using FramePFX.Editing.ResourceManaging.Resources;
using FramePFX.Utils.RDA;
using SkiaSharp;

namespace FramePFX.Avalonia.Editing.Resources.Lists;

public abstract class ResourceExplorerListItemContent : TemplatedControl {
    public static readonly ModelControlRegistry<BaseResource, ResourceExplorerListItemContent> Registry;

    public ResourceExplorerListBoxItem? ListItem { get; private set; }

    public BaseResource? Resource => this.ListItem?.Resource;

    protected ResourceExplorerListItemContent() {
    }

    static ResourceExplorerListItemContent() {
        Registry = new ModelControlRegistry<BaseResource, ResourceExplorerListItemContent>();
        Registry.RegisterType<ResourceFolder>(() => new RELICFolder());
        Registry.RegisterType<ResourceColour>(() => new RELICColour());
        Registry.RegisterType<ResourceImage>(() => new RELICImage());
        Registry.RegisterType<ResourceTextStyle>(() => new RELICTextStyle());
        Registry.RegisterType<ResourceAVMedia>(() => new RELICAVMedia());
        Registry.RegisterType<ResourceComposition>(() => new RELICComposition());
    }

    public void Connect(ResourceExplorerListBoxItem item) {
        this.ListItem = item ?? throw new ArgumentNullException(nameof(item));
        this.OnConnected();
    }

    public void Disconnect() {
        this.OnDisconnected();
        this.ListItem = null;
    }

    protected virtual void OnConnected() {
    }

    protected virtual void OnDisconnected() {
    }
}

public class RELICFolder : ResourceExplorerListItemContent {
    public static readonly DirectProperty<RELICFolder, int> ItemCountProperty = AvaloniaProperty.RegisterDirect<RELICFolder, int>(nameof(ItemCount), o => o.ItemCount);
    
    private int itemCount;

    public int ItemCount {
        get => this.itemCount;
        private set => this.SetAndRaise(ItemCountProperty, ref this.itemCount, value);
    }

    public new ResourceFolder? Resource => (ResourceFolder?) base.Resource;

    public RELICFolder() {
    }

    protected override void OnConnected() {
        base.OnConnected();
        this.Resource!.ResourceAdded += this.OnResourceAddedOrRemoved;
        this.Resource.ResourceRemoved += this.OnResourceAddedOrRemoved;
        this.Resource.ResourceMoved += this.OnResourceMoved;
        this.UpdateItemCount();
    }

    protected override void OnDisconnected() {
        base.OnDisconnected();
        this.Resource!.ResourceAdded -= this.OnResourceAddedOrRemoved;
        this.Resource.ResourceRemoved -= this.OnResourceAddedOrRemoved;
        this.Resource.ResourceMoved -= this.OnResourceMoved;
    }

    private void OnResourceAddedOrRemoved(ResourceFolder parent, BaseResource item, int index) => this.UpdateItemCount();

    private void OnResourceMoved(ResourceFolder sender, ResourceMovedEventArgs e) => this.UpdateItemCount();

    private void UpdateItemCount() {
        this.ItemCount = this.Resource!.Items.Count;
    }
}

public class RELICColour : ResourceExplorerListItemContent {
    public static readonly StyledProperty<SolidColorBrush?> BrushProperty = AvaloniaProperty.Register<RELICColour, SolidColorBrush?>(nameof(Brush));

    public SolidColorBrush? Brush {
        get => this.GetValue(BrushProperty);
        set => this.SetValue(BrushProperty, value);
    }

    public new ResourceColour? Resource => (ResourceColour?) base.Resource;

    private readonly AutoUpdateAndEventPropertyBinder<ResourceColour> colourBinder = new AutoUpdateAndEventPropertyBinder<ResourceColour>(BrushProperty, nameof(ResourceColour.ColourChanged), binder => {
        RELICColour element = (RELICColour) binder.Control;
        SKColor c = binder.Model.Colour;
        element.Brush!.Color = Color.FromArgb(c.Alpha, c.Red, c.Green, c.Blue);
    }, binder => {
        RELICColour element = (RELICColour) binder.Control;
        Color c = element.Brush!.Color;
        binder.Model.Colour = new SKColor(c.R, c.G, c.B, c.A);
    });

    public RELICColour() {
        this.Brush = new SolidColorBrush();
    }

    protected override void OnConnected() {
        base.OnConnected();
        this.colourBinder.Attach(this, this.Resource!);
    }

    protected override void OnDisconnected() {
        base.OnDisconnected();
        this.colourBinder.Detach();
    }
}

public class RELICImage : ResourceExplorerListItemContent {
    public new ResourceImage? Resource => (ResourceImage?) base.Resource;

    private Image PART_Image;
    private WriteableBitmap? bitmap;

    public RELICImage() {
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
                this.bitmap = new WriteableBitmap(new PixelSize(bmp.Width, bmp.Height), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Premul);
            }

            // Lock the WriteableBitmap for writing
            using (var lockedFramebuffer = this.bitmap.Lock()) {
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
        this.bitmap = null;
        this.PART_Image.Source = null;
    }
}

public class RELICTextStyle : ResourceExplorerListItemContent {
    public RELICTextStyle() {
    }
}

public class RELICAVMedia : ResourceExplorerListItemContent {
    public RELICAVMedia() {
    }
}

public class RELICComposition : ResourceExplorerListItemContent {
    private SKPreviewViewPortEx PART_ViewPort;

    public new ResourceComposition Resource => (ResourceComposition) base.Resource;

    private readonly RateLimitedDispatchAction updatePreviewExecutor;

    public RELICComposition() {
        this.updatePreviewExecutor = new RateLimitedDispatchAction(this.OnUpdatePreview, TimeSpan.FromSeconds(0.2));
    }

    private async Task OnUpdatePreview() {
        if (this.PART_ViewPort == null || this.ListItem == null)
            return;

        await IoC.Dispatcher.Invoke(async () => {
            if (this.PART_ViewPort == null || this.ListItem == null) {
                return;
            }

            ResourceComposition resource = this.Resource;
            RenderManager rm = resource.Timeline.RenderManager;
            if (rm.surface != null) {
                await (rm.LastRenderTask ?? Task.CompletedTask);
                if (rm.LastRenderRect.Width > 0 && rm.LastRenderRect.Height > 0) {
                    if (this.PART_ViewPort.BeginRenderWithSurface(rm.ImageInfo)) {
                        this.PART_ViewPort.EndRenderWithSurface(rm.surface);
                    }
                }
            }
        });
    }

    private void OnFrameRendered() {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_ViewPort = e.NameScope.GetTemplateChild<SKPreviewViewPortEx>(nameof(this.PART_ViewPort));
    }

    protected override void OnConnected() {
        base.OnConnected();
        this.Resource.Timeline.RenderManager.FrameRendered += this.RenderManagerOnFrameRendered;
        this.updatePreviewExecutor.InvokeAsync();
    }

    protected override void OnDisconnected() {
        base.OnDisconnected();
        this.Resource.Timeline.RenderManager.FrameRendered -= this.RenderManagerOnFrameRendered;
    }

    private void RenderManagerOnFrameRendered(RenderManager manager) {
        this.updatePreviewExecutor.InvokeAsync();
    }
}