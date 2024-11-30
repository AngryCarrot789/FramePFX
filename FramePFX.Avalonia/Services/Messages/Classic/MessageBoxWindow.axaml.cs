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

using Avalonia;
using Avalonia.Controls;
using FramePFX.Services.Messaging;

namespace FramePFX.Avalonia.Services.Messages.Classic;

public delegate void MessageBoxWindowResultChangedEventHandler(MessageBoxWindow sender);

public partial class MessageBoxWindow : Window {
    public static readonly StyledProperty<string?> HeaderMessageProperty = AvaloniaProperty.Register<MessageBoxWindow, string?>("HeaderMessage");
    public static readonly StyledProperty<string?> MessageProperty = AvaloniaProperty.Register<MessageBoxWindow, string?>("Message");

    private MessageBoxResult result;
    
    public string? HeaderMessage {
        get => this.GetValue(HeaderMessageProperty);
        set => this.SetValue(HeaderMessageProperty, value);
    }

    public string? Message {
        get => this.GetValue(MessageProperty);
        set => this.SetValue(MessageProperty, value);
    }

    public MessageBoxResult Result {
        get => this.result;
        private set {
            if (this.result == value)
                return;

            this.result = value;
            this.ResultChanged?.Invoke(this);
        }
    }

    public event MessageBoxWindowResultChangedEventHandler? ResultChanged;

    public MessageBoxWindow() {
        this.InitializeComponent();
    }
    
    static MessageBoxWindow() {
        HeaderMessageProperty.Changed.AddClassHandler<MessageBoxWindow, string?>((o, e) => {
            o.PART_MessageHeader.IsVisible = !string.IsNullOrWhiteSpace(e.GetNewValue<string?>());
        });
    }
}