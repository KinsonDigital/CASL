// <copyright file="IFilePathResolver.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.NativeInterop
{
    /// <summary>
    /// Resolves paths to files.
    /// </summary>
    internal interface IFilePathResolver
    {
        /// <summary>
        /// Gets the path to the file with the given name.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>The file name with a resolved path.</returns>
        string GetFilePath(string fileName);

        /// <summary>
        /// Gets the path to native libraries for an application.
        /// </summary>
        /// <returns>The directory path to the native libraries.</returns>
        string GetDirPath();
    }
}
