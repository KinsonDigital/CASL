// <copyright file="ExtensionMethods.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.NativeInterop;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

/// <summary>
/// Provides helper methods for types in the <see cref="CASL.NativeInterop"/> namespace.
/// </summary>
internal static class ExtensionMethods
{
    /// <summary>
    /// Converts the data located at the given pointer to a string.
    /// </summary>
    /// <param name="ptrToStringData">The string to convert.</param>
    /// <returns>The string data pointed to by the <see langword="nint"/>.</returns>
    public static string ToManagedUTF8String(this nint ptrToStringData)
    {
        var result = string.Empty;

        // NOTE: Use to have a default parameter like this => bool freePtr = false
        if (ptrToStringData == 0)
        {
            return result;
        }

        unsafe
        {
            var ptr = (byte*)ptrToStringData;

            while (*ptr != 0)
            {
                ptr++;
            }

            result = Encoding.UTF8.GetString(
                (byte*)ptrToStringData,
                (int)(ptr - (byte*)ptrToStringData));
        }

        return result;
    }

    /// <summary>
    /// Converts the list of <paramref name="items"/> to a <see cref="ReadOnlyCollection{T}"/>.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of item in the list of <paramref name="items"/> and the
    ///     returned <see cref="ReadOnlyCollection{T}"/>.
    /// </typeparam>
    /// <param name="items">The list of items to convert.</param>
    /// <returns>The read only collection of the items.</returns>
    public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> items) => new (items.ToList());
}
