// <copyright file="OptionsProcessor.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTesting;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CASL;
using CASL.Devices;
using CommandLine;
using CommandOptions;

public class OptionsProcessor
{
    private const string AudioDirName = "AudioFiles";
    private static readonly string DefaultAudioLibDirPath = $@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}{Path.DirectorySeparatorChar}{AudioDirName}";
    private readonly Type[] options;
    private string? audioLibDirPath;
    private Task? audioPosTask;
    private CancellationTokenSource? audioPosTokenSrc;
    private Audio? audio;
    private bool isUnloading;
    private bool isSleeping;

    /// <summary>
    /// Initializes a new instance of the <see cref="OptionsProcessor"/> class.
    /// </summary>
    public OptionsProcessor() =>
        this.options =
        [
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
            typeof(GetLoopStatusOptions),
            typeof(ListAudioOptions),
            typeof(ChangeDeviceOptions),
            typeof(SetLibPathOptions),
            typeof(LoadOptions),
            typeof(UnloadOptions),
            typeof(ClearOptions),
            typeof(ExitOption)
        ];

    /// <summary>
    /// Processes all of the possible commands and options.
    /// </summary>
    [SuppressMessage("csharpsquid", "S3776", Justification = "Planned for simplification in the future.")]
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
#pragma warning disable SA1503
            Parser.Default.ParseArguments(Console.ReadLine().Split(), this.options)
                .WithParsed<PlayOptions>(_ =>
                {
                    if (AudioNotLoaded()) return;
                    Console.WriteLine($"Playing the audio file {Path.GetFileName(this.audio.FilePath)}\n");
                    this.audio.Play();
                })
                .WithParsed<PauseOptions>(_ =>
                {
                    if (AudioNotLoaded()) return;
                    this.audio.Pause();
                    Console.WriteLine($"Paused the audio fle {Path.GetFileName(this.audio.FilePath)}\n");
                })
                .WithParsed<ResetOptions>(_ =>
                {
                    if (AudioNotLoaded()) return;
                    this.audio.Reset();
                    Console.WriteLine("Audio reset back to the beginning.\n");
                })
                .WithParsed<SetPositionOptions>(o =>
                {
                    if (AudioNotLoaded()) return;
                    this.audio.SetTimePosition(o.Seconds);
                    Console.WriteLine($"Audio Position Set To {o.Seconds}(sec).\n");
                })
                .WithParsed<GetPositionOptions>(_ =>
                {
                    if (AudioNotLoaded()) return;
                    WriteLine($"Audio position is: {Math.Round(this.audio.Position.TotalSeconds, 2)}(sec).", enterBlankAfter: true);
                })
                .WithParsed<FastForwardOptions>(o =>
                {
                    if (AudioNotLoaded()) return;
                    this.audio.FastForward(o.Seconds);
                    Console.WriteLine($"Audio Fast Forwarded To: {o.Seconds}(sec).\n");
                })
                .WithParsed<RewindOptions>(o =>
                {
                    if (AudioNotLoaded()) return;
                    this.audio.Rewind(o.Seconds);
                    Console.WriteLine($"Audio rewound to {o.Seconds}(sec).\n");
                })
                .WithParsed<GetVolumeOptions>(_ => Console.WriteLine($"Volume Set To: {this.audio.Volume}\n"))
                .WithParsed<SetVolumeOptions>(o =>
                {
                    if (AudioNotLoaded()) return;
                    this.audio.Volume = o.Value;
                    Console.WriteLine($"Volume Set To: {o.Value}");
                })
                .WithParsed<GetPlaySpeedOptions>(_ =>
                {
                    if (AudioNotLoaded()) return;
                    Console.WriteLine($"Play Speed: {this.audio.PlaySpeed}\n");
                })
                .WithParsed<SetPlaySpeedOptions>(o =>
                {
                    if (AudioNotLoaded()) return;
                    this.audio.PlaySpeed = o.Value;
                    Console.WriteLine($"Set the speed to {o.Value}\n");
                })
                .WithParsed<GetLoopStatusOptions>(_ =>
                {
                    if (AudioNotLoaded()) return;
                    var loopStatus = this.audio.IsLooping ? "enabled" : "disabled";
                    Console.WriteLine($"Audio looping {loopStatus}.\n");
                })
                .WithParsed<ToggleLoopingOptions>(_ =>
                {
                    if (AudioNotLoaded()) return;
                    this.audio.IsLooping = !this.audio.IsLooping;
                    var loopStatus = this.audio.IsLooping ? "enabled" : "disabled";

                    Console.WriteLine($"Audio looping set to {loopStatus}.\n");
                })
                .WithParsed<ListAudioOptions>(ListAudio)
                .WithParsed<ListDevicesOptions>(ListDevices)
                .WithParsed<ChangeDeviceOptions>(ChangeDevice)
                .WithParsed<SetLibPathOptions>(SetLibPath)
                .WithParsed<LoadOptions>(Load)
                .WithParsed<UnloadOptions>(_ =>
                {
                    var fileName = Path.GetFileName(this.audio.FilePath);

                    this.isUnloading = true;

                    while (true)
                    {
                        if (this.isSleeping)
                        {
                            break;
                        }

                        Thread.Sleep(100);
                    }

                    this.audio.Dispose();
                    this.audio = null;
                    Console.Title = "No Sound Loaded";

                    WriteLine($"The audio file '{fileName}' has been unloaded.", enterBlankAfter: true);
                    this.isUnloading = false;
                })
                .WithParsed<ClearOptions>(_ =>
                {
                    Console.Clear();
                    Console.WriteLine("Type 'help' to see a list of commands.\n");
                })
                .WithParsed<ExitOption>(_ =>
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
                    this.audio?.Dispose();
                });
