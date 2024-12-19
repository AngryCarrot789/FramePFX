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

using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;
using FramePFX.AdvancedMenuService;
using FramePFX.Avalonia.Utils;
using FramePFX.Interactivity.Contexts;
using FramePFX.Utils;

namespace FramePFX.Avalonia.AdvancedMenuService;

public class AdvancedContextCallbackMenuItem : AdvancedContextMenuItem
{
    private TextBlock? InputGestureTextBlock;

    public new CallbackContextEntry? Entry => (CallbackContextEntry?) base.Entry;

    public AdvancedContextCallbackMenuItem() {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        e.NameScope.GetTemplateChild("PART_InputGestureText", out this.InputGestureTextBlock);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        this.UpdateCanExecute();
        base.OnLoaded(e);
    }

    protected override void OnClick(RoutedEventArgs e)
    {
        base.OnClick(e);
        this.DispatchCommand();
    }

    private bool DispatchCommand()
    {
        IContextData? context = this.Container?.Context;
        if (context == null)
        {
            return false;
        }

        CallbackContextEntry entry = this.Entry!;

        Dispatcher.UIThread.Post(() => this.ExecuteCommand(entry, context), DispatcherPriority.Render);
        return true;
    }

    private async void ExecuteCommand(CallbackContextEntry entry, IContextData context)
    {
        try
        {
            entry.Action?.Invoke(entry, context);
        }
        catch (Exception e)
        {
            if (!Debugger.IsAttached)
            {
                await IoC.MessageService.ShowMessage(
                    "Error",
                    "An unexpected error occurred while processing command. " +
                    "FramePFX may or may not crash now, but you should probably restart and save just in case",
                    e.GetToString());
            }
        }
        finally
        {
            this.UpdateCanExecute();
        }
    }
}