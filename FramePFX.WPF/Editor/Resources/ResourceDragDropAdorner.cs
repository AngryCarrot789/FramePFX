using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using FramePFX.Interactivity;

namespace FramePFX.WPF.Editor.Resources {
    public class ResourceDragDropAdorner : Adorner {
        private FormattedText text;
        private EnumDropType lastDropType;

        private static readonly FormattedText NoDropType;
        private static readonly FormattedText CopyDropType;
        private static readonly FormattedText MoveDropType;
        private static readonly FormattedText LinkDropType;
        private static readonly FormattedText[] Types;

        static ResourceDragDropAdorner() {
            Typeface typeFace = new Typeface("Segoe UI");
            NoDropType = new FormattedText("Invalid Drop Action", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeFace, 16, Brushes.White, 96);
            CopyDropType = new FormattedText("Copy resource", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeFace, 16, Brushes.White, 96);
            MoveDropType = new FormattedText("Move resource", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeFace, 16, Brushes.White, 96);
            LinkDropType = new FormattedText("Link resource to target", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeFace, 16, Brushes.White, 96);
            Types = new []{NoDropType, CopyDropType, MoveDropType, LinkDropType};
        }

        public ResourceDragDropAdorner(UIElement adornedElement) : base(adornedElement) {
            this.text = NoDropType;
            this.IsHitTestVisible = false;
            this.Width = Types.Max(x => x.Width) + 20;
            this.Height = Types.Max(x => x.Height) + 20;
        }

        public bool OnDropType(EnumDropType type) {
            if (this.lastDropType == type) {
                return false;
            }

            if ((type & EnumDropType.Move) != 0) {
                this.text = MoveDropType;
            }
            else if ((type & EnumDropType.Copy) != 0) {
                this.text = CopyDropType;
            }
            else if ((type & EnumDropType.Link) != 0) {
                this.text = LinkDropType;
            }
            else {
                this.text = NoDropType;
            }

            this.lastDropType = type;
            return true;
        }

        protected override void OnRender(DrawingContext dc) {
            base.OnRender(dc);
            Size size = this.RenderSize;
            dc.PushOpacity(0.2);
            dc.DrawRectangle(Brushes.DimGray, null, new Rect(new Point(), this.RenderSize));
            dc.Pop();
            dc.DrawText(this.text, new Point(size.Width / 2d - this.text.Width / 2d, size.Height / 2d - this.text.Height / 2d));
        }
    }
}