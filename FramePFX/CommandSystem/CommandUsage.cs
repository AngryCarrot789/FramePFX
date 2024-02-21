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

namespace FramePFX.CommandSystem {
    public delegate void CommandPresentationEventHandler(CommandUsage usage);

    /// <summary>
    /// Represents the state of a command being used in a specific place in the application's UI.
    /// This class manages visibility, display text, etc.
    /// </summary>
    public class CommandUsage {
        private string displayName;
        private string description;
        private bool isVisible;
        private bool isEnabled;

        /// <summary>
        /// Gets or sets this command's main display text, e.g. the primary name of the command
        /// </summary>
        public string DisplayName {
            get => this.displayName;
            set {
                if (this.displayName == value)
                    return;
                this.displayName = value;
                this.DisplayNameChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Gets or sets the command's description
        /// </summary>
        public string Description {
            get => this.description;
            set {
                if (this.description == value)
                    return;
                this.description = value;
                this.ToolTipChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Gets or sets if this command is currently visible in the current context
        /// </summary>
        public bool IsVisible {
            get => this.isVisible;
            set {
                if (this.isVisible == value)
                    return;
                this.isVisible = value;
                this.IsVisibleChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Gets or sets if this command can actually be executed based on the current context
        /// </summary>
        public bool IsEnabled {
            get => this.isEnabled;
            set {
                if (this.isEnabled == value)
                    return;
                this.isEnabled = value;
                this.IsEnabledChanged?.Invoke(this);
            }
        }

        public event CommandPresentationEventHandler DisplayNameChanged;
        public event CommandPresentationEventHandler ToolTipChanged;
        public event CommandPresentationEventHandler IsVisibleChanged;
        public event CommandPresentationEventHandler IsEnabledChanged;

        public CommandUsage() {

        }

        public CommandUsage(string displayName, bool isEnabled) {
            this.displayName = displayName;
            this.isEnabled = isEnabled;
        }

        public CommandUsage(string displayName, string description, bool isVisible, bool isEnabled) {
            this.displayName = displayName;
            this.description = description;
            this.isVisible = isVisible;
            this.isEnabled = isEnabled;
        }

        public void SetEnabledAndVisible(bool both) {
            this.IsVisible = both;
            this.IsEnabled = both;
        }

        public void SetEnabledAndVisible(bool isEnabled, bool isVisible) {
            this.IsVisible = isVisible;
            this.IsEnabled = isEnabled;
        }
    }
}