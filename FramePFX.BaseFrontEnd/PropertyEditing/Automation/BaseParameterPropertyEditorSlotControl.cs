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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using FramePFX.BaseFrontEnd.Utils;
using FramePFX.Editing.Automation;
using FramePFX.Editing.Automation.Keyframes;
using FramePFX.PropertyEditing;
using FramePFX.PropertyEditing.Automation;

namespace FramePFX.BaseFrontEnd.PropertyEditing.Automation;

public abstract class BaseParameterPropertyEditorSlotControl : BasePropertyEditorSlotControl {
    public static readonly StyledProperty<IBrush?> AutomationLedBrushProperty = AvaloniaProperty.Register<BaseParameterPropertyEditorSlotControl, IBrush?>(nameof(AutomationLedBrush), Brushes.OrangeRed);

    public IBrush? AutomationLedBrush {
        get => this.GetValue(AutomationLedBrushProperty);
        set => this.SetValue(AutomationLedBrushProperty, value);
    }

    protected IAutomatable? singleHandler;
    protected AutomationSequence? singleHandlerSequence;
    private TextBlock? displayName;
    private KeyFrameToolsControl? keyFrameTools;
    private Ellipse? automationLed;

    protected BaseParameterPropertyEditorSlotControl() {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.displayName = e.NameScope.GetTemplateChild<TextBlock>("PART_DisplayName");
        this.automationLed = e.NameScope.GetTemplateChild<Ellipse>("PART_AutomationLED");

        // For some reason the template isn't being applied automatically in enough time.
        // OnHandlerListChanged is called and while keyFrameTools is non-null, it has no template.
        // So we can just force it
        this.keyFrameTools = e.NameScope.GetTemplateChild<KeyFrameToolsControl>("PART_KeyFrameTools");
        this.keyFrameTools.ApplyStyling();
        this.keyFrameTools.ApplyTemplate();
    }

    protected override void OnConnected() {
        ParameterPropertyEditorSlot slot = (ParameterPropertyEditorSlot) this.SlotModel!;
        slot.HandlersLoaded += this.OnHandlersChanged;
        slot.HandlersCleared += this.OnHandlersChanged;
        slot.DisplayNameChanged += this.OnSlotDisplayNameChanged;
        this.displayName!.Text = slot.DisplayName;
        this.OnHandlerListChanged(true);
    }

    protected override void OnDisconnected() {
        ParameterPropertyEditorSlot slot = (ParameterPropertyEditorSlot) this.SlotModel!;
        slot.DisplayNameChanged -= this.OnSlotDisplayNameChanged;
        slot.HandlersLoaded -= this.OnHandlersChanged;
        slot.HandlersCleared -= this.OnHandlersChanged;
        this.OnHandlerListChanged(false);
    }

    private void OnSlotDisplayNameChanged(ParameterPropertyEditorSlot slot) {
        if (this.displayName != null)
            this.displayName.Text = slot.DisplayName;
    }

    private void OnHandlerListChanged(bool connect) {
        ParameterPropertyEditorSlot slot = (ParameterPropertyEditorSlot) this.SlotModel!;
        if (connect && slot.Handlers.Count == 1) {
            this.singleHandler = (IAutomatable) slot.Handlers[0];
            this.keyFrameTools!.IsVisible = true;
            this.singleHandlerSequence = this.singleHandler.AutomationData[slot.Parameter];
            this.keyFrameTools!.AutomationSequence = this.singleHandlerSequence;
            this.singleHandlerSequence.OverrideStateChanged += this.OnOverrideStateChanged;
            this.singleHandlerSequence.KeyFrameAdded += this.OnKeyFrameAddedOrRemoved;
            this.singleHandlerSequence.KeyFrameRemoved += this.OnKeyFrameAddedOrRemoved;
            this.UpdateLEDColour(this.singleHandlerSequence);
        }
        else {
            this.keyFrameTools!.IsVisible = false;
            if (this.singleHandlerSequence != null) {
                this.singleHandlerSequence.OverrideStateChanged -= this.OnOverrideStateChanged;
                this.singleHandlerSequence.KeyFrameAdded -= this.OnKeyFrameAddedOrRemoved;
                this.singleHandlerSequence.KeyFrameRemoved -= this.OnKeyFrameAddedOrRemoved;
                this.singleHandlerSequence = null;
            }

            this.UpdateLEDColour(null);
            this.keyFrameTools!.AutomationSequence = null;
        }
    }

    private void OnHandlersChanged(PropertyEditorSlot sender) {
        this.OnHandlerListChanged(true);
    }

    private void OnKeyFrameAddedOrRemoved(AutomationSequence sequence, KeyFrame keyframe, int index) {
        this.UpdateLEDColour(sequence);
    }

    private void OnOverrideStateChanged(AutomationSequence sequence) {
        this.UpdateLEDColour(sequence);
    }

    private void UpdateLEDColour(AutomationSequence? sequence) {
        if (this.automationLed != null) {
            if (sequence != null) {
                this.automationLed.IsVisible = !sequence.IsEmpty;
                this.automationLed.Fill = sequence.IsOverrideEnabled ? Brushes.Gray : this.AutomationLedBrush;
            }
            else {
                this.automationLed.IsVisible = false;
                this.automationLed.Fill = null;
            }
        }
    }
}