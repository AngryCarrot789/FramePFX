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

using System;

namespace FramePFX.Shortcuts.WPF
{
    public class ActivationHandlerReference
    {
        private readonly WeakReference<ShortcutActivateHandler> weakReference;
        private readonly ShortcutActivateHandler strongReference;

        public ShortcutActivateHandler Value
        {
            get
            {
                if (this.weakReference != null)
                {
                    return this.weakReference.TryGetTarget(out ShortcutActivateHandler target) ? target : null;
                }
                else
                {
                    return this.strongReference;
                }
            }
        }

        public bool IsWeak => this.weakReference != null;

        public bool IsStrong => this.weakReference == null;

        public ActivationHandlerReference(ShortcutActivateHandler handler, bool weak)
        {
            if (weak)
            {
                this.weakReference = new WeakReference<ShortcutActivateHandler>(handler);
            }
            else
            {
                this.strongReference = handler;
            }
        }
    }
}