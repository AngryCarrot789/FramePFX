using System;
using FramePFX.Core.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace FramePFX.ResourceManaging.Items {
    public class ResourceImage : ResourceItem {
        public string FilePath { get; set; }

        public bool IsFileBased => !string.IsNullOrEmpty(this.FilePath);

        public Image<Bgra32> ImageData { get; set; }

        public ResourceImage(ResourceManager manager) : base(manager) {

        }

        public void LoadImageData(string path) {
            this.ImageData = Image.Load<Bgra32>(path);
        }

        protected override void DisposeResource(ExceptionStack stack) {
            base.DisposeResource(stack);
            try {
                this.ImageData?.Dispose();
            }
            catch (Exception e) {
                stack.Add(e);
            }
        }
    }
}