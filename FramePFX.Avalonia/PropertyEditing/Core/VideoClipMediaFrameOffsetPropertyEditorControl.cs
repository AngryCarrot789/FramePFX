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

using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using FramePFX.Avalonia.Utils;
using FramePFX.Editing.PropertyEditors;
using FramePFX.PropertyEditing;

namespace FramePFX.Avalonia.PropertyEditing.Core;

public class VideoClipMediaFrameOffsetPropertyEditorControl : BasePropEditControlContent {
    public VideoClipMediaFrameOffsetPropertyEditorSlot? SlotModel => (VideoClipMediaFrameOffsetPropertyEditorSlot?) base.SlotControl?.Model;

    private TextBlock? textBlock;

    public VideoClipMediaFrameOffsetPropertyEditorControl() {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.textBlock = e.NameScope.GetTemplateChild<TextBlock>("PART_TextBlock");
    }

    private void SlotModelOnUpdateMediaFrameOffset(PropertyEditorSlot sender) {
        if (this.textBlock != null)
            this.textBlock.Text = this.SlotModel!.MediaFrameOffset.ToString();
    }

    protected override void OnConnected() {
        this.SlotModel!.UpdateMediaFrameOffset += this.SlotModelOnUpdateMediaFrameOffset;
    }

    protected override void OnDisconnected() {
        this.SlotModel!.UpdateMediaFrameOffset -= this.SlotModelOnUpdateMediaFrameOffset;
    }
}