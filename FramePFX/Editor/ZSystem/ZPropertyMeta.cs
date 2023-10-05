using System;

namespace FramePFX.Editor.ZSystem
{
    /// <summary>
    /// Extra metadata for a <see cref="ZProperty"/>
    /// </summary>
    public abstract class ZPropertyMeta
    {
        private bool isSealed;

        protected ZPropertyMeta()
        {
        }

        public void Seal()
        {
            if (this.isSealed)
                return;
            this.isSealed = true;
        }

        protected void ValidateNotSealed()
        {
            if (this.isSealed)
            {
                throw new Exception("Cannot modify object when it is sealed");
            }
        }
    }

    public class ZPropertyMeta<T> : ZPropertyMeta
    {
        public ZPropertyMeta()
        {
        }
    }
}