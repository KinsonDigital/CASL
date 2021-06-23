// <copyright file="IDelegateFactory.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.NativeInterop.Factories
{
    /// <summary>
    /// Creates delegates to native library functions.
    /// </summary>
    public interface IDelegateFactory
    {
        /// <summary>
        /// Creates a delegate of the given type <typeparamref name="TDelegate"/>
        /// to a funciton pointed to by the given <paramref name="libraryPtr"/>.
        /// </summary>
        /// <typeparam name="TDelegate">The type of delegate to return.</typeparam>
        /// <param name="libraryPtr">
        ///     The pointer to the library containing the function execute with the created delgate.
        /// </param>
        /// <param name="procName">The name of the library function.</param>
        /// <returns>The delegate to the native library function.</returns>
        TDelegate CreateDelegate<TDelegate>(nint libraryPtr, string procName);
    }
}
