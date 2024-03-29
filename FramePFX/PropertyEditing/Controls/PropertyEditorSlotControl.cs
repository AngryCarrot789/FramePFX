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

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using FramePFX.Editors.Controls.Bindings;
using FramePFX.Utils;

namespace FramePFX.PropertyEditing.Controls
{
    public class PropertyEditorSlotControl : ContentControl
    {
        public static readonly DependencyProperty IsSelectedProperty =
            Selector.IsSelectedProperty.AddOwner(
                typeof(PropertyEditorSlotControl),
                new FrameworkPropertyMetadata(
                    BoolBox.False,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                    (d, e) => ((PropertyEditorSlotControl) d).OnSelectionChanged((bool) e.OldValue, (bool) e.NewValue),
                    (o, value) => ((PropertyEditorSlotControl) o).IsSelectable ? value : BoolBox.False));

        public static readonly DependencyProperty IsSelectableProperty =
            DependencyProperty.Register(
                "IsSelectable",
                typeof(bool),
                typeof(PropertyEditorSlotControl),
                new PropertyMetadata(BoolBox.False, (o, e) => ((PropertyEditorSlotControl) o).CoerceValue(IsSelectedProperty)));

        /// <summary>
        /// Whether or not this slot is selected. Setting this property automatically affects
        /// our <see cref="PropertyEditing.PropertyEditor"/>'s selected items
        /// </summary>
        [Category("Appearance")]
        public bool IsSelected
        {
            get => (bool) this.GetValue(IsSelectedProperty);
            set => this.SetValue(IsSelectedProperty, value.Box());
        }

        [Category("Appearance")]
        public bool IsSelectable
        {
            get => (bool) this.GetValue(IsSelectableProperty);
            set => this.SetValue(IsSelectableProperty, value.Box());
        }

        public PropertyEditorSlot Model { get; private set; }

        public PropertyEditorGroupControl OwnerGroup { get; private set; }

        private readonly GetSetAutoEventPropertyBinder<PropertyEditorSlot> isSelectedBinder = new GetSetAutoEventPropertyBinder<PropertyEditorSlot>(IsSelectedProperty, nameof(PropertyEditorSlot.IsSelectedChanged), b => b.Model.IsSelected.Box(), (b, v) => b.Model.IsSelected = (bool) v);

        public PropertyEditorSlotControl()
        {
        }

        static PropertyEditorSlotControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyEditorSlotControl), new FrameworkPropertyMetadata(typeof(PropertyEditorSlotControl)));
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            if (!e.Handled && this.IsSelectable)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
                {
                    this.IsSelected = !this.IsSelected;
                }
                else if (this.Model.PropertyEditor is BasePropertyEditor editor)
                {
                    editor.ClearSelection();
                    this.IsSelected = true;
                }
                else
                {
                    return;
                }

                if (this.OwnerGroup?.PropertyEditor is PropertyEditorControl editorControl)
                {
                    editorControl.TouchedSlot = this;
                }

                // if (!(e.OriginalSource is UIElement element)) {
                //     e.Handled = true;
                //     return;
                // }
                //
                // if (CanHandleClick(element, element is FrameworkElement fe ? (fe.TemplatedParent as Control) : null)) {
                //     e.Handled = true;
                // }
            }
        }

        private static bool CanHandleClick(UIElement originalSource, Control templatedParent)
        {
            if (originalSource.Focusable || templatedParent != null && templatedParent.Focusable)
            {
                return false;
            }

            if (originalSource is TextBoxBase || originalSource.GetType().Name == "TextBoxView")
            {
                return false;
            }

            return true;
        }

        public void OnAdding(PropertyEditorGroupControl ownerGroup, PropertyEditorSlot item)
        {
            BasePropEditControlContent content = BasePropEditControlContent.NewContentInstance(item.GetType());
            this.Model = item;
            this.OwnerGroup = ownerGroup;
            this.Content = content;
        }

        public void ConnectModel()
        {
            this.IsSelectable = this.Model.IsSelectable;
            this.isSelectedBinder.Attach(this, this.Model);
            this.Model.IsCurrentlyApplicableChanged += this.Model_IsCurrentlyApplicableChanged;

            BasePropEditControlContent content = (BasePropEditControlContent) this.Content;
            content.InvalidateMeasure();
            content.ApplyTemplate();
            content.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            content.Connect(this);
            content.InvalidateMeasure();
            this.UpdateVisibility();
            // content.UpdateLayout();
        }

        public void DisconnectModel()
        {
            ((BasePropEditControlContent) this.Content).Disconnect();
            this.isSelectedBinder.Detach();
            this.UpdateVisibility();
            this.Model = null;
            this.OwnerGroup = null;
        }

        private void OnSelectionChanged(bool oldValue, bool newValue)
        {
        }

        private void Model_IsCurrentlyApplicableChanged(BasePropertyEditorItem sender)
        {
            this.UpdateVisibility();
        }

        protected virtual void UpdateVisibility()
        {
            if (this.Model.IsVisible)
            {
                if (this.Visibility != Visibility.Visible)
                {
                    this.Visibility = Visibility.Visible;
                }
            }
            else if (this.Visibility != Visibility.Collapsed)
            {
                this.Visibility = Visibility.Collapsed;
            }
        }
    }
}