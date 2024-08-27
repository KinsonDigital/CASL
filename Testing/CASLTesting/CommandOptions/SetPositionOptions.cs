// <copyright file="SetPositionOptions.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTesting.CommandOptions;

using CommandLine;

[Verb("set-pos")]
public class SetPositionOptions
{
    [Option('s', "seconds", Required = true)]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public float Seconds { get; set; }
}
