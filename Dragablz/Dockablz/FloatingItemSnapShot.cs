using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Dragablz.Dockablz {
    /// <summary>
    /// experimentational.  might have to puish this back to mvvm only
    /// </summary>    
    internal class FloatingItemSnapShot {
        private readonly object _content;
        private readonly Rect _location;
        private readonly int _zIndex;
        private readonly WindowState _state;

        public FloatingItemSnapShot(object content, Rect location, int zIndex, WindowState state) {
            if (content == null)
                throw new ArgumentNullException("content");

            this._content = content;
            this._location = location;
            this._zIndex = zIndex;
            this._state = state;
        }

        public static FloatingItemSnapShot Take(DragablzItem dragablzItem) {
            if (dragablzItem == null)
                throw new ArgumentNullException("dragablzItem");

            return new FloatingItemSnapShot(
                dragablzItem.Content,
                new Rect(dragablzItem.X, dragablzItem.Y, dragablzItem.ActualWidth, dragablzItem.ActualHeight),
                Panel.GetZIndex(dragablzItem),
                Layout.GetFloatingItemState(dragablzItem));
        }

        public void Apply(DragablzItem dragablzItem) {
            if (dragablzItem == null)
                throw new ArgumentNullException("dragablzItem");

            dragablzItem.SetCurrentValue(DragablzItem.XProperty, this.Location.Left);
            dragablzItem.SetCurrentValue(DragablzItem.YProperty, this.Location.Top);
            dragablzItem.SetCurrentValue(FrameworkElement.WidthProperty, this.Location.Width);
            dragablzItem.SetCurrentValue(FrameworkElement.HeightProperty, this.Location.Height);
            Layout.SetFloatingItemState(dragablzItem, this.State);
            Panel.SetZIndex(dragablzItem, this.ZIndex);
        }

        public object Content {
            get { return this._content; }
        }

        public Rect Location {
            get { return this._location; }
        }

        public int ZIndex {
            get { return this._zIndex; }
        }

        public WindowState State {
            get { return this._state; }
        }
    }
}