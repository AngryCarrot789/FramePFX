using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace FrameControl.AttachedProperties {
    /// <summary>
    /// A class for implementing a "latching" <see cref="GridSplitter"/>, where a column has a minimum width, but once the
    /// grid splitter is dragged a certain threshold, it closes the column (by setting its width or height to 0)
    /// <para>
    /// This is the same behaviour seen in ableton live's device rack (on the bottom), and
    /// also double-clicking the grid splitters will close them
    /// </para>
    /// </summary>
    public static class GridLatchSplitter {
        public static readonly DependencyProperty MinimumSizeProperty =
            DependencyProperty.RegisterAttached(
                "MinimumSize",
                typeof(double),
                typeof(GridLatchSplitter),
                new FrameworkPropertyMetadata(100d));

        public static readonly DependencyProperty MaximumSizeProperty =
            DependencyProperty.RegisterAttached(
                "MaximumSize",
                typeof(double),
                typeof(GridLatchSplitter),
                new FrameworkPropertyMetadata(double.NaN));

        public static readonly DependencyProperty ThresholdSizeToCloseProperty =
            DependencyProperty.RegisterAttached(
                "ThresholdSizeToClose",
                typeof(double),
                typeof(GridLatchSplitter),
                new FrameworkPropertyMetadata(30d));

        public static readonly DependencyProperty ThresholdSizeToOpenProperty =
            DependencyProperty.RegisterAttached(
                "ThresholdSizeToOpen",
                typeof(double),
                typeof(GridLatchSplitter),
                new FrameworkPropertyMetadata(70d));

        public static readonly DependencyProperty ClosedSizeProperty =
            DependencyProperty.RegisterAttached(
                "ClosedSize",
                typeof(double),
                typeof(GridLatchSplitter),
                new FrameworkPropertyMetadata(0d));

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(GridLatchSplitter),
                new FrameworkPropertyMetadata(false, OnIsEnabledChanged));

        public static readonly DependencyProperty TargetColumnProperty =
            DependencyProperty.RegisterAttached(
                "TargetColumn",
                typeof(ColumnDefinition),
                typeof(GridLatchSplitter),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty TargetRowProperty =
            DependencyProperty.RegisterAttached(
                "TargetRow",
                typeof(RowDefinition),
                typeof(GridLatchSplitter),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.RegisterAttached(
                "IsOpen",
                typeof(bool),
                typeof(GridLatchSplitter),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsColumnOpenPropertyChanged));

        public static readonly DependencyProperty IsHorizontalCloseDirectionLeftProperty =
            DependencyProperty.RegisterAttached(
                "IsHorizontalCloseDirectionLeft",
                typeof(bool),
                typeof(GridLatchSplitter),
                new FrameworkPropertyMetadata(true));
        public static readonly DependencyProperty IsVerticalCloseDirectionDownProperty =
            DependencyProperty.RegisterAttached(
                "IsVerticalCloseDirectionDown",
                typeof(bool),
                typeof(GridLatchSplitter),
                new FrameworkPropertyMetadata(true));

        #region Getters and Setters

        /// <summary>
        /// Sets the minimum size of the column. Default value is 100 (pixels)
        /// </summary>
        public static void SetMinimumSize(DependencyObject element, double value) {
            element.SetValue(MinimumSizeProperty, value);
        }

        public static double GetMinimumSize(DependencyObject element) {
            return (double) element.GetValue(MinimumSizeProperty);
        }

        /// <summary>
        /// Sets the maximum size of the column. Default value is NaN, meaning it is auto-calculated based on the size of the parent and grid-splitter
        /// </summary>
        public static void SetMaximumSize(DependencyObject element, double value) {
            element.SetValue(MaximumSizeProperty, value);
        }

        public static double GetMaximumSize(DependencyObject element) {
            return (double) element.GetValue(MaximumSizeProperty);
        }

        /// <summary>
        /// Sets the threshold width for the column to "close" (where its width will be set <see cref="ClosedSizeProperty"/>).
        /// <para>
        /// Ideally, this should be slightly below or equal to half of <see cref="MinimumSizeProperty"/> and
        /// less than or equal to <see cref="ThresholdSizeToOpenProperty"/>
        /// </para>
        /// </summary>
        public static void SetThresholdSizeToClose(DependencyObject element, double value) {
            element.SetValue(ThresholdSizeToCloseProperty, value);
        }

        public static double GetThresholdSizeToClose(DependencyObject element) {
            return (double) element.GetValue(ThresholdSizeToCloseProperty);
        }

        /// <summary>
        /// Sets the threshold width for the column to "open" (where its width will be set <see cref="MinimumSizeProperty"/>).
        /// <para>
        /// Ideally, this should be slightly above or equal to half of <see cref="MinimumSizeProperty"/> and
        /// more than or equal to <see cref="ThresholdSizeToCloseProperty"/>
        /// </para>
        /// </summary>
        public static void SetThresholdSizeToOpen(DependencyObject element, double value) {
            element.SetValue(ThresholdSizeToOpenProperty, value);
        }

        public static double GetThresholdSizeToOpen(DependencyObject element) {
            return (double) element.GetValue(ThresholdSizeToOpenProperty);
        }

        public static void SetClosedSize(DependencyObject element, double value) {
            element.SetValue(ClosedSizeProperty, value);
        }

        public static double GetClosedSize(DependencyObject element) {
            return (double) element.GetValue(ClosedSizeProperty);
        }

        public static void SetIsEnabled(DependencyObject element, bool value) {
            element.SetValue(IsEnabledProperty, value);
        }

        public static bool GetIsEnabled(DependencyObject element) {
            return (bool) element.GetValue(IsEnabledProperty);
        }

        public static void SetTargetColumn(DependencyObject element, ColumnDefinition value) {
            element.SetValue(TargetColumnProperty, value);
        }

        public static ColumnDefinition GetTargetColumn(DependencyObject element) {
            return (ColumnDefinition) element.GetValue(TargetColumnProperty);
        }

        public static void SetTargetRow(DependencyObject element, RowDefinition value) {
            element.SetValue(TargetRowProperty, value);
        }

        public static RowDefinition GetTargetRow(DependencyObject element) {
            return (RowDefinition) element.GetValue(TargetRowProperty);
        }

        public static void SetIsHorizontalCloseDirectionLeft(DependencyObject element, bool value) {
            element.SetValue(IsHorizontalCloseDirectionLeftProperty, value);
        }

        public static bool GetIsHorizontalCloseDirectionLeft(DependencyObject element) {
            return (bool) element.GetValue(IsHorizontalCloseDirectionLeftProperty);
        }

        public static void SetIsVerticalCloseDirectionDown(DependencyObject element, bool value) {
            element.SetValue(IsVerticalCloseDirectionDownProperty, value);
        }

        public static bool GetIsVerticalCloseDirectionDown(DependencyObject element) {
            return (bool) element.GetValue(IsVerticalCloseDirectionDownProperty);
        }

        #endregion

        public static void SetIsOpen(DependencyObject element, bool value) {
            element.SetValue(IsOpenProperty, value);
        }

        public static bool GetIsOpen(DependencyObject element) {
            return (bool) element.GetValue(IsOpenProperty);
        }

        private static bool IsColumnMode(this GridSplitter splitter, out ColumnDefinition column) {
            if (splitter.ResizeDirection == GridResizeDirection.Columns || splitter.ResizeDirection == GridResizeDirection.Auto) {
                column = GetTargetColumn(splitter);
                return column != null;
            }

            column = null;
            return false;
        }

        private static bool IsRowMode(this GridSplitter splitter, out RowDefinition row) {
            if (splitter.ResizeDirection == GridResizeDirection.Rows || splitter.ResizeDirection == GridResizeDirection.Auto) {
                row = GetTargetRow(splitter);
                return row != null;
            }

            row = null;
            return false;
        }

        #region Property changed handlers

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is GridSplitter splitter) {
                splitter.DragDelta -= OnGridSplitterDragDelta;
                splitter.MouseDoubleClick -= OnGridSplitterDoubleClicked;
                splitter.Loaded -= OnGridSplitterLoaded;
                if ((bool) e.NewValue) {
                    splitter.DragDelta += OnGridSplitterDragDelta;
                    splitter.MouseDoubleClick += OnGridSplitterDoubleClicked;
                    splitter.Loaded += OnGridSplitterLoaded;
                }
            }
        }

        private static void OnIsColumnOpenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (e.NewValue == e.OldValue) {
                return;
            }

            if (d is GridSplitter splitter) {
                if (splitter.IsColumnMode(out ColumnDefinition col)) {
                    double min = GetMinimumSize(splitter);
                    if ((bool) e.NewValue) {
                        if (col.ActualWidth < min) {
                            // this usually shouldn't be false...
                            col.Width = new GridLength(min, GridUnitType.Pixel);
                        }
                    }
                    else {
                        col.Width = new GridLength(GetClosedSize(splitter), GridUnitType.Pixel);
                    }
                }
                else if (splitter.IsRowMode(out RowDefinition row)) {
                    double min = GetMinimumSize(splitter);
                    if ((bool) e.NewValue) {
                        if (row.ActualHeight < min) { // this usually shouldn't be false...
                            row.Height = new GridLength(min, GridUnitType.Pixel);
                        }
                    }
                    else {
                        row.Height = new GridLength(GetClosedSize(splitter), GridUnitType.Pixel);
                    }
                }
            }
        }

        private static void OnGridSplitterLoaded(object sender, RoutedEventArgs e) {
            if (sender is GridSplitter splitter) {
                if (splitter.IsColumnMode(out ColumnDefinition col)) {
                    // This is just to make sure the column isn't a star width, because it glitches when the max width is exceeded
                    col.Width = new GridLength(col.ActualWidth, GridUnitType.Pixel);
                    double min = GetMinimumSize(splitter);
                    double threshold = GetThresholdSizeToOpen(splitter);
                    if (col.ActualWidth < min) {
                        // if width is greater than threshold, then might as well open it
                        SetIsOpen(splitter, col.ActualWidth >= threshold);
                    }
                    else {
                        SetIsOpen(splitter, true);
                    }
                }
                else if (splitter.IsRowMode(out RowDefinition row)) {
                    // This is just to make sure the column isn't a star height, because it glitches when the max height is exceeded
                    row.Height = new GridLength(row.ActualHeight, GridUnitType.Pixel);
                    double min = GetMinimumSize(splitter);
                    double threshold = GetThresholdSizeToOpen(splitter);
                    if (row.ActualHeight < min) {
                        // if height is greater than threshold, then might as well open it
                        SetIsOpen(splitter, row.ActualHeight >= threshold);
                    }
                    else {
                        SetIsOpen(splitter, true);
                    }
                }
            }
        }

        private static void OnGridSplitterDragDelta(object sender, DragDeltaEventArgs e) {
            if (sender is GridSplitter splitter) {
                if (splitter.IsColumnMode(out ColumnDefinition col)) {
                    double targetWidth;
                    if (GetIsHorizontalCloseDirectionLeft(splitter)) {
                        targetWidth = col.ActualWidth + e.HorizontalChange;
                    }
                    else {
                        targetWidth = col.ActualWidth - e.HorizontalChange;
                    }

                    double minWidth = GetMinimumSize(splitter);
                    double closeThreshold = Math.Min(GetThresholdSizeToClose(splitter), minWidth);
                    double openThreshold = Math.Min(GetThresholdSizeToOpen(splitter), minWidth);
                    double closedSize = GetClosedSize(splitter);
                    if (targetWidth < minWidth) {
                        e.Handled = true;
                        if (targetWidth <= closeThreshold) {
                            // close the column
                            col.Width = new GridLength(closedSize, GridUnitType.Pixel);
                            if (GetIsOpen(splitter)) {
                                SetIsOpen(splitter, false);
                            }
                        }
                        else if (targetWidth >= openThreshold) {
                            // keep the size at the minWidth
                            col.Width = new GridLength(minWidth, GridUnitType.Pixel);
                            if (!GetIsOpen(splitter)) {
                                SetIsOpen(splitter, true);
                            }
                        }
                        else {
                            // set to it's current size; it won't change size at all
                            col.Width = new GridLength(col.ActualWidth, GridUnitType.Pixel);
                        }
                    }
                    else if (col.Parent is Grid grid) {
                        double maxWidth = GetMaximumSize(splitter);
                        if (double.IsNaN(maxWidth)) {
                            if (double.IsPositiveInfinity(maxWidth = grid.MaxWidth)) {
                                maxWidth = grid.ActualWidth;
                            }
                        }

                        double splitterWidth = splitter.ActualWidth + (GetIsHorizontalCloseDirectionLeft(splitter) ? splitter.Margin.Left : splitter.Margin.Right);
                        if ((targetWidth + splitterWidth) > maxWidth) {
                            col.Width = new GridLength(maxWidth - splitterWidth, GridUnitType.Pixel);
                        }
                    }
                }
                else if (splitter.IsRowMode(out RowDefinition row)) {
                    double targetHeight;
                    if (GetIsVerticalCloseDirectionDown(splitter)) {
                        targetHeight = row.ActualHeight - e.VerticalChange;
                    }
                    else {
                        targetHeight = row.ActualHeight + e.VerticalChange;
                    }

                    double minHeight = GetMinimumSize(splitter);
                    double closeThreshold = Math.Min(GetThresholdSizeToClose(splitter), minHeight);
                    double openThreshold = Math.Min(GetThresholdSizeToOpen(splitter), minHeight);
                    double closedSize = GetClosedSize(splitter);
                    if (targetHeight < minHeight) {
                        e.Handled = true;
                        if (targetHeight <= closeThreshold) {
                            // close the row
                            row.Height = new GridLength(closedSize, GridUnitType.Pixel);
                            if (GetIsOpen(splitter)) {
                                SetIsOpen(splitter, false);
                            }
                        }
                        else if (targetHeight >= openThreshold) {
                            // keep the size at the minHeight
                            row.Height = new GridLength(minHeight, GridUnitType.Pixel);
                            if (!GetIsOpen(splitter)) {
                                SetIsOpen(splitter, true);
                            }
                        }
                        else {
                            // set to it's current size; it won't change size at all
                            row.Height = new GridLength(row.ActualHeight, GridUnitType.Pixel);
                        }
                    }
                    else if (row.Parent is Grid grid) {
                        double maxHeight = GetMaximumSize(splitter);
                        if (double.IsNaN(maxHeight)) {
                            if (double.IsPositiveInfinity(maxHeight = grid.MaxHeight)) {
                                maxHeight = grid.ActualHeight;
                            }
                        }

                        double splitterHeight = splitter.ActualHeight + (GetIsVerticalCloseDirectionDown(splitter) ? splitter.Margin.Bottom : splitter.Margin.Top);
                        if ((targetHeight + splitterHeight) > maxHeight) {
                            row.Height = new GridLength(maxHeight - splitterHeight, GridUnitType.Pixel);
                        }
                    }
                }
            }
        }

        private static void OnGridSplitterDoubleClicked(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left && sender is GridSplitter splitter && splitter.IsMouseOver) {
                if (splitter.IsColumnMode(out ColumnDefinition column)) {
                    double minWidth = GetMinimumSize(splitter);
                    if (column.ActualWidth < minWidth) {
                        column.Width = new GridLength(minWidth, GridUnitType.Pixel);
                        SetIsOpen(splitter, true);
                    }
                    else {
                        double closedSize = GetClosedSize(splitter);
                        column.Width = new GridLength(closedSize, GridUnitType.Pixel);
                        SetIsOpen(splitter, false);
                    }

                    e.Handled = true;
                }
                else if (splitter.IsRowMode(out RowDefinition row)) {
                    double minHeight = GetMinimumSize(splitter);
                    if (row.ActualHeight < minHeight) {
                        row.Height = new GridLength(minHeight, GridUnitType.Pixel);
                        SetIsOpen(splitter, true);
                    }
                    else {
                        double closedSize = GetClosedSize(splitter);
                        row.Height = new GridLength(closedSize, GridUnitType.Pixel);
                        SetIsOpen(splitter, false);
                    }

                    e.Handled = true;
                }
            }
        }

        #endregion
    }
}
