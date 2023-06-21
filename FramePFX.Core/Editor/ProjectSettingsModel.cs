using FramePFX.Core.RBC;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor {
    public class ProjectSettingsModel : IRBESerialisable {
        public Resolution Resolution { get; set; }

        public Rational FrameRate { get; set; }

        public ProjectSettingsModel() {
            
        }

        public void ReadFromRBE(RBEDictionary data) {
            this.Resolution = data.GetStruct<Resolution>(nameof(this.Resolution));
            this.FrameRate = data.GetStruct<Rational>(nameof(this.FrameRate));
        }

        public void WriteToRBE(RBEDictionary data) {
            data.SetStruct(nameof(this.Resolution), this.Resolution);
            data.SetStruct(nameof(this.FrameRate), this.FrameRate);
        }
    }
}