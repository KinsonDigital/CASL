// <copyright file="LoadOptions.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTesting.CommandOptions;

using CASL;
using CommandLine;

[Verb("load")]
public class LoadOptions
{
    private string dirPath = string.Empty;

    [Option('p', "path", Required = false, Default = null)]
    public string? Path
    {
        get => this.dirPath;
        set
        {
            this.dirPath = value.Trim(' ', '"');
            this.dirPath = this.dirPath.Replace('\\', '/');
            this.dirPath = this.dirPath.EndsWith('/') ? this.dirPath[..^1] : this.dirPath;
        }
    }

    [Option(
        't',
        "type",
        Required = false,
        Default = BufferType.Stream,
        MetaValue = "[Stream|Full]")]
    public BufferType Type { get; set; }
}
