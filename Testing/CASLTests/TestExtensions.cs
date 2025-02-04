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
}
