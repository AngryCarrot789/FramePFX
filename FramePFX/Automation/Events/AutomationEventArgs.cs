using System;
using FramePFX.Automation.Keys;

namespace FramePFX.Automation.Events {
    public class AutomationEventArgs : EventArgs {
        public AutomationKey Key { get; }

        /// <summary>
        /// The automation data associated with the automation event. This contains the instance whose data is to be modified
        /// </summary>
        public AutomationData AutomationData { get; }

        /// <summary>
        /// Whether or not the automation event occurred while the editor is rendering and
        /// should therefore not modify the front-end, in order to improve performance
        /// <para>
        /// Alternative name: "IsFastMode"
        /// </para>
        /// </summary>
        public bool IsRendering { get; }

        public AutomationEventArgs(AutomationKey key, AutomationData automationData, bool isRendering) {
            this.Key = key;
            this.AutomationData = automationData;
            this.IsRendering = isRendering;
        }
    }
}