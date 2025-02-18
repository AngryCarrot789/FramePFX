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

using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace PFXToolKitUI.Avalonia.CommandUsages;

/// <summary>
/// A class wrapper for button-like controls
/// </summary>
public abstract class ClickableControlHelper {
    public Control Control { get; }

    public Action? Action { get; set; }

    public abstract ICommand? Command { get; set; }

    public virtual bool IsEnabled {
        get => this.Control.IsEnabled;
        set => this.Control.IsEnabled = value;
    }

    protected ClickableControlHelper(Control obj, Action? onClick = null) {
        this.Control = obj;
        this.Action = onClick;
    }

    public static ClickableControlHelper Create(AvaloniaObject? obj, Action? onClick = null) {
        switch (obj) {
            // Button includes hyperlink too
            case Button b:   return new ButtonImpl(b, onClick);
            case MenuItem m: return new MenuItemImpl(m, onClick);
            // case SplitButton b: return new SplitButtonImpl(b, onClick);
        }

        throw new InvalidOperationException("Unknown control type for a button-esc control: " + (obj?.GetType().Name ?? "null"));
    }

    private class ButtonImpl : ClickableControlHelper {
        public new Button Control => (Button) base.Control;

        public override ICommand? Command {
            get => this.Control.Command;
            set => this.Control.Command = value;
        }

        public ButtonImpl(Button button, Action? onClick = null) : base(button, onClick) {
            button.Click += this.OnClicked;
        }

        private void OnClicked(object? sender, RoutedEventArgs e) {
            this.Action?.Invoke();
        }

        public override void Dispose() {
            base.Dispose();
            this.Command = null;
            this.Control.Click -= this.OnClicked;
        }
    }

    private class MenuItemImpl : ClickableControlHelper {
        public new MenuItem Control => (MenuItem) base.Control;

        public override ICommand? Command {
            get => this.Control.Command;
            set => this.Control.Command = value;
        }

        public MenuItemImpl(MenuItem button, Action? onClick = null) : base(button, onClick) {
            button.Click += this.OnClicked;
        }

        private void OnClicked(object? sender, RoutedEventArgs e) {
            this.Action?.Invoke();
        }

        public override void Dispose() {
            base.Dispose();
            this.Command = null;
            this.Control.Click -= this.OnClicked;
        }
    }

    public virtual void Dispose() {
    }
}