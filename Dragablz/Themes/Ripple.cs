using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Dragablz.Themes {
    [TemplateVisualState(GroupName = "CommonStates", Name = TemplateStateNormal)]
    [TemplateVisualState(GroupName = "CommonStates", Name = TemplateStateMousePressed)]
    [TemplateVisualState(GroupName = "CommonStates", Name = TemplateStateMouseOut)]
    public class Ripple : ContentControl {
        public const string TemplateStateNormal = "Normal";
        public const string TemplateStateMousePressed = "MousePressed";
        public const string TemplateStateMouseOut = "MouseOut";

        private static readonly HashSet<Ripple> PressedInstances = new HashSet<Ripple>();

        static Ripple() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Ripple), new FrameworkPropertyMetadata(typeof(Ripple)));

            EventManager.RegisterClassHandler(typeof(Window), Mouse.PreviewMouseUpEvent, new MouseButtonEventHandler(MouseButtonEventHandler), true);
            EventManager.RegisterClassHandler(typeof(Window), Mouse.MouseMoveEvent, new MouseEventHandler(MouseMouveEventHandler), true);
            EventManager.RegisterClassHandler(typeof(UserControl), Mouse.PreviewMouseUpEvent, new MouseButtonEventHandler(MouseButtonEventHandler), true);
            EventManager.RegisterClassHandler(typeof(UserControl), Mouse.MouseMoveEvent, new MouseEventHandler(MouseMouveEventHandler), true);
        }

        public Ripple() {
            this.SizeChanged += this.OnSizeChanged;
        }

        private static void MouseButtonEventHandler(object sender, MouseButtonEventArgs e) {
            foreach (var ripple in PressedInstances) {
                // adjust the transition scale time according to the current animated scale
                var scaleTrans = ripple.Template.FindName("ScaleTransform", ripple) as ScaleTransform;
                if (scaleTrans != null) {
                    double currentScale = scaleTrans.ScaleX;
                    var newTime = TimeSpan.FromMilliseconds(300 * (1.0 - currentScale));

                    // change the scale animation according to the current scale
                    var scaleXKeyFrame = ripple.Template.FindName("MousePressedToNormalScaleXKeyFrame", ripple) as EasingDoubleKeyFrame;
                    if (scaleXKeyFrame != null) {
                        scaleXKeyFrame.KeyTime = KeyTime.FromTimeSpan(newTime);
                    }

                    var scaleYKeyFrame = ripple.Template.FindName("MousePressedToNormalScaleYKeyFrame", ripple) as EasingDoubleKeyFrame;
                    if (scaleYKeyFrame != null) {
                        scaleYKeyFrame.KeyTime = KeyTime.FromTimeSpan(newTime);
                    }
                }

                VisualStateManager.GoToState(ripple, TemplateStateNormal, true);
            }

            PressedInstances.Clear();
        }

        private static void MouseMouveEventHandler(object sender, MouseEventArgs e) {
            foreach (var ripple in PressedInstances.ToList()) {
                var relativePosition = Mouse.GetPosition(ripple);
                if (relativePosition.X < 0
                    || relativePosition.Y < 0
                    || relativePosition.X >= ripple.ActualWidth
                    || relativePosition.Y >= ripple.ActualHeight) {
                    VisualStateManager.GoToState(ripple, TemplateStateMouseOut, true);
                    PressedInstances.Remove(ripple);
                }
            }
        }

        public static readonly DependencyProperty FeedbackProperty = DependencyProperty.Register(
            "Feedback", typeof(Brush), typeof(Ripple), new PropertyMetadata(default(Brush)));

        public Brush Feedback {
            get { return (Brush) this.GetValue(FeedbackProperty); }
            set { this.SetValue(FeedbackProperty, value); }
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e) {
            var point = e.GetPosition(this);

            if (RippleAssist.GetIsCentered(this)) {
                var innerContent = (this.Content as FrameworkElement);

                if (innerContent != null) {
                    var position = innerContent.TransformToAncestor(this).Transform(new Point(0, 0));

                    this.RippleX = position.X + innerContent.ActualWidth / 2 - this.RippleSize / 2;
                    this.RippleY = position.Y + innerContent.ActualHeight / 2 - this.RippleSize / 2;
                }
                else {
                    this.RippleX = this.ActualWidth / 2 - this.RippleSize / 2;
                    this.RippleY = this.ActualHeight / 2 - this.RippleSize / 2;
                }
            }
            else {
                this.RippleX = point.X - this.RippleSize / 2;
                this.RippleY = point.Y - this.RippleSize / 2;
            }

            VisualStateManager.GoToState(this, TemplateStateNormal, false);
            VisualStateManager.GoToState(this, TemplateStateMousePressed, true);
            PressedInstances.Add(this);

            base.OnPreviewMouseLeftButtonDown(e);
        }

        private static readonly DependencyPropertyKey RippleSizePropertyKey =
            DependencyProperty.RegisterReadOnly(
                "RippleSize", typeof(double), typeof(Ripple),
                new PropertyMetadata(default(double)));

        public static readonly DependencyProperty RippleSizeProperty =
            RippleSizePropertyKey.DependencyProperty;

        public double RippleSize {
            get { return (double) this.GetValue(RippleSizeProperty); }
            private set { this.SetValue(RippleSizePropertyKey, value); }
        }

        private static readonly DependencyPropertyKey RippleXPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "RippleX", typeof(double), typeof(Ripple),
                new PropertyMetadata(default(double)));

        public static readonly DependencyProperty RippleXProperty =
            RippleXPropertyKey.DependencyProperty;

        public double RippleX {
            get { return (double) this.GetValue(RippleXProperty); }
            private set { this.SetValue(RippleXPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey RippleYPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "RippleY", typeof(double), typeof(Ripple),
                new PropertyMetadata(default(double)));

        public static readonly DependencyProperty RippleYProperty =
            RippleYPropertyKey.DependencyProperty;

        public double RippleY {
            get { return (double) this.GetValue(RippleYProperty); }
            private set { this.SetValue(RippleYPropertyKey, value); }
        }

        /// <summary>
        ///   The DependencyProperty for the RecognizesAccessKey property. 
        ///   Default Value: false 
        /// </summary> 
        public static readonly DependencyProperty RecognizesAccessKeyProperty =
            DependencyProperty.Register(
                "RecognizesAccessKey", typeof(bool), typeof(Ripple),
                new PropertyMetadata(default(bool)));

        /// <summary> 
        ///   Determine if Ripple should use AccessText in its style
        /// </summary> 
        public bool RecognizesAccessKey {
            get { return (bool) this.GetValue(RecognizesAccessKeyProperty); }
            set { this.SetValue(RecognizesAccessKeyProperty, value); }
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            VisualStateManager.GoToState(this, TemplateStateNormal, false);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs) {
            var innerContent = (this.Content as FrameworkElement);

            double width, height;

            if (RippleAssist.GetIsCentered(this) && innerContent != null) {
                width = innerContent.ActualWidth;
                height = innerContent.ActualHeight;
            }
            else {
                width = sizeChangedEventArgs.NewSize.Width;
                height = sizeChangedEventArgs.NewSize.Height;
            }

            var radius = Math.Sqrt(Math.Pow(width, 2) + Math.Pow(height, 2));

            this.RippleSize = 2 * radius * RippleAssist.GetRippleSizeMultiplier(this);
        }
    }
}