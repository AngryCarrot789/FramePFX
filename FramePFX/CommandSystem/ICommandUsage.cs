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

using FramePFX.Interactivity.Contexts;

namespace FramePFX.CommandSystem;

/// <summary>
/// An object that is associated with, typically, a single UI control, and manages specific behaviours in relation to a command
/// </summary>
public interface ICommandUsage {
    /// <summary>
    /// Gets the target command ID for this usage instance. This is not null, not empty and
    /// does not consist of only whitespaces; it's a fully valid command ID
    /// </summary>
    string CommandId { get; }

    /// <summary>
    /// Gets the context data that is current available. May be empty, but will not be null
    /// </summary>
    IContextData ContextData { get; }

    /// <summary>
    /// Triggers an update on this usage. This may cause a button (that executes the command) to
    /// become enabled or disabled based on the available information in our <see cref="ContextData"/>
    /// </summary>
    void UpdateCanExecuteLater();

    /// <summary>
    /// Updates the UI component based on whether the command can currently be executed
    /// </summary>
    void UpdateCanExecute();
}