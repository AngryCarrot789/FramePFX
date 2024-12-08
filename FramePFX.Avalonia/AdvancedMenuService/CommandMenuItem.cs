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

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FramePFX.Avalonia.Interactivity;
using FramePFX.CommandSystem;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Avalonia.AdvancedMenuService;

public class CommandMenuItem : MenuItem
{
    public static readonly StyledProperty<string?> CommandIdProperty = AvaloniaProperty.Register<CommandMenuItem, string?>("CommandId");

    private IContextData? loadedContextData;
    private bool canExecute;
    private bool generateItemsOnLoad;

    protected override Type StyleKeyOverride => typeof(MenuItem);

    public string? CommandId
    {
        get => this.GetValue(CommandIdProperty);
        set => this.SetValue(CommandIdProperty, value);
    }

    protected bool CanExecute
    {
        get => this.canExecute;
        set
        {
            this.canExecute = value;

            // Causes IsEnableCore to be fetched, which returns false if we are executing something or
            // we have no valid command, causing this menu item to be "disabled"
            this.UpdateIsEffectivelyEnabled();
        }
    }

    protected override bool IsEnabledCore => base.IsEnabledCore && this.CanExecute;

    public CommandMenuItem() {
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        IContextData? captured = ContextCapturingMenu.GetCapturedContext(this);
        this.loadedContextData = captured ?? DataManager.GetFullContextData(this);
        string? id = this.CommandId;
        if (string.IsNullOrWhiteSpace(id))
            id = null;

        Executability state = id != null ? CommandManager.Instance.CanExecute(id, this.loadedContextData) : Executability.Invalid;
        this.CanExecute = state == Executability.Valid;
        // if (this.CanExecute) {
        //     if (CommandIdToGestureConverter.CommandIdToGesture(id, null, out string value)) {
        //         /// this.InputGesture
        //         // this.SetCurrentValue(, value);
        //     }
        // }

        if (this.generateItemsOnLoad)
        {
            this.generateItemsOnLoad = false;
            this.GenerateChildren();
        }
    }

    private void GenerateChildren()
    {
        string? cmdId = this.CommandId;
        if (string.IsNullOrWhiteSpace(cmdId))
            return;

        if (!CommandManager.Instance.TryGetCommandById(cmdId, out Command? command) || !(command is CommandGroup group))
            return;

        ItemCollection list = this.Items;
        list.Clear();
        foreach (string childCmdId in group.Commands)
        {
            list.Add(new CommandMenuItem() { CommandId = childCmdId });
        }
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        this.loadedContextData = null;
    }

    protected override void OnClick(RoutedEventArgs e)
    {
        if (this.loadedContextData != null && this.CommandId is string commandId)
        {
            CommandManager.Instance.Execute(commandId, this.loadedContextData);
        }
        else
        {
            base.OnClick(e);
        }
    }
}