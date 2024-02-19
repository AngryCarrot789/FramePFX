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
using System.Windows.Threading;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.AdvancedContextService.WPF {
    public class AdvancedContextEventMenuItem : AdvancedContextMenuItem {
        public new EventContextEntry Entry => (EventContextEntry) base.Entry;

        public AdvancedContextEventMenuItem() {

        }

        protected override void OnClick() {
            EventContextEntry entry = this.Entry;
            DataContext context = this.Menu.ContextOnMenuOpen;
            if (entry != null && context != null) {
                this.Dispatcher.BeginInvoke((Action) (() => entry.Action?.Invoke(context)), DispatcherPriority.Render);
            }

            base.OnClick();
        }
    }
}