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

namespace FramePFX.CommandSystem;

/// <summary>
/// A command group that overrides <see cref="Command.CanExecuteCore"/> to return an executability state based on the available context
/// </summary>
public class SimpleCommandGroup : CommandGroup {
    private readonly HashSet<string> required;
    private readonly HashSet<string> any;

    private SimpleCommandGroup(HashSet<string> required, HashSet<string> any) {
        this.required = required;
        this.any = any;
    }

    public static SimpleCommandGroup RequireAll(HashSet<string> keys) => new SimpleCommandGroup(keys, null);
    public static SimpleCommandGroup RequireAny(HashSet<string> keys) => new SimpleCommandGroup(null, keys);
    public static SimpleCommandGroup RequireAllAndAny(HashSet<string> allOf, HashSet<string> anyOf) => new SimpleCommandGroup(allOf, anyOf);

    protected override Executability CanExecuteCore(CommandEventArgs e) {
        if (this.required == null && this.any == null)
            return base.CanExecuteCore(e);

        bool isValid;
        if (this.any == null)
            isValid = this.HasAllKeys(e);
        else if (this.required == null)
            isValid = this.HasAnyKey(e);
        else
            isValid = this.HasAllKeys(e) && this.HasAnyKey(e);

        return isValid ? Executability.Valid : Executability.Invalid;
    }

    private bool HasAllKeys(CommandEventArgs e) => this.required.All(key => e.ContextData.ContainsKey(key));

    private bool HasAnyKey(CommandEventArgs e) => this.any.Any(key => e.ContextData.ContainsKey(key));
}