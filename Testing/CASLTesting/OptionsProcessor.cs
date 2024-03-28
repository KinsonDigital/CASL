// <copyright file="OptionsProcessor.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTesting;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CASL;
using CASL.Devices;
using CommandLine;

public class OptionsProcessor
{
    private const string AudioDirName = "AudioFiles";
    private static readonly string DefaultAudioLibDirPath = $@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}{Path.DirectorySeparatorChar}{AudioDirName}";
    private readonly Type[] options;
    private string? audioLibDirPath;
    private Task? audioPosTask;
    private CancellationTokenSource? audioPosTokenSrc;
    private Audio? audio;

    /// <summary>
    /// Initializes a new instance of the <see cref="OptionsProcessor"/> class.
    /// </summary>
    public OptionsProcessor() =>
        this.options = new[]
        {
            typeof(PlayOptions),
            typeof(PauseOptions),
            typeof(ResetOptions),
            typeof(SetPositionOptions),
            typeof(GetPositionOptions),
            typeof(FastForwardOptions),
            typeof(RewindOptions),
            typeof(GetVolumeOptions),
            typeof(SetVolumeOptions),
            typeof(GetPlaySpeedOptions),
            typeof(SetPlaySpeedOptions),
            typeof(ListDevicesOptions),
            typeof(ToggleLoopingOptions),
            typeof(ListAudioOptions),
            typeof(ChangeDeviceOptions),
            typeof(SetLibPathOptions),
            typeof(LoadOptions),
            typeof(UnloadOptions),
            typeof(ClearOptions),
            typeof(ExitOption),
        };

    public void ProcessOptions()
    {
        this.audioLibDirPath = DefaultAudioLibDirPath.Replace('\\', '/');
        this.audioLibDirPath = this.audioLibDirPath.EndsWith('/')
            ? DefaultAudioLibDirPath[..^1]
            : DefaultAudioLibDirPath;

        SetDefaultSoundFile();
        StartPosUpdater();

        var exitApp = false;

        while (!exitApp)
        {
            Parser.Default.ParseArguments(Console.ReadLine().Split(), this.options)
                .WithParsed<PlayOptions>(_ =>
                {
                    Console.WriteLine($"Playing the audio file {Path.GetFileName(this.audio.FilePath)}");
                    this.audio.Play();
                })
                .WithParsed<PauseOptions>(_ =>
                {
                    this.audio.Pause();
                    Console.WriteLine($"Paused the audio fle {Path.GetFileName(this.audio.FilePath)}");
                })
                .WithParsed<ResetOptions>(_ =>
                {
                    this.audio.Reset();
                })
                .WithParsed<SetPositionOptions>(o =>
                {
                    this.audio.SetTimePosition(o.Seconds);
                    Console.WriteLine($"Audio Position Set To {o.Seconds}(seconds).");
                })
                .WithParsed<GetPositionOptions>(_ => Console.WriteLine($"Audio position is: {this.audio.Position}"))
                .WithParsed<FastForwardOptions>(o =>
                {
                    this.audio.FastForward(o.Seconds);
                    Console.WriteLine($"Audio Fast Forwarded To: {o.Seconds}(seconds).");
                })
                .WithParsed<RewindOptions>(o =>
                {
                    this.audio.Rewind(o.Seconds);
                    Console.WriteLine($"Audio rewound to {o.Seconds} seconds.");
                })
                .WithParsed<GetVolumeOptions>(_ => Console.WriteLine($"Volume Set To: {this.audio.Volume}"))
                .WithParsed<SetVolumeOptions>(o =>
                {
                    this.audio.Volume = o.Value;
                    Console.WriteLine($"Volume Set To: {o.Value}");
                })
                .WithParsed<GetPlaySpeedOptions>(_ => Console.WriteLine($"PLay Speed: {this.audio.PlaySpeed}"))
                .WithParsed<SetPlaySpeedOptions>(o =>
                {
                    this.audio.PlaySpeed = o.Value;
                    Console.WriteLine($"Set Speed to {o.Value}");
                })
                .WithParsed<ToggleLoopingOptions>(_ =>
                {
                    this.audio.IsLooping = !this.audio.IsLooping;
                    Console.WriteLine($"Audio Set To {(this.audio.IsLooping ? "Loop" : "Not Loop")}");
                })
                .WithParsed<ListAudioOptions>(ListAudio)
                .WithParsed<ListDevicesOptions>(ListDevices)
                .WithParsed<ChangeDeviceOptions>(ChangeDevice)
                .WithParsed<SetLibPathOptions>(SetLibPath)
                .WithParsed<LoadOptions>(Load)
                .WithParsed<UnloadOptions>(_ => this.audio.Dispose())
                .WithParsed<ClearOptions>(_ => Console.Clear())
                .WithParsed<ExitOption>(
                    _ =>
                    {
                        Console.WriteLine("Exiting app. . .");
                        exitApp = true;
                        this.audioPosTokenSrc.Cancel();

                        while (!this.audioPosTask.IsCompleted)
                        {
                            Thread.Sleep(100);
                        }

                        this.audioPosTokenSrc.Dispose();
                        this.audioPosTask.Dispose();
                        this.audio.Dispose();
                    });
        }
    }

    private static void ListDevices(ListDevicesOptions o)
    {
        var deviceList = AudioDevice.AudioDevices;

        WriteLine("Audio Devices:", enterBlankBefore: true, enterBlankAfter: true);

        for (var i = 0; i < deviceList.Length; i++)
        {
            WriteLine($"  {i + 1}: {Path.GetFileName(deviceList[i])}");
        }
    }

    private static void ChangeDevice(ChangeDeviceOptions o)
    {
        var deviceNames = AudioDevice.AudioDevices;

        WriteLine("Enter a number to choose from the list of devices.");

        for (var i = 0; i < deviceNames.Length; i++)
        {
            TabbedWriteLine($"  {i + 1}: {deviceNames[i]}");
        }

        Console.WriteLine(string.Join("\n", deviceNames));

        Write("Enter a device item number: ", enterBlankBefore: true);

        bool parseSuccess;

        do
        {
            parseSuccess = int.TryParse(Console.ReadLine(), out var chosenNumber);

            if (!parseSuccess)
            {
                WriteLine("Invalid device number.  Please use a number from the device list.");
                continue;
            }

            var index = chosenNumber - 1;
            var chosenDevice = deviceNames[index];

            AudioDevice.SetAudioDevice(chosenDevice);
            WriteLine($"The audio device set to '{chosenDevice}'.", enterBlankBefore: true, enterBlankAfter: true);
        }
        while (!parseSuccess);
    }

    private static void SetLibPath(SetLibPathOptions o)
    {
        if (!Directory.Exists(o.Path))
        {
            Console.WriteLine($"The music library path '{o.Path}' does not exist.");
            return;
        }

        var audioFiles = GetAudioFiles(o.Path);
        var containsFiles = audioFiles.Length > 0;

        if (containsFiles)
        {
            var totalMp3Files = audioFiles.Count(f => f.ToLower().EndsWith(".mp3"));
            var totalOggFiles = audioFiles.Count(f => f.ToLower().EndsWith(".ogg"));

            Console.WriteLine($"Total MP3 Files: {totalMp3Files}");
            Console.WriteLine($"Total OGG Files: {totalOggFiles}");
        }
        else
        {
            var beforeClr = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"The music library path '{o.Path}' does not exist.\n");
            Console.ForegroundColor = beforeClr;
        }
    }

    private static string[] GetAudioFiles(string path)
    {
        var oggFiles = Directory.GetFiles(path, "*.ogg");
        var mp3Files = Directory.GetFiles(path, "*.mp3");

        var validFiles = new List<string>(oggFiles);
        validFiles.AddRange(mp3Files);

        return validFiles.ToArray();
    }

    private static void Write(string msg, bool enterBlankBefore = false, bool enterBlankAfter = false)
    {
        if (enterBlankBefore)
        {
            WriteBlank();
        }

        Console.Write(msg);

        if (enterBlankAfter)
        {
            WriteBlank();
        }
    }

    private static void WriteLine(string msg, bool enterBlankBefore = false, bool enterBlankAfter = false)
    {
        if (enterBlankBefore)
        {
            WriteBlank();
        }

        Console.WriteLine(msg);

        if (enterBlankAfter)
        {
            WriteBlank();
        }
    }

    private static void WriteBlank() => Console.WriteLine();

    private static void TabbedWriteLine(string msg, int tabCount = 1, bool enterBlankBefore = false, bool enterBlankAfter = false)
    {
        for (var i = 0; i < tabCount; i++)
        {
            msg = $"\t{msg}";
        }

        WriteLine(msg, enterBlankBefore, enterBlankAfter);
    }

    private void ListAudio(ListAudioOptions o)
    {
        var audioFiles = GetAudioFiles(this.audioLibDirPath);

        WriteLine("Audio Library Files:", enterBlankBefore: true);
        foreach (var audioFile in audioFiles)
        {
            var fileName = Path.GetFileName(audioFile);

            Console.WriteLine($"  {fileName}");
        }
    }

    private void Load(LoadOptions o)
    {
        var soundList = GetAudioFiles(this.audioLibDirPath);

        WriteLine("Enter a number to choose from the list of sounds.", enterBlankBefore: true);

        for (var i = 0; i < soundList.Length; i++)
        {
            TabbedWriteLine($"  {i + 1}: {Path.GetFileName(soundList[i])}");
        }

        Write("Enter a sound item number: ", enterBlankBefore: true);

        bool parseSuccess;

        do
        {
            parseSuccess = int.TryParse(Console.ReadLine(), out var chosenNumber);

            if (!parseSuccess)
            {
                WriteLine("Invalid sound number.  Please use a number from the sound list.");
                continue;
            }

            var index = chosenNumber - 1;
            var chosenSound = soundList[index];
            LoadSound(chosenSound, o.Type);
        }
        while (!parseSuccess);
    }

    private void StartPosUpdater()
    {
        this.audioPosTokenSrc = new CancellationTokenSource();

        this.audioPosTask = new Task(
            () =>
            {
                while (!this.audioPosTokenSrc.IsCancellationRequested)
                {
                    this.audioPosTokenSrc.Token.WaitHandle.WaitOne(250);

                    var minutes = (int)Math.Floor(this.audio.Position.Minutes);
                    var seconds = (int)Math.Round(this.audio.Position.Seconds, 0);
                    var minSec = $"{minutes}:{seconds}";
                    var totalSeconds = (int)Math.Round(this.audio.Position.TotalSeconds, 0);

                    Console.Title = $"{minSec} |  Total Secs: {totalSeconds}";
                }
            },
            this.audioPosTokenSrc.Token);

        this.audioPosTask.Start();
    }

    private void LoadSound(string soundFile, BufferType bufferType)
    {
        this.audio.Dispose();
        this.audio = new Audio(soundFile, bufferType);

        var fileName = Path.GetFileName(soundFile);

#pragma warning disable CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.
        var msg = bufferType switch
#pragma warning restore CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.
        {
            BufferType.Full => $"The file '{fileName}' has been fully loaded.",
            BufferType.Stream => $"The file '{fileName}' has been loaded as a stream.",
        };

        WriteLine(msg, enterBlankAfter: true);
    }

    private void SetDefaultSoundFile()
    {
        var soundFileName = Path.GetFileName(GetAudioFiles(this.audioLibDirPath)[0]);
        var soundLibPath = $"{this.audioLibDirPath}/{soundFileName}";
        this.audio = new Audio(soundLibPath, BufferType.Stream);

        WriteLine($"Music Library set to '{this.audioLibDirPath}'.");
        WriteLine($"Default sound file set to '{soundFileName}'", enterBlankAfter: true);
    }
}
