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
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using FramePFX.CommandSystem;
using FramePFX.CommandSystem.Usages;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Editors.Controls.Timelines.CommandContexts
{
    public class BasicButtonCommandUsage : CommandUsage
    {
        private bool isExecuting;

        public BasicButtonCommandUsage(string commandId) : base(commandId)
        {
        }

        protected override void OnConnected()
        {
            base.OnConnected();
            if (!(this.Control is ButtonBase))
                throw new InvalidOperationException("Cannot connect to non-button");
            ((ButtonBase) this.Control).Click += this.OnButtonClick;
        }

        protected override void OnDisconnected()
        {
            base.OnDisconnected();
            ((ButtonBase) this.Control).Click -= this.OnButtonClick;
        }

        protected virtual void OnButtonClick(object sender, RoutedEventArgs e)
        {
            if (!this.isExecuting)
            {
                this.DoExecuteAsync();
            }
        }

        private async void DoExecuteAsync()
        {
            this.isExecuting = true;
            this.UpdateCanExecute();
            try
            {
                await CommandManager.Instance.TryExecute(this.CommandId, () => DataManager.GetFullContextData(this.Control));
            }
            finally
            {
                this.isExecuting = true;
                this.UpdateCanExecute();
            }
        }

        protected override void UpdateCanExecute()
        {
            if (this.isExecuting)
            {
                ((ButtonBase) this.Control).IsEnabled = false;
            }
            else
            {
                base.UpdateCanExecute();
            }
        }

        protected override void OnUpdateForCanExecuteState(ExecutabilityState state)
        {
            ((ButtonBase) this.Control).IsEnabled = !this.isExecuting && state == ExecutabilityState.Executable;
        }
    }
}