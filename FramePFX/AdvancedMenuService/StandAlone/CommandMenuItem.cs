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

using System.Windows;
using System.Windows.Controls;
using FramePFX.CommandSystem;
using FramePFX.Interactivity.Contexts;
using FramePFX.Shortcuts.WPF.Converters;

namespace FramePFX.AdvancedMenuService.StandAlone
{
    /// <summary>
    /// A stand-alone menu item that represents a <see cref="Command"/> in the command system (not an <see cref="System.Windows.Input.ICommand"/>)
    /// </summary>
    public class CommandMenuItem : MenuItem
    {
        public static readonly DependencyProperty CommandIdProperty = DependencyProperty.Register("CommandId", typeof(string), typeof(CommandMenuItem), new PropertyMetadata(null, (d, e) => ((CommandMenuItem) d).OnCommandIdChanged()));

        /// <summary>
        /// Gets or sets the command ID that this menu item represents
        /// </summary>
        public string CommandId
        {
            get => (string) this.GetValue(CommandIdProperty);
            set => this.SetValue(CommandIdProperty, value);
        }

        protected bool CanExecute
        {
            get => this.canExecute;
            set
            {
                this.canExecute = value;

                // Causes IsEnableCore to be fetched, which returns false if we are executing something or
                // we have no valid command, causing this menu item to be "disabled"
                this.CoerceValue(IsEnabledProperty);
            }
        }

        protected override bool IsEnabledCore => base.IsEnabledCore && this.CanExecute;

        private IContextData loadedContextData;
        private bool canExecute;
        private bool generateItemsOnLoad;

        public CommandMenuItem()
        {
            this.Click += this.OnClick;
            this.Loaded += this.OnLoaded;
            this.Unloaded += this.OnUnloaded;
        }

        private void OnCommandIdChanged()
        {
            if (this.IsLoaded)
            {
                this.generateItemsOnLoad = false;
                this.GenerateChildren();
            }
            else
            {
                this.generateItemsOnLoad = true;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.loadedContextData = ContextCapturingMenu.GetCapturedContextData(this) ?? DataManager.GetFullContextData(this);
            string id = this.CommandId;
            if (string.IsNullOrWhiteSpace(id))
                id = null;

            Executability state = id != null ? CommandManager.Instance.CanExecute(id, this.loadedContextData) : Executability.Invalid;
            this.CanExecute = state == Executability.Valid;
            if (this.CanExecute)
            {
                if (CommandIdToGestureConverter.CommandIdToGesture(id, null, out string value))
                {
                    this.SetCurrentValue(InputGestureTextProperty, value);
                }
            }

            if (this.generateItemsOnLoad)
            {
                this.generateItemsOnLoad = false;
                this.GenerateChildren();
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.loadedContextData = null;
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            if (this.loadedContextData != null && this.CommandId is string commandId)
            {
                CommandManager.Instance.Execute(commandId, this.loadedContextData);
            }
        }

        private void GenerateChildren()
        {
            string cmdId = this.CommandId;
            if (string.IsNullOrWhiteSpace(cmdId))
                return;

            if (!CommandManager.Instance.TryGetCommandById(cmdId, out Command command) || !(command is CommandGroup group))
                return;

            ItemCollection list = this.Items;
            list.Clear();
            foreach (string childCmdId in group.Commands)
            {
                list.Add(new CommandMenuItem() {CommandId = childCmdId});
            }
        }
    }
}