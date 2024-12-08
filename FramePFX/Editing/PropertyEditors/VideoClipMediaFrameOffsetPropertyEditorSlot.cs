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

using FramePFX.Editing.Timelines.Clips;
using FramePFX.Editing.Timelines.Clips.Video;
using FramePFX.PropertyEditing;

namespace FramePFX.Editing.PropertyEditors;

public class VideoClipMediaFrameOffsetPropertyEditorSlot : PropertyEditorSlot
{
    public override bool IsSelectable => false;

    public override HandlerCountMode HandlerCountMode => HandlerCountMode.Single;

    public VideoClip SingleSelection => this.Handlers.Count == 1 ? ((VideoClip) this.Handlers[0]) : null;

    public long MediaFrameOffset => this.SingleSelection?.MediaFrameOffset ?? 0L;

    public event PropertyEditorSlotEventHandler? UpdateMediaFrameOffset;

    public VideoClipMediaFrameOffsetPropertyEditorSlot() : base(typeof(VideoClip)) {
    }

    protected override void OnHandlersLoaded()
    {
        base.OnHandlersLoaded();
        this.SingleSelection.MediaFrameOffsetChanged += this.OnMediaFrameOffsetChanged;
        this.UpdateMediaFrameOffset?.Invoke(this);
    }

    protected override void OnClearingHandlers()
    {
        base.OnClearingHandlers();
        this.SingleSelection.MediaFrameOffsetChanged -= this.OnMediaFrameOffsetChanged;
    }

    private void OnMediaFrameOffsetChanged(Clip clip, long oldoffset, long newoffset)
    {
        this.UpdateMediaFrameOffset?.Invoke(this);
    }
}