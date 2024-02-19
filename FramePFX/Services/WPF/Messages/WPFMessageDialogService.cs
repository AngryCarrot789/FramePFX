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

using System.Windows;
using FramePFX.Services.Messages;

namespace FramePFX.Services.WPF.Messages {
    public class WPFMessageDialogService : IMessageDialogService {
        public MessageBoxResult ShowMessage(string caption, string message, MessageBoxButton buttons = MessageBoxButton.OK) {
            MessageDialog dialog = new MessageDialog() {
                Title = caption,
                Header = null,
                Message = message,
                Buttons = buttons
            };

            dialog.ShowDialog();
            return dialog.GetClickedButton();
        }

        public MessageBoxResult ShowMessage(string caption, string header, string message, MessageBoxButton buttons = MessageBoxButton.OK) {
            MessageDialog dialog = new MessageDialog() {
                Title = caption,
                Header = header,
                Message = message,
                Buttons = buttons
            };

            dialog.ShowDialog();
            return dialog.GetClickedButton();
        }
    }
}