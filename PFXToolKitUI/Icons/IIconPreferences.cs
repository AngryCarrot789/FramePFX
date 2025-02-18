namespace PFXToolKitUI.Icons;

public interface IIconPreferences {
    public static IIconPreferences Instance => Application.Instance.ServiceManager.GetService<IIconPreferences>();

    bool UseAntiAliasing { get; set; }
}