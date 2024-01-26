namespace FramePFX.Editors.Automation {
    public static class AutomatableHelper {
        public static T UpdateBackingStorage<T>(T obj) where T : IAutomatable {
            obj.AutomationData.UpdateBackingStorage();
            return obj;
        }
    }
}