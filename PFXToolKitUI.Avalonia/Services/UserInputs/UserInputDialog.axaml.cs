// 
// Copyright (c) 2023-2024 REghZy
// 
// This file is part of FramePFX.
// 
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PFXToolKitUI.Avalonia.Bindings;
using PFXToolKitUI.Avalonia.Services.Colours;
using PFXToolKitUI.Avalonia.Services.Messages.Controls;
using PFXToolKitUI.Avalonia.Shortcuts.Dialogs;
using PFXToolKitUI.Avalonia.Themes.Controls;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Services.ColourPicking;
using PFXToolKitUI.Services.InputStrokes;
using PFXToolKitUI.Services.UserInputs;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.Services.UserInputs;

public partial class UserInputDialog : WindowEx {
    public static readonly SingleUserInputInfo DummySingleInput = new SingleUserInputInfo("Text Input Here") { Message = "A primary message here", ConfirmText = "Confirm", CancelText = "Cancel", Caption = "The caption here", Label = "The label here" };
    public static readonly DoubleUserInputInfo DummyDoubleInput = new DoubleUserInputInfo("Text A Here", "Text B Here") { Message = "A primary message here", ConfirmText = "Confirm", CancelText = "Cancel", Caption = "The caption here", LabelA = "Label A Here:", LabelB = "Label B Here:" };

    public static readonly ModelControlRegistry<UserInputInfo, Control> Registry;

    public static readonly StyledProperty<UserInputInfo?> UserInputInfoProperty = AvaloniaProperty.Register<UserInputDialog, UserInputInfo?>("UserInputInfo");

    public UserInputInfo? UserInputInfo {
        get => this.GetValue(UserInputInfoProperty);
        set => this.SetValue(UserInputInfoProperty, value);
    }

    public Thickness ContentMargin {
        get => this.PART_InputFieldContent.Margin;
        set => this.PART_InputFieldContent.Margin = value;
    }

    /// <summary>
    /// Gets the dialog result for this user input dialog
    /// </summary>
    public bool? DialogResult { get; private set; }

    private readonly DataParameterPropertyBinder<UserInputInfo> captionBinder = new DataParameterPropertyBinder<UserInputInfo>(TitleProperty, UserInputInfo.CaptionParameter);
    private readonly DataParameterPropertyBinder<UserInputInfo> messageBinder = new DataParameterPropertyBinder<UserInputInfo>(TextBlock.TextProperty, UserInputInfo.MessageParameter);
    private readonly DataParameterPropertyBinder<UserInputInfo> confirmTextBinder = new DataParameterPropertyBinder<UserInputInfo>(ContentProperty, UserInputInfo.ConfirmTextParameter);
    private readonly DataParameterPropertyBinder<UserInputInfo> cancelTextBinder = new DataParameterPropertyBinder<UserInputInfo>(ContentProperty, UserInputInfo.CancelTextParameter);

    public UserInputDialog() {
        this.InitializeComponent();
        this.captionBinder.AttachControl(this);
        this.messageBinder.AttachControl(this.PART_Message);
        this.confirmTextBinder.AttachControl(this.PART_ConfirmButton);
        this.cancelTextBinder.AttachControl(this.PART_CancelButton);
        this.PART_Message.PropertyChanged += this.OnMessageTextBlockPropertyChanged;

        this.PART_ConfirmButton.Click += this.OnConfirmButtonClicked;
        this.PART_CancelButton.Click += this.OnCancelButtonClicked;
    }

