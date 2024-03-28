// <copyright file="SetPlaySpeedOptions.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTesting.CommandOptions;

using CommandLine;

[Verb("set-speed")]
public class SetPlaySpeedOptions
{
    private float speed;

    [Option('v', "value", Required = true)]
    public float Value
    {
        get => this.speed;
        set
        {
            this.speed = value;

            this.speed = this.speed < 0f ? 0f : this.speed;
            this.speed = this.speed > 1f ? 1f : this.speed;
        }
    }
}
