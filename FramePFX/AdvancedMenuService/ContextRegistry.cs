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

using FramePFX.Interactivity.Contexts;

namespace FramePFX.AdvancedMenuService;

public delegate void ContextRegistryEventHandler(ContextRegistry registry);

public delegate void ContextRegistryContextEventHandler(ContextRegistry registry, IContextData context);

/// <summary>
/// A class which stores a context entry hierarchy for use in context menus.
/// <para>
/// The idea is that there are known identifiable "groups" for certain actions,
/// so that when plugins are available they can inject their own commands
/// into the right group... hopefully
/// </para>
/// </summary>
public class ContextRegistry {
    private readonly Dictionary<int, Dictionary<string, IContextGroup>> groups;
    private string? caption;

    /// <summary>
    /// Gets the groups in our registry
    /// </summary>
    public IEnumerable<KeyValuePair<string, IContextGroup>> Groups => this.groups.OrderBy(x => x.Key).Select(x => x.Value).SelectMany(x => x);

    /// <summary>
    /// Gets or sets this registry's caption
    /// </summary>
    public string? Caption {
        get => this.caption;
        set {
            if (this.caption == value)
                return;

            this.caption = value;
            this.CaptionChanged?.Invoke(this);
        }
    }

    public bool IsOpened { get; private set; }

    public event ContextRegistryEventHandler? CaptionChanged;
    public event ContextRegistryContextEventHandler? Opened;
    public event ContextRegistryEventHandler? Closed;

    public ContextRegistry(string caption) {
        this.groups = new Dictionary<int, Dictionary<string, IContextGroup>>();
        this.Caption = caption;
    }

    public void OnOpened(IContextData context) {
        if (this.IsOpened)
            throw new InvalidOperationException("Already open");

        this.IsOpened = true;
        this.Opened?.Invoke(this, context ?? EmptyContext.Instance);
    }

    public void OnClosed() {
        if (!this.IsOpened)
            throw new InvalidOperationException("Not opened");

        this.IsOpened = false;
        this.Closed?.Invoke(this);
    }

    public FixedContextGroup GetFixedGroup(string name, int weight = 0) {
        if (!this.GetDictionary(weight).TryGetValue(name, out IContextGroup? group))
            this.SetDictionary(weight, name, group = new FixedContextGroup());
        else if (!(group is FixedContextGroup))
            throw new InvalidOperationException("Context group is not fixed: " + name);
        return (FixedContextGroup) group;
    }

    public DynamicContextGroup CreateDynamicGroup(string name, DynamicGenerateContextFunction generate, int weight = 0) {
        if (!this.GetDictionary(weight).TryGetValue(name, out IContextGroup? group))
            this.SetDictionary(weight, name, group = new DynamicContextGroup(generate));
        else if (!(group is DynamicContextGroup))
            throw new InvalidOperationException("Context group is not dynamic: " + name);
        return (DynamicContextGroup) group;
    }

    private Dictionary<string, IContextGroup> GetDictionary(int weight) {
        if (!this.groups.TryGetValue(weight, out Dictionary<string, IContextGroup>? dict))
            this.groups[weight] = dict = new Dictionary<string, IContextGroup>();
        return dict;
    }

    private void SetDictionary(int weight, string name, IContextGroup group) {
        this.GetDictionary(weight)[name] = group;
    }

    public static void AddStaticOnlineStateHandlers(ContextRegistry registry, ContextRegistryContextEventHandler? onOpened, ContextRegistryEventHandler? onClosed) {
        if (onOpened != null)
            registry.Opened += onOpened;

        if (onClosed != null)
            registry.Closed += onClosed;
    }
}