using System;
using FramePFX.FFmpeg;
using FramePFX.RBC;
using FramePFX.Utils;

namespace FramePFX.Editor.ResourceManaging.Resources {
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

        protected override void OnDisableCore(bool user) {
            base.OnDisableCore(user);
            this.CloseReader();
        }

        public override void Dispose() {
            base.Dispose();
            try {
                this.CloseReader();
            }
            catch (Exception e) {
                AppLogger.WriteLine("Exception while closing Mpeg reader: " + e.GetToString());
            }
        }

        public void CloseReader() {
            FFmpegReader r = this.reader;
            this.reader = null;
            if (r != null && r.IsOpen) {
                r.Close();
            }
        }

        public void LoadMedia(string filePath) {
            if (this.reader != null) {
                this.CloseReader();
            }

            this.reader = new FFmpegReader();
            try {
                this.reader.Open(filePath, false);
            }
            catch (Exception e) {
                try {
                    this.reader.Close();
                }
                catch (Exception ex) {
                    e.AddSuppressed(ex);
                }
                finally {
                    this.reader = null;
                }

                throw new Exception("Failed to open reader", e);
            }
        }
    }
}