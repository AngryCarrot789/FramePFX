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

using Avalonia;

namespace PFXToolKitUI.Avalonia.Interactivity.Contexts;

/// <summary>
/// Context data for a control that automatically invalidates the control's inherited context data when modifying this instance
/// </summary>
public sealed class ControlContextData : BaseControlContextData {
    public ControlContextData(AvaloniaObject owner) : base(owner) {
    }

    public ControlContextData(AvaloniaObject owner, InheritingControlContextData? copyFromNonInherited) : this(owner) {
        this.CopyFrom(copyFromNonInherited?.NonInheritedEntries);
    }

    public override MultiChangeToken BeginChange() => new MultiChangeTokenImpl(this);

    private class MultiChangeTokenImpl : MultiChangeToken {
        public MultiChangeTokenImpl(ControlContextData context) : base(context) {
            context.batchCounter++;
        }

        protected override void OnDisposed() {
            ((ControlContextData) this.Context).OnMultiChangeTokenDisposed();
        }
    }
}