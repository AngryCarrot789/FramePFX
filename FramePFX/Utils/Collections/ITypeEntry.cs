using System;
using System.Collections.Generic;

namespace FramePFX.Utils.Collections {
    /// <summary>
    /// An interface for an entry in an <see cref="InheritanceDictionary{T}"/>
    /// </summary>
    /// <typeparam name="T">The type of value this entry stores</typeparam>
    public interface ITypeEntry<out T> {
        /// <summary>
        /// Gets the CLR type that keys to this instance (as in, the key to the <see cref="InheritanceDictionary{T}"/>)
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Returns true if this entry has a local value set
        /// </summary>
        bool HasLocalValue { get; }

        /// <summary>
        /// Returns true if this entry has an inherited value, otherwise false. This may be false if
        /// there is no base type and there is no local value set. When <see cref="HasLocalValue"/>
        /// is true, this value is also true
        /// </summary>
        bool HasInheritedValue { get; }

        /// <summary>
        /// Gets the local value for this entry
        /// </summary>
        /// <exception cref="InvalidOperationException">There is no local value set</exception>
        T LocalValue { get; }

        /// <summary>
        /// Gets the effective/inherited value for this entry
        /// </summary>
        /// <exception cref="InvalidOperationException">There is no inherited value available</exception>
        T EffectiveValue { get; }

        /// <summary>
        /// Gets a read-only list of this entry's derived types. This is updated live
        /// </summary>
        IReadOnlyList<ITypeEntry<T>> DerivedTypes { get; }

        /// <summary>
        /// Gets this entry's base type entry
        /// </summary>
        ITypeEntry<T> BaseType { get; }

        /// <summary>
        /// Gets the nearest entry which has a local value set
        /// </summary>
        ITypeEntry<T> NearestBaseTypeWithLocalValue { get; }
    }
}