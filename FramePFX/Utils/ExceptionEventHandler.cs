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

namespace FramePFX.Utils;

/// <summary>
/// An event raised when an exception is caught
/// <param name="sender">The object that raised this event</param>
/// <param name="e">The args</param>
/// </summary>
public delegate void ExceptionEventHandler(object sender, ExceptionEventArgs e);

/// <summary>
/// A class which stores event args for a <see cref="ExceptionEventHandler"/>
/// </summary>
public class ExceptionEventArgs : EventArgs
{
    /// <summary>
    /// The exception that was caught. This will not be null
    /// </summary>
    public Exception Exception { get; }

    public ExceptionEventArgs(Exception exception)
    {
        this.Exception = exception ?? throw new ArgumentNullException(nameof(exception));
    }
}