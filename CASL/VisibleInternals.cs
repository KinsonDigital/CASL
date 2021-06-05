// <copyright file="VisibleInternals.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(assemblyName: "CASLTests", AllInternalsVisible = true)]

// Used to expose internal types to the Moq mocking framework
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2", AllInternalsVisible = true)]
