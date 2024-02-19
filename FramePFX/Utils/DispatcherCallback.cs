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

namespace FramePFX.Utils {
    public class DispatcherCallback {
        private volatile bool isScheduled;
        private readonly Action action;
        private readonly Action actuallyInvoke;
        private readonly Dispatcher dispatcher;
        private volatile DispatcherOperation operation;

        public DispatcherOperation Operation => this.operation;

        public DispatcherCallback(Action action, Dispatcher dispatcher) {
            this.action = action;
            this.dispatcher = dispatcher;
            this.actuallyInvoke = this.DoInvokeAction;
        }

        public bool InvokeAsync() {
            if (this.isScheduled) {
                return false;
            }

            this.isScheduled = true;
            this.operation = this.dispatcher.InvokeAsync(this.actuallyInvoke);
            return true;
        }

        private void DoInvokeAction() {
            try {
                this.action();
            }
            finally {
                this.isScheduled = false;
            }
        }
    }
}