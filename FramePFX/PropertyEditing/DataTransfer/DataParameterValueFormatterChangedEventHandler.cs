using FramePFX.Interactivity.Formatting;

namespace FramePFX.PropertyEditing.DataTransfer;

public delegate void DataParameterValueFormatterChangedEventHandler(DataParameterPropertyEditorSlot sender, IValueFormatter? oldValueFormatter, IValueFormatter? newValueFormatter);
