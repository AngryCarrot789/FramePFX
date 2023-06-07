namespace FramePFX.Core.Settings {
    public class UserSettingsModel {
        // /// <summary>
        // /// Sets the parent settings that this instance will access if a property is not set
        // /// </summary>
        // public UserSettingsModel Parent { get; set; }

        public bool StopOnTogglePlay { get; set; }

        public UserSettingsModel() {

        }

        /*
        public bool TryGetProperty<T>(string key, out T value) {
            if (this.map.TryGetValue(key, out object val)) {
                value = (T) val; // can throw class cast exception
                return true;
            }
            else if (this.Parent != null) {
                return this.Parent.TryGetProperty(key, out value);
            }
            else {
                value = default;
                return false;
            }
        }

        public T GetProperty<T>(string key) {
            return this.GetProperty<T>(key, default);
        }

        public T GetProperty<T>(string key, in T def) {
            if (this.map.TryGetValue(key, out object val)) {
                return (T) val; // can throw class cast exception
            }
            else if (this.Parent != null) {
                return this.Parent.GetProperty<T>(key);
            }
            else {
                return def;
            }
        }
        */
    }
}