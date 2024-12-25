//
// Copyright (c) 2023-2024 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using FramePFX.Avalonia.Bindings;
using FramePFX.Avalonia.Utils;
using FramePFX.Editing.ResourceManaging.Resources;
using FramePFX.Services.Messaging;
using FramePFX.Utils.Commands;

namespace FramePFX.Avalonia.Editing.ResourceManaging.Autoloading;

public class InvalidMediaPathEntryControl : InvalidResourceEntryControl
{
    private TextBox? filePathBox;
    private TextBox? errorMessageBlock;
    private Button? confirmButton;

    private readonly GetSetAutoUpdateAndEventPropertyBinder<InvalidMediaPathEntry> filePathBinder;
    private readonly GetSetAutoUpdateAndEventPropertyBinder<InvalidMediaPathEntry> exceptionMsgBinder;

    public new InvalidMediaPathEntry? Entry => (InvalidMediaPathEntry?) base.Entry;

    public InvalidMediaPathEntryControl()
    {
        this.filePathBinder = new GetSetAutoUpdateAndEventPropertyBinder<InvalidMediaPathEntry>(TextBox.TextProperty, nameof(InvalidMediaPathEntry.FilePathChanged), b => b.Model.FilePath, (b, v) => b.Model.FilePath = (string) v!);
        this.exceptionMsgBinder = new GetSetAutoUpdateAndEventPropertyBinder<InvalidMediaPathEntry>(TextBox.TextProperty, nameof(InvalidMediaPathEntry.ExceptionMessageChanged), b => b.Model.ExceptionMessage, null);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        this.filePathBox = e.NameScope.GetTemplateChild<TextBox>("PART_TextBox");
        this.confirmButton = e.NameScope.GetTemplateChild<Button>("PART_Button");
        this.errorMessageBlock = e.NameScope.GetTemplateChild<TextBox>("PART_TextBlockErrMsg");
        this.confirmButton.Command = new AsyncRelayCommand(async () =>
        {
            if (!await this.Entry!.TryLoad())
            {
                await IMessageDialogService.Instance.ShowMessage("No such file", "Media file path is still invalid");
            }
        });
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        this.filePathBinder.Attach(this.filePathBox!, this.Entry!);
        this.exceptionMsgBinder.Attach(this.errorMessageBlock!, this.Entry!);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        this.filePathBinder.Detach();
        this.exceptionMsgBinder.Detach();
    }
}