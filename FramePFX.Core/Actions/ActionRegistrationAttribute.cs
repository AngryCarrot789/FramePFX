using System;

namespace FramePFX.Core.Actions {
    /// <summary>
    /// A helper attribute for registering actions. <see cref="ActionManager.SearchAndRegisterActions(ActionManager)"/> will 
    /// search all types for this attribute and use it to register a new instance of the action
    /// </summary>
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