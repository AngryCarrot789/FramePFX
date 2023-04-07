using System.Collections.Generic;
using System.ComponentModel;
using FramePFX.Render;
using FramePFX.ResourceManaging.Items;
using FramePFX.Timeline.ViewModels.ClipProperties;
using FramePFX.Timeline.ViewModels.ClipProperties.Resizable;
using OpenTK.Graphics.OpenGL;

namespace FramePFX.Timeline.ViewModels.Clips.Resizable {
    public class ShapeClipViewModel : ResizableVideoClipViewModel {
        private ResourceColourViewModel resource;
        public ResourceColourViewModel Resource {
            get => this.resource;
            set {
                if (this.resource != null)
                    this.resource.PropertyChanged -= this.OnPropertyChanged;
                this.RaisePropertyChanged(ref this.resource, value);
                if (value != null)
                    this.resource.PropertyChanged += this.OnPropertyChanged;
            }
        }

        public float R {
            get => this.resource.Red;
            set => this.resource.Red = value;
        }

        public float G {
            get => this.resource.Green;
            set => this.resource.Green = value;
        }

        public float B {
            get => this.resource.Blue;
            set => this.resource.Blue = value;
        }

        public float A {
            get => this.resource.Alpha;
            set => this.resource.Alpha = value;
        }

        public ShapeClipViewModel() {
            this.Resource = new ResourceColourViewModel() {
                Red = 1f,
                Green = 1f,
                Blue = 1f,
                Alpha = 1f
            };

            this.PropertyChanged += this.OnPropertyChanged;
        }

        public override void AccumulatePropertyGroups(ICollection<PropertyGroupViewModel> list) {
            base.AccumulatePropertyGroups(list);
            list.Add(new ShapePropertyGroupViewModel(this));
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
            this.Container?.MarkForRender();
        }

        // Just gonna put rendering in the ViewModels... sure it's not very "MVVM-ey", but
        // rendering is done off the WPF main thread, so it can't be done in the UI controls
        // because dependency property access must be done on the main thread, and having
        // 3 sets of the same data ("ClipImpl", "ClipViewModel", "ClipControl") is just annoying
        // ViewModel data is thread-safe for the most part, because it references a field

        public override void RenderCore(IViewPort vp, long frame) {
            GL.Begin(PrimitiveType.Quads);
            GL.Color4(this.R, this.G, this.B, this.A);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(1, 0, 0);
            GL.Vertex3(1, 1, 0);
            GL.Vertex3(0, 1, 0);
            GL.End();
        }
    }
}