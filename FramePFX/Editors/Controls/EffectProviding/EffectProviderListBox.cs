using System.Windows;
using System.Windows.Controls;
using FramePFX.Editors.EffectSource;

namespace FramePFX.Editors.Controls.EffectProviding {
    public class EffectProviderListBox : ListBox {
        public static readonly DependencyProperty EffectProviderManagerProperty = DependencyProperty.Register("EffectProviderManager", typeof(EffectProviderManager), typeof(EffectProviderListBox), new PropertyMetadata(null, (d, e) => ((EffectProviderListBox) d).OnProviderManagerChanged((EffectProviderManager) e.OldValue, (EffectProviderManager) e.NewValue)));

        public EffectProviderManager EffectProviderManager {
            get => (EffectProviderManager) this.GetValue(EffectProviderManagerProperty);
            set => this.SetValue(EffectProviderManagerProperty, value);
        }

        /// <summary>
        /// The drag-drop identifier for an effect source drag-drop
        /// </summary>
        public const string EffectProviderDropType = "PFXEffectSource_DropType";
        
        public EffectProviderListBox() {
        }

        private void OnProviderManagerChanged(EffectProviderManager oldManager, EffectProviderManager newManager) {
            if (oldManager == newManager)
                return;
            if (oldManager != null) {
                for (int i = this.Items.Count - 1; i >= 0; i--) {
                    this.RemoveItemInternal(i);
                }
            }

            if (newManager != null) {
                int i = 0;
                foreach (EffectProviderEntry entry in newManager.Entries) {
                    this.InsertItemInternal(entry, i++);
                }
            }
        }
        
        private void InsertItemInternal(EffectProviderEntry entry, int index) {
            EffectProviderListBoxItem control = new EffectProviderListBoxItem();
            control.OnAdding(this, entry);
            this.Items.Insert(index, control);
            // UpdateLayout must be called explicitly, so that the visual tree
            // can be measured, allowing templates to be applied
            control.InvalidateMeasure();
            control.UpdateLayout();
            control.OnAdded();
        }

        private void RemoveItemInternal(int index) {
            EffectProviderListBoxItem control = (EffectProviderListBoxItem) this.Items[index];
            control.OnRemoving();
            this.Items.RemoveAt(index);
            control.OnRemoved();
        }
    }
}
