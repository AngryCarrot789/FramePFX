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
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using FramePFX.Editors.PropertyEditors.AControls;
using FramePFX.Editors.PropertyEditors.Clips;
using FramePFX.PropertyEditing.Automation;
using FramePFX.PropertyEditing.Controls.Automation;
using FramePFX.PropertyEditing.Controls.DataTransfer;
using FramePFX.PropertyEditing.DataTransfer;

namespace FramePFX.PropertyEditing.Controls
{
    public abstract class BasePropEditControlContent : Control
    {
        private static readonly Dictionary<Type, Func<BasePropEditControlContent>> Constructors;

        public PropertyEditorSlotControl SlotControl { get; private set; }

        public PropertyEditorSlot SlotModel => this.SlotControl.Model;

        protected BasePropEditControlContent()
        {
        }

        static BasePropEditControlContent()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BasePropEditControlContent), new FrameworkPropertyMetadata(typeof(BasePropEditControlContent)));
            Constructors = new Dictionary<Type, Func<BasePropEditControlContent>>();
            // specific case editors
            RegisterType(typeof(DisplayNamePropertyEditorSlot), () => new DisplayNamePropertyEditorControl());
            RegisterType(typeof(VideoClipMediaFrameOffsetPropertyEditorSlot), () => new VideoClipMediaFrameOffsetPropertyEditorControl());
            RegisterType(typeof(TimecodeFontFamilyPropertyEditorSlot), () => new TimecodeFontFamilyPropertyEditorControl());

            // standard editors
            RegisterType(typeof(ParameterLongPropertyEditorSlot), () => new ParameterLongPropertyEditorControl());
            RegisterType(typeof(ParameterDoublePropertyEditorSlot), () => new ParameterDoublePropertyEditorControl());
            RegisterType(typeof(ParameterFloatPropertyEditorSlot), () => new ParameterFloatPropertyEditorControl());
            RegisterType(typeof(ParameterBooleanPropertyEditorSlot), () => new ParameterBooleanPropertyEditorControl());
            RegisterType(typeof(ParameterVector2PropertyEditorSlot), () => new ParameterVector2PropertyEditorControl());

            RegisterType(typeof(DataParameterLongPropertyEditorSlot), () => new DataParameterLongPropertyEditorControl());
            RegisterType(typeof(DataParameterDoublePropertyEditorSlot), () => new DataParameterDoublePropertyEditorControl());
            RegisterType(typeof(DataParameterFloatPropertyEditorSlot), () => new DataParameterFloatPropertyEditorControl());
            RegisterType(typeof(DataParameterBooleanPropertyEditorSlot), () => new DataParameterBooleanPropertyEditorControl());
            RegisterType(typeof(DataParameterStringPropertyEditorSlot), () => new DataParameterStringPropertyEditorControl());
        }

        public static void RegisterType<T>(Type slotType, Func<T> func) where T : BasePropEditControlContent
        {
            if (!typeof(PropertyEditorSlot).IsAssignableFrom(slotType))
                throw new ArgumentException("Slot type is invalid: cannot assign " + slotType + " to " + typeof(PropertyEditorSlot));
            Constructors.Add(slotType, func);
        }

        public static BasePropEditControlContent NewContentInstance(Type slotType)
        {
            if (slotType == null)
            {
                throw new ArgumentNullException(nameof(slotType));
            }

            // Just try to find a base control type. It should be found first try unless I forgot to register a new control type
            bool hasLogged = false;
            for (Type type = slotType; type != null; type = type.BaseType)
            {
                if (Constructors.TryGetValue(type, out Func<BasePropEditControlContent> func))
                {
                    return func();
                }

                if (!hasLogged)
                {
                    hasLogged = true;
                    Debugger.Break();
                    Debug.WriteLine("Could not find control for track type on first try. Scanning base types");
                }
            }

            throw new Exception("No such content control for track type: " + slotType.Name);
        }

        public void Connect(PropertyEditorSlotControl slot)
        {
            this.SlotControl = slot;
            this.OnConnected();
        }

        public void Disconnect()
        {
            this.OnDisconnected();
            this.SlotControl = null;
        }

        protected abstract void OnConnected();

        protected abstract void OnDisconnected();

        protected void GetTemplateChild<T>(string name, out T value) where T : DependencyObject
        {
            if ((value = this.GetTemplateChild(name) as T) == null)
                throw new Exception("Missing part: " + name + " of type " + typeof(T));
        }

        protected T GetTemplateChild<T>(string name) where T : DependencyObject
        {
            this.GetTemplateChild(name, out T value);
            return value;
        }

        protected bool TryGetTemplateChild<T>(string name, out T value) where T : DependencyObject
        {
            return (value = this.GetTemplateChild(name) as T) != null;
        }
    }
}