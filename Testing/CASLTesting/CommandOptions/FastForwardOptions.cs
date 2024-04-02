// <copyright file="FastForwardOptions.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTesting.CommandOptions;

using CommandLine;

[Verb("fast-forward")]
public class FastForwardOptions
{
    private float seconds;

    [Option(
        's',
        "seconds",
        Required = true)]
    public float Seconds
    {
        get => this.seconds;
        set
        {
            this.seconds = value;

            this.seconds = this.seconds < 0f ? 0f : this.seconds;
        }
    }
}
