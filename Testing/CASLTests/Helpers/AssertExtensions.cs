// <copyright file="AssertExtensions.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

#pragma warning disable IDE0002 // Name can be simplified
namespace CASLTests.Helpers;

using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;
using FluentAssertions;

/// <summary>
/// Provides helper methods for the <see cref="Xunit"/>'s <see cref="Assert"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
// ReSharper disable once ClassNeverInstantiated.Global
public class AssertExtensions : Assert
{
    /// <summary>
    /// Verifies that the exact exception is thrown (and not a derived exception type) and that
    /// the exception message matches the given <paramref name="expectedMessage"/>.
    /// </summary>
    /// <typeparam name="T">The type of exception that the test is verifying.</typeparam>
    /// <param name="testCode">The code that will be throwing the expected exception.</param>
    /// <param name="expectedMessage">The expected message of the exception.</param>
    public static void ThrowsWithMessage<T>(Action testCode, string expectedMessage)
        where T : Exception
    {
        Assert.Throws<T>(testCode).Message.Should().Be(expectedMessage);
    }

    /// <summary>
    /// Asserts that the given test code does not throw the exception of type <typeparamref name="T"/> is not thrown.
    /// </summary>
    /// <typeparam name="T">The type of exception to check for.</typeparam>
    /// <param name="testCode">The test code that should not throw the exception.</param>
    public static void DoesNotThrow<T>(Action testCode)
        where T : Exception
    {
        if (testCode is null)
        {
            throw new ArgumentNullException(nameof(testCode), "The parameter must not be null");
        }

        try
        {
            testCode();
        }
        catch (T)
        {
            var flag = false;
            flag.Should().BeTrue($"Expected the exception {typeof(T).Name} to not be thrown.");
        }
    }

    /// <summary>
    /// Asserts that the given <paramref name="testCode"/> does not throw a null reference exception.
    /// </summary>
    /// <param name="testCode">The test that should not throw an exception.</param>
    public static void DoesNotThrowNullReference(Action testCode)
    {
        if (testCode is null)
        {
            throw new ArgumentNullException(nameof(testCode), "The parameter must not be null");
        }

        try
        {
            testCode();
        }
        catch (Exception ex)
        {
            if (ex.GetType() == typeof(NullReferenceException))
            {
                var flag = false;
                flag.Should().BeTrue($"Expected not to raise a {nameof(NullReferenceException)} exception.");
            }
            else
            {
                var flag = true;
                flag.Should().BeTrue();
            }
        }
    }

    /// <summary>
    /// Verifies that all items in the collection pass when executed against the given action.
    /// </summary>
    /// <typeparam name="T">The type of object to be verified.</typeparam>
    /// <param name="collection">The 2-dimensional collection.</param>
    /// <param name="width">The width of the first dimension.</param>
    /// <param name="height">The height of the second dimension.</param>
    /// <param name="action">The action to test each item against.</param>
    /// <remarks>
    ///     The last 2 <see langword="in"/> parameters T2 and T3 of type <see langword="int"/> of the <paramref name="action"/>
    ///     is the X and Y location within the <paramref name="collection"/> that failed the assertion.
    /// </remarks>
    [SuppressMessage("csharpsquid", "S2368", Justification = "The purpose of this is to test the jagged array.")]
    public static void All<T>(T[,] collection, int width, int height, Action<T, int, int> action)
    {
        var actionInvoked = false;

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                actionInvoked = true;
                action(collection[x, y], x, y);
            }
        }

        actionInvoked.Should().BeTrue($"No assertions were actually made in {nameof(AssertExtensions)}.{nameof(All)}<T>.  Are there any items?");
    }

    /// <summary>
    /// Verifies that all items in the collection pass when executed against the given action.
    /// </summary>
    /// <typeparam name="T">The type of object to be verified.</typeparam>
    /// <param name="collection">The collection.</param>
    /// <param name="action">The action to test each item against.</param>
    public static void All<T>(T[] collection, Action<T, int> action)
    {
        var actionInvoked = false;

        for (var i = 0; i < collection.Length; i++)
        {
            actionInvoked = true;
            action(collection[i], i);
        }

        actionInvoked.Should().BeTrue($"No assertions were actually made in {nameof(AssertExtensions)}.{nameof(All)}<T>.  Are there any items?");
    }
}
