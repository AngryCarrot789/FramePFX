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

namespace FramePFX.Utils.Commands;

/// <summary>
/// A simple relay command, which does not take any parameters
/// </summary>
/// <typeparam name="T">The type of parameter</typeparam>
public class RelayCommand : BaseRelayCommand
{
    private readonly Action execute;
    private readonly Func<bool>? canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        if (execute == null)
        {
            throw new ArgumentNullException(nameof(execute), "Execute callback cannot be null");
        }

        this.execute = execute;
        this.canExecute = canExecute;
    }

    public override bool CanExecute(object? parameter) => base.CanExecute(parameter) && (this.canExecute == null || this.canExecute());

    public override void Execute(object? parameter) => this.execute();
}

/// <summary>
/// A simple relay command, which may take a parameter
/// </summary>
/// <typeparam name="T">The type of parameter</typeparam>
public class RelayCommand<T> : BaseRelayCommand
{
    private readonly Action<T?> execute;
    private readonly Func<T?, bool>? canExecute;

    public bool ConvertParameter { get; set; }

    public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null, bool convertParameter = true)
    {
        if (execute == null)
        {
            throw new ArgumentNullException(nameof(execute), "Execute callback cannot be null");
        }

        this.execute = execute;
        this.canExecute = canExecute;
        this.ConvertParameter = convertParameter;
    }

    public override bool CanExecute(object? parameter)
    {
        if (this.ConvertParameter)
        {
            parameter = GetConvertedParameter<T>(parameter);
        }

        if (base.CanExecute(parameter))
        {
            return this.canExecute == null || parameter == null && this.canExecute(default) || parameter is T t && this.canExecute(t);
        }

        return false;
    }

    public override void Execute(object? parameter)
    {
        if (this.ConvertParameter)
        {
            parameter = GetConvertedParameter<T>(parameter);
        }

        if (parameter == null)
        {
            this.execute(default);
        }
        else if (parameter is T value)
        {
            this.execute(value);
        }
        else
        {
            throw new InvalidCastException($"Parameter type ({parameter.GetType()}) cannot be used for the callback method (which requires type {typeof(T).Name})");
        }
    }
}