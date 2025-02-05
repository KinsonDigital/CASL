// <copyright file="TestExtensions.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests;

using System.Collections.Generic;

/// <summary>
/// Testing related extension methods.
/// </summary>
public static class TestExtensions
{
    /// <summary>
    /// Generates a range of float values.
    /// </summary>
    /// <param name="start">The starting value of the range.</param>
    /// <param name="count">The number of items in the range.</param>
    /// <returns>The range of values.</returns>
    public static IEnumerable<float> RangeOfFloats(float start, int count)
    {
        for (float i = 0; i < count; i++)
        {
            yield return start + i;
        }
    }

    /// <summary>
    /// Generates a range of byte values.
    /// </summary>
    /// <param name="start">The starting value of the range.</param>
    /// <param name="count">The number of items in the range.</param>
    /// <returns>The range of values.</returns>
    public static IEnumerable<byte> RangeOfBytes(byte start, int count)
    {
        var value = start;
        var iterator = 0;

        while (true)
        {
            if (iterator >= count)
            {
                break;
            }

            if (value == byte.MaxValue)
            {
                value = 0;
            }
            else
            {
                value += 1;
            }

            iterator += 1;

            yield return value;
        }

        yield return value;
    }
}
