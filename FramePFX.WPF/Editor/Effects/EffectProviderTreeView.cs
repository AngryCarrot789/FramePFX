using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using FramePFX.Editor.ViewModels;

namespace FramePFX.WPF.Editor.Effects {
    public class EffectProviderTreeView : TreeView {
        public EffectProviderListViewModel EffectProviderList => (EffectProviderListViewModel) this.DataContext;

        protected override bool IsItemItsOwnContainerOverride(object item) {
            return item is EffectProviderTreeViewItem;
        }

        protected override DependencyObject GetContainerForItemOverride() {
            return new EffectProviderTreeViewItem();
        }
    }
}
