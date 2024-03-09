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
using FramePFX.Editors.Exporting.FFMPEG;

namespace FramePFX.Editors.Exporting.Controls
{
    /// <summary>
    /// The content for a specific exporter config
    /// </summary>
    public abstract class ExporterContent : Control
    {
        private static readonly Dictionary<Type, Func<ExporterContent>> Constructors = new Dictionary<Type, Func<ExporterContent>>();

        public Exporter Exporter { get; private set; }

        protected ExporterContent()
        {
        }

        public void Connected(Exporter exporter)
        {
            this.Exporter = exporter;
            this.OnConnected();
        }

        public void Disconnected()
        {
            this.OnDisconnected();
            this.Exporter = null;
        }

        public abstract void OnConnected();

        public abstract void OnDisconnected();

        static ExporterContent()
        {
            RegisterType(typeof(FFmpegExporter), () => new FFmpegExporterContent());
        }

        public static void RegisterType<T>(Type trackType, Func<T> func) where T : ExporterContent
        {
            Constructors[trackType] = func;
        }

        public static ExporterContent NewInstance(Type exporterType)
        {
            if (exporterType == null)
            {
                throw new ArgumentNullException(nameof(exporterType));
            }

            // Just try to find a base control type. It should be found first try unless I forgot to register a new control type
            if (Constructors.TryGetValue(exporterType, out Func<ExporterContent> func))
            {
                return func();
            }

            Debugger.Break();
            throw new Exception("No such content control for export type: " + exporterType.Name);
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