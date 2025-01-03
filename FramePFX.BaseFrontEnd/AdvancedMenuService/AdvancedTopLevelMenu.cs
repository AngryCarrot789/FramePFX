using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using FramePFX.AdvancedMenuService;
using FramePFX.BaseFrontEnd.Interactivity;
using FramePFX.BaseFrontEnd.Shortcuts.Avalonia;
using FramePFX.Interactivity.Contexts;
using FramePFX.Utils.Collections.Observable;

namespace FramePFX.BaseFrontEnd.AdvancedMenuService;

public sealed class AdvancedTopLevelMenu : Menu, IAdvancedMenu {
    // We maintain a map of the registries to the context menu. This is to
    // save memory, since we don't have to create a context menu for each handler
    public static readonly StyledProperty<TopLevelMenuRegistry?> TopLevelMenuRegistryProperty = AvaloniaProperty.Register<AdvancedTopLevelMenu, TopLevelMenuRegistry?>(nameof(TopLevelMenuRegistry));

    public TopLevelMenuRegistry? TopLevelMenuRegistry {
        get => this.GetValue(TopLevelMenuRegistryProperty);
        set => this.SetValue(TopLevelMenuRegistryProperty, value);
    }

    public IContextData CapturedContext => DataManager.GetFullContextData(this);

    IAdvancedMenu IAdvancedMenuOrItem.OwnerMenu => this;

    protected override Type StyleKeyOverride => typeof(Menu);

    private readonly Dictionary<Type, Stack<Control>> itemCache;
    private InputElement? lastFocus;
    
    public AdvancedTopLevelMenu() {
        this.itemCache = new Dictionary<Type, Stack<Control>>();
    }

    static AdvancedTopLevelMenu() {
        TopLevelMenuRegistryProperty.Changed.AddClassHandler<AdvancedTopLevelMenu, TopLevelMenuRegistry?>((d, e) => d.OnTopLevelMenuRegistryChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) => new AdvancedMenuItem();

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey) {
        if (item is MenuItem || item is Separator || item is CaptionSeparator) {
            recycleKey = null;
            return false;
        }

        recycleKey = DefaultRecycleKey;
        return true;
    }

    private void OnTopLevelMenuRegistryChanged(TopLevelMenuRegistry? oldValue, TopLevelMenuRegistry? newValue) {
        if (ReferenceEquals(oldValue, newValue)) {
            return; // should be impossible... but just in case let's check
        }

        ItemCollection list = this.Items;
        if (oldValue != null) {
            for (int i = list.Count - 1; i >= 0; i--) {
                AdvancedMenuItem entry = (AdvancedMenuItem) list[i]!;
                Type type = entry.Entry!.GetType();
                entry.OnRemoving();
                list.RemoveAt(i);
                entry.OnRemoved();
                this.PushCachedItem(type, entry);
            }
        }

        if (newValue != null) {
            int i = 0;
            foreach (ContextEntryGroup entry in newValue.Items) {
                AdvancedMenuItem menuItem = (AdvancedMenuItem) this.CreateItem(entry);
                menuItem.OnAdding(this, this, entry);
                list.Insert(i++, menuItem);
                menuItem.ApplyStyling();
                menuItem.ApplyTemplate();
                menuItem.OnAdded();
            }
        }
    }

    protected override void OnSubmenuOpened(RoutedEventArgs e) {
        if (e.Source is AdvancedMenuItem menuItem) {
            AdvancedMenuService.NormaliseSeparators(menuItem);
        }

        base.OnSubmenuOpened(e);
    }

    public override void Close() {
        bool wasOpen = this.IsOpen;
        base.Close();
        if (wasOpen && this.lastFocus != null) {
            DataManager.ClearDelegateContextData(this);
            if (this.lastFocus != null) {
                this.lastFocus.Focus();
                this.lastFocus = null;
            }
        }
    }

    protected override void OnGotFocus(GotFocusEventArgs e) {
        this.lastFocus = null;
        if (TopLevel.GetTopLevel(this) is TopLevel topLevel) {
            this.lastFocus = UIInputManager.GetLastFocusedElement(topLevel);
        }

        base.OnGotFocus(e);
        if (this.lastFocus != null) {
            this.CaptureContextFromObject(this.lastFocus);
        }
    }

    private void CaptureContextFromObject(InputElement inputElement) {
        DataManager.SetDelegateContextData(this, DataManager.GetFullContextData(inputElement));
    }

    public bool PushCachedItem(Type entryType, Control item) => AdvancedMenuService.PushCachedItem(this.itemCache, entryType, item);

    public Control? PopCachedItem(Type entryType) => AdvancedMenuService.PopCachedItem(this.itemCache, entryType);

    public Control CreateItem(IContextObject entry) => AdvancedMenuService.CreateChildItem(this, entry);

    public void StoreDynamicGroup(DynamicGroupPlaceholderContextObject groupPlaceholder, int index) {
        throw new InvalidOperationException("It should be impossible to use dynamic entries in a top-level menu");
    }
}