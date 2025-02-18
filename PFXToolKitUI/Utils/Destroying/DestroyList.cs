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

namespace PFXToolKitUI.Utils.Destroying;

public sealed class DestroyList : IDestroy {
    private object? list;

    public void Add(IDestroy destroy) {
        if (this.list == null) {
            this.list = destroy;
        }
        else if (this.list is IDestroy otherDestroy) {
            this.list = new List<IDestroy>() { otherDestroy, destroy };
        }
        else {
            ((List<IDestroy>) this.list).Add(destroy);
        }
    }

    public void Destroy() {
        switch (this.list) {
            case null:             return;
            case IDestroy destroy: destroy.Destroy(); break;
            case List<IDestroy> destroyList: {
                using ErrorList errorList = new ErrorList("Exception destroying one or more objects", true, false);
                foreach (IDestroy destroy in destroyList) {
                    try {
                        destroy.Destroy();
                    }
                    catch (Exception e) {
                        errorList.Add(e);
                    }
                }

                break;
            }
        }
    }
}