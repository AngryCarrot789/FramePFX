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

namespace FramePFX.Progression {
    public delegate void ProgressTrackerEventHandler(IProgressTracker tracker);

    /// <summary>
    /// An interface for an object used to track progression
    /// </summary>
    public interface IProgressTracker {
        /// <summary>
        /// Gets or sets if this tracker's completions state is indeterminate
        /// </summary>
        bool IsIndeterminate { get; set; }

        /// <summary>
        /// Gets or sets a value between 0.0 and 1.0 that represents how far completed the operation is
        /// </summary>
        double CompletionValue { get; set; }

        /// <summary>
        /// Gets or sets the header text (aka operation caption)
        /// </summary>
        string HeaderText { get; set; }

        /// <summary>
        /// Gets or sets the description text (aka operation description, describing what's happening)
        /// </summary>
        string Text { get; set; }

        event ProgressTrackerEventHandler IsIndeterminateChanged;
        event ProgressTrackerEventHandler CompletionValueChanged;
        event ProgressTrackerEventHandler HeaderTextChanged;
        event ProgressTrackerEventHandler TextChanged;
    }
}