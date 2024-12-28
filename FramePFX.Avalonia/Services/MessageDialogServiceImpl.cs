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

using System.Threading.Tasks;
using Avalonia.Controls;
using FramePFX.BaseFrontEnd;
using FramePFX.Services.Messaging;
using FramePFX.Utils;
using MessageBoxDialog = FramePFX.BaseFrontEnd.Services.Messages.Controls.MessageBoxDialog;

namespace FramePFX.Avalonia.Services;

public class MessageDialogServiceImpl : IMessageDialogService {
    public Task<MessageBoxResult> ShowMessage(string caption, string message, MessageBoxButton buttons = MessageBoxButton.OK) {
        MessageBoxInfo info = new MessageBoxInfo(caption, message) { Buttons = buttons };
        info.SetDefaultButtonText();
        return this.ShowMessage(info);
    }

    public Task<MessageBoxResult> ShowMessage(string caption, string header, string message, MessageBoxButton buttons = MessageBoxButton.OK) {
        MessageBoxInfo info = new MessageBoxInfo(caption, header, message) { Buttons = buttons };
        info.SetDefaultButtonText();
        return this.ShowMessage(info);
    }

    public async Task<MessageBoxResult> ShowMessage(MessageBoxInfo info) {
        Validate.NotNull(info);
        if (Application.Instance.Dispatcher.CheckAccess()) {
            return await ShowMessageMainThread(info);
        }
        else {
            return await Application.Instance.Dispatcher.InvokeAsync(() => ShowMessageMainThread(info)).Unwrap();
        }
    }

    private static async Task<MessageBoxResult> ShowMessageMainThread(MessageBoxInfo info) {
        Validate.NotNull(info);
        if (IFrontEndApplication.Instance.TryGetActiveWindow(out Window? window)) {
            MessageBoxDialog dialog = new MessageBoxDialog {
                MessageBoxData = info
            };

            MessageBoxResult? result = await dialog.ShowDialog<MessageBoxResult?>(window);
            return result ?? MessageBoxResult.None;
        }

        return MessageBoxResult.None;
    }
}