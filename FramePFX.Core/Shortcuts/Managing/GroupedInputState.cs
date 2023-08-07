using System;
using System.Threading.Tasks;
using FramePFX.Core.Shortcuts.Inputs;

namespace FramePFX.Core.Shortcuts.Managing
{
    public class GroupedInputState
    {
        private IInputStroke activationStroke;
        private IInputStroke deactivationStroke;

        /// <summary>
        /// The collection that owns this managed input state
        /// </summary>
        public ShortcutGroup Group { get; }

        /// <summary>
        /// The name of this grouped input state. This will not be null or empty and will not consist of only whitespaces;
        /// this is always some sort of valid string (even if only 1 character)
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// This key state's full path (the parent's path (if available/not root) and this shortcut's name). Will not be null and will always containing valid characters
        /// </summary>
        public string FullPath { get; }

        /// <summary>
        /// The state of this input state
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// The input stroke that activates this key state (as in, sets <see cref="IsActive"/> to true)
        /// </summary>
        /// <exception cref="ArgumentNullException">Value cannot be null</exception>
        public IInputStroke ActivationStroke
        {
            get => this.activationStroke;
            set => this.activationStroke = value ?? throw new ArgumentNullException(nameof(value), "Activation stroke cannot be null");
        }

        /// <summary>
        /// The input stroke that deactivates this key state (as in, sets <see cref="IsActive"/> to false)
        /// </summary>
        /// <exception cref="ArgumentNullException">Value cannot be null</exception>
        public IInputStroke DeactivationStroke
        {
            get => this.deactivationStroke;
            set => this.deactivationStroke = value ?? throw new ArgumentNullException(nameof(value), "Activation stroke cannot be null");
        }

        /// <summary>
        /// A feature that allows this input state to be "locked" active, if the amount of time since activation and deactivation
        /// is less than <see cref="ThresholdUntilDeactivateOnStroke"/>.
        /// <para>
        /// Default value is true. This allows similar behaviour to Cinema 4D's "activate while holding" or "click to activate" functionality
        /// </para>
        /// </summary>
        public bool IsAutoLockThresholdEnabled { get; set; } = true;

        /// <summary>
        /// The amount of time since <see cref="ActivationStroke"/> was triggered. This will be -1 if this state was never
        /// triggered, or when <see cref="DeactivationStroke"/> is triggered. A value that isn't -1 means that <see cref="ActivationStroke"/>
        /// was triggered at some point, and this value is the timestamp of that event
        /// </summary>
        public long LastActivationTime { get; set; } = -1;

        /// <summary>
        /// Default value is 500 (milliseconds). If <see cref="DeactivationStroke"/> is triggered, there are 2 possible outcomes.
        /// Either A, the amount of time since <see cref="ActivationStroke"/> was triggered is less than this value, in which cases,
        /// the <see cref="IsActive"/> state is locked to true (until explicitly unlocked). Or B, the time is greater than or equal to
        /// this value, in which case, <see cref="IsActive"/> is set to false
        /// </summary>
        public long ThresholdUntilDeactivateOnStroke { get; set; } = 500L;

        /// <summary>
        /// Whether or not this input state can be deactivated when <see cref="ActivationStroke"/> is triggered while
        /// this input state is also locked open (see <see cref="IsCurrentlyLockedOpen"/> for more info).
        /// <para>
        /// Default value is true
        /// </para>
        /// </summary>
        public bool CanDeactivateAutoLockOnActivation { get; set; } = true;

        /// <summary>
        /// Whether or not the <see cref="IsActive"/> state was set to true because the amount of time between activation
        /// and deactivation was less than <see cref="ThresholdUntilDeactivateOnStroke"/>
        /// </summary>
        public bool IsCurrentlyLockedOpen { get; set; }

        public GroupedInputState(ShortcutGroup group, string name, IInputStroke activationStroke, IInputStroke deactivationStroke)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be null, empty, or consist of only whitespaces");
            this.Group = group ?? throw new ArgumentNullException(nameof(group), "Collection cannot be null");
            this.Name = name;
            this.FullPath = group.GetPathForName(name);
            this.ActivationStroke = activationStroke;
            this.DeactivationStroke = deactivationStroke;
        }

        public Task OnActivate()
        {
            return Task.CompletedTask;
        }

        public Task OnDeactivate()
        {
            return Task.CompletedTask;
        }

        public override string ToString()
        {
            return $"{nameof(GroupedInputState)} ({this.FullPath}: {(this.IsActive ? "pressed" : "released")} [{this.activationStroke}, {this.deactivationStroke}])";
        }
    }
}