// <copyright file="SetVolumeOptions.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTesting.CommandOptions;

using CommandLine;

[Verb("set-volume")]
public class SetVolumeOptions
{
    private float volume;

    [Option('v', "value", Required = true)]
    public float Value
    {
        get => this.volume;
        set
        {
            this.volume = value;

            this.volume = this.volume < 0f ? 0f : this.volume;
            this.volume = this.volume > 100f ? 1f : this.volume;
        }
    }
}
