using System;

namespace FramePFX.History
{
    public readonly struct HistoryUsage : IDisposable
    {
        private readonly IHistoryHolder holder;

        public HistoryUsage(IHistoryHolder holder)
        {
            this.holder = holder;
            holder.IsHistoryChanging = true;
        }

        public void Dispose()
        {
            this.holder.IsHistoryChanging = false;
        }
    }
}