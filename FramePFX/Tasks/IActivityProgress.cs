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

namespace FramePFX.Tasks {
    public delegate void ActivityProgressEventHandler(IActivityProgress tracker);

    /// <summary>
    /// An interface for an object used to track progression
    /// </summary>
    public interface IActivityProgress {
        /// <summary>
        /// Gets or sets if this tracker's completions state is indeterminate
        /// </summary>
        bool IsIndeterminate { get; set; }

        /// <summary>
        /// Gets or sets a value between 0.0 and 1.0 that represents how far completed the entire operation has completed
        /// </summary>
        double TotalCompletion { get; set; }

        /// <summary>
        /// Gets or sets the header text (aka operation caption)
        /// </summary>
        string HeaderText { get; set; }

        /// <summary>
        /// Gets or sets the description text (aka operation description, describing what's happening)
        /// </summary>
        string Text { get; set; }

        event ActivityProgressEventHandler IsIndeterminateChanged;
        event ActivityProgressEventHandler CompletionValueChanged;
        event ActivityProgressEventHandler HeaderTextChanged;
        event ActivityProgressEventHandler TextChanged;

        /// <summary>
        /// Pushes a new completion range. The difference between min and max is the completion
        /// value that will be added to the parent range's completion value.
        /// <para>
        /// A completion range should be pushed when you're about to begin an 'operation phase', that is,
        /// something that can have a completion percentage. The operation itself can push its own completion
        /// ranges, but it is your job to push a range which represents how much actual work the operation
        /// does relative to the current operation
        /// </para>
        /// <para>
        /// The reason for a min and max is so that you can identify possible mis-uses and bugs.
        /// In a code block, the total amount of range pushed should equal 1.0. For example, you
        /// push 0.0->0.2, 0.2->0.7, 0.7->1.0, where the differences between max and min for those
        /// cases sum to 1.0. If they don't, then it probably means this method was used incorrectly
        /// </para>
        /// </summary>
        /// <param name="min">The minimum bound</param>
        /// <param name="max">The maximum bound</param>
        /// <returns>
        /// A disposable struct that, when <see cref="PopDispose.Dispose"/> is called, calls <see cref="PopCompletionRange"/>.
        /// This struct can be used in a using statement, where the 'operation' is inside the using block, for convenience
        /// and clean code sakes. The struct does not need to be used; <see cref="PopCompletionRange"/> can be called manually
        /// </returns>
        PopDispose PushCompletionRange(double min, double max);

        /// <summary>
        /// Pops the completion range on the top of the stack
        /// </summary>
        void PopCompletionRange();

        /// <summary>
        /// Adds the given value to <see cref="TotalCompletion"/>. The final added amount depends on the
        /// completion ranges currently on the stack. If there are none, then this method is the same as
        /// adding the value to <see cref="TotalCompletion"/> directly
        /// </summary>
        /// <param name="value">The value to append (multiplied based on the current ranges on the stack)</param>
        void OnProgress(double value);
    }

    /// <summary>
    /// A struct used to automatically pop a completion range from a tracker, to make the code easier to
    /// #read. This can only pop once, then calling Dispose again does nothing
    /// </summary>
    public struct PopDispose : IDisposable {
        private IActivityProgress tracker;

        public PopDispose(IActivityProgress tracker) {
            this.tracker = tracker;
        }

        public void Dispose() {
            IActivityProgress t = this.tracker;
            this.tracker = null;
            t?.PopCompletionRange();
        }
    }
}