using System;
using System.Collections.Generic;
using FramePFX.Shortcuts.Inputs;

namespace FramePFX.Shortcuts.Managing
{
    public readonly struct GroupEvaulationArgs
    {
        public readonly IInputStroke stroke;
        public readonly List<GroupedShortcut> shortcuts;
        public readonly List<(GroupedInputState, bool)> inputStates;
        public readonly Predicate<GroupedShortcut> filter;
        public readonly bool canProcessInputStates;

        public GroupEvaulationArgs(IInputStroke stroke, List<GroupedShortcut> shortcuts, List<(GroupedInputState, bool)> inputStates, Predicate<GroupedShortcut> filter, bool canProcessInputStates)
        {
            this.stroke = stroke;
            this.shortcuts = shortcuts;
            this.inputStates = inputStates;
            this.filter = filter;
            this.canProcessInputStates = canProcessInputStates;
        }
    }
}