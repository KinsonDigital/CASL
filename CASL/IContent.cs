﻿// <copyright file="IContent.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL
{
    using System;

    /// <summary>
    /// Represents loadable content data.
    /// </summary>
    public interface IContent : IDisposable
    {
        /// <summary>
        /// Gets the name of the content.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the path to the content.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Gets a value indicating whether the content item has been unloaded.
        /// </summary>
        bool Unloaded { get; }
    }
}
