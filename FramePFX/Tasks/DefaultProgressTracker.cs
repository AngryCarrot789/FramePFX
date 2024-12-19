// 
// Copyright (c) 2024-2024 REghZy
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

using FramePFX.Utils.RDA;

namespace FramePFX.Tasks;

public class DefaultProgressTracker : IActivityProgress
{
    private readonly object dataLock = new object(); // only really used as a memory barrier
    private bool isIndeterminate;
    private double completionValue;
    private string? headerText;
    private string? descriptionText;

    public bool IsIndeterminate
    {
        get => this.isIndeterminate;
        set
        {
            lock (this.dataLock)
            {
                if (this.isIndeterminate == value)
                    return;
                this.isIndeterminate = value;
            }

            this.updateIsIndeterminate?.InvokeAsync();
        }
    }

    public string? Caption
    {
        get => this.headerText;
        set
        {
            lock (this.dataLock)
            {
                if (this.headerText == value)
                    return;
                this.headerText = value;
            }

            this.updateHeaderText?.InvokeAsync();
        }
    }

    public string? CurrentAction
    {
        get => this.descriptionText;
        set
        {
            lock (this.dataLock)
            {
                if (this.descriptionText == value)
                    return;
                this.descriptionText = value;
            }

            this.updateText?.InvokeAsync();
        }
    }

    public event ActivityProgressEventHandler? IsIndeterminateChanged;
    public event ActivityProgressEventHandler? CaptionChanged;
    public event ActivityProgressEventHandler? CurrentActionChanged;

    private readonly RapidDispatchActionEx updateIsIndeterminate;
    private readonly RapidDispatchActionEx updateHeaderText;
    private readonly RapidDispatchActionEx updateText;
    private readonly DispatchPriority eventDispatchPriority;

    public CompletionState CompletionState { get; }
    
    public DefaultProgressTracker() : this(DispatchPriority.Loaded) {
    }

    public DefaultProgressTracker(DispatchPriority eventDispatchPriority)
    {
        this.eventDispatchPriority = eventDispatchPriority;
        this.updateIsIndeterminate = RapidDispatchActionEx.ForSync(() => this.IsIndeterminateChanged?.Invoke(this), eventDispatchPriority);
        this.updateHeaderText = RapidDispatchActionEx.ForSync(() => this.CaptionChanged?.Invoke(this), eventDispatchPriority);
        this.updateText = RapidDispatchActionEx.ForSync(() => this.CurrentActionChanged?.Invoke(this), eventDispatchPriority);
        this.CompletionState = new ConcurrentCompletionState(eventDispatchPriority);
    }
}