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

using System;
using System.Collections.Generic;
using FramePFX.Editors.Timelines;

namespace FramePFX.Editors.DataTransfer {
    /// <summary>
    /// A class which helps manage data properties relative to a specific instance of an owner
    /// object. This class manages firing value changed events and processing data property flags,
    /// which is why it is important that the setter methods of the data properties are not called directly
    /// </summary>
    public class TransferableData {
        private Dictionary<int, ParameterData> sequences;

        public ITransferableData Owner { get; }

        /// <summary>
        /// An event fired when any parameter's value changes relative to our owner
        /// </summary>
        public event DataParameterValueChangedEventHandler ValueChanged;

        public TransferableData(ITransferableData owner) {
            this.Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        /// <summary>
        /// Adds a value changed event handler to the given parameter for this specific property data's owner
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="handler"></param>
        public void AddValueChangedHandler(DataParameter parameter, DataParameterValueChangedEventHandler handler) {
            this.GetParamData(parameter).ValueChanged += handler;
        }

        /// <summary>
        /// Removes a value changed event handler for the given parameter from this specific property data's owner
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="handler"></param>
        public void RemoveValueChangedHandler(DataParameter parameter, DataParameterValueChangedEventHandler handler) {
            if (this.TryGetParameterData(parameter, out ParameterData data)) {
                data.ValueChanged -= handler;
            }
        }

        public bool IsValueChanging(DataParameter parameter) {
            return this.TryGetParameterData(parameter, out ParameterData data) && data.isValueChanging;
        }

        public bool IsParameterValid(DataParameter parameter) {
            return parameter.OwnerType.IsInstanceOfType(this.Owner);
        }

        public void ValidateParameter(DataParameter parameter) {
            if (!this.IsParameterValid(parameter))
                throw new ArgumentException("Invalid parameter key for this automation data: " + parameter.Key + ". The owner types are incompatible");
        }

        private bool TryGetParameterData(DataParameter parameter, out ParameterData data) {
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter), "Parameter cannot be null");
            if (this.sequences != null && this.sequences.TryGetValue(parameter.GlobalIndex, out data))
                return true;
            this.ValidateParameter(parameter);
            data = null;
            return false;
        }

        private ParameterData GetParamData(DataParameter parameter) {
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter), "Parameter cannot be null");
            
            ParameterData data;
            if (this.sequences == null)
                this.sequences = new Dictionary<int, ParameterData>();
            else if (this.sequences.TryGetValue(parameter.GlobalIndex, out data))
                return data;
            this.ValidateParameter(parameter);
            this.sequences[parameter.GlobalIndex] = data = new ParameterData();
            return data;

        }

        private class ParameterData {
            public bool isValueChanging;
            public event DataParameterValueChangedEventHandler ValueChanged;

            public void OnValueChanged(DataParameter param, ITransferableData owner) {
                 this.ValueChanged?.Invoke(param, owner);
            }
        }

        public static void InternalBeginValueChange(DataParameter parameter, ITransferableData owner) {
            ParameterData internalData = owner.TransferableData.GetParamData(parameter);
            if (internalData.isValueChanging) {
                throw new InvalidOperationException("Value is already changing. This exception is thrown as the alternative is most likely a stack overflow exception");
            }

            internalData.isValueChanging = true;
        }

        public static void InternalEndValueChange(DataParameter parameter, ITransferableData owner) {
            TransferableData data = owner.TransferableData;
            ParameterData internalData = data.GetParamData(parameter);
            try {
                internalData.OnValueChanged(parameter, owner);
                data.ValueChanged?.Invoke(parameter, owner);
                DataParameter.InternalOnParameterValueChanged(parameter, owner);
                if (parameter.Flags != DataParameterFlags.None) {
                    if ((parameter.Flags & DataParameterFlags.ModifiesProject) != 0 && owner is IHaveProject projHolder && projHolder.Project is Project project)
                        project.MarkModified();
                    if ((parameter.Flags & DataParameterFlags.AffectsRender) != 0 && owner is IHaveTimeline timelineHolder && timelineHolder.Timeline is Timeline timeline)
                        timeline.RenderManager.InvalidateRender();
                }
            }
            finally {
                internalData.isValueChanging = false;
            }
        }
    }
}