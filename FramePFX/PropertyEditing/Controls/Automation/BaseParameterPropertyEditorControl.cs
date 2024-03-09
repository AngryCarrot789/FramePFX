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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using FramePFX.Editors.Automation;
using FramePFX.Editors.Automation.Keyframes;
using FramePFX.PropertyEditing.Automation;

namespace FramePFX.PropertyEditing.Controls.Automation
{
    public abstract class BaseParameterPropertyEditorControl : BasePropEditControlContent
    {
        protected IAutomatable singleHandler;
        protected AutomationSequence singleHandlerSequence;
        private TextBlock displayName;
        private KeyFrameToolsControl keyFrameTools;
        private Ellipse automationLed;

        protected BaseParameterPropertyEditorControl()
        {
        }

        static BaseParameterPropertyEditorControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BaseParameterPropertyEditorControl), new FrameworkPropertyMetadata(typeof(BaseParameterPropertyEditorControl)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.displayName = this.GetTemplateChild<TextBlock>("PART_DisplayName");
            this.automationLed = (Ellipse) this.GetTemplateChild("PART_AutomationLED");
            this.keyFrameTools = this.GetTemplateChild<KeyFrameToolsControl>("PART_KeyFrameTools");
        }

        protected override void OnConnected()
        {
            ParameterPropertyEditorSlot slot = (ParameterPropertyEditorSlot) this.SlotModel;
            slot.HandlersLoaded += this.OnHandlersChanged;
            slot.HandlersCleared += this.OnHandlersChanged;
            slot.DisplayNameChanged += this.OnSlotDisplayNameChanged;
            this.displayName.Text = slot.DisplayName;
            this.OnHandlerListChanged(true);
        }

        protected override void OnDisconnected()
        {
            ParameterPropertyEditorSlot slot = (ParameterPropertyEditorSlot) this.SlotModel;
            slot.DisplayNameChanged -= this.OnSlotDisplayNameChanged;
            slot.HandlersLoaded -= this.OnHandlersChanged;
            slot.HandlersCleared -= this.OnHandlersChanged;
            this.OnHandlerListChanged(false);
        }

        private void OnSlotDisplayNameChanged(ParameterPropertyEditorSlot slot)
        {
            if (this.displayName != null)
                this.displayName.Text = slot.DisplayName;
        }

        private void OnHandlerListChanged(bool connect)
        {
            ParameterPropertyEditorSlot slot = (ParameterPropertyEditorSlot) this.SlotModel;
            if (connect && slot != null && slot.Handlers.Count == 1)
            {
                this.singleHandler = (IAutomatable) slot.Handlers[0];
                this.keyFrameTools.Visibility = Visibility.Visible;
                this.singleHandlerSequence = this.singleHandler.AutomationData[slot.Parameter];
                this.keyFrameTools.AutomationSequence = this.singleHandlerSequence;
                this.singleHandlerSequence.OverrideStateChanged += this.OnOverrideStateChanged;
                this.singleHandlerSequence.KeyFrameAdded += this.OnKeyFrameAddedOrRemoved;
                this.singleHandlerSequence.KeyFrameRemoved += this.OnKeyFrameAddedOrRemoved;
                this.UpdateLEDColour(this.singleHandlerSequence);
            }
            else
            {
                this.keyFrameTools.Visibility = Visibility.Collapsed;
                if (this.singleHandlerSequence != null)
                {
                    this.singleHandlerSequence.OverrideStateChanged -= this.OnOverrideStateChanged;
                    this.singleHandlerSequence.KeyFrameAdded -= this.OnKeyFrameAddedOrRemoved;
                    this.singleHandlerSequence.KeyFrameRemoved -= this.OnKeyFrameAddedOrRemoved;
                    this.singleHandlerSequence = null;
                }

                this.keyFrameTools.AutomationSequence = null;
            }
        }

        private void OnHandlersChanged(PropertyEditorSlot sender)
        {
            this.OnHandlerListChanged(true);
        }

        private void OnKeyFrameAddedOrRemoved(AutomationSequence sequence, KeyFrame keyframe, int index)
        {
            this.UpdateLEDColour(sequence);
        }

        private void OnOverrideStateChanged(AutomationSequence sequence)
        {
            this.UpdateLEDColour(sequence);
        }

        private void UpdateLEDColour(AutomationSequence sequence)
        {
            if (this.automationLed != null && sequence != null)
            {
                this.automationLed.Visibility = sequence.IsEmpty ? Visibility.Collapsed : Visibility.Visible;
                this.automationLed.Fill = sequence.IsOverrideEnabled ? Brushes.Gray : Brushes.OrangeRed;
            }
        }
    }
}