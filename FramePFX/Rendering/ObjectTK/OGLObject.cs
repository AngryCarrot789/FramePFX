using System;
using FramePFX.Logger;

namespace FramePFX.Rendering.ObjectTK
{
    public abstract class OGLObject : IDisposable
    {
        public int Handle { get; protected set; }

        protected OGLObject() : this(0)
        {
        }

        protected OGLObject(int handle)
        {
            this.Handle = handle;
        }

        ~OGLObject()
        {
            AppLogger.WriteLine("OGLObject leak: " + this.GetType());
            this.Dispose(false);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.Dispose(true);
        }

        protected abstract void Dispose(bool disposing);
    }
}