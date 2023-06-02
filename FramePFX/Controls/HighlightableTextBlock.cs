using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using FramePFX.Highlighting;
using TextRange = FramePFX.Core.Utils.TextRange;

namespace FramePFX.Controls {
    public class HighlightableTextBlock : TextBlock {
        public static readonly DependencyProperty HighlightProperty =
            DependencyProperty.Register(
                "Highlight",
                typeof(IEnumerable<TextRange>),
                typeof(HighlightableTextBlock),
                new FrameworkPropertyMetadata(null, (d, e) => ((HighlightableTextBlock) d).OnHighlightChanged(e)));

        public static readonly DependencyProperty NormalTextRunStyleProperty =
            DependencyProperty.Register(
                "NormalTextRunStyle",
                typeof(Style),
                typeof(HighlightableTextBlock),
                new PropertyMetadata(null, (d,e) => ((HighlightableTextBlock) d).RegenerateInlines()));

        public static readonly DependencyProperty HighlightTextRunStyleProperty =
            DependencyProperty.Register(
                "HighlightTextRunStyle",
                typeof(Style),
                typeof(HighlightableTextBlock),
                new PropertyMetadata(null, (d,e) => ((HighlightableTextBlock) d).RegenerateInlines()));

        public Style NormalTextRunStyle {
            get => (Style) this.GetValue(NormalTextRunStyleProperty);
            set => this.SetValue(NormalTextRunStyleProperty, value);
        }

        public Style HighlightTextRunStyle {
            get => (Style) this.GetValue(HighlightTextRunStyleProperty);
            set => this.SetValue(HighlightTextRunStyleProperty, value);
        }

        public IEnumerable<TextRange> Highlight {
            get { return (IEnumerable<TextRange>) this.GetValue(HighlightProperty); }
            set { this.SetValue(HighlightProperty, value); }
        }

        private readonly Func<string, Run> normalRunFunc;
        private readonly Func<string, Run> highlightRunFunc;

        public HighlightableTextBlock() {
            this.normalRunFunc = this.CreateNormalRun;
            this.highlightRunFunc = this.CreateHighlightRun;
        }

        private void OnHighlightChanged(DependencyPropertyChangedEventArgs e) {
            if (e.OldValue is ObservableCollection<TextRange> oldList) {
                oldList.CollectionChanged -= this.OnHighlightCollectionModified;
            }

            if (e.NewValue is ObservableCollection<TextRange> newList) {
                newList.CollectionChanged += this.OnHighlightCollectionModified;
            }

            if (e.NewValue != null) {
                this.RegenerateInlines();
            }
        }

        public void RegenerateInlines() {
            this.Inlines.Clear();
            IEnumerable<TextRange> ranges = this.Highlight;
            if (ranges != null) {
                this.Inlines.AddRange(InlineHelper.CreateHighlight(this.Text, ranges, this.normalRunFunc, this.highlightRunFunc));
            }
        }

        private void OnHighlightCollectionModified(object sender, NotifyCollectionChangedEventArgs e) {
            this.RegenerateInlines();
        }

        private Run CreateNormalRun(string text) {
            Style style = this.NormalTextRunStyle;
            Run run = style != null ? new Run() { Style = style } : new Run();
            run.Text = text;
            return run;
        }

        private Run CreateHighlightRun(string text) {
            Style style = this.HighlightTextRunStyle;
            Run run = style != null ? new Run() { Style = style } : new Run() {
                FontStyle = FontStyles.Italic
            };

            run.Text = text;
            return run;
        }
    }
}