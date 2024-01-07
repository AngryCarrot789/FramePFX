using System;
using FramePFX.Commands;
using FramePFX.Editor.ViewModels.Timelines;

namespace FramePFX.PropertyEditing.Editors {
    public class BaseAutomatablePropertyEditorViewModel : BasePropertyEditorViewModel {
        public RelayCommand ResetValueCommand { get; }

        public RelayCommand InsertKeyFrameCommand { get; }

        private readonly ClipAutomatableEditorHelper clipAutomatableEditorHelper;

        public BaseAutomatablePropertyEditorViewModel(Type applicableType) : base(applicableType) {
            this.ResetValueCommand = new RelayCommand(this.ResetValue, this.CanResetValue);
            this.InsertKeyFrameCommand = new RelayCommand(this.InsertKeyFrame, this.CanInsertKeyFrame);
            this.clipAutomatableEditorHelper = new ClipAutomatableEditorHelper(this, this.OnUpdateCommands);
        }

        protected virtual void OnUpdateCommands(ClipAutomatableEditorHelper obj) {
            this.ResetValueCommand.RaiseCanExecuteChanged();
            this.InsertKeyFrameCommand.RaiseCanExecuteChanged();
        }

        public virtual bool CanResetValue() {
            if (!this.HasHandlers)
                return false;

            if (this.IsSingleSelection && this.Handlers[0] is ClipViewModel clip)
                return clip.IsTimelineFrameInRange(clip.Timeline?.PlayHeadFrame ?? 0);

            return true;
        }

        public virtual void ResetValue() {

        }

        public virtual bool CanInsertKeyFrame() {
            if (this.IsMultiSelection)
                return false;

            if (this.IsSingleSelection && this.Handlers[0] is ClipViewModel clip)
                return clip.IsTimelineFrameInRange(clip.Timeline?.PlayHeadFrame ?? 0);

            return true;

        }

        public virtual void InsertKeyFrame() {

        }
    }
}