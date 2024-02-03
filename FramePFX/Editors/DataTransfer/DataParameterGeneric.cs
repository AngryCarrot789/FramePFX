using System;
using FramePFX.Editors.Automation.Params;

namespace FramePFX.Editors.DataTransfer {
    /// <summary>
    /// A main and generic implementation of <see cref="DataParameter"/>, which also provides a default value property
    /// <para>
    /// While creating derived types is not necessary, you can do so to add things like value range limits
    /// </para>
    /// </summary>
    /// <typeparam name="T">The type of value this data parameter deals with</typeparam>
    public class DataParameterGeneric<T> : DataParameter {
        private readonly ValueAccessor<T> accessor;
        protected readonly bool isObjectAccessPreferred;

        public T DefaultValue { get; }

        public DataParameterGeneric(Type ownerType, string key, ValueAccessor<T> accessor, DataParameterFlags flags = DataParameterFlags.None) : base(ownerType, key, flags) {
            if (accessor == null)
                throw new ArgumentNullException(nameof(accessor));
            this.accessor = accessor;
            this.isObjectAccessPreferred = accessor.IsObjectPreferred;
        }

        public DataParameterGeneric(Type ownerType, string key, T defaultValue, ValueAccessor<T> accessor, DataParameterFlags flags = DataParameterFlags.None) : this(ownerType, key, accessor, flags) {
            this.DefaultValue = defaultValue;
        }

        /// <summary>
        /// Gets the generic value for this parameter
        /// </summary>
        /// <param name="owner">The owner instance</param>
        /// <returns>The generic value</returns>
        public virtual T GetValue(ITransferableData owner) {
            return this.accessor.GetValue(owner);
        }

        /// <summary>
        /// Sets the generic value for this parameter
        /// </summary>
        /// <param name="owner">The owner instance</param>
        /// <param name="value">The new value</param>
        public virtual void SetValue(ITransferableData owner, T value) {
            this.OnBeginValueChange(owner);
            Exception error = null;
            try {
                this.accessor.SetValue(owner, value);
            }
            catch (Exception e) {
                error = e;
            }
            finally {
                this.OnEndValueChangedHelper(owner, ref error);
            }
        }

        public override object GetObjectValue(ITransferableData owner) {
            return this.accessor.GetObjectValue(owner);
        }

        public override void SetObjectValue(ITransferableData owner, object value) {
            this.OnBeginValueChange(owner);
            Exception error = null;
            try {
                this.accessor.SetObjectValue(owner, value);
            }
            catch (Exception e) {
                error = e;
            }
            finally {
                this.OnEndValueChangedHelper(owner, ref error);
            }
        }

        protected void OnEndValueChangedHelper(ITransferableData owner, ref Exception e) {
            try {
                this.OnEndValueChange(owner);
            }
            catch (Exception exception) {
                e = e != null ? new AggregateException("An exception occurred while updating the value and finalizing the transaction", e, exception) : exception;
            }
        }
    }
}