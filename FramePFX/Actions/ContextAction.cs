using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using FramePFX.Commands;
using FramePFX.Utils.Collections;

namespace FramePFX.Actions {
    /// <summary>
    /// Represents some sort of action that can be executed. This class is designed to be used as a singleton,
    /// meaning there should only ever be a single instance of any implementation of this class
    /// <para>
    /// These actions are only really used by shortcut processors due to the lack of explicit context during key strokes,
    /// therefore, the context can be calculated in an action and the appropriate methods can be invoked (e.g. save project
    /// by finding a video editor or something that has access to the video editor such as a clip or timeline).
    /// <para>
    /// Actions are also used by context menus because of a different reason (creating duplicate context menu items for UI
    /// objects in a list for example is just very wasteful, even if it uses very little memory. Therefore, these actions allow
    /// dynamically created/removed context menu items that execute these actions)
    /// </para>
    /// </para>
    /// <para>
    /// These actions can be executed through the <see cref="ActionManager.Execute(string, Contexts.IDataContext, bool)"/> function
    /// </para>
    /// </summary>
    public abstract class ContextAction {
        /// <summary>
        /// Gets the unique singleton ID for this context action. This is set after the
        /// current instance is registered with a <see cref="ActionManager"/>
        /// </summary>
        public string Id { get; internal set; }

        /// <summary>
        /// Gets or sets the display name for this action. May be null, making it an unnamed action
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a readable description for what this action
        /// </summary>
        public string Description { get; set; }

        protected ContextAction() {
        }

        /// <summary>
        /// Checks if this action can actually be executed. This typically isn't checked before
        /// <see cref="ExecuteAsync"/> is invoked; this is mainly used by the UI to determine if
        /// something like a button or menu item is actually clickable
        /// <para>
        /// This method should be quick to execute, as it may be called quite often
        /// </para>
        /// </summary>
        /// <param name="e">The action event args, containing info about the current context</param>
        /// <returns>True if executing this action would most likely result in success, otherwise false</returns>
        public virtual bool CanExecute(ContextActionEventArgs e) {
            return true;
        }

        /// <summary>
        /// Executes this specific action with the given action event args
        /// </summary>
        /// <param name="e">The action event args, containing info about the current context</param>
        /// <returns>Whether the action execution was handled</returns>
        public abstract Task ExecuteAsync(ContextActionEventArgs e);

        /// <summary>
        /// Creates a builder, which can be used to build an action based on specific data context types
        /// </summary>
        /// <returns></returns>
        public static CommandActionBuilder Builder() {
            return new CommandActionBuilder();
        }
    }

    public class CommandActionBuilder {
        private readonly List<EntryBase> entries;
        private readonly List<ObjectConverter> converters;

        public CommandActionBuilder() {
            this.entries = new List<EntryBase>();
            this.converters = new List<ObjectConverter>();
        }

        /// <summary>
        /// Adds an entry for the type of <see cref="T"/>, with the given execute and canExecute methods
        /// </summary>
        /// <param name="execute">The executor</param>
        /// <param name="canExecute">Can the execute method run</param>
        /// <typeparam name="T">The type of parameter supplied to the function</typeparam>
        /// <returns>The command builder</returns>
        public CommandActionBuilder Execute<T>(Func<T, Task<bool>> execute, Predicate<T> canExecute = null) {
            Type type = typeof(T);
            for (int i = 0, count = this.entries.Count; i < count; i++) {
                if (this.entries[i].exactType == type) {
                    throw new InvalidOperationException("Type already added: " + type);
                }
            }

            Predicate<object> canExec = null;
            if (canExecute != null)
                canExec = o => canExecute((T) o);
            EntryBase entry = new FuncEntry(type, o => execute((T) o), canExec);
            this.entries.Add(entry);
            return this;
        }

        /// <summary>
        /// Adds an entry for the type of <see cref="T"/>, with the given function that returns a command to invoke
        /// </summary>
        /// <param name="getCommand">The function that returns a command</param>
        /// <typeparam name="T">The type of parameter supplied to the function</typeparam>
        /// <returns>The command builder</returns>
        public CommandActionBuilder Command<T>(Func<T, ICommand> getCommand) {
            Type type = typeof(T);
            for (int i = 0, count = this.entries.Count; i < count; i++) {
                if (this.entries[i].exactType == type) {
                    throw new InvalidOperationException("Type already added: " + type);
                }
            }

            EntryBase entry = new CommandEntry(type, o => getCommand((T) o));
            this.entries.Add(entry);
            return this;
        }

        /// <summary>
        /// Inserts a converter that can be used by a type entry to access an object that may not otherwise be
        /// in the provided data context during an action execution. Example is accessing a video editor from a clip
        /// </summary>
        /// <param name="converter">The converter</param>
        /// <typeparam name="TFrom">The type to convert from</typeparam>
        /// <typeparam name="TTo">The output type</typeparam>
        /// <returns>The command builder</returns>
        public CommandActionBuilder Convert<TFrom, TTo>(Func<TFrom, TTo> converter) {
            Type tFrom = typeof(TFrom);
            Type tTo = typeof(TTo);
            foreach (ObjectConverter c in this.converters) {
                if (c.fromType == tFrom && c.toType == tTo) {
                    throw new InvalidOperationException("Cannot add a converter that uses the exact same from and to types");
                }
            }

            this.converters.Add(new ObjectConverter(tFrom, tTo, (a) => converter((TFrom) a)));
            return this;
        }

