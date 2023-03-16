using System.Collections.Generic;
using System.IO;

namespace FramePFX.Core.RBC {
    public class RBCDictionary : RBCBase {
        private readonly Dictionary<string, RBCBase> map;

        public override int TypeId { get; }
        
        public RBCDictionary() {
            this.map = new Dictionary<string, RBCBase>();
        }

        public override void Load(BinaryReader reader) {
            throw new System.NotImplementedException();
        }

        public override void Save(BinaryWriter writer) {
            writer.Write(this.map.Count);
            foreach (KeyValuePair<string, RBCBase> entry in this.map) {
                writer.Write((byte) entry.Key.Length);
                writer.Write(entry.Key.ToCharArray());
                entry.Value.Save(writer);
            }
        }
    }
}