namespace FramePFX.Editor.ZSystem
{
    public readonly struct TransferValueCommand
    {
        public readonly ZObject Owner;
        public readonly ZProperty Property;

        public TransferValueCommand(ZObject owner, ZProperty property)
        {
            this.Owner = owner;
            this.Property = property;
        }
    }
}