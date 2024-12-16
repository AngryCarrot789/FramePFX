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

using FramePFX.AdvancedMenuService;
using FramePFX.Editing.Factories;
using FramePFX.Editing.ResourceManaging.Commands;
using FramePFX.Editing.ResourceManaging.Resources;
using FramePFX.Editing.ResourceManaging.UI;
using FramePFX.Interactivity;
using FramePFX.Interactivity.Contexts;
using FramePFX.Serialisation;
using FramePFX.Utils.Destroying;
using FramePFX.Utils.RBC;

namespace FramePFX.Editing.ResourceManaging;

/// <summary>
/// Base class for resource items and groups
/// </summary>
public abstract class BaseResource : IDisplayName, IDestroy
{
    public static readonly SerialisationRegistry SerialisationRegistry;
    public static readonly ContextRegistry ResourceItemContextRegistry;
    public static readonly ContextRegistry ResourceFolderContextRegistry;
    public static readonly ContextRegistry ResourceSurfaceContextRegistry;

    private string displayName;

    /// <summary>
    /// The manager that this resource belongs to. This will be non-null when <see cref="Parent"/> is non-null
    /// </summary>
    public ResourceManager? Manager { get; private set; }

    /// <summary>
    /// The folder that this object is currently in. This will be null or either the root folder in a resource manager,
    /// or if this resource just isn't in a resource tree. If this is null, then <see cref="Manager"/> will also be null
    /// </summary>
    public ResourceFolder? Parent { get; private set; }

    /// <summary>
    /// This resource object's registry ID, used to reflectively create an instance of it while deserializing data
    /// </summary>
    public string FactoryId => ResourceTypeFactory.Instance.GetId(this.GetType());

    public string? DisplayName
    {
        get => this.displayName;
        set
        {
            string oldName = this.displayName;
            if (oldName == value)
                return;
            this.displayName = value ?? "";
            this.DisplayNameChanged?.Invoke(this, oldName, value);
        }
    }

    public event DisplayNameChangedEventHandler? DisplayNameChanged;

    protected BaseResource()
    {
        this.displayName = "A Resource";
    }

