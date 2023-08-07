namespace FramePFX.Core.Views.Dialogs.Message
{
    /// <summary>
    /// A static class that contains some of the general message dialogs
    /// </summary>
    public static class Dialogs
    {
        public static readonly MessageDialog OkDialog;
        public static readonly MessageDialog OkCancelDialog;
        public static readonly MessageDialog YesCancelDialog;
        public static readonly MessageDialog YesNoDialog;
        public static readonly MessageDialog YesNoCancelDialog;

        public static readonly MessageDialog ItemAlreadyExistsDialog;
        public static readonly MessageDialog OpenFileFailureDialog;
        public static readonly MessageDialog UnknownFileFormatDialog;
        public static readonly MessageDialog ClipboardUnavailableDialog;
        public static readonly MessageDialog InvalidClipboardDataDialog;
        public static readonly MessageDialog InvalidPathDialog;
        public static readonly MessageDialog RemoveItemWhenDeletingDialog;

        static Dialogs()
        {
            YesNoCancelDialog = new MessageDialog("yes");
            YesNoCancelDialog.AddButton("Yes", "yes");
            YesNoCancelDialog.AddButton("No", "no");
            YesNoCancelDialog.AddButton("Cancel", "cancel", false);
            YesNoCancelDialog.MarkReadOnly();

            YesNoDialog = new MessageDialog("yes");
            YesNoDialog.AddButton("Yes", "yes");
            YesNoDialog.AddButton("No", "no");
            YesNoDialog.MarkReadOnly();

            OkDialog = new MessageDialog("ok");
            OkDialog.AddButton("OK", "ok");
            OkDialog.MarkReadOnly();

            OkCancelDialog = new MessageDialog("ok");
            OkCancelDialog.AddButton("OK", "ok");
            OkCancelDialog.AddButton("Cancel", "cancel", false);
            OkCancelDialog.MarkReadOnly();

            YesCancelDialog = new MessageDialog("yes");
            YesCancelDialog.AddButton("Yes", "yes");
            YesCancelDialog.AddButton("Cancel", "cancel", false);
            YesCancelDialog.MarkReadOnly();

            ClipboardUnavailableDialog = OkDialog.Clone();
            ClipboardUnavailableDialog.ShowAlwaysUseNextResultOption = true;

            InvalidClipboardDataDialog = OkDialog.Clone();
            InvalidClipboardDataDialog.ShowAlwaysUseNextResultOption = true;

            ItemAlreadyExistsDialog = new MessageDialog("replace") {ShowAlwaysUseNextResultOption = true};
            ItemAlreadyExistsDialog.AddButton("Replace", "replace").ToolTip = "Replace the existing item with the new item";
            ItemAlreadyExistsDialog.AddButton("Add anyway", "keep").ToolTip = "Keeps the existing item and adds the new item, resulting in 2 items with the same file path";
            ItemAlreadyExistsDialog.AddButton("Ignore", "ignore").ToolTip = "Ignores the file, leaving the existing item as-is";
            ItemAlreadyExistsDialog.AddButton("Cancel", "cancel", false).ToolTip = "Stop adding files and remove all files that have been added";

            UnknownFileFormatDialog = new MessageDialog("ok") {ShowAlwaysUseNextResultOption = true};
            UnknownFileFormatDialog.AddButton("OK", "ok");
            UnknownFileFormatDialog.AddButton("Cancel", "cancel", false);

            OpenFileFailureDialog = OkDialog.Clone();
            OpenFileFailureDialog.MarkReadOnly();

            InvalidPathDialog = OkDialog.Clone();
            InvalidPathDialog.MarkReadOnly();

            RemoveItemWhenDeletingDialog = new MessageDialog("yes") {ShowAlwaysUseNextResultOption = true};
            RemoveItemWhenDeletingDialog.AddButton("Yes", "yes");
            RemoveItemWhenDeletingDialog.AddButton("No", "no");
            RemoveItemWhenDeletingDialog.AddButton("Cancel", "cancel", false);
        }
    }
}