    static UserInputDialog() {
        Registry = new ModelControlRegistry<UserInputInfo, Control>();
        Registry.RegisterType<SingleUserInputInfo>(() => new SingleUserInputControl());
        Registry.RegisterType<DoubleUserInputInfo>(() => new DoubleUserInputControl());
        Registry.RegisterType<ColourUserInputInfo>(() => new ColourUserInputControl());
        Registry.RegisterType<KeyStrokeUserInputInfo>(() => new KeyStrokeUserInputControl());
        Registry.RegisterType<MouseStrokeUserInputInfo>(() => new MouseStrokeUserInputControl());

        UserInputInfoProperty.Changed.AddClassHandler<UserInputDialog, UserInputInfo?>((o, e) => o.OnUserInputDataChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    protected override void OnKeyDown(KeyEventArgs e) {
        base.OnKeyDown(e);
        if (!e.Handled && e.Key == Key.Escape) {
            this.TryCloseDialog(false);
        }
    }

    private void OnMessageTextBlockPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e) {
        if (e.Property == TextBlock.TextProperty) {
            this.PART_MessageContainer.IsVisible = !string.IsNullOrWhiteSpace(e.GetNewValue<string?>());
        }
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        this.PART_DockPanelRoot.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        Size size = this.PART_DockPanelRoot.DesiredSize;
        size = new Size(size.Width + 2, size.Height);
        if (size.Width > 300.0) {
            this.Width = size.Width;
        }

        const double TitleBarHeight = 32;
        this.Height = size.Height + TitleBarHeight;
    }

    private void OnConfirmButtonClicked(object? sender, RoutedEventArgs e) => this.TryCloseDialog(true);

    private void OnCancelButtonClicked(object? sender, RoutedEventArgs e) => this.TryCloseDialog(false);

    private void OnUserInputDataChanged(UserInputInfo? oldData, UserInputInfo? newData) {
        if (oldData != null) {
            (this.PART_InputFieldContent.Content as IUserInputContent)?.Disconnect();
            this.PART_InputFieldContent.Content = null;
            oldData.HasErrorsChanged -= this.OnHasErrorsChanged;
        }

        // Create this first just in case there's a problem with no registrations
        Control? control = newData != null ? Registry.NewInstance(newData) : null;

        this.captionBinder.SwitchModel(newData);
        this.messageBinder.SwitchModel(newData);
        this.confirmTextBinder.SwitchModel(newData);
        this.cancelTextBinder.SwitchModel(newData);
        if (control != null) {
            newData!.HasErrorsChanged += this.OnHasErrorsChanged;
            this.PART_InputFieldContent.Content = control;
            control.ApplyStyling();
            control.ApplyTemplate();
            (control as IUserInputContent)?.Connect(this, newData);
        }

        this.UpdateAllErrors();
        Dispatcher.UIThread.InvokeAsync(() => {
            if ((this.PART_InputFieldContent.Content as IUserInputContent)?.FocusPrimaryInput() == true) {
                return;
            }

            if (this.UserInputInfo?.DefaultButton is bool boolean) {
                if (boolean) {
                    this.PART_ConfirmButton.Focus();
                }
                else {
                    this.PART_CancelButton.Focus();
                }
            }
        }, DispatcherPriority.Loaded);
    }

    private void OnHasErrorsChanged(UserInputInfo info) {
        this.PART_ConfirmButton.IsEnabled = !info.HasErrors();
    }

    /// <summary>
    /// Invokes the <see cref="UserInputInfo.UpdateAllErrors"/> on our user input info to forcefully
    /// update all errors, and then updates our confirm button
    /// </summary>
    public void UpdateAllErrors() {
        if (this.UserInputInfo is UserInputInfo info) {
            info.UpdateAllErrors();
            this.PART_ConfirmButton.IsEnabled = !info.HasErrors();
        }
        else {
            this.PART_ConfirmButton.IsEnabled = false;
        }
    }

    /// <summary>
    /// Tries to close the dialog
    /// </summary>
    /// <param name="result">The dialog result wanted</param>
    /// <returns>
    /// True if the dialog was closed (regardless of the dialog result),
    /// false if it could not be closed due to a validation error or other error
    /// </returns>
    public bool TryCloseDialog(bool result) {
        if (result) {
            UserInputInfo? data = this.UserInputInfo;
            if (data == null || data.HasErrors()) {
                return false;
            }

            this.Close(this.DialogResult = true);
        }
        else {
            this.Close(this.DialogResult = false);
        }

        return true;
    }

    /// <summary>
    /// Shows a new instance of <see cref="UserInputDialog"/> as a dialog, using the given user information to create the content
    /// </summary>
    /// <param name="info">The input info</param>
    /// <returns>A task to await the dialog close result</returns>
    public static async Task<bool?> ShowDialogAsync(UserInputInfo info) {
        Validate.NotNull(info);

        if (IFrontEndApplication.Instance.TryGetActiveWindow(out Window? window)) {
            UserInputDialog dialog = new UserInputDialog {
                UserInputInfo = info
            };

            bool? result = await dialog.ShowDialog<bool?>(window);
            dialog.UserInputInfo = null;
            if (result == true && dialog.DialogResult == true) {
                return true;
            }

            return result;
        }

        return null;
    }
}