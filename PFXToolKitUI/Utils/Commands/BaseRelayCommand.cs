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

namespace PFXToolKitUI.Utils.Commands;

/// <summary>
/// A base relay command class, that implements ICommand, and also has a simple implementation for dealing with
/// the <see cref="CanExecuteChanged"/> event handler (via <see cref="RaiseCanExecuteChanged"/>)
/// </summary>
public abstract class BaseRelayCommand : IRelayCommand {
    private bool isEnabled;

    public bool IsEnabled {
        get => this.isEnabled;
        set {
            this.isEnabled = value;
            this.RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    /// Raised when <see cref="RaiseCanExecuteChanged"/> is called
    /// </summary>
    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// Initializes a new instance of <see cref="BaseRelayCommand"/>
    /// </summary>
    /// <param name="canExecute">The execution status logic</param>
    protected BaseRelayCommand() {
        this.isEnabled = true;
    }

    public abstract void Execute(object? parameter);

    /// <summary>
    /// Determines whether this <see cref="BaseRelayCommand"/> can execute in its current state
    /// </summary>
    /// <param name="parameter">
    /// Data used by the command. This may be null, if the command does not require a parameter
    /// </param>
    /// <returns>
    /// True if the command can be executed, otherwise false if it cannot be executed
    /// </returns>
    public virtual bool CanExecute(object? parameter) {
        return this.IsEnabled;
    }

    /// <summary>
    /// Method used to raise the <see cref="CanExecuteChanged"/> event to indicate that the
    /// return value of the <see cref="CanExecute"/> method likely changed
    /// <para>
    /// This can be called from a view model, which, for example, may cause a
    /// button to become greyed out (disabled) if <see cref="CanExecute"/> returns false
    /// </para>
    /// </summary>
    public virtual void RaiseCanExecuteChanged() {
        if (this.CanExecuteChanged != null) {
            if (ApplicationPFX.Instance.Dispatcher.CheckAccess()) {
                this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
            else {
                ApplicationPFX.Instance.Dispatcher.Invoke(() => this.CanExecuteChanged?.Invoke(this, EventArgs.Empty));
            }
        }
    }

    /// <summary>
    /// A helper function for converting a command parameter to a generic type. This will return null if the parameter is null
    /// </summary>
    /// <param name="value">Input value/Command Parameter</param>
    /// <typeparam name="T">The type to convert the value to</typeparam>
    /// <returns>An object which is an instance of T</returns>
    /// <exception cref="Exception">The value is not null and could not be converted to T</exception>
    protected static object? GetConvertedParameter<T>(object? value) {
        switch (value) {
            case null:           return null;
            case T _:            return value;
            case IConvertible c: return c.ToType(typeof(T), null);
            default:             throw new Exception($"Parameter (of type {value.GetType()}) could not be converted to {typeof(T)}");
        }
    }
}