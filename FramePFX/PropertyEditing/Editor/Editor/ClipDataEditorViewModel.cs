using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Editor.History;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.Editor.ViewModels.Timelines.Clips;
using FramePFX.History;
using FramePFX.History.Tasks;

namespace FramePFX.PropertyEditing.Editor.Editor {
    public class ClipDataEditorViewModel : HistoryAwarePropertyEditorViewModel {
        protected readonly HistoryBuffer<HistoryClipDisplayName> displayNameHistory;

        private string displayName;

        public string DisplayName {
            get => this.displayName;
            set {
                this.RaisePropertyChanged(ref this.displayName, value);
                if (this.HistoryManager != null && !this.IsChangingAny()) {
                    if (!this.displayNameHistory.TryGetAction(out HistoryClipDisplayName action)) {
                        this.displayNameHistory.PushAction(this.HistoryManager, action = new HistoryClipDisplayName(this.Clips));
                    }

                    foreach (Transaction<string> t in action.DisplayName) {
                        t.Current = value;
                    }
                }

                foreach (object handler in this.Handlers) {
                    ((VideoClipViewModel) handler).DisplayName = value;
                }
            }
        }

        public IEnumerable<ClipViewModel> Clips => this.Handlers.Cast<ClipViewModel>();

        public ClipDataEditorViewModel() : base(typeof(ClipViewModel)) {
            this.displayNameHistory = new HistoryBuffer<HistoryClipDisplayName>();
        }

        protected override void OnClearHandlers() {
            base.OnClearHandlers();
            this.displayNameHistory.Clear();
        }

        protected override void OnHandlersLoaded() {
            base.OnHandlersLoaded();
            this.displayName = GetEqualValue(this.Handlers, (x) => ((ClipViewModel) x).DisplayName, out string name) ? name : IoC.Translator.GetString("S.PropertyEditor.Clips.DifferingDisplayNames");
            this.RaisePropertyChanged(nameof(this.DisplayName));
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