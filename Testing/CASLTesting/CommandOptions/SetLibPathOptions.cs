// <copyright file="SetLibPathOptions.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTesting.CommandOptions;

using CommandLine;

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
