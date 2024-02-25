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

using FramePFX.Utils;

namespace FramePFX.Progression {
    public class ModalProgressTracker : IProgressTracker {
        private bool isIndeterminate;
        private double completionValue;
        private string headerText;
        private string descriptionText;

        public bool IsIndeterminate {
            get => this.isIndeterminate;
            set {
                if (this.isIndeterminate == value)
                    return;
                this.isIndeterminate = value;
                this.IsIndeterminateChanged?.Invoke(this);
            }
        }

        public double CompletionValue {
            get => this.completionValue;
            set {
                if (DoubleUtils.AreClose(this.completionValue, value))
                    return;
                this.completionValue = value;
                this.CompletionValueChanged?.Invoke(this);
            }
        }

        public string HeaderText {
            get => this.headerText;
            set {
                if (this.headerText == value)
                    return;
                this.headerText = value;
                this.HeaderTextChanged?.Invoke(this);
            }
        }

        public string Text {
            get => this.descriptionText;
            set {
                if (this.descriptionText == value)
                    return;
                this.descriptionText = value;
                this.TextChanged?.Invoke(this);
            }
        }

        public event ProgressTrackerEventHandler IsIndeterminateChanged;
        public event ProgressTrackerEventHandler CompletionValueChanged;
        public event ProgressTrackerEventHandler HeaderTextChanged;
        public event ProgressTrackerEventHandler TextChanged;

        public ModalProgressTracker() {
        }
    }
}