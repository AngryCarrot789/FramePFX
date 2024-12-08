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

using Avalonia.Controls.Primitives;
using FramePFX.Editing.Timelines;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Avalonia.Editing.CommandContexts;

public class ToggleLoopCommandUsage : BasicButtonCommandUsage
{
    private Timeline? myTimeline;

    public ToggleLoopCommandUsage() : base("commands.editor.ToggleLoopTimelineRegion") {
    }

    protected override void OnContextChanged()
    {
        base.OnContextChanged();

        if (this.myTimeline != null)
        {
            this.myTimeline.IsLoopRegionEnabledChanged -= this.OnIsLoopRegionEnabledChanged;
            this.myTimeline = null;
        }

        if (this.GetContextData() is IContextData ctx && DataKeys.TimelineKey.TryGetContext(ctx, out this.myTimeline))
        {
            this.myTimeline.IsLoopRegionEnabledChanged += this.OnIsLoopRegionEnabledChanged;
            this.UpdateIsChecked();
        }
    }

    private void OnIsLoopRegionEnabledChanged(Timeline timeline)
    {
        this.UpdateIsChecked();
    }

    private void UpdateIsChecked()
    {
        if (this.Control is ToggleButton toggleButton)
        {
            toggleButton.IsChecked = this.myTimeline?.IsLoopRegionEnabled ?? false;
        }
    }
}