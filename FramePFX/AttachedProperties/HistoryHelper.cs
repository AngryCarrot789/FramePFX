using System;
using System.Diagnostics;
using System.Windows;
using FramePFX.Controls.Dragger;
using FramePFX.Core.History;

namespace FramePFX.AttachedProperties
{
    /// <summary>
    /// A helper class for use with bound properties that affect the history (mainly for controls like sliders, number draggers,
    /// etc, where you only want to push the history change when a drag is stopped)
    /// </summary>
    public static class HistoryHelper
    {
        public static readonly DependencyProperty DragIdProperty = DependencyProperty.RegisterAttached("DragId", typeof(string), typeof(HistoryHelper), new PropertyMetadata(null, OnDragIdChanged));

        public static void SetDragId(DependencyObject element, string value)
        {
            element.SetValue(DragIdProperty, value);
        }

        public static string GetDragId(DependencyObject element)
        {
            return (string) element.GetValue(DragIdProperty);
        }

        private static void OnDragIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NumberDragger dragger)
            {
                dragger.EditStarted -= OnDraggerEditStarted;
                dragger.EditCompleted -= OnDraggerEditCompleted;
                if (e.NewValue != null)
                {
                    dragger.EditStarted += OnDraggerEditStarted;
                    dragger.EditCompleted += OnDraggerEditCompleted;
                    dragger.RestoreValueOnCancel = false;
                }
            }
            else
            {
                throw new Exception($"Type is not allowed: {d?.GetType().Name}");
            }
        }

        // When the properties are in invalid state, log to debugger, and do not change anything just in case...

        private static void OnDraggerEditStarted(object sender, EditStartEventArgs e)
        {
            if (!(sender is NumberDragger dragger))
            {
                throw new Exception("Wot.");
            }

            if (FrontEndHistoryHelper.ActiveDragId != null)
            {
                Debug.WriteLine($"Warning! {nameof(FrontEndHistoryHelper)}.{nameof(FrontEndHistoryHelper.ActiveDragId)} was still set as {FrontEndHistoryHelper.ActiveDragId}!");
                return;
            }

            if (FrontEndHistoryHelper.OnDragEnd != null)
            {
                Debug.WriteLine($"Warning! {nameof(FrontEndHistoryHelper)}.{nameof(FrontEndHistoryHelper.OnDragEnd)} was still set!");
                return;
            }

            if (dragger.GetValue(DragIdProperty) is string id)
            {
                FrontEndHistoryHelper.ActiveDragId = id;
            }
        }

        private static void OnDraggerEditCompleted(object sender, EditCompletedEventArgs e)
        {
            if (!(sender is NumberDragger dragger))
            {
                throw new Exception("Wot.");
            }

            if (FrontEndHistoryHelper.ActiveDragId == null)
            {
                return;
            }

            if (FrontEndHistoryHelper.OnDragEnd == null)
            {
                Debug.WriteLine($"Warning! {nameof(FrontEndHistoryHelper)}.{nameof(FrontEndHistoryHelper.OnDragEnd)} was not set! Something weird has happened");
                return;
            }

            if (dragger.GetValue(DragIdProperty) is string id)
            {
                if (FrontEndHistoryHelper.ActiveDragId != id)
                {
                    Debug.WriteLine($"Warning! {nameof(FrontEndHistoryHelper)}.{nameof(FrontEndHistoryHelper.ActiveDragId)} does not equal the Id of the dragger that completed its drag: {id}");
                    return;
                }

                try
                {
                    FrontEndHistoryHelper.OnDragEnd(id, e.IsCancelled);
                }
                finally
                {
                    FrontEndHistoryHelper.OnDragEnd = null;
                    FrontEndHistoryHelper.ActiveDragId = null;
                }
            }
        }
    }
}