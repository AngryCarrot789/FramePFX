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
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FramePFX.Editors.Controls.Bindings;
using FramePFX.Editors.Controls.Viewports;
using FramePFX.Editors.Rendering;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.ResourceManaging.Events;
using FramePFX.Editors.ResourceManaging.Resources;
using FramePFX.Utils;
using SkiaSharp;

namespace FramePFX.Editors.Controls.Resources.Explorers
{
    /// <summary>
    /// The abstract control for the content of a resource item in the explorer list (aka RELIC)
    /// </summary>
    public abstract class ResourceExplorerListItemContent : Control
    {
        private static readonly Dictionary<Type, Func<ResourceExplorerListItemContent>> Constructors;

        public ResourceExplorerListItem ListItem { get; private set; }

        public BaseResource Resource => this.ListItem?.Model;

        protected ResourceExplorerListItemContent()
        {
        }

        public static void RegisterType<T>(Type resourceType, Func<T> func) where T : ResourceExplorerListItemContent
        {
            Constructors[resourceType] = func;
        }

        public static ResourceExplorerListItemContent NewInstance(Type resourceType)
        {
            if (resourceType == null)
            {
                throw new ArgumentNullException(nameof(resourceType));
            }

            // Just try to find a base control type. It should be found first try unless I forgot to register a new control type
            bool hasLogged = false;
            for (Type type = resourceType; type != null; type = type.BaseType)
            {
                if (Constructors.TryGetValue(type, out Func<ResourceExplorerListItemContent> func))
                {
                    return func();
                }

                if (!hasLogged)
                {
                    hasLogged = true;
                    Debugger.Break();
                    Debug.WriteLine("Could not find control for resource type on first try. Scanning base types");
                }
            }

            throw new Exception("No such content control for resource type: " + resourceType.Name);
        }

        static ResourceExplorerListItemContent()
        {
            Constructors = new Dictionary<Type, Func<ResourceExplorerListItemContent>>();
            RegisterType(typeof(ResourceFolder), () => new RELICFolder());
            RegisterType(typeof(ResourceColour), () => new RELICColour());
            RegisterType(typeof(ResourceImage), () => new RELICImage());
            RegisterType(typeof(ResourceTextStyle), () => new RELICTextStyle());
            RegisterType(typeof(ResourceAVMedia), () => new RELICAVMedia());
            RegisterType(typeof(ResourceComposition), () => new RELICComposition());
        }

        protected override Size MeasureOverride(Size constraint)
        {
            return base.MeasureOverride(constraint);
        }

        public void Connect(ResourceExplorerListItem item)
        {
            this.ListItem = item ?? throw new ArgumentNullException(nameof(item));
            this.OnConnected();
        }

        public void Disconnect()
        {
            this.OnDisconnected();
            this.ListItem = null;
        }

        protected virtual void OnConnected()
        {
        }

        protected virtual void OnDisconnected()
        {
        }

        #region Template Utils

        protected void GetTemplateChild<T>(string name, out T value) where T : DependencyObject
        {
            if ((value = this.GetTemplateChild(name) as T) == null)
                throw new Exception("Missing part: " + name + " of type " + typeof(T));
        }

        protected T GetTemplateChild<T>(string name) where T : DependencyObject
        {
            this.GetTemplateChild(name, out T value);
            return value;
        }

        protected bool TryGetTemplateChild<T>(string name, out T value) where T : DependencyObject
        {
            return (value = this.GetTemplateChild(name) as T) != null;
        }

        #endregion
    }

    public class RELICFolder : ResourceExplorerListItemContent
    {
        public static readonly DependencyProperty ItemCountProperty = DependencyProperty.Register("ItemCount", typeof(int), typeof(RELICFolder), new PropertyMetadata(0));

        public int ItemCount
        {
            get => (int) this.GetValue(ItemCountProperty);
            private set => this.SetValue(ItemCountProperty, value);
        }

        public new ResourceFolder Resource => (ResourceFolder) base.Resource;

        public RELICFolder()
        {
        }

        protected override void OnConnected()
        {
            base.OnConnected();
            this.Resource.ResourceAdded += this.OnResourceAddedOrRemoved;
            this.Resource.ResourceRemoved += this.OnResourceAddedOrRemoved;
            this.Resource.ResourceMoved += this.OnResourceMoved;
            this.UpdateItemCount();
        }

        protected override void OnDisconnected()
        {
            base.OnDisconnected();
            this.Resource.ResourceAdded -= this.OnResourceAddedOrRemoved;
            this.Resource.ResourceRemoved -= this.OnResourceAddedOrRemoved;
            this.Resource.ResourceMoved -= this.OnResourceMoved;
        }

        private void OnResourceAddedOrRemoved(ResourceFolder parent, BaseResource item, int index) => this.UpdateItemCount();

        private void OnResourceMoved(ResourceFolder sender, ResourceMovedEventArgs e) => this.UpdateItemCount();

        private void UpdateItemCount()
        {
            this.ItemCount = this.Resource.Items.Count;
        }
    }

    public class RELICColour : ResourceExplorerListItemContent
    {
        public static readonly DependencyProperty ColourProperty = DependencyProperty.Register("Colour", typeof(Brush), typeof(RELICColour), new PropertyMetadata(null));

