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

namespace FramePFX.Editing;

/// <summary>
/// A helper class for locking a piece of data that can be disposed on from any thread,
/// but allowing the render thread to dispose of the resource if another thread tried to dispose of it
/// </summary>
/// <typeparam name="T">The type of data</typeparam>
public class RenderLockedData<T> where T : IDisposable {
    private readonly object renderLock;
    private volatile bool isRendering;
    private volatile int disposeState;
    private volatile bool hasValue;
    private T value;

    public RenderLockedData() {
        this.renderLock = new object();
    }

    public void OnPrepareRender(T newValue) {
        if (this.isRendering)
            throw new InvalidOperationException("Cannot set data while rendering");
        this.disposeState = 0;
        this.value = newValue;
        this.hasValue = true;
    }

    public bool OnRenderBegin(out T theValue) {
        lock (this.renderLock) {
            if (!this.hasValue) {
                theValue = default;
                return false;
            }
            else {
                theValue = this.value;
                this.isRendering = true;
                return true;
            }
        }
    }

    public void OnRenderFinished() {
        if (!this.isRendering)
            throw new InvalidOperationException("Expected to be rendering");
        lock (this.renderLock) {
            if (this.disposeState == 1) {
                this.DisposeResource();
            }
            else {
                this.hasValue = false;
                this.value = default;
                this.isRendering = false;
            }
        }
    }

    /// <summary>
    /// Marks the value to be disposed if in use, or disposes of the resource right now if not in use
    /// </summary>
    public void Dispose() {
        lock (this.renderLock) {
            if (this.isRendering) {
                this.disposeState = 1;
            }
            else {
                this.DisposeResource();
            }
        }
    }

    private void DisposeResource() {
        this.disposeState = 2;
        if (this.hasValue) {
            this.hasValue = false;
            this.value.Dispose();
            this.value = default;
        }
    }
}