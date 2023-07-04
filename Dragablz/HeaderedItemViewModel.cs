using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Dragablz {
    /// <summary>
    /// Helper class to create view models, particularly for tool/MDI windows.
    /// </summary>
    public class HeaderedItemViewModel : INotifyPropertyChanged {
        private bool _isSelected;
        private object _header;
        private object _content;

        public HeaderedItemViewModel() {
        }

        public HeaderedItemViewModel(object header, object content, bool isSelected = false) {
            this._header = header;
            this._content = content;
            this._isSelected = isSelected;
        }

        public object Header {
            get { return this._header; }
            set {
                if (this._header == value)
                    return;
                this._header = value;
                #if NET40
                this.OnPropertyChanged("Header");
                #else
                this.OnPropertyChanged();
                #endif
            }
        }

        public object Content {
            get { return this._content; }
            set {
                if (this._content == value)
                    return;
                this._content = value;
                #if NET40
                this.OnPropertyChanged("Content");
                #else
                this.OnPropertyChanged();
                #endif
            }
        }

        public bool IsSelected {
            get { return this._isSelected; }
            set {
                if (this._isSelected == value)
                    return;
                this._isSelected = value;
                #if NET40
                this.OnPropertyChanged("IsSelected");
                #else
                this.OnPropertyChanged();
                #endif
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #if NET40
        protected virtual void OnPropertyChanged(string propertyName)
        #else
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            #endif
        {
            var handler = this.PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}