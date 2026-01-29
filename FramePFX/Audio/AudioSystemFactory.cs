// 
// Copyright (c) 2026-2026 REghZy
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

namespace FramePFX.Audio;

public abstract class AudioSystemFactory {
    /// <summary>
    /// Gets or sets the factory used to create audio systems
    /// </summary>
    public static AudioSystemFactory? Instance { get; set; }

    /// <summary>
    /// Creates factory args that specify how to create an audio system
    /// </summary>
    /// <returns>New args</returns>
    public abstract AudioSystemFactoryArgs CreateArgs();
    
    /// <summary>
    /// Creates the audio system
    /// </summary>
    /// <param name="_args">The args created by <see cref="CreateArgs"/></param>
    /// <returns>The new audio system</returns>
    public abstract AudioSystem Create(AudioSystemFactoryArgs _args);
}

/// <summary>
/// Args passed to <see cref="AudioSystemFactory.Create"/>
/// </summary>
public abstract class AudioSystemFactoryArgs {
    /// <summary>
    /// Gets the audio system sample rate
    /// </summary>
    public int SampleRate { get; set; }
    
    /// <summary>
    /// Gets the number of channels supported by the audio system
    /// </summary>
    public int Channels { get; set; }
}