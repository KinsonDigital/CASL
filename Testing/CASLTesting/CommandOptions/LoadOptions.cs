// <copyright file="LoadOptions.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTesting.CommandOptions;

using CASL;
using CommandLine;

[Verb("load")]
public class LoadOptions
{
    [Option('t', "type", Required = false, Default = BufferType.Stream)]
    public BufferType Type { get; set; }
}
