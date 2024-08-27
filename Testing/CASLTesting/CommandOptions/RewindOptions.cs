// <copyright file="RewindOptions.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTesting.CommandOptions;

using CommandLine;

[Verb("rewind")]
public class RewindOptions
{
    private float seconds;

    [Option(
        's',
        "seconds",
        Required = true)]
    public float Seconds
    {
        get => this.seconds;
        // ReSharper disable once UnusedMember.Global
        set
        {
            this.seconds = value;

            this.seconds = this.seconds < 0f ? 0f : this.seconds;
        }
    }
}
