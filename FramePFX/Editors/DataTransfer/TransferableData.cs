using System;
using System.Collections.Generic;

namespace FramePFX.Editors.DataTransfer {
    /// <summary>
    /// A class which helps manage data properties relative to a specific instance of an owner
    /// object. This class manages firing value changed events and processing data property flags,
    /// which is why it is important that the setter methods of the data properties are not called directly
    /// </summary>
    public class TransferableData {
        private readonly SortedList<DataParameter, ParameterData> sequences;

        public ITransferableData Owner { get; }

        /// <summary>
        /// An event fired when any parameter's value changes relative to our owner
        /// </summary>
        public event DataParameterValueChangedEventHandler ValueChanged;

        public TransferableData(ITransferableData owner) {
            this.Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            this.sequences = new SortedList<DataParameter, ParameterData>();
        }

        public void SetValueFromObject(DataParameter parameter, object value) {
            this.GetParamData(parameter).SetObjectValue(value);
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
            ParameterData data = this.GetInternalParameterDataOrNull(parameter);
            if (data != null) {
                data.ValueChanged -= handler;
            }
        }

        public bool IsParameterValid(DataParameter parameter) {
            return parameter.OwnerType.IsInstanceOfType(this.Owner);
        }

        public void ValidateParameter(DataParameter parameter) {
            if (!this.IsParameterValid(parameter))
                throw new ArgumentException("Invalid parameter key for this automation data: " + parameter.Key + ". The owner types are incompatible");
        }

        private ParameterData GetParamData(DataParameter parameter) {
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter), "Parameter cannot be null");
            if (this.sequences.TryGetValue(parameter, out ParameterData data))
                return data;

            this.ValidateParameter(parameter);
            this.sequences[parameter] = data = new ParameterData(this, parameter);
            return data;
        }

        private ParameterData GetInternalParameterDataOrNull(DataParameter parameter) {
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter), "Parameter cannot be null");
            if (this.sequences.TryGetValue(parameter, out ParameterData data))
                return data;
            this.ValidateParameter(parameter);
            return null;
        }

        private class ParameterData {
            public readonly TransferableData propertyData;
            public readonly DataParameter parameter;
            public bool isValueChanging;

            public event DataParameterValueChangedEventHandler ValueChanged;


            public ParameterData(TransferableData propertyData, DataParameter parameter) {
                this.propertyData = propertyData;
                this.parameter = parameter;
            }

            public void OnValueChanged(DataParameter param, ITransferableData owner) {
                this.ValueChanged?.Invoke(param, owner);
            }

            public void SetObjectValue(object value) {
                if (this.isValueChanging) {
                    throw new InvalidOperationException("Already updating the value");
                }

                try {
                    this.isValueChanging = true;
                    ITransferableData owner = this.propertyData.Owner;
                    this.parameter.SetObjectValue(owner, value);
                }
                finally {
                    this.isValueChanging = false;
                }
            }
        }

        public static void InternalBeginValueChange(DataParameter parameter, ITransferableData owner) {
            ParameterData internalData = owner.TransferableData.GetParamData(parameter);
            if (internalData.isValueChanging) {
                throw new InvalidOperationException("Value is already changing. This would most likely result in a stack overflow exception");
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
                if ((parameter.Flags & DataParameterFlags.AffectsRender) != 0 && owner is IHaveTimeline timelineOwner) {
                    timelineOwner?.Timeline.InvalidateRender();
                }
            }
            finally {
                internalData.isValueChanging = false;
            }
        }
    }
}