        public Brush Colour
        {
            get => (Brush) this.GetValue(ColourProperty);
            set => this.SetValue(ColourProperty, value);
        }

        public new ResourceColour Resource => (ResourceColour) base.Resource;

        private readonly UpdaterAutoEventPropertyBinder<ResourceColour> colourBinder = new UpdaterAutoEventPropertyBinder<ResourceColour>(ColourProperty, nameof(ResourceColour.ColourChanged), binder =>
        {
            RELICColour element = (RELICColour) binder.Control;
            SKColor c = binder.Model.Colour;
            ((SolidColorBrush) element.Colour).Color = Color.FromArgb(c.Alpha, c.Red, c.Green, c.Blue);
        }, binder =>
        {
            RELICColour element = (RELICColour) binder.Control;
            Color c = ((SolidColorBrush) element.Colour).Color;
            binder.Model.Colour = new SKColor(c.R, c.G, c.B, c.A);
        });

        public RELICColour()
        {
            this.Colour = new SolidColorBrush();
        }

        protected override void OnConnected()
        {
            base.OnConnected();
            this.colourBinder.Attach(this, this.Resource);
        }

        protected override void OnDisconnected()
        {
            base.OnDisconnected();
            this.colourBinder.Detach();
        }
    }

    public class RELICImage : ResourceExplorerListItemContent
    {
        public new ResourceImage Resource => (ResourceImage) base.Resource;

        private Image PART_Image;
        private WriteableBitmap bitmap;

        public RELICImage()
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_Image = this.GetTemplateChild<Image>(nameof(this.PART_Image));
        }

        protected override void OnConnected()
        {
            base.OnConnected();
            this.Resource.ImageChanged += this.ResourceOnImageChanged;
            this.TryLoadImage(this.Resource);
        }

        protected override void OnDisconnected()
        {
            base.OnDisconnected();
            this.Resource.ImageChanged -= this.ResourceOnImageChanged;
            this.ClearImage();
        }

        private void ResourceOnImageChanged(BaseResource resource)
        {
            this.TryLoadImage((ResourceImage) resource);
        }

        private void TryLoadImage(ResourceImage imgRes)
        {
            if (imgRes.bitmap != null)
            {
                SKBitmap bmp = imgRes.bitmap;
                if (this.bitmap == null || this.bitmap.PixelWidth != bmp.Width || this.bitmap.PixelHeight != bmp.Height)
                {
                    this.bitmap = new WriteableBitmap(bmp.Width, bmp.Height, 96, 96, PixelFormats.Pbgra32, null);
                }

                this.bitmap.WritePixels(new Int32Rect(0, 0, bmp.Width, bmp.Height), bmp.GetPixels(), bmp.ByteCount, bmp.RowBytes, 0, 0);
                this.PART_Image.Source = this.bitmap;
            }
            else
            {
                this.ClearImage();
            }
        }

        private void ClearImage()
        {
            this.bitmap = null;
            this.PART_Image.Source = null;
        }
    }

    public class RELICTextStyle : ResourceExplorerListItemContent
    {
        public RELICTextStyle()
        {
        }
    }

    public class RELICAVMedia : ResourceExplorerListItemContent
    {
        public RELICAVMedia()
        {
        }
    }

    public class RELICComposition : ResourceExplorerListItemContent
    {
        private SKPreviewViewPortEx PART_ViewPort;

        public new ResourceComposition Resource => (ResourceComposition) base.Resource;

        private readonly RateLimitedExecutor updatePreviewExecutor;

        public RELICComposition()
        {
            this.updatePreviewExecutor = new RateLimitedExecutor(this.OnUpdatePreview, TimeSpan.FromSeconds(0.2));
        }

        private async Task OnUpdatePreview()
        {
            if (this.PART_ViewPort == null || this.ListItem == null)
                return;

            await IoC.Dispatcher.Invoke(async () =>
            {
                if (this.PART_ViewPort == null || this.ListItem == null)
                {
                    return;
                }

                ResourceComposition resource = this.Resource;
                RenderManager rm = resource.Timeline.RenderManager;
                if (rm.surface != null)
                {
                    await (rm.LastRenderTask ?? Task.CompletedTask);
                    if (rm.LastRenderRect.Width > 0 && rm.LastRenderRect.Height > 0)
                    {
                        if (this.PART_ViewPort.BeginRenderWithSurface(rm.ImageInfo))
                        {
                            this.PART_ViewPort.EndRenderWithSurface(rm.surface);
                        }
                    }
                }
            });
        }

        private void OnFrameRendered()
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_ViewPort = this.GetTemplateChild<SKPreviewViewPortEx>(nameof(this.PART_ViewPort));
        }

        protected override void OnConnected()
        {
            base.OnConnected();
            this.Resource.Timeline.RenderManager.FrameRendered += this.RenderManagerOnFrameRendered;
            this.updatePreviewExecutor.OnInput();
        }

        protected override void OnDisconnected()
        {
            base.OnDisconnected();
            this.Resource.Timeline.RenderManager.FrameRendered -= this.RenderManagerOnFrameRendered;
        }

        private void RenderManagerOnFrameRendered(RenderManager manager)
        {
            this.updatePreviewExecutor.OnInput();
        }
    }
}