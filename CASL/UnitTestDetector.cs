// <copyright file="UnitTestDetector.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL;

using System;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Detects if the code ahs been executed from a unit test vs the rest of the application.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Do not need to see coverage for code used for testing.")]
internal static class UnitTestDetector
{
    private static readonly bool IsRunningFromUnitTestValue;

    /// <summary>
    /// Initializes static members of the <see cref="UnitTestDetector"/> class.
    /// </summary>
    static UnitTestDetector()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            if (string.IsNullOrEmpty(assembly.FullName) ||
                !assembly.FullName.ToLowerInvariant().StartsWith("xunit."))
            {
                continue;
            }

            IsRunningFromUnitTestValue = true;
            break;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the code is being executed from a unit test.
    /// </summary>
    public static bool IsRunningFromUnitTest
    {
        get
        {
#if DEBUG
            return IsRunningFromUnitTestValue;
#else
            return false;
#endif
        }
    }
}
