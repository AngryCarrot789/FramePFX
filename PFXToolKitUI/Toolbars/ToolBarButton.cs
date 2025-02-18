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

using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Icons;
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Services.Messaging;
using PFXToolKitUI.Utils;
using PFXToolKitUI.Utils.Commands;
using PFXToolKitUI.Utils.RDA;

namespace PFXToolKitUI.Toolbars;

/// <summary>
/// The base class for a button that exists in a toolbar
/// </summary>
public abstract class ToolBarButton {
    /// <summary>
    /// Gets the button's UI implementation used by this toolbar button
    /// </summary>
    public IButtonElement Button { get; }

    /// <summary>
    /// Gets or sets the icon displayed in the button
    /// </summary>
    public Icon? Icon {
        get => this.Button.Icon;
        set => this.Button.Icon = value;
    }

    /// <summary>
    /// Returns whether to hide this button when the <see cref="CanExecute"/> method returns <see cref="Executability.Invalid"/>
    /// </summary>
    public virtual bool CanHideButton => true;

    /// <summary>
    /// Gets whether the button is currently running a task
    /// </summary>
    public bool IsExecuting { get; private set; }

    /// <summary>
    /// Gets out button's current context data
    /// </summary>
    public IContextData ContextData => this.Button.ContextData;

    private RapidDispatchAction? delayedContextUpdate;

    protected ToolBarButton(IButtonElement button) {
        this.Button = button ?? throw new ArgumentNullException(nameof(button));
        this.Button.ContextInvalidated += this.OnContextInvalidated;
        this.Button.Command = new AsyncRelayCommand(this.OnClickedAsyncImpl);
    }

    /// <summary>
    /// Invoked on load priority. This method should update the visuals of the button
    /// </summary>
    protected virtual void OnUpdateCanExecute() {
        Executability state = this.CanExecute();
        if (this.CanHideButton) {
            this.Button.IsVisible = state != Executability.Invalid;
        }

        this.Button.IsEnabled = state == Executability.Valid;
    }

    public virtual Executability CanExecute() {
        return Executability.Valid;
    }

    protected abstract Task OnClickedAsync();

    private async Task OnClickedAsyncImpl() {
        this.IsExecuting = true;
        this.UpdateCanExecuteLater();

        try {
            await this.OnClickedAsync();
        }
        catch (Exception e) {
            await IMessageDialogService.Instance.ShowMessage("Error", "An exception occurred while executing toolbar command", e.GetToString());
        }
        finally {
            this.IsExecuting = false;
        }

        this.UpdateCanExecuteLater();
    }

    /// <summary>
    /// Invoked when our context data changes, or we are just about to execute the command, or we have finished executing the command.
    /// <para>
    /// This method schedules <see cref="OnUpdateCanExecute"/> to be invoked later on
    /// </para>
    /// </summary>
    public virtual void UpdateCanExecuteLater() {
        RapidDispatchAction guard = this.delayedContextUpdate ??= new RapidDispatchAction(this.OnUpdateCanExecute, DispatchPriority.Loaded, "BaseIconToolBarButton::UpdateCanExecuteLater");
        guard.InvokeAsync();
    }

    /// <summary>
    /// Invoked when our inherited context data changes, either by being added to or removed from the visual tree, or our button being attached or detached
    /// </summary>
    protected virtual void OnContextChanged() {
        this.UpdateCanExecuteLater();
    }

    private void OnContextInvalidated(IButtonElement button) {
        this.OnContextChanged();
    }
}