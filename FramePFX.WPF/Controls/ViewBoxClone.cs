using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FramePFX.Utils;
using Rect = System.Windows.Rect;

namespace FramePFX.WPF.Controls
{
    public class ViewBoxClone : Decorator
    {
        public static readonly DependencyProperty StretchDirectionProperty =
            DependencyProperty.Register(
                nameof(StretchDirection),
                typeof(StretchDirection),
                typeof(ViewBoxClone),
                new FrameworkPropertyMetadata(StretchDirection.Both, FrameworkPropertyMetadataOptions.AffectsMeasure), ValidateStretchDirectionValue);

        private ContainerVisual _internalVisual;

        private ContainerVisual InternalVisual
        {
            get
            {
                if (this._internalVisual == null)
                {
                    this._internalVisual = new ContainerVisual();
                    this.AddVisualChild(this._internalVisual);
                }

                return this._internalVisual;
            }
        }

        private UIElement InternalChild
        {
            get
            {
                VisualCollection children = this.InternalVisual.Children;
                return children.Count != 0 ? children[0] as UIElement : null;
            }
            set
            {
                VisualCollection children = this.InternalVisual.Children;
                if (children.Count != 0)
                    children.Clear();
                children.Add(value);
            }
        }

        private Transform InternalTransform
        {
            get => this.InternalVisual.Transform;
            set => this.InternalVisual.Transform = value;
        }

        public override UIElement Child
        {
            get => this.InternalChild;
            set
            {
                UIElement internalChild = this.InternalChild;
                if (internalChild == value)
                    return;
                this.RemoveLogicalChild(internalChild);
                if (value != null)
                    this.AddLogicalChild(value);
                this.InternalChild = value;
                this.InvalidateMeasure();
            }
        }

        protected override int VisualChildrenCount => 1;

        protected override IEnumerator LogicalChildren => (this.InternalChild == null ? new List<object>() : new List<object>() {this.InternalChild}).GetEnumerator();

        public StretchDirection StretchDirection
        {
            get => (StretchDirection) this.GetValue(StretchDirectionProperty);
            set => this.SetValue(StretchDirectionProperty, value);
        }

        public ViewBoxClone()
        {
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index != 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "index out of range: " + index);
            return this.InternalVisual;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            UIElement internalChild = this.InternalChild;
            Size size = new Size();
            if (internalChild != null)
            {
                Size availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
                internalChild.Measure(availableSize);
                Size desiredSize = internalChild.DesiredSize;
                Size scaleFactor = ComputeScaleFactor(constraint, desiredSize, this.StretchDirection);
                size.Width = scaleFactor.Width * desiredSize.Width;
                size.Height = scaleFactor.Height * desiredSize.Height;
            }

            return size;
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            UIElement internalChild = this.InternalChild;
            if (internalChild != null)
            {
                Size desiredSize = internalChild.DesiredSize;
                Size scaleFactor = ComputeScaleFactor(arrangeSize, desiredSize, this.StretchDirection);
                this.InternalTransform = new ScaleTransform(scaleFactor.Width, scaleFactor.Height);
                internalChild.Arrange(new Rect(new Point(), internalChild.DesiredSize));
                arrangeSize.Width = scaleFactor.Width * desiredSize.Width;
                arrangeSize.Height = scaleFactor.Height * desiredSize.Height;
            }

            return arrangeSize;
        }

        public static Size ComputeScaleFactor(Size availableSize, Size contentSize, StretchDirection stretchDirection)
        {
            bool isWidthPositiveInfinite = double.IsPositiveInfinity(availableSize.Width);
            bool isHeightPositiveInfinite = double.IsPositiveInfinity(availableSize.Height);
            if (isWidthPositiveInfinite && isHeightPositiveInfinite)
            {
                return new Size(1, 1);
            }

            double width = Maths.Equals(contentSize.Width, 0) ? 0.0 : availableSize.Width / contentSize.Width;
            double height = Maths.Equals(contentSize.Height, 0) ? 0.0 : availableSize.Height / contentSize.Height;
            if (isWidthPositiveInfinite)
            {
                width = height;
            }
            else if (isHeightPositiveInfinite)
            {
                height = width;
            }
            else
            {
                double value = (width < height ? width : height);
                width = height = value;
            }

            switch (stretchDirection)
            {
                case StretchDirection.UpOnly:
                    if (width < 1.0)
                        width = 1.0;
                    if (height < 1.0)
                        height = 1.0;
                    break;
                case StretchDirection.DownOnly:
                    if (width > 1.0)
                        width = 1.0;
                    if (height > 1.0)
                        height = 1.0;

                    break;
            }

            return new Size(width, height);
        }

        private static bool ValidateStretchValue(object value)
        {
            Stretch stretch = (Stretch) value;
            switch (stretch)
            {
                case Stretch.None:
                case Stretch.Fill:
                case Stretch.Uniform:
                    return true;
                default: return stretch == Stretch.UniformToFill;
            }
        }

        private static bool ValidateStretchDirectionValue(object value)
        {
            StretchDirection stretchDirection = (StretchDirection) value;
            switch (stretchDirection)
            {
                case StretchDirection.DownOnly:
                case StretchDirection.Both:
                    return true;
                default: return stretchDirection == StretchDirection.UpOnly;
            }
        }
    }
}