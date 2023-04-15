using System;

namespace FramePFX.Core.Actions {
    [AttributeUsage(AttributeTargets.Class)]
    public class ActionRegistrationAttribute : Attribute {
        public string ActionId { get; }

        public ActionRegistrationAttribute(string actionId) {
            ActionManager.ValidateId(actionId);
            this.ActionId = actionId;
        }

        public override bool Equals(object obj) {
            return obj is ActionRegistrationAttribute attrib && this.ActionId.Equals(attrib.ActionId);
        }

        public override int GetHashCode() {
            return this.ActionId.GetHashCode();
        }
    }
}