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

using FramePFX.CommandSystem;
using FramePFX.Interactivity.Contexts;
using FramePFX.Utils;

namespace FramePFX.Avalonia.CommandSystem.Usages;

public class EmptyCommandUsage : ICommandUsage {
    public string CommandId { get; }

    public EmptyCommandUsage(string commandId) {
        Validate.NotNullOrWhiteSpaces(commandId);
        this.CommandId = commandId;
    }

    public IContextData ContextData => EmptyContext.Instance;
    
    public void UpdateCanExecuteLater() {
    }

    public void UpdateCanExecute() {
    }
}