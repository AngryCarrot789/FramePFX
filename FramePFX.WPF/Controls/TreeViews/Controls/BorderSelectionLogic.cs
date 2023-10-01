using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FramePFX.WPF.Controls.TreeViews.Controls
{
    internal class BorderSelectionLogic : IDisposable
    {
        #region Private fields

        private MultiSelectTreeView treeView;
        private readonly Border border;
        private readonly ScrollViewer scrollViewer;
        private readonly ItemsPresenter content;

        private bool isFirstMove;
        private bool mouseDown;
        private Point startPoint;
        private DateTime lastScrollTime;
        private HashSet<object> initialSelection;

        public static (int millis, int lines) AutoScrollData = (25, 2);

        #endregion Private fields

        #region Constructor

        public BorderSelectionLogic(MultiSelectTreeView treeView, Border selectionBorder, ScrollViewer scrollViewer, ItemsPresenter content)
        {
            this.treeView = treeView ?? throw new ArgumentNullException(nameof(treeView));
            this.border = selectionBorder ?? throw new ArgumentNullException(nameof(selectionBorder));
            this.scrollViewer = scrollViewer ?? throw new ArgumentNullException(nameof(scrollViewer));
            this.content = content ?? throw new ArgumentNullException(nameof(content));

            treeView.MouseDown += this.OnMouseDown;
            treeView.MouseMove += this.OnMouseMove;
            treeView.MouseUp += this.OnMouseUp;
            treeView.KeyDown += this.OnKeyDown;
            treeView.KeyUp += this.OnKeyUp;
        }

        #endregion Constructor

        #region Public methods

        public void Dispose()
        {
            if (this.treeView != null)
            {
                this.treeView.MouseDown -= this.OnMouseDown;
                this.treeView.MouseMove -= this.OnMouseMove;
                this.treeView.MouseUp -= this.OnMouseUp;
                this.treeView.KeyDown -= this.OnKeyDown;
                this.treeView.KeyUp -= this.OnKeyUp;
                this.treeView = null;
            }

            GC.SuppressFinalize(this);
        }

        #endregion Public methods

        #region Methods

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.mouseDown = true;
            this.startPoint = Mouse.GetPosition(this.content);

            // Debug.WriteLine("Initialize drwawing");
            this.isFirstMove = true;
            // Capture the mouse right now so that the MouseUp event will not be missed
            Mouse.Capture(this.treeView);

            IList selection = this.treeView.SelectedItems;
            this.initialSelection = selection == null ? new HashSet<object>() : new HashSet<object>(selection.Cast<object>());
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (this.mouseDown)
            {
                if (DateTime.UtcNow > this.lastScrollTime.AddMilliseconds(AutoScrollData.millis))
                {
                    Point currentPointWin = Mouse.GetPosition(this.scrollViewer);
                    if (currentPointWin.Y < 16)
                    {
                        for (int i = AutoScrollData.lines; i > 0; i--)
                            this.scrollViewer.LineUp();
                        this.scrollViewer.UpdateLayout();
                        this.lastScrollTime = DateTime.UtcNow;
                    }

                    if (currentPointWin.Y > this.scrollViewer.ActualHeight - 16)
                    {
                        for (int i = AutoScrollData.lines; i > 0; i--)
                            this.scrollViewer.LineDown();
                        this.scrollViewer.UpdateLayout();
                        this.lastScrollTime = DateTime.UtcNow;
                    }
                }

                Point currentPoint = Mouse.GetPosition(this.content);
                double width = currentPoint.X - this.startPoint.X + 1;
                double height = currentPoint.Y - this.startPoint.Y + 1;
                double left = this.startPoint.X;
                double top = this.startPoint.Y;

                if (this.isFirstMove)
                {
                    if (Math.Abs(width) <= SystemParameters.MinimumHorizontalDragDistance && Math.Abs(height) <= SystemParameters.MinimumVerticalDragDistance)
                    {
                        return;
                    }

                    this.isFirstMove = false;
                    if (!SelectionMultiple.IsControlKeyDown)
                    {
                        if (!this.treeView.ClearSelectionByRectangle())
                        {
                            this.EndAction();
                            return;
                        }
                    }
                }

                // Debug.WriteLine(string.Format("Drawing: {0};{1};{2};{3}",startPoint.X,startPoint.Y,width,height));
                if (width < 1)
                {
                    width = Math.Abs(width - 1) + 1;
                    left = this.startPoint.X - width + 1;
                }

                if (height < 1)
                {
                    height = Math.Abs(height - 1) + 1;
                    top = this.startPoint.Y - height + 1;
                }

                this.border.Width = width;
                Canvas.SetLeft(this.border, left);
                this.border.Height = height;
                Canvas.SetTop(this.border, top);

                this.border.Visibility = Visibility.Visible;

                double right = left + width - 1;
                double bottom = top + height - 1;

                // Debug.WriteLine(string.Format("left:{1};right:{2};top:{3};bottom:{4}", null, left, right, top, bottom));
                SelectionMultiple selection = (SelectionMultiple) this.treeView.Selection;
                bool foundFocusItem = false;

                IList selectedItems = this.treeView.SelectedItems;
                foreach (MultiSelectTreeViewItem item in MultiSelectTreeView.GetEntireTreeRecursive(this.treeView, false, false))
                {
                    FrameworkElement itemContent = (FrameworkElement) item.Template.FindName("headerBorder", item);
                    Point p = itemContent.TransformToAncestor(this.content).Transform(new Point());
                    double itemLeft = p.X;
                    double itemRight = p.X + itemContent.ActualWidth - 1;
                    double itemTop = p.Y;
                    double itemBottom = p.Y + itemContent.ActualHeight - 1;

                    // Debug.WriteLine(string.Format("element:{0};itemleft:{1};itemright:{2};itemtop:{3};itembottom:{4}",item.DataContext,itemLeft,itemRight,itemTop,itemBottom));

                    // Compute the current input states for determining the new selection state of the item
                    bool intersect = !(itemLeft > right || itemRight < left || itemTop > bottom || itemBottom < top);
                    bool initialSelected = this.initialSelection != null && this.initialSelection.Contains(item.DataContext);
                    bool ctrl = SelectionMultiple.IsControlKeyDown;

                    // Decision matrix:
                    // If the Ctrl key is pressed, each intersected item will be toggled from its initial selection.
                    // Without the Ctrl key, each intersected item is selected, others are deselected.
                    //
                    // newSelected
                    // ─────────┬───────────────────────
                    //          │ intersect
                    //          │  0        │  1
                    //          ├───────────┴───────────
                    //          │ initial
                    //          │  0  │  1  │  0  │  1
                    // ─────────┼─────┼─────┼─────┼─────
                    // ctrl  0  │  0  │  0  │  1  │  1   = intersect
                    // ─────────┼─────┼─────┼─────┼─────
                    //       1  │  0  │  1  │  1  │  0   = intersect XOR initial
                    //
                    bool newSelected = intersect ^ (initialSelected && ctrl);

                    // The new selection state for this item has been determined. Apply it.
                    if (newSelected)
                    {
                        // The item shall be selected
                        if (selectedItems == null || !selectedItems.Contains(item.DataContext))
                        {
                            // The item is not currently selected. Try to select it.
                            if (!selection.SelectByRectangle(item))
                            {
                                if (selection.LastCancelAll)
                                {
                                    this.EndAction();
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        // The item shall be deselected
                        if (selectedItems != null && selectedItems.Contains(item.DataContext))
                        {
                            // The item is currently selected. Try to deselect it.
                            if (!selection.DeselectByRectangle(item))
                            {
                                if (selection.LastCancelAll)
                                {
                                    this.EndAction();
                                    return;
                                }
                            }
                        }
                    }

                    // Always focus and bring into view the item under the mouse cursor
                    if (!foundFocusItem &&
                        currentPoint.X >= itemLeft && currentPoint.X <= itemRight &&
                        currentPoint.Y >= itemTop && currentPoint.Y <= itemBottom)
                    {
                        FocusHelper.Focus(item, true);
                        this.scrollViewer.UpdateLayout();
                        foundFocusItem = true;
                    }
                }

                if (e != null)
                {
                    e.Handled = true;
                }
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            this.EndAction();

            // Clear selection if this was a non-ctrl click outside of any item (i.e. in the background)
            Point currentPoint = e.GetPosition(this.content);
            double width = currentPoint.X - this.startPoint.X + 1;
            double height = currentPoint.Y - this.startPoint.Y + 1;
            if (Math.Abs(width) <= SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(height) <= SystemParameters.MinimumVerticalDragDistance &&
                !SelectionMultiple.IsControlKeyDown)
            {
                this.treeView.ClearSelection();
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            // The mouse move handler reads the Ctrl key so is dependent on it.
            // If the key state has changed, the selection needs to be updated.
            this.OnMouseMove(null, null);
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            // The mouse move handler reads the Ctrl key so is dependent on it.
            // If the key state has changed, the selection needs to be updated.
            this.OnMouseMove(null, null);
        }

        private void EndAction()
        {
            Mouse.Capture(null);
            this.mouseDown = false;
            this.border.Visibility = Visibility.Collapsed;
            this.initialSelection = null;

            // Debug.WriteLine("End drawing");
        }

        #endregion Methods
    }
}