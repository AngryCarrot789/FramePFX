using System.Collections.Generic;
using FramePFX.Configurations.Sections;
using Newtonsoft.Json.Linq;

namespace FramePFX.Configurations {
    public class JsonConfig {
        public static ISection ReadConfig(string jsonText) {
            MemorySection root = new MemorySection(null, null);
            LoadJsonObject(root, JObject.Parse(jsonText));
            return root;
        }

        public static void LoadJsonObject(ISection section, JObject obj) {
            foreach (JProperty property in obj.Properties()) {
                LoadJsonElement(section, property.Name, property.Value);
            }
        }

        public static void LoadJsonElement(ISection section, string key, JToken value) {
            if (value is JObject obj) {
                LoadJsonObject(section.CreateSection(key), obj);
            }
            else if (value is JArray arr) {
                List<object> list = new List<object>();
                foreach (JToken child in arr.Children()) {
                    if (child is JValue val) {
                        list.Add(val.Value);
                    }
                    else {
                        list.Add(child.ToString());
                    }
                }

                section[key] = list;
            }
            else if (value is JValue val) {
                section[key] = val.Value;
            }
            else {
                section[key] = value != null ? value.ToString() : null;
            }
        }
    }
}