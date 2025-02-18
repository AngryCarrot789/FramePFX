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

namespace PFXToolKitUI.Avalonia.Interactivity.Contexts;

public abstract class MultiChangeToken : IDisposable {
    public readonly IControlContextData Context;
    private bool disposed;

    public MultiChangeToken(IControlContextData context) {
        this.Context = context;
    }

    /// <summary>
    /// Disposes this token
    /// </summary>
    public void Dispose() {
        if (this.disposed)
            throw new ObjectDisposedException("Already disposed");

        this.disposed = true;
        this.OnDisposed();
    }

    protected abstract void OnDisposed();
}