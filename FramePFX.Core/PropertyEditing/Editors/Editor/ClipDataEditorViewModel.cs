using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.Core.Editor.History;
using FramePFX.Core.Editor.ViewModels.Timelines;
using FramePFX.Core.Editor.ViewModels.Timelines.Clips;
using FramePFX.Core.History;

namespace FramePFX.Core.PropertyEditing.Editors.Editor {
    public class ClipDataEditorViewModel : HistoryAwarePropertyEditorViewModel {
        protected HistoryClipDisplayName displayNameHistory;

        private string displayName;

        public string DisplayName {
            get => this.displayName;
            set {
                this.RaisePropertyChanged(ref this.displayName, value);
                if (this.displayNameHistory != null && this.HistoryManager != null && !this.IsChangingAny()) {
                    foreach (Transaction<string> t in this.displayNameHistory.DisplayName) {
                        t.Current = value;
                    }
                }

                foreach (object handler in this.Handlers) {
                    ((VideoClipViewModel) handler).DisplayName = value;
                }
            }
        }

        public ClipDataEditorViewModel() : base(typeof(ClipViewModel)) {

        }

        protected override void OnClearHandlers() {
            base.OnClearHandlers();
            this.displayNameHistory = null;
        }

        protected class HistoryClipDisplayName : BaseHistoryMultiHolderAction<ClipViewModel> {
            public readonly Transaction<string>[] DisplayName;

            public HistoryClipDisplayName(IEnumerable<ClipViewModel> holders) : base(holders) {
                this.DisplayName = Transactions.NewArray(this.Holders, x => x.DisplayName);
            }

            protected override Task UndoAsyncCore(ClipViewModel holder, int i) {
                holder.DisplayName = this.DisplayName[i].Original;
                return Task.CompletedTask;
            }

            protected override Task RedoAsyncCore(ClipViewModel holder, int i) {
                holder.DisplayName = this.DisplayName[i].Current;
                return Task.CompletedTask;
            }
        }
    }
}