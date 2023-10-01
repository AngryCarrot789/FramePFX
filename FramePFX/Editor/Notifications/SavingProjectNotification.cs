using System.Threading.Tasks;
using FramePFX.Notifications;

namespace FramePFX.Editor.Notifications
{
    public class SavingProjectNotification : NotificationViewModel
    {
        private bool isSaving;

        public bool IsSaving
        {
            get => this.isSaving;
            private set => this.RaisePropertyChanged(ref this.isSaving, value);
        }

        private bool isSuccess;

        public bool IsSuccess
        {
            get => this.isSuccess;
            private set => this.RaisePropertyChanged(ref this.isSuccess, value);
        }

        private string errorMessage;

        public string ErrorMessage
        {
            get => this.errorMessage;
            private set => this.RaisePropertyChanged(ref this.errorMessage, value);
        }

        public SavingProjectNotification()
        {
        }

        public override Task HideAction()
        {
            return this.IsSaving ? Task.CompletedTask : base.HideAction();
        }

        public void BeginSave()
        {
            this.IsSaving = true;
            this.IsSuccess = false;
            this.ErrorMessage = null;
        }

        public void OnSaveComplete()
        {
            this.IsSaving = false;
            this.IsSuccess = true;
            this.ErrorMessage = null;
        }

        public void OnSaveFailed(string errMsg)
        {
            this.IsSaving = false;
            this.IsSuccess = false;
            this.ErrorMessage = errMsg;
        }
    }
}