using System.Windows;
using FramePFX.Interactivity;

namespace FramePFX.WPF.Interactivity {
    public class DataObjectWrapper : IDataObjekt {
        private readonly IDataObject mObject;

        public DataObjectWrapper(IDataObject mObject) {
            this.mObject = mObject;
        }

        public object GetData(string format) {
            return this.mObject.GetData(format);
        }

        public object GetData(string format, bool autoConvert) {
            return this.mObject.GetData(format, autoConvert);
        }

        public bool GetDataPresent(string format) {
            return this.mObject.GetDataPresent(format);
        }

        public bool GetDataPresent(string format, bool autoConvert) {
            return this.mObject.GetDataPresent(format, autoConvert);
        }

        public string[] GetFormats() {
            return this.mObject.GetFormats();
        }

        public string[] GetFormats(bool autoConvert) {
            return this.mObject.GetFormats(autoConvert);
        }

        public void SetData(object data) {
            this.mObject.SetData(data);
        }

        public void SetData(string format, object data) {
            this.mObject.SetData(format, data);
        }

        public void SetData(string format, object data, bool autoConvert) {
            this.mObject.SetData(format, data, autoConvert);
        }
    }
}