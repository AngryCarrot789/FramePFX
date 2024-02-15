using System.Collections.Generic;
using System.Linq;
using System.Windows;
using FramePFX.Shortcuts.WPF.Converters;
using FramePFX.Utils;

namespace FramePFX.AdvancedContextService.WPF {
    public class AdvancedContextShortcutMenuItem : AdvancedContextMenuItem {
        public static readonly DependencyProperty ShortcutIdProperty =
            DependencyProperty.Register(
                "ShortcutId",
                typeof(string),
                typeof(AdvancedContextShortcutMenuItem),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty ShortcutIdsProperty =
            DependencyProperty.Register(
                "ShortcutIds",
                typeof(IEnumerable<string>),
                typeof(AdvancedContextShortcutMenuItem),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty AutoGenerateDetailsProperty =
            DependencyProperty.Register(
                "AutoGenerateDetails",
                typeof(bool),
                typeof(AdvancedContextShortcutMenuItem),
                new PropertyMetadata(BoolBox.True));

        /// <summary>
        /// The primary shortcut key to pull the tooltip and description from
        /// </summary>
        public string ShortcutId {
            get => (string) this.GetValue(ShortcutIdProperty);
            set => this.SetValue(ShortcutIdProperty, value);
        }

        /// <summary>
        /// An optional collection of extra shortcuts that are used as either the replacement for <see cref="ShortcutId"/> if it's not set (only the first element
        /// in this collection is used for the tooltip and header), or it's used to calculate all possible input gestures
        /// </summary>
        public IEnumerable<string> ShortcutIds {
            get => (IEnumerable<string>) this.GetValue(ShortcutIdsProperty);
            set => this.SetValue(ShortcutIdsProperty, value);
        }

        public bool AutoGenerateDetails {
            get => (bool) this.GetValue(AutoGenerateDetailsProperty);
            set => this.SetValue(AutoGenerateDetailsProperty, value);
        }

        private bool hasFirstLoad;
        private bool hasExplicitHeader;
        private bool hasExplicitToolTip;
        private bool hasExplicitGesture;

        public AdvancedContextShortcutMenuItem() {
            this.Loaded += this.OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            if (!this.AutoGenerateDetails) {
                // this.CoerceValue(CommandIdProperty);
                return;
            }

            List<string> ids = new List<string>();
            string firstId = this.ShortcutId;
            if (!string.IsNullOrEmpty(firstId))
                ids.Add(firstId);

            foreach (string shortcut in this.ShortcutIds ?? Enumerable.Empty<string>()) {
                if (!string.IsNullOrEmpty(shortcut))
                    ids.Add(shortcut);
            }

            if (ids.Count < 1)
                return;

            if (string.IsNullOrEmpty(firstId))
                firstId = ids[0];

            bool hasNoHeader = this.IsValueUnset(HeaderProperty);
            bool hasNoTooltip = this.IsValueUnset(ToolTipProperty);
            bool hasNoGesture = this.IsValueUnset(InputGestureTextProperty);
            if (!this.hasFirstLoad) {
                this.hasExplicitHeader = !hasNoHeader;
                this.hasExplicitToolTip = !hasNoTooltip;
                this.hasExplicitGesture = !hasNoGesture;
            }

            if (!this.hasExplicitHeader && (hasNoHeader || this.hasFirstLoad)) {
                if (ShortcutIdToHeaderConverter.ShortcutIdToHeader(firstId, firstId, out string value)) {
                    this.SetCurrentValue(HeaderProperty, value);
                }
            }

            if (!this.hasExplicitToolTip && (hasNoTooltip || this.hasFirstLoad)) {
                if (ShortcutIdToToolTipConverter.ShortcutIdToTooltip(firstId, null, out string value)) {
                    this.SetCurrentValue(ToolTipProperty, value);
                }
            }

            if (!this.hasExplicitGesture && (hasNoGesture || this.hasFirstLoad)) {
                if (ShortcutIdToGestureConverter.ShortcutIdToGesture(ids, null, out string value)) {
                    this.SetCurrentValue(InputGestureTextProperty, value);
                }
            }

            if (!this.hasFirstLoad) {
                this.hasFirstLoad = true;
            }
        }

        protected bool IsValueUnset(DependencyProperty property) {
            if (this.GetValue(property) == null) {
                // allow empty bound strings
                return this.ReadLocalValue(property) == DependencyProperty.UnsetValue;
            }

            return false;
        }
    }
}