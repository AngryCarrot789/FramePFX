namespace FramePFX.WPF.Controls.Dragger
{
    public interface IChangeMapper
    {
        void OnValueChanged(double value, out double tiny, out double small, out double normal, out double large);
    }
}