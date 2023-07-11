using System.Windows;
using System.Windows.Controls;

namespace Dragablz.Dockablz {
    [TemplatePart(Name = FirstContentPresenterPartName, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = SecondContentPresenterPartName, Type = typeof(ContentPresenter))]
    public class Branch : Control {
        private const string FirstContentPresenterPartName = "PART_FirstContentPresenter";
        private const string SecondContentPresenterPartName = "PART_SecondContentPresenter";

        static Branch() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Branch), new FrameworkPropertyMetadata(typeof(Branch)));
        }

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            "Orientation", typeof(Orientation), typeof(Branch), new PropertyMetadata(default(Orientation)));

        public Orientation Orientation {
            get { return (Orientation) this.GetValue(OrientationProperty); }
            set { this.SetValue(OrientationProperty, value); }
        }

        public static readonly DependencyProperty FirstItemProperty = DependencyProperty.Register(
            "FirstItem", typeof(object), typeof(Branch), new PropertyMetadata(default(object)));

        public object FirstItem {
            get { return this.GetValue(FirstItemProperty); }
            set { this.SetValue(FirstItemProperty, value); }
        }

        public static readonly DependencyProperty FirstItemLengthProperty = DependencyProperty.Register(
            "FirstItemLength", typeof(GridLength), typeof(Branch), new FrameworkPropertyMetadata(new GridLength(0.49999, GridUnitType.Star), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public GridLength FirstItemLength {
            get { return (GridLength) this.GetValue(FirstItemLengthProperty); }
            set { this.SetValue(FirstItemLengthProperty, value); }
        }

        public static readonly DependencyProperty SecondItemProperty = DependencyProperty.Register(
            "SecondItem", typeof(object), typeof(Branch), new PropertyMetadata(default(object)));

        public object SecondItem {
            get { return this.GetValue(SecondItemProperty); }
            set { this.SetValue(SecondItemProperty, value); }
        }

        public static readonly DependencyProperty SecondItemLengthProperty = DependencyProperty.Register(
            "SecondItemLength", typeof(GridLength), typeof(Branch), new FrameworkPropertyMetadata(new GridLength(0.50001, GridUnitType.Star), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public GridLength SecondItemLength {
            get { return (GridLength) this.GetValue(SecondItemLengthProperty); }
            set { this.SetValue(SecondItemLengthProperty, value); }
        }

        /// <summary>
        /// Gets the proportional size of the first item, between 0 and 1, where 1 would represent the entire size of the branch.
        /// </summary>
        /// <returns></returns>
        public double GetFirstProportion() {
            return (1 / (this.FirstItemLength.Value + this.SecondItemLength.Value)) * this.FirstItemLength.Value;
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            this.FirstContentPresenter = this.GetTemplateChild(FirstContentPresenterPartName) as ContentPresenter;
            this.SecondContentPresenter = this.GetTemplateChild(SecondContentPresenterPartName) as ContentPresenter;
        }

        internal ContentPresenter FirstContentPresenter { get; private set; }
        internal ContentPresenter SecondContentPresenter { get; private set; }
    }
}