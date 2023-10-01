using System;
using System.Runtime.Serialization;

namespace FramePFX.Editor.Timelines
{
    /// <summary>
    /// An exception thrown when a render error occurs during the render process and is safely handled
    /// </summary>
    public class RenderException : Exception
    {
        public RenderException()
        {
        }

        protected RenderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public RenderException(string message) : base(message)
        {
        }

        public RenderException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}