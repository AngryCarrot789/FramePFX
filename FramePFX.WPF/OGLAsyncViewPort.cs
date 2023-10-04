using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FramePFX.Utils;
using FramePFX.Utils.Numerics;
using OpenTK.Graphics.OpenGL;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
using Rect = System.Windows.Rect;
using Vector2 = System.Numerics.Vector2;

namespace FramePFX.WPF
{
    public class OGLAsyncViewPort : FrameworkElement
    {
        public static readonly DependencyProperty UseNearestNeighbourRenderingProperty =
            DependencyProperty.Register(
                "UseNearestNeighbourRendering",
                typeof(bool),
                typeof(OGLAsyncViewPort),
                new FrameworkPropertyMetadata(
                    BoolBox.False,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    (d, e) => RenderOptions.SetBitmapScalingMode(d, (bool) e.NewValue ? BitmapScalingMode.NearestNeighbor : BitmapScalingMode.Unspecified)));

        public bool UseNearestNeighbourRendering
        {
            get => (bool) this.GetValue(UseNearestNeighbourRenderingProperty);
            set => this.SetValue(UseNearestNeighbourRenderingProperty, value);
        }

        private readonly OGLContextWrapper ogl;
        private readonly bool designMode;
        private WriteableBitmap bitmap;
        private IntPtr hBitmap;
        private bool isRendering;
        private bool isCurrent;

        public Vector2 FrameSize { get; private set; }

        // [DllImport("Kernel32.dll")]
        // private static extern IntPtr GetModuleHandleA([MarshalAs(UnmanagedType.LPStr)] string lpModuleName);

        private IntPtr hRenderDoc;

        public OGLAsyncViewPort()
        {
            this.designMode = DesignerProperties.GetIsInDesignMode(this);
            this.ogl = new OGLContextWrapper();
            this.ogl.MakeCurrent(true);
            this.isCurrent = true;

            // this.hRenderDoc = GetModuleHandleA("renderdoc.dll");
            // if (this.hRenderDoc != IntPtr.Zero)
            // {
            // 
            // }

            GL.ClearColor(0.2f, 0.4f, 0.8f, 1.0f);
            // disabled so that both faces are rendered
            // GL.Enable(EnableCap.CullFace);
            // GL.CullFace(CullFaceMode.Back);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            GL.Enable(EnableCap.Multisample);
            GL.DepthMask(true);
        }

        public bool BeginRender()
        {
            PresentationSource source;
            if (this.isRendering || this.designMode || (source = PresentationSource.FromVisual(this)) == null)
            {
                return false;
            }

            SizeI scaledSize = this.CreateSize(out SizeI unscaledSize, out double scaleX, out double scaleY, source);
            if (unscaledSize.Width <= 0 || unscaledSize.Height <= 0)
            {
                return false;
            }

            this.FrameSize = new Vector2(unscaledSize.Width, unscaledSize.Height);

            this.MakeContextCurrent();
            WriteableBitmap bmp = this.bitmap;
            if (bmp == null || unscaledSize.Width != bmp.PixelWidth || unscaledSize.Height != bmp.PixelHeight)
            {
                this.bitmap = bmp = new WriteableBitmap(unscaledSize.Width, unscaledSize.Height, 96d, 96d, PixelFormats.Pbgra32, null);
                bmp.Lock();
                this.hBitmap = bmp.BackBuffer;
                bmp.Unlock();
            }

            this.isRendering = true;
            return true;
        }

        public void EndRender()
        {
            GL.Flush();
            GL.Finish();
            GL.ReadBuffer(ReadBufferMode.Back);
            GL.ReadPixels(0, 0, this.bitmap.PixelWidth, this.bitmap.PixelHeight, PixelFormat.Bgra, PixelType.UnsignedByte, this.hBitmap);
            this.bitmap.Lock();
            this.bitmap.AddDirtyRect(new Int32Rect(0, 0, this.bitmap.PixelWidth, this.bitmap.PixelHeight));
            this.bitmap.Unlock();
            this.isRendering = false;
            this.InvalidateVisual();
        }

        public void ClearContext()
        {
            if (this.isCurrent)
            {
                this.ogl.MakeCurrent(false);
                this.isCurrent = false;
            }
        }

        public void MakeContextCurrent()
        {
            if (!this.isCurrent)
            {
                this.ogl.MakeCurrent(true);
                this.isCurrent = true;
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            WriteableBitmap bmp = this.bitmap;
            if (bmp != null)
            {
                dc.DrawImage(bmp, new Rect(0d, 0d, this.ActualWidth, this.ActualHeight));
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            bool setCurrent = false;
            if (!this.isCurrent)
            {
                this.ogl.MakeCurrent(true);
                setCurrent = true;
            }

            int width = (int) Math.Floor(sizeInfo.NewSize.Width);
            int height = (int) Math.Floor(sizeInfo.NewSize.Height);
            this.ogl.UpdateSize(width, height);
            if (setCurrent)
            {
                this.ogl.MakeCurrent(false);
            }

            this.InvalidateVisual();
        }

        private SizeI CreateSize(out SizeI unscaledSize, out double scaleX, out double scaleY, PresentationSource source)
        {
            unscaledSize = SizeI.Empty;
            scaleX = 1f;
            scaleY = 1f;
            Size size = this.RenderSize;
            if (IsPositive(size.Width) && IsPositive(size.Height))
            {
                unscaledSize = new SizeI((int) size.Width, (int) size.Height);
                Matrix transformToDevice = source.CompositionTarget?.TransformToDevice ?? Matrix.Identity;
                scaleX = transformToDevice.M11;
                scaleY = transformToDevice.M22;
                return new SizeI((int) (size.Width * scaleX), (int) (size.Height * scaleY));
            }

            return SizeI.Empty;
        }

        private static bool IsPositive(double value) => !double.IsNaN(value) && !double.IsInfinity(value) && value > 0.0;
    }
}