// <copyright file="Options.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTesting;

using CASL;
using CommandLine;

public class InteractiveOptions
{
    [Option("interactive", Required = false)]
    public bool Interactive { get; set; }
}

[Verb("exit")]
public class ExitOption
{
}

[Verb("play")]
public class PlayOptions
{
}

[Verb("pause")]
public class PauseOptions
{
}

[Verb("reset")]
public class ResetOptions
{
}

[Verb("set-pos")]
public class SetPositionOptions
{
    [Option('s', "seconds", Required = true)]
    public float Seconds { get; set; }
}

[Verb("get-pos")]
public class GetPositionOptions
{
}

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
        set
        {
            this.seconds = value;

            this.seconds = this.seconds < 0f ? 0f : this.seconds;
        }
    }
}

[Verb("get-volume")]
public class GetVolumeOptions
{
}

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

[Verb("get-speed")]
public class GetPlaySpeedOptions
{
}

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

[Verb("toggle-looping")]
public class ToggleLoopingOptions
{
}

[Verb("list-audio")]
public class ListAudioOptions
{
}

[Verb("list-devices")]
public class ListDevicesOptions
{
}

[Verb("change-device")]
public class ChangeDeviceOptions
{
}

[Verb("set-lib-path")]
public class SetLibPathOptions
{
    private string dirPath = string.Empty;

    [Option('p', "path", Required = true)]
    public string Path
    {
        get => this.dirPath;
        set
        {
            this.dirPath = value.Trim();
            this.dirPath = this.dirPath.Replace('\\', '/');
            this.dirPath = this.dirPath.EndsWith('/') ? this.dirPath[..^1] : this.dirPath;
        }
    }
}

[Verb("load")]
public class LoadOptions
{
    [Option('t', "type", Required = false, Default = BufferType.Stream)]
    public BufferType Type { get; set; }
}

[Verb("unload")]
public class UnloadOptions
{
}

[Verb("clear", aliases: ["cls"])]
public class ClearOptions
{
}
