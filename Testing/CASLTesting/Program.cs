// <copyright file="Program.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTesting;

using System;
using System.IO;
using CASL.Devices;
using CommandLine;
using CommandOptions;

/// <summary>
/// The main entry point for the application.
/// </summary>
public static class Program
{
    public static void Main(string[] args)
    {
        var isInteractive = false;

        Parser.Default.ParseArguments<InteractiveOptions>(args)
            .WithParsed(o =>
            {
                if (o.Interactive)
                {
                    Console.WriteLine("You are in interactive mode. Type 'exit' to exit.\n");
                    isInteractive = true;
                }
            });

        if (isInteractive)
        {
            Console.WriteLine("Type 'help' to see a list of commands.\n");
            var processor = new OptionsProcessor();
            processor.ProcessOptions();
        }
        else
        {
            var options = new[] { typeof(ListDevicesOptions) };

            Parser.Default.ParseArguments(args, options)
                .WithParsed<ListDevicesOptions>(_ =>
                {
                    var deviceList = AudioDevice.AudioDevices;

                    Console.WriteLine("\nAudio Devices:");

                    for (var i = 0; i < deviceList.Length; i++)
                    {
                        Console.WriteLine($"  {i + 1}: {Path.GetFileName(deviceList[i])}");
                    }
                });
        }
    }
}
