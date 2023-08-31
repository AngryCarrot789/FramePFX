using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FramePFX.PropertyEditing.Editor.Primitives {
    public class CheckBoxGridEditorViewModel : BasePropertyEditorViewModel, IEnumerable<CheckBoxEditorViewModel> {
        public ObservableCollection<CheckBoxEditorViewModel> Editors { get; }

        public CheckBoxGridEditorViewModel(Type applicableType) : base(applicableType) {
            this.Editors = new ObservableCollection<CheckBoxEditorViewModel>();
        }

        public void Add(CheckBoxEditorViewModel editor) {
            this.Editors.Add(editor);
        }

        protected override void OnHandlersLoaded() {
            base.OnHandlersLoaded();
            foreach (CheckBoxEditorViewModel editor in this.Editors) {
                editor.SetHandlers(this.Handlers);
            }
        }

        protected override void OnClearHandlers() {
            base.OnClearHandlers();
            foreach (CheckBoxEditorViewModel editor in this.Editors) {
                editor.ClearHandlers();
            }
        }

        public IEnumerator<CheckBoxEditorViewModel> GetEnumerator() => this.Editors.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.Editors.GetEnumerator();
    }
}