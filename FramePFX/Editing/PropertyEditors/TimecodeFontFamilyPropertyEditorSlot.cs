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
using FramePFX.Editing.Timelines.Clips.Core;
using FramePFX.PropertyEditing;
using FramePFX.Utils;

namespace FramePFX.Editing.PropertyEditors;

public class TimecodeFontFamilyPropertyEditorSlot : PropertyEditorSlot
{
    public IEnumerable<TimecodeClip> Clips => this.Handlers.Cast<TimecodeClip>();

    public TimecodeClip SingleSelection => (TimecodeClip) this.Handlers[0];

    public string FontFamily { get; private set; }

    public override bool IsSelectable => true;

    public event PropertyEditorSlotEventHandler? FontFamilyChanged;
    private bool isProcessingValueChange;

    public TimecodeFontFamilyPropertyEditorSlot() : base(typeof(TimecodeClip)) {
    }

    public void SetValue(string value)
    {
        this.isProcessingValueChange = true;

        this.FontFamily = value;
        for (int i = 0, c = this.Handlers.Count; i < c; i++)
        {
            TimecodeClip clip = (TimecodeClip) this.Handlers[i];
            clip.FontFamily = value;
        }

        this.FontFamilyChanged?.Invoke(this);
        this.isProcessingValueChange = false;
    }

    protected override void OnHandlersLoaded()
    {
        base.OnHandlersLoaded();
        if (this.Handlers.Count == 1)
        {
            this.SingleSelection.FontFamilyChanged += this.OnClipFontFamilyChanged;
        }

        this.RequeryOpacityFromHandlers();
    }

    protected override void OnClearingHandlers()
    {
        base.OnClearingHandlers();
        if (this.Handlers.Count == 1)
        {
            this.SingleSelection.FontFamilyChanged -= this.OnClipFontFamilyChanged;
        }
    }

    public void RequeryOpacityFromHandlers()
    {
        this.FontFamily = CollectionUtils.GetEqualValue(this.Handlers, x => ((TimecodeClip) x).FontFamily, out string? d) ? d : "<different values>";
        this.FontFamilyChanged?.Invoke(this);
    }

    private void OnClipFontFamilyChanged(Clip theClip)
    {
        if (this.isProcessingValueChange)
            return;

        TimecodeClip clip = (TimecodeClip) theClip;
        if (this.FontFamily != clip.FontFamily)
        {
            this.FontFamily = clip.FontFamily;
            this.FontFamilyChanged?.Invoke(this);
        }
    }
}