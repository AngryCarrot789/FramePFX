using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FramePFX.Editors.Controls.Binders;

namespace FramePFX.AdvancedContextService.WPF {
    /// <summary>
    /// A menu item entry in a <see cref="AdvancedContextMenu"/> or <see cref="AdvancedContextMenuItem"/>
    /// </summary>
    public class AdvancedContextMenuItem : MenuItem {
        public AdvancedContextMenu Menu { get; private set; }

        public AdvancedContextMenuItem ParentNode { get; private set; }

        public BaseContextEntry Entry { get; private set; }

        public ItemsControl ParentObject => (ItemsControl) this.ParentNode ?? this.Menu;

        private readonly GetSetAutoPropertyBinder<BaseContextEntry> headerBinder = new GetSetAutoPropertyBinder<BaseContextEntry>(HeaderProperty, nameof(BaseContextEntry.HeaderChanged), b => b.Model.Header, (b, v) => b.Model.Header = v?.ToString());
        private readonly GetSetAutoPropertyBinder<BaseContextEntry> toolTipBinder = new GetSetAutoPropertyBinder<BaseContextEntry>(ToolTipProperty, nameof(BaseContextEntry.DescriptionChanged), b => b.Model.Description, (b, v) => b.Model.Description = v?.ToString());

        public AdvancedContextMenuItem() {

        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
            base.OnPropertyChanged(e);
            this.headerBinder.OnPropertyChanged(e);
            this.toolTipBinder.OnPropertyChanged(e);
        }

        public virtual void OnAdding(AdvancedContextMenu menu, AdvancedContextMenuItem parent, BaseContextEntry entry) {
            this.Menu = menu;
            this.ParentNode = parent;
            this.Entry = entry;
        }

        public virtual void OnAdded() {
            this.headerBinder.Attach(this, this.Entry);
            this.toolTipBinder.Attach(this, this.Entry);
            if (this.Entry.Children != null) {
                AdvancedContextMenu.InsertItemNodes(this.Menu, this, this.Entry.Children.ToList());
            }
        }

        public virtual void OnRemoving() {
            this.headerBinder.Detatch();
            this.toolTipBinder.Detatch();
            AdvancedContextMenu.ClearItemNodes(this);
        }

        public virtual void OnRemoved() {
            this.Menu = null;
            this.ParentNode = null;
            this.Entry = null;
        }
    }
}