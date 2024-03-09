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
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Clips.Core;

namespace FramePFX.Editors.Controls.Timelines.Tracks.Clips
{
    public abstract class TimelineClipContent : Control
    {
        private static readonly Dictionary<Type, Func<TimelineClipContent>> Constructors = new Dictionary<Type, Func<TimelineClipContent>>();

        public TimelineClipControl ClipControl { get; private set; }

        public Clip Model => this.ClipControl?.Model;

        protected TimelineClipContent()
        {
        }

        static TimelineClipContent()
        {
            RegisterType(typeof(ImageVideoClip), () => new ImageClipContent());
        }

        public void Connect(TimelineClipControl owner)
        {
            this.ClipControl = owner;


            this.OnConnected();
        }

        public void Disconnect()
        {
            this.OnDisconnected();
            this.ClipControl = null;
        }

        protected virtual void OnConnected()
        {
        }

        protected virtual void OnDisconnected()
        {
        }

        public static void RegisterType<T>(Type trackType, Func<T> func) where T : TimelineClipContent
        {
            Constructors[trackType] = func;
        }

        public static TimelineClipContent NewInstance(Type clipType)
        {
            if (clipType == null)
            {
                throw new ArgumentNullException(nameof(clipType));
            }

            for (Type type = clipType; type != null; type = type.BaseType)
            {
                if (Constructors.TryGetValue(type, out Func<TimelineClipContent> func))
                {
                    return func();
                }
            }

            return null;
        }

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