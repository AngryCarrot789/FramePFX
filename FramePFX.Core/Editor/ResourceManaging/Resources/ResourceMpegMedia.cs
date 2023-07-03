using System.Threading.Tasks;
using FramePFX.Core.FFmpeg;
using FramePFX.Core.RBC;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ResourceManaging.Resources {
    /// <summary>
    /// A resource that contains resources for decoding mpeg media
    /// </summary>
    public class ResourceMpegMedia : ResourceItem {
        public string FilePath { get; set; }

        public FFmpegReader reader;

        public ResourceMpegMedia() {

        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            if (!string.IsNullOrEmpty(this.FilePath))
                data.SetString(nameof(this.FilePath), this.FilePath);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.FilePath = data.GetString(nameof(this.FilePath), null);
        }

        protected override void OnDisableCore(ExceptionStack stack, bool user) {
            base.OnDisableCore(stack, user);
            this.CloseReader();
        }

        protected override void DisposeCore(ExceptionStack stack) {
            base.DisposeCore(stack);
            this.CloseReader();
        }

        public void CloseReader() {
            if (this.reader != null && this.reader.IsOpen)
                this.reader.Close();
            this.reader = null;
        }
    }
}