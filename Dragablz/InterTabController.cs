using System.Dynamic;
using System.Windows;

namespace Dragablz {
    public class InterTabController : FrameworkElement {
        public InterTabController() {
            this.HorizontalPopoutGrace = 8;
            this.VerticalPopoutGrace = 8;
            this.MoveWindowWithSolitaryTabs = true;
        }

        public static readonly DependencyProperty HorizontalPopoutGraceProperty = DependencyProperty.Register(
            "HorizontalPopoutGrace", typeof(double), typeof(InterTabController), new PropertyMetadata(8.0));

        public double HorizontalPopoutGrace {
            get { return (double) this.GetValue(HorizontalPopoutGraceProperty); }
            set { this.SetValue(HorizontalPopoutGraceProperty, value); }
        }

        public static readonly DependencyProperty VerticalPopoutGraceProperty = DependencyProperty.Register(
            "VerticalPopoutGrace", typeof(double), typeof(InterTabController), new PropertyMetadata(8.0));

        public double VerticalPopoutGrace {
            get { return (double) this.GetValue(VerticalPopoutGraceProperty); }
            set { this.SetValue(VerticalPopoutGraceProperty, value); }
        }

        public static readonly DependencyProperty MoveWindowWithSolitaryTabsProperty = DependencyProperty.Register(
            "MoveWindowWithSolitaryTabs", typeof(bool), typeof(InterTabController), new PropertyMetadata(true));

        public bool MoveWindowWithSolitaryTabs {
            get { return (bool) this.GetValue(MoveWindowWithSolitaryTabsProperty); }
            set { this.SetValue(MoveWindowWithSolitaryTabsProperty, value); }
        }

        public static readonly DependencyProperty InterTabClientProperty = DependencyProperty.Register(
            "InterTabClient", typeof(IInterTabClient), typeof(InterTabController),
            new PropertyMetadata(new DefaultInterTabClient()));

        public IInterTabClient InterTabClient {
            get { return (IInterTabClient) this.GetValue(InterTabClientProperty); }
            set { this.SetValue(InterTabClientProperty, value); }
        }

        /*
        public static readonly DependencyProperty PartitionProperty = DependencyProperty.Register(
            "Partition", typeof (object), typeof (InterTabController), new PropertyMetadata(default(object)));

        /// <summary>
        /// The partition allows on or more tab environments in a single application.  Only tabs which have a tab controller
        /// with a common partition will be allowed to have tabs dragged between them.  <c>null</c> is a valid partition (i.e global).
        /// </summary>
        public object Partition
        {
            get { return (object) GetValue(PartitionProperty); }
            set { SetValue(PartitionProperty, value); }
        }
         */

        /// <summary>
        /// The partition allows on or more tab environments in a single application.  Only tabs which have a tab controller
        /// with a common partition will be allowed to have tabs dragged between them.  <c>null</c> is a valid partition (i.e global).
        /// </summary>
        public string Partition { get; set; }
    }
}