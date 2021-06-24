// <copyright file="Enums.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests.Helpers
{
    /// <summary>
    /// Used to choose which type of platform to mock.
    /// </summary>
    public enum PlatformType
    {
        Windows = 0,
        Posix = 1,
    }

    /// <summary>
    /// Used to choose which type of process to mock.
    /// </summary>
    public enum ProcessType
    {
        x86 = 0,
        x64 = 1,
    }
}
