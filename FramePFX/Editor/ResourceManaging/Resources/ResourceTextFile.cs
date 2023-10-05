using FramePFX.RBC;

namespace FramePFX.Editor.ResourceManaging.Resources {
    public class ResourceTextFile : ResourceItem {
        public ProjectPath? Path;

        public ResourceTextFile() {
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            this.Path?.WriteToRBE(data.CreateDictionary(nameof(this.Path)));
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            if (data.TryGetDictionary(nameof(this.Path), out RBEDictionary dictionary)) {
                this.Path = ProjectPath.ReadFromRBE(dictionary);
            }
        }
    }
}