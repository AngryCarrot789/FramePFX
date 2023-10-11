using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Editor.History;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.History;
using FramePFX.History.Tasks;
using FramePFX.History.ViewModels;
using FramePFX.PropertyEditing;

namespace FramePFX.Editor.PropertyEditors.Tracks {
    public class TrackDataEditorViewModel : BasePropertyEditorViewModel {
        protected readonly HistoryBuffer<HistoryTrackDisplayName> displayNameHistory;

        private string displayName;

        public string DisplayName {
            get => this.displayName;
            set {
                this.RaisePropertyChanged(ref this.displayName, value);
                if (!this.displayNameHistory.TryGetAction(out HistoryTrackDisplayName action)) {
                    this.displayNameHistory.PushAction(HistoryManagerViewModel.Instance, action = new HistoryTrackDisplayName(this.Tracks));
                }

                foreach (Transaction<string> t in action.DisplayName) {
                    t.Current = value;
                }

                foreach (TrackViewModel handler in this.Tracks) {
                    using (handler.PushUsage()) {
                        handler.DisplayName = value;
                    }
                }
            }
        }

        public IEnumerable<TrackViewModel> Tracks => this.Handlers.Cast<TrackViewModel>();


        public TrackDataEditorViewModel() : base(typeof(TrackViewModel)) {
            this.displayNameHistory = new HistoryBuffer<HistoryTrackDisplayName>();
        }

        protected override void OnClearingHandlers() {
            base.OnClearingHandlers();
            this.displayNameHistory.Clear();
        }

        protected override void OnHandlersLoaded() {
            base.OnHandlersLoaded();
            this.displayName = GetEqualValue(this.Handlers, (x) => ((TrackViewModel) x).DisplayName, out string name) ? name : Services.Translator.GetString("S.PropertyEditor.NamedObject.DifferingDisplayNames");
            this.RaisePropertyChanged(nameof(this.DisplayName));
        }

        protected class HistoryTrackDisplayName : BaseHistoryMultiHolderAction<TrackViewModel> {
            public readonly Transaction<string>[] DisplayName;

            public HistoryTrackDisplayName(IEnumerable<TrackViewModel> holders) : base(holders) {
                this.DisplayName = Transactions.NewArray(this.Holders, x => x.DisplayName);
            }

            protected override Task UndoAsync(TrackViewModel holder, int i) {
                holder.DisplayName = this.DisplayName[i].Original;
                return Task.CompletedTask;
            }

            protected override Task RedoAsync(TrackViewModel holder, int i) {
                holder.DisplayName = this.DisplayName[i].Current;
                return Task.CompletedTask;
            }
        }
    }
}