        public ContextAction Build() {
            return new BuiltActionImpl(this.entries, this.converters);
        }

        private class BuiltActionImpl : ContextAction {
            private readonly InheritanceDictionary<EntryBase> entryMap;
            private readonly InheritanceDictionary<ObjectConverter> converterMap;

            public BuiltActionImpl(List<EntryBase> entries, List<ObjectConverter> converters) {
                this.entryMap = new InheritanceDictionary<EntryBase>(entries.Count);
                foreach (EntryBase entry in entries) {
                    this.entryMap.SetValue(entry.exactType, entry);
                }

                this.converterMap = new InheritanceDictionary<ObjectConverter>(converters.Count);
                foreach (ObjectConverter converter in converters) {
                    this.converterMap.SetValue(converter.fromType, converter);
                }
            }

            private IEnumerable<object> GetFinalObjects(ContextActionEventArgs e) {
                if (this.converterMap.IsEmpty)
                    return e.DataContext.Context;
                return this.GetFinalObjectsImpl(e);
            }

            private IEnumerable<object> GetFinalObjectsImpl(ContextActionEventArgs e) {
                foreach (object context in e.DataContext.Context) {
                    yield return context;
                    InheritanceDictionary<ObjectConverter>.LocalValueEntryEnumerator enumerable = this.converterMap.GetLocalValueEnumerator(context.GetType());
                    while (enumerable.MoveNext()) {
                        ObjectConverter converter = enumerable.Current.LocalValue;
                        if (converter.Convert(context, out object newValue)) {
                            yield return newValue;
                        }
                    }
                }
            }

            public override bool CanExecute(ContextActionEventArgs e) {
                foreach (object value in this.GetFinalObjects(e)) {
                    InheritanceDictionary<EntryBase>.LocalValueEntryEnumerator enumerator = this.entryMap.GetLocalValueEnumerator(value.GetType());
                    while (enumerator.MoveNext()) {
                        EntryBase entry = enumerator.Current.LocalValue;
                        if (entry.CanExecute(value)) {
                            return true;
                        }
                    }
                }

                return false;
            }

            public override async Task ExecuteAsync(ContextActionEventArgs e) {
                foreach (object value in this.GetFinalObjects(e)) {
                    InheritanceDictionary<EntryBase>.LocalValueEntryEnumerator enumerator = this.entryMap.GetLocalValueEnumerator(value.GetType());
                    while (enumerator.MoveNext()) {
                        EntryBase entry = enumerator.Current.LocalValue;
                        if (entry.CanExecute(value) && await entry.Execute(value)) {
                            return;
                        }
                    }
                }
            }
        }

        private abstract class EntryBase {
            public readonly Type exactType;

            protected EntryBase(Type exactType) {
                this.exactType = exactType;
            }

            public abstract bool CanExecute(object parameter);
            public abstract Task<bool> Execute(object parameter);
        }

        private class FuncEntry : EntryBase {
            public readonly Func<object, Task<bool>> execute;
            public readonly Predicate<object> canExecute;

            public FuncEntry(Type exactType, Func<object, Task<bool>> execute, Predicate<object> canExecute) : base(exactType) {
                this.execute = execute;
                this.canExecute = canExecute;
            }

            public override bool CanExecute(object parameter) {
                return this.canExecute?.Invoke(parameter) ?? true;
            }

            public override Task<bool> Execute(object parameter) {
                return this.execute(parameter);
            }
        }

        private class CommandEntry : EntryBase {
            public readonly Func<object, ICommand> getCmd;

            public CommandEntry(Type exactType, Func<object, ICommand> getCmd) : base(exactType) {
                this.getCmd = getCmd;
            }

            public override bool CanExecute(object parameter) {
                return this.getCmd(parameter)?.CanExecute(null) ?? false;
            }

            public override async Task<bool> Execute(object parameter) {
                if (this.getCmd(parameter) is ICommand command) {
                    if (command is BaseAsyncRelayCommand) {
                        if (await ((BaseAsyncRelayCommand) command).TryExecuteAsync(null)) {
                            return true;
                        }
                    }
                    else if (command.CanExecute(null)) {
                        command.Execute(null);
                        return true;
                    }
                }

                return false;
            }
        }

        private class ObjectConverter {
            public readonly Type fromType;
            public readonly Type toType;
            public readonly Func<object, object> converter;

            public ObjectConverter(Type fromType, Type toType, Func<object, object> converter) {
                this.fromType = fromType;
                this.toType = toType;
                this.converter = converter;
            }

            public bool Convert(object input, out object output) {
                return (output = this.converter(input)) != null;
            }
        }
    }
}