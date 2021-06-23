// <copyright file="NativeDependencyManagerFake.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests.Fakes
{
    using System.IO.Abstractions;
    using CASL.NativeInterop;

    /// <summary>
    /// Used for testing the abstract <see cref="NativeDependencyManager"/> class.
    /// </summary>
    internal class NativeDependencyManagerFake : NativeDependencyManager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NativeDependencyManagerFake"/> class.
        /// </summary>
        /// <param name="platform">Mocked platform object.</param>
        /// <param name="file">Mocked file object.</param>
        /// <param name="path">Mocked path object.</param>
        public NativeDependencyManagerFake(IPlatform platform, IFile file, IPath path)
            : base(platform, file, path)
        {
        }
    }
}
