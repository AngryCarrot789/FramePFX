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

namespace PFXToolKitUI.CommandSystem;

/// <summary>
/// An enum that represents the state of the context information relative to a command.
/// <para>
/// For example, given a "DeleteSelectedClipsCommand" that can delete either a contextual clip,
/// or selected clips from a contextual track or timeline:
/// </para>
/// <para>
/// If the context does not contain a clip nor a track or timeline, then <see cref="Invalid"/> will be used.
/// </para>
/// <para>
/// If there IS a track or timeline, but there are no clips selected, then <see cref="ValidButCannotExecute"/>
/// will be used. If there's a clip then this value would never be useful, due to the nature of the command
/// </para>
/// <para>
/// If there's just a clip, or, there's a track or timeline and there are selected clips, then
/// <see cref="Valid"/> will be used, meaning <see cref="Command.Execute"/> will most likely result in
/// work being done (unless there's an issue in the command implementation, or there's no overridden implementation,
/// or the state of the contextual data changes since <see cref="Command.CanExecuteCore"/> was called)
/// </para>
/// </summary>
public enum Executability {
    /// <summary>
    /// The context does not contain the relevant data for the command to execute.
    /// This might also be used if a targeted command does not exist
    /// </summary>
    Invalid,

    /// <summary>
    /// The context contains the correct information but the state of the context
    /// data is not appropriate for the command execution
    /// </summary>
    ValidButCannotExecute,

    /// <summary>
    /// The context contains the correct information and the command is fully executable.
    /// This is the default value returned by <see cref="Command.CanExecuteCore"/>, just in case
    /// I was too lazy to override the method
    /// </summary>
    Valid
}