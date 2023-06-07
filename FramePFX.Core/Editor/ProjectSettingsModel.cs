using FramePFX.Core.RBC;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor {
    public class ProjectSettingsModel : IRBESerialisable {
        public Resolution Resolution { get; set; }

        public double FrameRate { get; set; }

        public ProjectSettingsModel() {
            
        }

        public void ReadFromRBE(RBEDictionary data) {
            this.Resolution = data.GetStruct<Resolution>(nameof(this.Resolution));
            this.FrameRate = data.GetDouble(nameof(this.FrameRate));
        }

        public void WriteToRBE(RBEDictionary data) {
            data.SetStruct(nameof(this.Resolution), this.Resolution);
            data.SetDouble(nameof(this.FrameRate), this.FrameRate);
        }
    }
}