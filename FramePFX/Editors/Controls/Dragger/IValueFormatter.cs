namespace FramePFX.Editors.Controls.Dragger {
    public interface IValueFormatter {
        string ToString(double value, int? roundedPlaces);
    }
}