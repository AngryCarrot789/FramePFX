using System;
using System.Collections.Generic;
using FramePFX.Editors.Automation.Keyframes;
using FramePFX.Editors.Automation.Params;
using FramePFX.RBC;

namespace FramePFX.Editors.Automation {
    /// <summary>
    /// Contains a collection of <see cref="AutomationSequence"/> objects mapped by an <see cref="Parameter"/>. This
    /// class is designed to be immutable; it is typically created in the constructor of an <see cref="IAutomatable"/>
    /// object, where sequences are assigned via <see cref="AssignKey"/>
    /// </summary>
    public class AutomationData {
        private static readonly Comparer<Parameter> AutomationParameterComparer = Comparer<Parameter>.Create((a, b) => a.GlobalIndex.CompareTo(b.GlobalIndex));
        private readonly SortedList<Parameter, AutomationSequence> sequences;

        /// <summary>
        /// Returns a read-only list of our sequences. Sequences are lazily created to save memory
        /// </summary>
        public IList<AutomationSequence> Sequences => this.sequences.Values;

        /// <summary>
        /// The object that owns this automation data instance
        /// </summary>
        public IAutomatable Owner { get; }

        /// <summary>
        /// Gets the automation sequence (aka timeline) for the specific automation key. If one does not exist, it is created
        /// </summary>
        /// <param name="parameter"></param>
        public AutomationSequence this[Parameter parameter] {
            get {
                if (parameter == null) {
                    throw new ArgumentNullException(nameof(parameter), "Key cannot be null");
                }

                if (this.sequences.TryGetValue(parameter, out AutomationSequence sequence)) {
                    return sequence;
                }

                this.ValidateParameter(parameter);
                this.sequences[parameter] = sequence = new AutomationSequence(this, parameter);
                return sequence;
            }
        }

        /// <summary>
        /// The active key's full ID, only really used by the UI
        /// </summary>
        public string ActiveKeyFullId { get; set; }

        /// <summary>
        /// An event fired when a sequence updates a value of its automation data owner. This event is fired after all handler to <see cref="AutomationSequence.ParameterChanged"/> have been invoked; this method is a general handler for all parameter value changes
        /// </summary>
        public event ParameterChangedEventHandler ParameterChanged;

        public AutomationData(IAutomatable owner) {
            this.Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            this.sequences = new SortedList<Parameter, AutomationSequence>(4, AutomationParameterComparer);
        }

        /// <summary>
        /// A helper method to add an event handler for the <see cref="AutomationSequence.ParameterChanged"/>
        /// </summary>
        /// <param name="parameter">The target parameter</param>
        /// <param name="handler">The event handler</param>
        public void AddParameterChangedHandler(Parameter parameter, ParameterChangedEventHandler handler) {
            this[parameter].ParameterChanged += handler;
        }

        /// <summary>
        /// A helper method to remove an event handler for the <see cref="AutomationSequence.ParameterChanged"/>
        /// </summary>
        /// <param name="parameter">The target parameter</param>
        /// <param name="handler">The event handler</param>
        public void RemoveParameterChangedHandler(Parameter parameter, ParameterChangedEventHandler handler) {
            this[parameter].ParameterChanged -= handler;
        }

        /// <summary>
        /// Gets a sequence for the given parameter, if one exists.
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetSequence(Parameter parameter, out AutomationSequence value) {
            if (this.sequences.TryGetValue(parameter, out value)) {
                return true;
            }

            this.ValidateParameter(parameter);
            return false;
        }

        public void WriteToRBE(RBEDictionary data) {
            if (this.ActiveKeyFullId != null)
                data.SetString(nameof(this.ActiveKeyFullId), this.ActiveKeyFullId);

            RBEList list = data.CreateList(nameof(this.Sequences));
            foreach (AutomationSequence sequence in this.sequences.Values) {
                RBEDictionary dictionary = list.AddDictionary();
                dictionary.SetString("KeyId", sequence.Parameter.Key.ToString());
                sequence.WriteToRBE(dictionary);
            }
        }

        public void ReadFromRBE(RBEDictionary data) {
            this.ActiveKeyFullId = data.GetString(nameof(this.ActiveKeyFullId), null);
            RBEList list = data.GetList(nameof(this.Sequences));
            foreach (RBEBase rbe in list.List) {
                if (!(rbe is RBEDictionary dictionary))
                    throw new Exception("Expected a list of dictionaries");
                
                string fullId = dictionary.GetString("KeyId");
                ParameterKey paramKey = ParameterKey.Parse(fullId);
                if (!Parameter.TryGetParameterByKey(paramKey, out Parameter parameter))
                    throw new Exception("Unknown automation parameter: " + fullId);

                this[parameter].ReadFromRBE(dictionary);
            }

            this.UpdateBackingStorage();
        }

        public void LoadDataIntoClone(AutomationData clone) {
            clone.ActiveKeyFullId = this.ActiveKeyFullId;
            IList<AutomationSequence> listA = this.sequences.Values;
            IList<AutomationSequence> listB = clone.sequences.Values;
            for (int i = 0, c = this.sequences.Count; i < c; i++) {
                AutomationSequence.LoadDataIntoClone(listA[i], listB[i]);
            }
        }

        /// <summary>
        /// Updates all sequences using a frame of -1, indicating that the <see cref="AutomationSequence.DefaultKeyFrame"/> should
        /// be used to query the value instead of any actual key frame. Useful just after reading the state of an automation owner's data
        /// </summary>
        public void UpdateBackingStorage() {
            IList<AutomationSequence> list = this.sequences.Values;
            for (int i = 0, count = list.Count; i < count; i++) {
                list[i].UpdateValue(-1);
            }
        }

        /// <summary>
        /// Updates all sequences' effective values
        /// </summary>
        /// <param name="frame">The frame used to calculate the effective value</param>
        public void Update(long frame) {
            IList<AutomationSequence> list = this.sequences.Values;
            for (int i = 0, count = list.Count; i < count; i++) {
                AutomationSequence sequence = list[i];
                if (sequence.CanAutomate)
                    sequence.UpdateValue(frame);
            }
        }

        private void ValidateParameter(Parameter parameter) {
            if (!parameter.OwnerType.IsInstanceOfType(this.Owner))
                throw new ArgumentException("Invalid parameter key for this automation data: " + parameter.Key + ". The owner types are incompatible");
        }

        internal static void OnParameterValueChanged(AutomationSequence sequence) {
            sequence.AutomationData.ParameterChanged?.Invoke(sequence);
        }

        public bool IsAutomated(Parameter parameter) {
            return this.TryGetSequence(parameter, out AutomationSequence sequence) && sequence.HasKeyFrames;
        }
    }
}