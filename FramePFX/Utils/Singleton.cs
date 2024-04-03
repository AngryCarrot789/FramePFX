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
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System;

namespace FramePFX.Utils
{
    /// <summary>
    /// A class that manages a lazily-created singleton instance. This class is thread safe
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Singleton<T> where T : class
    {
        private readonly Func<T> constructor;
        private readonly object locker;
        private volatile T instance;

        public T Instance
        {
            get
            {
                T val = this.instance;
                if (val != null)
                    return val;

                lock (this.locker)
                    if ((val = this.instance) == null)
                        this.instance = val = this.constructor();

                return val;
            }
        }

        public Singleton(Func<T> constructor)
        {
            this.constructor = constructor ?? throw new ArgumentNullException(nameof(constructor));
            this.locker = new object();
        }
    }
}