using FramePFX.Interactivity;
using FramePFX.PropertyEditing;
using FramePFX.PropertyEditing.Core;

namespace FramePFX.BaseFrontEnd.PropertyEditing;

public static class TestPropertyEditor {
    public static PropertyEditor DummyEditor { get; } = new PropertyEditor();
    public static BasePropertyEditorGroup DummyGroup { get; }

    static TestPropertyEditor() {
        DummyEditor.Root.AddItem(new SimplePropertyEditorGroup(typeof(TestObject)) {
            DisplayName = "Group 1"

        });

        SimplePropertyEditorGroup group = new SimplePropertyEditorGroup(typeof(TestObject)) {
            DisplayName = "Group 2", IsExpanded = true
        };
        
        group.AddItem(new DisplayNamePropertyEditorSlot());
        DummyEditor.Root.AddItem(group);
        DummyEditor.Root.SetupHierarchyState([new TestObject()]);
        DummyGroup = DummyEditor.Root;
    }

    public class TestObject : IDisplayName {
        public string? DisplayName { get; set; }
        
        public event DisplayNameChangedEventHandler? DisplayNameChanged;
    }
}