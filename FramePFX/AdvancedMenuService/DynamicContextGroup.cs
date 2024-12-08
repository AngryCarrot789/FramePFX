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

using FramePFX.Interactivity.Contexts;
using FramePFX.Utils;

namespace FramePFX.AdvancedMenuService;

public delegate void DynamicGenerateContextFunction(DynamicContextGroup group, IContextData ctx, List<IContextObject> items);

/// <summary>
/// A dynamic group. The docs for <see cref="IContextGroup"/> explain this better, but this class
/// contains a generator which generates the context objects based on the current state of the
/// application and also the <see cref="IContextData"/> provided to the generator
/// </summary>
public class DynamicContextGroup : IContextGroup
{
    private readonly DynamicGenerateContextFunction generate;

    public DynamicContextGroup(DynamicGenerateContextFunction generate)
    {
        Validate.NotNull(generate);
        this.generate = generate;
    }

    public List<IContextObject> GenerateItems(IContextData context)
    {
        List<IContextObject> list = new List<IContextObject>();
        this.generate(this, context, list);
        return list;
    }
}