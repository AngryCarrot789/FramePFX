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

public class InvalidImagePathEntryControl : InvalidResourceEntryControl
{
    private TextBox? filePathBox;
    private Button? confirmButton;

    private readonly GetSetAutoUpdateAndEventPropertyBinder<InvalidImagePathEntry> filePathBinder;

    public new InvalidImagePathEntry? Entry => (InvalidImagePathEntry?) base.Entry;

    public InvalidImagePathEntryControl()
    {
        this.filePathBinder = new GetSetAutoUpdateAndEventPropertyBinder<InvalidImagePathEntry>(TextBox.TextProperty, nameof(InvalidImagePathEntry.FilePathChanged), b => b.Model.FilePath, (b, v) => b.Model.FilePath = (string) v!);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        this.filePathBox = e.NameScope.GetTemplateChild<TextBox>("PART_TextBox");
        this.confirmButton = e.NameScope.GetTemplateChild<Button>("PART_Button");
        this.confirmButton.Command = new AsyncRelayCommand(async () =>
        {
            if (!await this.Entry!.TryLoad())
            {
                await IMessageDialogService.Instance.ShowMessage("No such file", "File path is still invalid");
            }
        });
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        this.filePathBinder.Attach(this.filePathBox!, this.Entry!);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        this.filePathBinder.Detach();
    }
}