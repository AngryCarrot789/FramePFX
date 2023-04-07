using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FramePFX.Core.Actions {
    public class ActionManager {
        public static ActionManager Instance { get; }

        private readonly Dictionary<string, Action> actions;

        public ActionManager() {
            this.actions = new Dictionary<string, Action>();
        }

        static ActionManager() {
            Instance = new ActionManager();
        }

        public void Register(string id, Action action) {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Action cannot be null or empty", nameof(id));
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (this.actions.TryGetValue(id, out Action existing))
                throw new Exception($"Action already registered with type '{id}': {existing.GetType()}");

            this.actions[id] = action;
        }

        public Action GetAction(string id) {
            return this.actions.TryGetValue(id, out Action action) ? action : null;
        }

        public async Task<bool> Execute(string id, object dataContext) {
            if (this.actions.TryGetValue(id, out Action action)) {
                return await action.Execute(new ActionEvent(dataContext));
            }

            return false;
        }
    }
}