    static BaseResource()
    {
        SerialisationRegistry = new SerialisationRegistry();
        SerialisationRegistry.Register<BaseResource>(0, (resource, data, ctx) =>
        {
            resource.DisplayName = data.GetString(nameof(resource.DisplayName), null);
        }, (resource, data, ctx) =>
        {
            if (!string.IsNullOrEmpty(resource.DisplayName))
                data.SetString(nameof(resource.DisplayName), resource.DisplayName);
        });

        // For the ListBox and TreeView
        ResourceSurfaceContextRegistry = new ContextRegistry("Resource Manager");

        // For ResourceFolder only
        ResourceFolderContextRegistry = new ContextRegistry("Resource Folder(s)");
        ResourceFolderContextRegistry.Opened += (r, ctx) =>
        {
            int selected;
            if (DataKeys.ResourceListUIKey.TryGetContext(ctx, out IResourceListElement? list))
                selected = list.Selection.SelectedItems.Count(x => x is ResourceFolder);
            else if (DataKeys.ResourceTreeUIKey.TryGetContext(ctx, out IResourceTreeElement? tree))
                selected = tree.Selection.SelectedItems.Count(x => x is ResourceFolder);
            else
                return;

            if (selected > 0)
                r.Caption = selected == 1 ? "1 Folder" : $"{selected} Folders";
        };

        ResourceFolderContextRegistry.Closed += (r) => r.Caption = "Resource Folder(s)";

        // For ResourceItem only
        ResourceItemContextRegistry = new ContextRegistry("Resource Item(s)");
        ResourceItemContextRegistry.Opened += (r, ctx) =>
        {
            List<BaseResource> selected;
            if (DataKeys.ResourceListUIKey.TryGetContext(ctx, out IResourceListElement? list))
                selected = list.Selection.SelectedItems.Where(x => x is ResourceItem).ToList();
            else if (DataKeys.ResourceTreeUIKey.TryGetContext(ctx, out IResourceTreeElement? tree))
                selected = tree.Selection.SelectedItems.Where(x => x is ResourceItem).ToList();
            else
                return;

            if (selected.Count == 1)
            {
                string? name = selected[0].DisplayName;
                if (string.IsNullOrWhiteSpace(name))
                    name = null;

                string typeName = selected[0].GetType().Name;
                r.Caption = name != null ? $"{typeName} ({name})" : typeName;
            }
            else
            {
                r.Caption = $"{selected} Items";
            }
        };

        ResourceItemContextRegistry.Closed += r => r.Caption = "Resource Item(s)";

        static void ApplyNewItemEntries(FixedContextGroup g)
        {
            g.AddHeader("Create Resources");
            g.AddEntry(new CommandContextEntry("commands.resources.AddResourceImage", "Add Image", "Create a new image resource"));
            g.AddEntry(new CommandContextEntry("commands.resources.AddResourceAVMedia", "Add Media", "Create a new media resource"));
            g.AddEntry(new CommandContextEntry("commands.resources.AddResourceColour", "Add Colour", "Create a new colour resource"));
            g.AddEntry(new CommandContextEntry("commands.resources.AddResourceComposition", "Add Composition Timeline", "Create a composition timeline new resource"));
        }

        static void ApplyModifyGeneral(FixedContextGroup g)
        {
            g.AddHeader("General");
            g.AddCommand("commands.resources.RenameResource", "Rename", "Rename this resource");
        }

        static void ApplyModifyDestruction(FixedContextGroup g)
        {
            g.AddCommand("commands.resources.DeleteResources", "Delete", "Delete this/these resource(s)");
        }

        ApplyModifyGeneral(ResourceItemContextRegistry.GetFixedGroup("modify.general"));
        ApplyModifyGeneral(ResourceFolderContextRegistry.GetFixedGroup("modify.general"));
        ApplyNewItemEntries(ResourceSurfaceContextRegistry.GetFixedGroup("modify.subcreation"));
        ApplyNewItemEntries(ResourceFolderContextRegistry.GetFixedGroup("modify.subcreation"));

        ResourceItemContextRegistry.GetFixedGroup("modify.general").AddDynamicSubGroup((group, ctx, items) =>
        {
            if (ResourceCommandUtils.GetSingleItem(ctx, out BaseResource? resource))
            {
                switch (resource)
                {
                    case ResourceColour: 
                        items.Add(new CommandContextEntry("commands.resources.ChangeResourceColour", "Change Colour", "Change the colour of the resource")); 
                        break;
                    case ResourceComposition: 
                        items.Add(new CommandContextEntry("commands.editor.OpenCompositionTimeline", "Open Timeline", "Opens this composition resource's timeline in the editor")); 
                        break;
                }
            }
        });

        const string cmdID = "commands.resources.GroupResources";
        ResourceSurfaceContextRegistry.GetFixedGroup("Modify2").AddCommand(cmdID, "Group");
        ResourceItemContextRegistry.GetFixedGroup("Modify2").AddCommand(cmdID, "Group Item(s)");

        ResourceItemContextRegistry.CreateDynamicGroup("ModifyOnlineStates", (g, ctx, items) =>
        {
            if (!ToggleOnlineStateCommand.GetTargetItems(ctx, out List<ResourceItem>? list))
            {
                return;
            }

            items.Add(new CaptionEntry("Modify Online State"));
            if (list.Count == 1)
            {
                if (list[0].IsOnline)
                {
                    items.Add(new CommandContextEntry("commands.resources.SetResourcesOffline", "Set Offline", "Set the selected resources offline"));
                }
                else
                {
                    items.Add(new CommandContextEntry("commands.resources.SetResourcesOnline", "Set Online", "Set the selected resources online"));
                }
            }
            else
            {
                items.Add(new CommandContextEntry("commands.resources.SetResourcesOnline", "Set Online", "Set the selected resources online"));
                items.Add(new CommandContextEntry("commands.resources.SetResourcesOffline", "Set Offline", "Set the selected resources offline"));
                items.Add(new CommandContextEntry("commands.resources.ToggleOnlineState", "Toggle Online", "Toggles the online state of the selected resources"));
            }
        });

        ApplyModifyDestruction(ResourceItemContextRegistry.GetFixedGroup("modify.destruction", 100000));
        ApplyModifyDestruction(ResourceFolderContextRegistry.GetFixedGroup("modify.destruction", 100000));
    }

    /// <summary>
    /// Creates a clone of the item, and also any child items if the item is a group
    /// </summary>
    /// <param name="item">The item to clone</param>
    /// <returns>A cloned and fully registered but offline resource</returns>
    /// <exception cref="Exception">Internal error with the resource registry; cloned item type does not match the original item</exception>
    public static BaseResource Clone(BaseResource item)
    {
        BaseResource clone = ResourceTypeFactory.Instance.NewResource(item.FactoryId);
        if (clone.GetType() != item.GetType())
            throw new Exception("Cloned object type does not match the item type");
        item.LoadDataIntoClone(clone);
        return clone;
    }

