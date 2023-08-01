using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using FramePFX.Core.Utils;

namespace FramePFX.Shortcuts.Bindings {
    /// <summary>
    /// An input binding that is triggered by a shortcut. <see cref="InputBinding.Gesture"/> is unused
    /// </summary>
    public class ShortcutCommandBinding : Freezable {
        public static readonly DependencyProperty ShortcutPathProperty = DependencyProperty.Register(nameof(ShortcutPath), typeof(string), typeof(ShortcutCommandBinding));
        public static readonly DependencyProperty CommandProperty = InputBinding.CommandProperty.AddOwner(typeof(ShortcutCommandBinding));
        public static readonly DependencyProperty CommandParameterProperty = InputBinding.CommandParameterProperty.AddOwner(typeof(ShortcutCommandBinding));
        public static readonly DependencyProperty AllowChainExecutionProperty = DependencyProperty.Register("AllowChainExecution", typeof(bool), typeof(ShortcutCommandBinding), new PropertyMetadata(BoolBox.False));

        /// <summary>
        /// The full path of the shortcut that must be activated in order for this binding's command to be executed
        /// </summary>
        public string ShortcutPath {
            get => (string) this.GetValue(ShortcutPathProperty);
            set => this.SetValue(ShortcutPathProperty, value);
        }

        [Localizability(LocalizationCategory.NeverLocalize)]
        [TypeConverter("System.Windows.Input.CommandConverter, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
        public ICommand Command {
            get => (ICommand) this.GetValue(CommandProperty);
            set => this.SetValue(CommandProperty, value);
        }

        public object CommandParameter {
            get => this.GetValue(CommandParameterProperty);
            set => this.SetValue(CommandParameterProperty, value);
        }

        /// <summary>
        /// If there is more than 1 command triggered by the same <see cref="ShortcutPath"/> in the same collection, this determines
        /// whether or not the current instance can be executed if one of those commands have already been executed. False by default,
        /// as the first command should be the last, and no more commands should be executed
        /// </summary>
        public bool AllowChainExecution {
            get => (bool) this.GetValue(AllowChainExecutionProperty);
            set => this.SetValue(AllowChainExecutionProperty, value.Box());
        }

        public ShortcutCommandBinding() {
        }

        public ShortcutCommandBinding(string shortcutPath, ICommand command) {
            if (string.IsNullOrWhiteSpace(shortcutPath))
                throw new ArgumentException("Shortcut ID must not be null, empty or consist of only whitespaces", nameof(shortcutPath));
            this.ShortcutPath = shortcutPath;
            this.Command = command;
        }

        protected override Freezable CreateInstanceCore() => new ShortcutCommandBinding();
    }
}