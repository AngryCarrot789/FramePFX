using System;

namespace FramePFX.Utils {
    public static class TypeUtils {
        /// <summary>
        /// Checks if the left type is an instance of the right type.
        /// This is equivalent to:
        /// <code>'right.IsAssignableFrom(left)' or 'leftObj is rightType'</code>
        /// </summary>
        /// <param name="left">The left hand type</param>
        /// <param name="right">The right hand type</param>
        /// <returns>A bool</returns>
        public static bool instanceof(this Type left, Type right) {
            return right.IsAssignableFrom(left);
        }

        /// <summary>
        /// Checks if the left type is an instance of the generic type.
        /// This is equivalent to:
        /// <code>'typeof(T).IsAssignableFrom(left)' or 'leftObj is T'</code>
        /// </summary>
        /// <param name="left">The left hand type</param>
        /// <typeparam name="T">The right hand type</typeparam>
        /// <returns>A bool</returns>
        public static bool instanceof<T>(this Type left) {
            return typeof(T).IsAssignableFrom(left);
        }

        /// <summary>
        /// Checks if the left type is an instance of the right type.
        /// This is equivalent to:
        /// <code>'right.IsAssignableFrom(left.GetType())' or 'left is rightType'</code>
        /// </summary>
        /// <param name="left">The left instance</param>
        /// <param name="right">The right hand type</param>
        /// <returns>A bool</returns>
        public static bool instanceof(this object left, Type right) {
            return right.IsInstanceOfType(left);
        }

        /// <summary>
        /// Checks if the left type is an instance of the generic type.
        /// This is equivalent to:
        /// <code>left is T</code>
        /// </summary>
        /// <param name="left">The left instance</param>
        /// <typeparam name="T">The right hand type</typeparam>
        /// <returns>A bool</returns>
        public static bool instanceof<T>(this object left) => left is T;
    }
}