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

using System.Diagnostics.CodeAnalysis;
using FramePFX.Editing.Automation.Keyframes;
using FramePFX.Editing.Automation.Params;
using FramePFX.Utils.RBC;

namespace FramePFX.Editing.Automation;

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
                throw new ArgumentNullException(nameof(parameter), "Parameter cannot be null");
            }

            if (this.sequences.TryGetValue(parameter, out AutomationSequence? sequence)) {
                return sequence;
            }

            this.ValidateParameter(parameter);
            this.sequences[parameter] = sequence = new AutomationSequence(this, parameter);
            return sequence;
        }
    }

    /// <summary>
    /// Gets or sets the parameter key that is currently active. This is used by
    /// the UI to display an automation sequence editor for a specific parameter.
    /// <para>
    /// The actual key's path is not verified, so this may return a parameter key
    /// that maps to a parameter that is incompatible with this automation data
    /// </para>
    /// </summary>
    public ParameterKey ActiveParameter { get; set; }

    /// <summary>
    /// An event fired when a sequence updates a value of its automation data owner. This event
    /// is fired after all handler to <see cref="AutomationSequence.ParameterChanged"/> have been invoked;
    /// this handler is a general handler for all parameter value changes relative to our owner
    /// </summary>
    public event ParameterChangedEventHandler? ParameterValueChanged;

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
    public bool TryGetSequence(Parameter parameter, [NotNullWhen(true)] out AutomationSequence? value) {
        if (this.sequences.TryGetValue(parameter, out value)) {
            return true;
        }

        this.ValidateParameter(parameter);
        return false;
    }

    public void WriteToRBE(RBEDictionary data) {
        if (!this.ActiveParameter.IsEmpty)
            data.SetString(nameof(this.ActiveParameter), this.ActiveParameter.ToString());

        RBEList list = data.CreateList(nameof(this.Sequences));
        foreach (AutomationSequence sequence in this.sequences.Values) {
            RBEDictionary dictionary = list.AddDictionary();
            dictionary.SetString("KeyId", sequence.Parameter.Key.ToString());
            sequence.WriteToRBE(dictionary);
        }
    }

    public void ReadFromRBE(RBEDictionary data) {
        if (data.TryGetString(nameof(this.ActiveParameter), out string? activeParamText))
            this.ActiveParameter = ParameterKey.Parse(activeParamText, default);
        RBEList list = data.GetList(nameof(this.Sequences));
        foreach (RBEBase rbe in list.List) {
            if (!(rbe is RBEDictionary dictionary))
                throw new Exception("Expected a list of dictionaries");

            ParameterKey paramKey = ParameterKey.Parse(dictionary.GetString("KeyId"));
            if (!Parameter.TryGetParameterByKey(paramKey, out Parameter? parameter))
                throw new Exception("Unknown automation parameter: " + paramKey);

            this[parameter].ReadFromRBE(dictionary);
        }

        this.UpdateBackingStorage();
    }

    public void LoadDataIntoClone(AutomationData clone) {
        clone.ActiveParameter = this.ActiveParameter;
        foreach (KeyValuePair<Parameter, AutomationSequence> seq in this.sequences) {
            AutomationSequence.LoadDataIntoClone(seq.Value, clone[seq.Key]);
        }

        clone.UpdateBackingStorage();
    }

    /// <summary>
    /// Updates all sequences using a frame of -1, indicating that the <see cref="AutomationSequence.DefaultKeyFrame"/> should
    /// be used to query the value instead of any actual key frame. Useful just after reading the state of an automation owner's data
    /// </summary>
    public void UpdateBackingStorage() => this.UpdateAll(-1);

    /// <summary>
    /// Updates all sequences' effective values
    /// </summary>
    /// <param name="frame">The frame used to calculate the effective value</param>
    public void UpdateAll(long frame) {
        IList<AutomationSequence> list = this.sequences.Values;
        for (int i = 0, count = list.Count; i < count; i++) {
            list[i].UpdateValue(frame);
        }
    }

    public void UpdateAll() {
        if (this.Owner.GetRelativePlayHead(out long playHead)) {
            this.UpdateAll(playHead);
        }
    }

    /// <summary>
    /// Updates all sequences' effective values, if there are key frames available and override is not enabled
    /// </summary>
    /// <param name="frame"></param>
    public void UpdateAllAutomated(long frame) {
        IList<AutomationSequence> list = this.sequences.Values;
        for (int i = 0, count = list.Count; i < count; i++) {
            AutomationSequence x = list[i];
            if (x.CanAutomate)
                x.UpdateValue(frame);
        }
    }

    public bool IsParameterValid(Parameter parameter) {
        return parameter.OwnerType.IsInstanceOfType(this.Owner);
    }

    public void ValidateParameter(Parameter parameter) {
        if (!this.IsParameterValid(parameter))
            throw new ArgumentException("Invalid parameter key for this automation data: " + parameter.Key + ". The owner types are incompatible");
    }

    public bool IsAutomated(Parameter parameter) {
        return this.TryGetSequence(parameter, out AutomationSequence? sequence) && sequence.HasKeyFrames;
    }

    public void InvalidateTimelineRender() => this.Owner.Timeline?.RenderManager.InvalidateRender();

    internal static void InternalOnParameterValueChanged(AutomationSequence sequence) {
        sequence.AutomationData.ParameterValueChanged?.Invoke(sequence);
        Parameter.InternalOnParameterValueChanged(sequence.Parameter, sequence);
    }
}