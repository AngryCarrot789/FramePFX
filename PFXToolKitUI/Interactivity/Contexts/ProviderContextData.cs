// 
// Copyright (c) 2024-2024 REghZy
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

using System.Diagnostics.CodeAnalysis;

namespace PFXToolKitUI.Interactivity.Contexts;

/// <summary>
/// A context data object that stores value provider functions instead of direct objects
/// </summary>
public class ProviderContextData : IContextData, IRandomAccessContextData {
    private Dictionary<string, ObjectProvider>? myData;

    public IEnumerable<KeyValuePair<string, object>> Entries {
        get {
            if (this.myData == null)
                yield break;

            foreach (KeyValuePair<string, ObjectProvider> entry in this.myData) {
                object? value = entry.Value.ProvideValue();
                if (value != null)
                    yield return new KeyValuePair<string, object>(entry.Key, value);
            }
        }
    }

    public int Count => this.myData?.Count ?? 0;

    public ProviderContextData() {
    }

    public void SetValue<T>(DataKey<T> key, T? value) => this.SetValueRaw(key.Id, value);

    public void SetValueRaw(DataKey key, object value) => this.SetValueRaw(key.Id, value);

    public void SetValueRaw(string key, object? value) => this.SetProviderImpl(key, ObjectProvider.ForValue(value));

    public void SetProvider<T>(DataKey<T> key, Func<T> provider) => this.SetProviderRaw(key.Id, () => provider()!);

    public void SetProviderRaw(DataKey key, Func<object> provider) => this.SetProviderRaw(key.Id, provider);

    public void SetProviderRaw(string key, Func<object> provider) => this.SetProviderImpl(key, ObjectProvider.ForProvider(provider));

    private void SetProviderImpl(string key, ObjectProvider provider) {
        (this.myData ??= new Dictionary<string, ObjectProvider>())[key] = provider;
    }

    public bool TryGetContext(string key, [NotNullWhen(true)] out object? value) {
        if (this.myData != null && this.myData.TryGetValue(key, out ObjectProvider provider)) {
            return (value = provider.ProvideValue()) != null;
        }

        value = null;
        return false;
    }

    public bool ContainsKey(string key) => this.TryGetContext(key, out _);

    public IContextData Clone() {
        return new ProviderContextData() {
            myData = this.myData != null ? new Dictionary<string, ObjectProvider>(this.myData) : null
        };
    }

    public void Merge(IContextData ctx) {
        Dictionary<string, ObjectProvider>? myMap;
        if (ctx is ProviderContextData provider) {
            if (provider.myData != null) {
                if ((myMap = this.myData) == null) {
                    this.myData = new Dictionary<string, ObjectProvider>(provider.myData);
                }
                else {
                    foreach (KeyValuePair<string, ObjectProvider> entry in provider.myData) {
                        myMap[entry.Key] = entry.Value;
                    }
                }
            }
        }
        else if (!(ctx is EmptyContext)) {
            using IEnumerator<KeyValuePair<string, object>> enumerator = ctx.Entries.GetEnumerator();
            if (!enumerator.MoveNext())
                return;

            myMap = this.myData ??= new Dictionary<string, ObjectProvider>();
            do {
                KeyValuePair<string, object> entry = enumerator.Current;
                myMap[entry.Key] = ObjectProvider.ForValue(entry.Value);
            } while (enumerator.MoveNext());
        }
    }

    private struct ObjectProvider {
        private int type;
        private object? value;

        public static ObjectProvider ForValue(object? value) => new ObjectProvider() {
            type = 1, value = value
        };

        public static ObjectProvider ForProvider(Func<object> provider) => new ObjectProvider() {
            type = 2, value = provider
        };

        public object? ProvideValue() {
            switch (this.type) {
                case 1:  return this.value;
                case 2:  return ((Func<object>) this.value!)();
                default: throw new Exception("Invalid object provider");
            }
        }
    }
}