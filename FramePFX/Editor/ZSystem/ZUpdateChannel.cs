using System;
using System.Collections.Generic;

namespace FramePFX.Editor.ZSystem
{
    /// <summary>
    /// An object that channels property update values, without interfering with other channels
    /// </summary>
    public class ZUpdateChannel
    {
        public const string DefaultChannelName = "ApplicationDefault";
        private static readonly Dictionary<string, ZUpdateChannel> Channels;
        internal readonly List<TransferValueCommand> _updateList;

        /// <summary>
        /// The unique name for this update channel
        /// </summary>
        public string Name { get; }

        private static ZUpdateChannel defaultChannel;

        /// <summary>
        /// Gets the default update channel for the application
        /// </summary>
        public static ZUpdateChannel Default
        {
            get => defaultChannel;
            set => defaultChannel = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Creates an instance of an update channel
        /// </summary>
        /// <param name="name">The unique name</param>
        /// <param name="initialCapacity">The initial capacity of the internal update command list</param>
        /// <exception cref="ArgumentException">Name is null, empty or consists of only whitespaces</exception>
        /// <exception cref="InvalidOperationException">Name is already in use</exception>
        private ZUpdateChannel(string name, int initialCapacity = 128)
        {
            this.Name = name;
            this._updateList = new List<TransferValueCommand>(initialCapacity);
        }

        static ZUpdateChannel()
        {
            Channels = new Dictionary<string, ZUpdateChannel>();
            defaultChannel = new ZUpdateChannel(DefaultChannelName);
        }

        public static ZUpdateChannel GetOrCreateChannel(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be null, empty or consist of only whitespaces", nameof(name));
            if (!Channels.TryGetValue(name, out var channel))
                Channels[name] = channel = new ZUpdateChannel(name);
            return channel;
        }

        public static ZUpdateChannel GetChannel(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be null, empty or consist of only whitespaces", nameof(name));
            Channels.TryGetValue(name, out ZUpdateChannel channel);
            return channel;
        }

        /// <summary>
        /// Adds a transfer value command to this channel's command queue. This is called automatically
        /// by <see cref="ZObject"/> when a property changes, and typically should not be invoked manually
        /// (due to the <see cref="ZObject"/> using internal flags to track if an update is already scheduled)
        /// </summary>
        /// <param name="command">The command to enqueue</param>
        public void Add(TransferValueCommand command)
        {
            this._updateList.Add(command);
        }

        /// <summary>
        /// Processes all transfer value commands
        /// </summary>
        public void ProcessUpdates() => ZObject.ProcessUpdates(this);
    }
}