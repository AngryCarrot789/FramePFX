//
// Copyright (c) 2023-2024 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace FramePFX.Editors.Controls
{
    /// <summary>
    /// A control which sits as the middle man between
    /// </summary>
    public class GridSlotAutoSizer : Decorator
    {
        public static readonly DependencyProperty ColumnDefinitionProperty = DependencyProperty.Register("ColumnDefinition", typeof(ColumnDefinition), typeof(GridSlotAutoSizer), new PropertyMetadata(null, OnColumnDefinitionChanged));

        private static readonly DependencyProperty AutoSizerListProperty = DependencyProperty.RegisterAttached("AutoSizerList", typeof(List<GridSlotAutoSizer>), typeof(GridSlotAutoSizer), new PropertyMetadata(null));

        private static void SetAutoSizerList(ColumnDefinition element, List<GridSlotAutoSizer> value)
        {
            element.SetValue(AutoSizerListProperty, value);
        }

        private static List<GridSlotAutoSizer> GetAutoSizerList(ColumnDefinition element)
        {
            return (List<GridSlotAutoSizer>) element.GetValue(AutoSizerListProperty);
        }

        public ColumnDefinition ColumnDefinition
        {
            get => (ColumnDefinition) this.GetValue(ColumnDefinitionProperty);
            set => this.SetValue(ColumnDefinitionProperty, value);
        }

        public GridSlotAutoSizer()
        {
        }

        private static void OnColumnDefinitionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GridSlotAutoSizer control = (GridSlotAutoSizer) d;
            if (e.OldValue is ColumnDefinition oldDefinition)
            {
                List<GridSlotAutoSizer> list = GetAutoSizerList(oldDefinition);
                if (list != null)
                {
                    list.Remove(control);
                }
            }

            if (e.NewValue is ColumnDefinition newDefinition)
            {
                List<GridSlotAutoSizer> list = GetAutoSizerList(newDefinition);
                if (list == null)
                {
                    SetAutoSizerList(newDefinition, list = new List<GridSlotAutoSizer>());
                    list.Add(control);
                }
                else if (!list.Contains(control))
                {
                    list.Add(control);
                }
            }
        }

        protected override void OnChildDesiredSizeChanged(UIElement child)
        {
            base.OnChildDesiredSizeChanged(child);
            if (this.ColumnDefinition is ColumnDefinition definition)
            {
                List<GridSlotAutoSizer> list = GetAutoSizerList(definition);
                if (list != null && list.Count > 0)
                {
                    double max = 0;
                    foreach (GridSlotAutoSizer controls in list)
                    {
                        max = Math.Max(controls.DesiredSize.Width, max);
                    }

                    definition.MinWidth = max;
                    definition.Width = new GridLength(max, GridUnitType.Pixel);
                }
                else
                {
                    definition.MinWidth = child.DesiredSize.Width;
                }
            }
        }
    }
}