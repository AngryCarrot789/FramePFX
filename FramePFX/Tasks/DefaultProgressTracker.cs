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
using System.Collections.Generic;
using System.Diagnostics;
using FramePFX.Utils;

namespace FramePFX.Tasks
{
    public class DefaultProgressTracker : IActivityProgress
    {
        private bool isIndeterminate;
        private double completionValue;
        private string headerText;
        private string descriptionText;

        public bool IsIndeterminate {
            get => this.isIndeterminate;
            set
            {
                if (this.isIndeterminate == value)
                    return;
                this.isIndeterminate = value;
                this.IsIndeterminateChanged?.Invoke(this);
            }
        }

        public double TotalCompletion {
            get => this.completionValue;
            set
            {
                if (DoubleUtils.AreClose(this.completionValue, value))
                    return;
                this.completionValue = value;
                this.CompletionValueChanged?.Invoke(this);
            }
        }

        public string HeaderText {
            get => this.headerText;
            set
            {
                if (this.headerText == value)
                    return;
                this.headerText = value;
                this.HeaderTextChanged?.Invoke(this);
            }
        }

        public string Text {
            get => this.descriptionText;
            set
            {
                if (this.descriptionText == value)
                    return;
                this.descriptionText = value;
                this.TextChanged?.Invoke(this);
            }
        }

        public event ActivityProgressEventHandler IsIndeterminateChanged;
        public event ActivityProgressEventHandler CompletionValueChanged;
        public event ActivityProgressEventHandler HeaderTextChanged;
        public event ActivityProgressEventHandler TextChanged;

        private readonly Stack<CompletionRange> ranges = new Stack<CompletionRange>();
        private double totalMultiplier;

        public DefaultProgressTracker()
        {
            this.totalMultiplier = 1.0;
        }

        public PopDispose PushCompletionRange(double min, double max)
        {
            CompletionRange range = new CompletionRange(max - min, this.totalMultiplier);
            this.totalMultiplier *= range.Range;
            this.ranges.Push(range);
            return new PopDispose(this);
        }

        public void PopCompletionRange()
        {
            if (this.ranges.Count < 1)
                throw new InvalidOperationException("Too many completion ranges popped: the stack is empty!");
            CompletionRange popped = this.ranges.Pop();
            this.totalMultiplier = popped.PreviousMultiplier;
        }

        public void OnProgress(double value)
        {
            if (this.ranges.Count > 0)
            {
                this.TotalCompletion += this.totalMultiplier * value;
            }
            else
            {
                // assert totalMultiplier == 1.0
                this.TotalCompletion += value;
            }
        }

        public static void TestCompletionRangeFunctionality()
        {
            // Begin: CloseActiveAndOpenProject

            DefaultProgressTracker tracker = new DefaultProgressTracker();
            using (tracker.PushCompletionRange(0.0, 0.5))
            {
                // Begin: CloseActive
                // parent range = 0.5, so 0.5 * 0.25 = 0.125.
                // TotalCompletion = 0.0 + 0.125
                tracker.OnProgress(0.25);
                // parent range = 0.5, so 0.5 * 0.75 = 0.375
                // TotalCompletion = 0.125 + 0.375 = 0.5
                tracker.OnProgress(0.75);
                // assert tracker.TotalCompletion == 0.5
                // End: CloseActive
            }

            using (tracker.PushCompletionRange(0.5, 1.0))
            {
                // Begin: OpenProject

                using (tracker.PushCompletionRange(0.0, 0.25))
                {
                    // Begin: PreLoad

                    using (tracker.PushCompletionRange(0.0, 0.1))
                    {
                        // Begin: ProcessPreLoad
                        tracker.OnProgress(0.5);
                        tracker.OnProgress(0.5);
                        // End: ProcessPreLoad
                    }

                    tracker.OnProgress(0.4);
                    tracker.OnProgress(0.5);
                    // End: PreLoad
                }

                using (tracker.PushCompletionRange(0.25, 0.5))
                {
                    // Begin: PostLoad
                    tracker.OnProgress(0.2);
                    tracker.OnProgress(0.8);
                    // End: PostLoad
                }

                using (tracker.PushCompletionRange(0.5, 1.0))
                {
                    // Begin: PostLoad
                    tracker.OnProgress(0.3);
                    tracker.OnProgress(0.6);
                    tracker.OnProgress(0.1);
                    // End: PostLoad
                }

                // End: OpenProject
            }

            if (!DoubleUtils.AreClose(tracker.TotalCompletion, 1.0))
            {
                Debugger.Break(); // test failed
                throw new Exception("Test failed. Completion ranges do not function as expected");
            }
        }
    }
}