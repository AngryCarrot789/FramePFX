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
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;
using FramePFX.BaseFrontEnd.Interactivity;
using FramePFX.BaseFrontEnd.Shortcuts.Converters;
using FramePFX.BaseFrontEnd.Utils;
using FramePFX.CommandSystem;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.BaseFrontEnd.AdvancedMenuService;

public class CommandMenuItem : MenuItem {
    public static readonly StyledProperty<string?> CommandIdProperty = AvaloniaProperty.Register<CommandMenuItem, string?>("CommandId");

    private IContextData? loadedContextData;
    private bool canExecute;
    private bool generateChildren;
    private TextBlock? InputGestureTextBlock;

    protected override Type StyleKeyOverride => typeof(MenuItem);

    public bool IsExecuting { get; private set; }
    
    public string? CommandId {
        get => this.GetValue(CommandIdProperty);
        set => this.SetValue(CommandIdProperty, value);
    }

    protected bool CanExecute {
        get => this.canExecute;
        set {
            this.canExecute = value;

            // Causes IsEnableCore to be fetched, which returns false if we are executing something or
            // we have no valid command, causing this menu item to be "disabled"
            this.UpdateIsEffectivelyEnabled();
        }
    }

    protected override bool IsEnabledCore => base.IsEnabledCore && this.CanExecute;

    public CommandMenuItem() {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        e.NameScope.GetTemplateChild("PART_InputGestureText", out this.InputGestureTextBlock);
        this.UpdateInputGestureText();
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        IContextData? captured = ContextCapturingMenu.GetCapturedContext(this);
        this.loadedContextData = captured ?? DataManager.GetFullContextData(this);
        this.UpdateCanExecute();
        this.UpdateInputGestureText();
        if (this.generateChildren) {
            this.generateChildren = false;
            this.GenerateChildren();
        }
    }

    private void UpdateInputGestureText() {
        if (this.InputGestureTextBlock == null) {
            return;
        }

        if (!(this.CommandId is string id) || string.IsNullOrWhiteSpace(id)) {
            return;
        }

        if (CommandManager.Instance.GetCommandById(id) != null) {
            if (CommandIdToGestureConverter.CommandIdToGesture(id, null, out string value)) {
                this.InputGestureTextBlock.Text = value;
            }
        }
    }

    private void GenerateChildren() {
        string? cmdId = this.CommandId;
        if (string.IsNullOrWhiteSpace(cmdId))
            return;

        if (CommandManager.Instance.TryFindCommandById(cmdId, out Command? command) && command is CommandGroup group) {
            ItemCollection list = this.Items;
            list.Clear();
            foreach (string childCmdId in group.Commands) {
                list.Add(new CommandMenuItem() { CommandId = childCmdId });
            }
        }
    }
    
    protected override void OnUnloaded(RoutedEventArgs e) {
        base.OnUnloaded(e);
        this.loadedContextData = null;
    }

    private void UpdateCanExecute() {
        if (!this.IsLoaded) {
            return;
        }

        if (this.IsExecuting) {
            this.CanExecute = false;
        }
        else {
            IContextData? context = this.loadedContextData;
            string? cmdId = this.CommandId;
            Executability state = !string.IsNullOrWhiteSpace(cmdId) && context != null ? CommandManager.Instance.CanExecute(cmdId, context, true) : Executability.Invalid;
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
        string? cmdId = this.CommandId;
        IContextData? context = this.loadedContextData;
        if (string.IsNullOrWhiteSpace(cmdId) || context == null) {
            base.OnClick(e);
            this.IsExecuting = false;
            this.UpdateCanExecute();
            return;
        }

        // disable execution while executing command
        this.CanExecute = false;
        base.OnClick(e);
        Dispatcher.UIThread.Post(async void () => {
            try {
                await AdvancedCommandMenuItem.ExecuteCommandAndHandleError(cmdId, context);
            }
            catch {
                // ignored, should be handled above
            }
            finally {
                this.IsExecuting = false;
                this.UpdateCanExecute();
            }
        }, DispatcherPriority.Render);
    }
}