    /// <summary>
    /// Invoked when this resource is added to a resource manager's resource hierarchy.
    /// <see cref="Manager"/> is set to a non-null value prior to this call.
    /// <para>
    /// The cause of this call is either this resource being added, or a folder containing
    /// this resource (possibly deep in the hierarchy) was added (assigning the manager
    /// reference for all sub-resources recursively)
    /// </para>
    /// </summary>
    protected internal virtual void OnAttachedToManager() {
    }

    /// <summary>
    /// Invoked when this resource is removed from a resource manager's resource hierarchy.
    /// <see cref="Manager"/> is set to null after this call.
    /// <para>
    /// The cause of this call is either this resource being removed, or a folder containing
    /// this resource (possibly deep in the hierarchy) was removed (clearing the manager
    /// reference for all sub-resources recursively)
    /// </para>
    /// </summary>
    protected internal virtual void OnDetachedFromManager() {
    }

    public static BaseResource ReadSerialisedWithType(RBEDictionary dictionary)
    {
        string registryId = dictionary.GetString(nameof(FactoryId), null);
        if (string.IsNullOrEmpty(registryId))
            throw new Exception("Missing the registry ID for item");
        RBEDictionary data = dictionary.GetDictionary("Data");
        BaseResource item = ResourceTypeFactory.Instance.NewResource(registryId);
        SerialisationRegistry.Deserialise(item, data);
        return item;
    }

    public static void WriteSerialisedWithType(RBEDictionary dictionary, BaseResource item)
    {
        if (!(item.FactoryId is string id))
            throw new Exception("Unknown resource item type: " + item.GetType());
        dictionary.SetString(nameof(FactoryId), id);
        SerialisationRegistry.Serialise(item, dictionary.CreateDictionary("Data"));
    }

    public static RBEDictionary WriteSerialisedWithType(BaseResource clip)
    {
        RBEDictionary dictionary = new RBEDictionary();
        WriteSerialisedWithType(dictionary, clip);
        return dictionary;
    }

    /// <summary>
    /// This is invoked during the clone process. The current instance is a new cloned object.
    /// load data from the given object into the current instance
    /// </summary>
    /// <param name="clone">An object to copy data from</param>
    protected virtual void LoadDataIntoClone(BaseResource clone)
    {
        clone.DisplayName = this.DisplayName;
    }

    /// <summary>
    /// Destroys this resource's data, such as disposing objects, unregistering event handlers, etc.
    /// <para>
    /// Any errors should be logged to the application logger
    /// </para>
    /// <para>
    /// If this object is a <see cref="ResourceFolder"/> then the child hierarchy will NOT be
    /// destroyed, use <see cref="ResourceFolder.ClearHierarchy"/> instead
    /// </para>
    /// <para>
    /// If this object is a <see cref="ResourceItem"/> then <see cref="ResourceItem.Disable"/> is
    /// called to disable the object
    /// </para>
    /// </summary>
    public virtual void Destroy() {
    }

    /// <summary>
    /// An internal method used to set a manager's root folder's manager
    /// </summary>
    internal static void InternalSetManagerForRootFolder(ResourceFolder root, ResourceManager owner)
    {
        // root folder selection should not be processed
        root.Manager = owner;
        root.OnAttachedToManager();
    }

    protected static void InternalOnItemAdded(BaseResource obj, ResourceFolder parent)
    {
        obj.Parent = parent;
        ResourceManager manager = parent.Manager;
        if (manager != null)
        {
            InternalSetResourceManager(obj, manager);
        }
    }

    protected static void InternalOnItemRemoved(BaseResource obj, ResourceFolder parent)
    {
        obj.Parent = null;
        if (obj.Manager != null)
        {
            ResourceManager.InternalProcessResourceOnDetached(obj);
            obj.OnDetachedFromManager();
            obj.Manager = null;
        }
    }

    protected static void InternalOnItemMoved(BaseResource obj, ResourceFolder newParent)
    {
        if (obj.Manager != newParent.Manager)
            throw new Exception("Manager was different");
        obj.Parent = newParent;
    }

    protected static void InternalSetResourceManager(BaseResource resource, ResourceManager manager)
    {
        if (ReferenceEquals(resource.Manager, manager))
        {
            throw new InvalidOperationException("Cannot set manager to same instance");
        }

        if (manager != null)
        {
            resource.Manager = manager;
            ResourceManager.InternalProcessResourceOnAttached(resource, manager);
            resource.OnAttachedToManager();
        }
        else
        {
            ResourceManager.InternalProcessResourceOnDetached(resource);
            resource.OnDetachedFromManager();
            resource.Manager = null;
        }
    }
}