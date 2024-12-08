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
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

namespace FramePFX.History;

/// <summary>
/// An order-safe implementation of <see cref="IHistoryAction"/>, that throws an exception if undo or redo were called in the wrong orders
/// </summary>
public abstract class HistoryAction : IHistoryAction
{
    private int state; // 0 = default, 1 = last was undo, 2 = last was redo

    protected HistoryAction() {
    }

    public bool Undo()
    {
        if (this.state == 1)
            throw new InvalidOperationException("Undo cannot be called sequentially more than once. Redo must be called before calling Undo again");
        if (!this.OnUndo())
            return false;
        this.state = 1;
        return true;
    }

    public bool Redo()
    {
        if (this.state == 0)
            throw new InvalidOperationException("Undo has not been called yet, therefore, redo cannot be called");
        if (this.state == 2)
            throw new InvalidOperationException("Redo cannot be called sequentially more than once. Undo must be called before calling Redo again");
        if (!this.OnRedo())
            return false;

        this.state = 2;
        return true;
    }

    /// <summary>
    /// Undoes this action
    /// </summary>
    /// <returns>See <see cref="IHistoryAction.Undo"/></returns>
    protected abstract bool OnUndo();

    /// <summary>
    /// Undoes this action
    /// </summary>
    /// <returns>See <see cref="IHistoryAction.Redo"/></returns>
    protected abstract bool OnRedo();
}