using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.History;

namespace FramePFX.Editor.History {
    public class HistoryBasicSingleProperty<TOwner, TValue> : BaseHistoryMultiHolderAction<TOwner> where TOwner : class, IHistoryHolder {
        public readonly Transaction<TValue>[] Values;
        private readonly Func<TOwner, TValue> getter;
        private readonly Action<TOwner, TValue> setter;
        private readonly Action onHistoryApplied;

        public HistoryBasicSingleProperty(IEnumerable<TOwner> holders, Func<TOwner, TValue> getter, Action<TOwner, TValue> setter, Action onHistoryApplied) : base(holders) {
            this.getter = getter;
            this.setter = setter;
            this.onHistoryApplied = onHistoryApplied;
            Transaction<TValue>[] array = new Transaction<TValue>[this.Holders.Count];
            for (int i = 0; i < array.Length; i++) {
                array[i] = Transactions.ForBoth(this.getter(this.Holders[i]));
            }

            this.Values = array;
        }

        public void SetCurrentValue(TValue value) {
            int i = 0;
            foreach (TOwner owner in this.Holders) {
                this.setter(owner, value);
                this.Values[i++].Current = value;
            }
        }

        protected override Task UndoAsync(TOwner holder, int i) {
            this.setter(holder, this.Values[i].Original);
            this.onHistoryApplied?.Invoke();
            return Task.CompletedTask;
        }

        protected override Task RedoAsync(TOwner holder, int i) {
            this.setter(holder, this.Values[i].Current);
            this.onHistoryApplied?.Invoke();
            return Task.CompletedTask;
        }
    }
}