#pragma warning restore SA1503
        }
    }

    [SuppressMessage("csharpsquid", "S1172", Justification = "Parameter is required.")]
    private static void ListDevices(ListDevicesOptions o)
    {
        var deviceList = AudioDevice.AudioDevices;

        WriteLine("Audio Devices:", enterBlankBefore: true);

        for (var i = 0; i < deviceList.Length; i++)
        {
            WriteLine($"  {i + 1}: {Path.GetFileName(deviceList[i])}");
        }

        WriteLine();
    }

    [SuppressMessage("csharpsquid", "S1172", Justification = "Parameter is required.")]
    private static void ChangeDevice(ChangeDeviceOptions o)
    {
        var deviceNames = AudioDevice.AudioDevices;

        WriteLine("Enter a number to choose from the list of devices.\nUse 'q' to cancel.", enterBlankBefore: true);

        for (var i = 0; i < deviceNames.Length; i++)
        {
            WriteLine($"  {i + 1}: {deviceNames[i]}");
        }

        Write("Enter a device item number: ", enterBlankBefore: true);

        bool parseSuccess;

        do
        {
            var userInput = Console.ReadLine();

            if (userInput.Equals("q", StringComparison.CurrentCultureIgnoreCase))
            {
                WriteLine("Device change stopped.", enterBlankAfter: true);
                break;
            }

            parseSuccess = int.TryParse(userInput, out var chosenNumber);

            if (!parseSuccess)
            {
                WriteLine("Invalid device number.  Please use a number from the device list.");
                Write("Enter a device item number: ", enterBlankBefore: true);
                continue;
            }

            var index = chosenNumber - 1;
            var chosenDevice = deviceNames[index];

            AudioDevice.SetAudioDevice(chosenDevice);
            WriteLine($"The audio device set to '{chosenDevice}'.", enterBlankBefore: true, enterBlankAfter: true);
        }
        while (!parseSuccess);
    }

    private static string[] GetAudioFiles(string path)
    {
        var oggFiles = Directory.GetFiles(path, "*.ogg");
        var mp3Files = Directory.GetFiles(path, "*.mp3");

        var validFiles = new List<string>(oggFiles);
        validFiles.AddRange(mp3Files);

        return validFiles.ToArray();
    }

    private static void Write(string? msg = null, bool enterBlankBefore = false, bool enterBlankAfter = false)
    {
        if (string.IsNullOrEmpty(msg))
        {
            Console.WriteLine();
            return;
        }

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

    private static void WriteLine(string? msg = null, bool enterBlankBefore = false, bool enterBlankAfter = false)
    {
        if (string.IsNullOrEmpty(msg))
        {
            Console.WriteLine();
            return;
        }

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

    private bool AudioNotLoaded()
    {
        if (this.audio is null)
        {
            Console.WriteLine("No audio file is currently loaded.  Load a file first with the 'load' command.\n");
        }

        return this.audio is null;
    }

    [SuppressMessage("csharpsquid", "S1172", Justification = "Parameter is required.")]
    private void SetLibPath(SetLibPathOptions o)
    {
        if (!Directory.Exists(o.Path))
        {
            Console.WriteLine($"The music library path '{o.Path}' does not exist.");
            return;
        }

        this.audioLibDirPath = o.Path;
        Console.WriteLine($"\nThe library path has been set to '{this.audioLibDirPath}'");

        var audioFiles = GetAudioFiles(o.Path);
        var containsFiles = audioFiles.Length > 0;

        if (containsFiles)
        {
            var totalMp3Files = audioFiles.Count(f => f.ToLower().EndsWith(".mp3"));
            var totalOggFiles = audioFiles.Count(f => f.ToLower().EndsWith(".ogg"));

            Console.WriteLine($"   Total Files: {totalMp3Files + totalOggFiles}");
            Console.WriteLine($"   Total MP3 Files: {totalMp3Files}");
            Console.WriteLine($"   Total OGG Files: {totalOggFiles}\n");
        }
        else
        {
            var beforeClr = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"The music library path '{o.Path}' does not contain any audio files of type '.mp3' or '.ogg'.\n");
            Console.ForegroundColor = beforeClr;
        }
    }

    [SuppressMessage("csharpsquid", "S1172", Justification = "Parameter is required.")]
    private void ListAudio(ListAudioOptions o)
    {
        var audioFiles = GetAudioFiles(this.audioLibDirPath);

        WriteLine("Audio Library Files:", enterBlankBefore: true);
        foreach (var audioFile in audioFiles)
        {
            var fileName = Path.GetFileName(audioFile);

            Console.WriteLine($"  {fileName}");
        }

        WriteLine();
    }

    private void Load(LoadOptions o)
    {
        var soundList = GetAudioFiles(this.audioLibDirPath);

        if (!string.IsNullOrEmpty(o.Path))
        {
            if (!File.Exists(o.Path))
            {
                WriteLine($"The audio file '{o.Path}' does not exist.", enterBlankAfter: true);
                return;
            }

            LoadSound(o.Path, o.Type);
            return;
        }

        var loadType = o.Type == BufferType.Full
            ? "fully load" : "load as a stream";

        WriteLine($"Enter a number to choose from the list of sounds to {loadType}.\nUse 'q' to cancel.", enterBlankBefore: true);

        for (var i = 0; i < soundList.Length; i++)
        {
            WriteLine($"  {i + 1}: {Path.GetFileName(soundList[i])}");
        }

        Write("Enter a sound item number: ", enterBlankBefore: true);

        bool parseSuccess;

        do
        {
            var userInput = Console.ReadLine();

            if (userInput.Equals("q", StringComparison.CurrentCultureIgnoreCase))
            {
                WriteLine("Load process stopped.", enterBlankAfter: true);
                break;
            }

            parseSuccess = int.TryParse(userInput, out var chosenNumber);

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
                    if (this.isUnloading || this.audio is null)
                    {
                        this.isSleeping = true;
                        Thread.Sleep(250);
                        continue;
                    }

                    this.audioPosTokenSrc.Token.WaitHandle.WaitOne(250);

                    var minAndSec = $"{(int)Math.Floor(this.audio.Position.Minutes)}:{(int)Math.Round(this.audio.Position.Seconds, 0):D2}";
                    var currentPosSec = (int)Math.Round(this.audio.Position.TotalSeconds, 0);
                    var fileName = Path.GetFileName(this.audio.FilePath);
                    var totalMin = (int)Math.Floor(this.audio.Length.TotalSeconds / 60);
                    var lenRemainingSec = (int)Math.Round(this.audio.Length.TotalSeconds % 60, 0);
                    var totalSec = Math.Round(this.audio.Length.TotalSeconds, 0);
                    var bufferType = this.audio.BufferType == BufferType.Full ? "full" : "stream";

                    var timeStr = $"Time: {minAndSec} m:s | {currentPosSec} s";
                    var bufferStr = $"Buffer: {bufferType}";
                    var totalTimeStr = $"{totalMin}:{lenRemainingSec}";

                    Console.Title = $"{timeStr} | {totalSec} s | {fileName}({totalTimeStr} | {bufferStr})";
                }
            },
            this.audioPosTokenSrc.Token);

        this.audioPosTask.Start();
    }

    private void LoadSound(string soundFile, BufferType bufferType)
    {
        this.audio?.Dispose();
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
        this.isSleeping = false;
        this.isUnloading = false;
    }

    private void SetDefaultSoundFile()
    {
        var audioFileName = Path.GetFileName(GetAudioFiles(this.audioLibDirPath)[0]);
        var audioFilePath = $"{this.audioLibDirPath}/{audioFileName}";
        this.audio = new Audio(audioFilePath, BufferType.Stream);

        WriteLine($"Music Library set to '{this.audioLibDirPath}'.");
        WriteLine($"Default sound file set to '{audioFileName}'", enterBlankAfter: true);
    }
}
