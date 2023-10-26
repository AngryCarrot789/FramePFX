namespace ColorPicker.Models {
    internal class SecondColorDecorator : IColorStateStorage {
        public ColorState ColorState {
            get => this.storage.SecondColorState;
            set => this.storage.SecondColorState = value;
        }

        private readonly ISecondColorStorage storage;

        public SecondColorDecorator(ISecondColorStorage storage) {
            this.storage = storage;
        }
    }
}