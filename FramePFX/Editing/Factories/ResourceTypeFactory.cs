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

using FramePFX.Editing.ResourceManaging;
using FramePFX.Editing.ResourceManaging.Resources;

namespace FramePFX.Editing.Factories;

public class ResourceTypeFactory : ReflectiveObjectFactory<BaseResource> {
    public static ResourceTypeFactory Instance { get; } = new ResourceTypeFactory();

    private ResourceTypeFactory() {
        this.RegisterType("r_group", typeof(ResourceFolder));
        this.RegisterType("r_argb", typeof(ResourceColour));
        this.RegisterType("r_img", typeof(ResourceImage));
        this.RegisterType("r_comp", typeof(ResourceComposition));
    }

    public BaseResource NewResource(string id) {
        return base.NewInstance(id);
    }

    public new void RegisterType(string id, Type type) => base.RegisterType(id, type);
}