// <copyright file="InteractiveOptions.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTesting.CommandOptions;

using CommandLine;

public class InteractiveOptions
{
    [Option("interactive", Required = false)]
    public bool Interactive { get; set; }
}
