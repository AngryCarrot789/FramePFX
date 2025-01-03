// 
// Copyright (c) 2024-2024 REghZy
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

using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;
using FramePFX.AdvancedMenuService;
using FramePFX.BaseFrontEnd.Shortcuts.Converters;
using FramePFX.BaseFrontEnd.Utils;
using FramePFX.CommandSystem;
using FramePFX.Interactivity.Contexts;
using FramePFX.Services.Messaging;
using FramePFX.Utils;

namespace FramePFX.BaseFrontEnd.AdvancedMenuService;

public class AdvancedCommandMenuItem : AdvancedMenuItem {
    private bool canExecute;
    private TextBlock? InputGestureTextBlock;

    public bool IsExecuting { get; private set; }

    protected bool CanExecute {
        get => this.canExecute;
        set {
            this.canExecute = value;

            // Causes IsEnableCore to be fetched, which returns false if we are executing something or
            // we have no valid command, causing this menu item to be "disabled"
            this.UpdateIsEffectivelyEnabled();
        }
    }

    public new CommandContextEntry? Entry => (CommandContextEntry?) base.Entry;

    protected override bool IsEnabledCore => base.IsEnabledCore && this.CanExecute;

    public AdvancedCommandMenuItem() {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        e.NameScope.GetTemplateChild("PART_InputGestureText", out this.InputGestureTextBlock);
        this.UpdateInputGestureText();
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        this.UpdateCanExecute();
        base.OnLoaded(e);
        this.UpdateInputGestureText();
    }

    private void UpdateInputGestureText() {
        if (this.InputGestureTextBlock == null) {
            return;
        }

        CommandContextEntry? entry = this.Entry;
        if (entry == null) {
            return;
        }

        if (CommandManager.Instance.GetCommandById(entry.CommandId) != null) {
            if (CommandIdToGestureConverter.CommandIdToGesture(entry.CommandId, null, out string value)) {
                this.InputGestureTextBlock.Text = value;
            }
        }
    }

    public override void UpdateCanExecute() {
        if (!this.IsLoaded)
            return;

        if (this.IsExecuting) {
            this.CanExecute = false;
        }
        else {
            IContextData? ctx = this.OwnerMenu?.CapturedContext;
            string? cmdId = this.Entry?.CommandId;
            Executability state = !string.IsNullOrWhiteSpace(cmdId) && ctx != null ? CommandManager.Instance.CanExecute(cmdId, ctx, true) : Executability.Invalid;
            this.CanExecute = state == Executability.Valid;
            this.IsVisible = state != Executability.Invalid;
        }
    }

    protected override void OnClick(RoutedEventArgs e) {
        if (this.IsExecuting) {
            this.CanExecute = false;
            return;
        }

        this.IsExecuting = true;
        string? cmdId = this.Entry?.CommandId;
        if (string.IsNullOrWhiteSpace(cmdId)) {
            base.OnClick(e);
            this.IsExecuting = false;
            this.UpdateCanExecute();
            return;
        }

        // disable execution while executing command
        this.CanExecute = false;
        base.OnClick(e);
        if (!this.DispatchCommand(cmdId)) {
            this.IsExecuting = false;
            this.CanExecute = true;
        }
    }

    private bool DispatchCommand(string cmdId) {
        IContextData? context = this.OwnerMenu?.CapturedContext;
        if (context == null) {
            return false;
        }

        Dispatcher.UIThread.Post(async void () => {
            try {
                await ExecuteCommandAndHandleError(cmdId, context);
            }
            catch {
                // ignored, should be handled above
            }
            finally {
                this.IsExecuting = false;
                this.UpdateCanExecute();
            }
        }, DispatcherPriority.Render);
        return true;
    }

    public static async ValueTask ExecuteCommandAndHandleError(string cmdId, IContextData context) {
        try {
            CommandManager.Instance.Execute(cmdId, context);
        }
        catch (Exception e) {
            if (!Debugger.IsAttached) {
                try {
                    await IMessageDialogService.Instance.ShowMessage(
                        "Error executing command",
                        "An unexpected error occurred while executing a command. " +
                        "FramePFX may or may not crash now, but you should probably restart WITHOUT saving just in case",
                        e.GetToString());
                }
                catch {
                    // ignored
                }
            }
        }
    }
}