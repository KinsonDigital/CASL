// <copyright file="Program.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTesting;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CASL;
using CASL.Devices;

/// <summary>
/// The main entry point for the application.
/// </summary>
public static class Program
{
    private const string AudioDirName = "AudioFiles";
    private static readonly string DefaultAudioLibDirPath = $@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}{Path.DirectorySeparatorChar}{AudioDirName}{Path.DirectorySeparatorChar}";
    private static readonly char[] Numbers = new[]
    {
        '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
    };

    private static Task audioPosTask;
    private static CancellationTokenSource audioPosTokenSrc;
    private static Audio? audio;
    private static StringBuilder helpText = new StringBuilder();

    public static void Main()
    {
        SetDefaultSoundFile();

        var fastForwardRegex = new Regex("(-f|--fast-forward)=[0-9]+");
        AddToHelp("Fast forwards the audio by a given value in seconds.", fastForwardRegex);

        var rewindRegex = new Regex("(-r|--rewind)=[0-9]+");
        AddToHelp("Rewinds the audio by a given value in seconds.", rewindRegex);

        var playRegex = new Regex("--play");
        AddToHelp("Plays the audio.", playRegex);

        var pauseRegex = new Regex("--pause");
        AddToHelp("Pauses the audio.", pauseRegex);

        var resetRegex = new Regex("--reset");
        AddToHelp("Stops the audio and resets the position back to the begining.", resetRegex);

        var setPosRegex = new Regex("--set-pos=(-|)[0-9]+");
        AddToHelp("Sets the position in the audio by a given value in seconds.", setPosRegex);

        var getPosRegex = new Regex("--get-pos");
        AddToHelp("Gets the current position in the audio.", getPosRegex);

        var listDevicesRegex = new Regex("--list-devices");
        AddToHelp("Lists the available audio devices.", listDevicesRegex);

        var changeDeviceRegex = new Regex("--change-device");
        AddToHelp("Starts the change audio device process.", changeDeviceRegex);

        var getVolumeRegex = new Regex("(-v|--get-volume)");
        AddToHelp("Gets the current volume setting of the audio.", getVolumeRegex);

        var setVolumeRegex = new Regex("(-v|--set-volume)=[0-9]+");
        AddToHelp("Sets the audio to a given value.", setVolumeRegex);

        var setPlaySpeedRegex = new Regex("--set-speed=[0-9]+");
        AddToHelp("Sets the playback speed of the audio.", setPlaySpeedRegex);

        var getPlaySpeed = new Regex("--get-speed");
        AddToHelp("Gets the current playback speed of the audio.", getPlaySpeed);

        var toggleLoopPosRegex = new Regex("--toggle-loop");
        AddToHelp("Toggles the audio looping setting.", toggleLoopPosRegex);

        var loadSoundRegex = new Regex("--load-sound");
        AddToHelp("Starts the load audio process.", loadSoundRegex);

        var unloadSoundRegex = new Regex("--unload");
        AddToHelp("Unloads the currently loaded audio.", unloadSoundRegex);

        var setLibPathRegex = new Regex("--set-lib-path=.+");
        AddToHelp("Sets the audio library directory path.", setLibPathRegex);

        var clearScreenRegex = new Regex("(-c|--clear)");
        AddToHelp("Clears the screen.", clearScreenRegex);

        var showHelpRegex = new Regex("(-h|--help)");
        AddToHelp("Shows help.", showHelpRegex);

        var quitRegex = new Regex("(-q|--quit)");
        AddToHelp("Quits the application.", quitRegex);

        audio = new Audio(filePath, BufferType.Stream);

        var soundLibDirPath = DefaultAudioLibDirPath;

        WriteLine("Type the command '-h | -help' for a list of commands.", enterBlankAfter: true);

        StartPosUpdater();

        while (true)
        {
            var input = Console.ReadLine().ToLower();

            if (quitRegex.IsMatch(input))
            {
                break;
            }

            if (showHelpRegex.IsMatch(input))
            {
                ShowHelp();
            }
            else if (clearScreenRegex.IsMatch(input))
            {
                Console.Clear();
            }
            else if (playRegex.IsMatch(input))
            {
                audio.Play();
            }
            else if (pauseRegex.IsMatch(input))
            {
                audio.Pause();
            }
            else if (fastForwardRegex.IsMatch(input))
            {
                if (int.TryParse(input.Split('=')[1], out var seconds))
                {
                    audio.FastForward(seconds);
                }
            }
            else if (rewindRegex.IsMatch(input))
            {
                if (int.TryParse(input.Split('=')[1], out var seconds))
                {
                    audio.Rewind(seconds);
                }
            }
            else if (resetRegex.IsMatch(input))
            {
                audio.Reset();
            }
            else if (setPosRegex.IsMatch(input))
            {
                if (int.TryParse(input.Split('=')[1], out var seconds))
                {
                    audio.SetTimePosition(seconds);
                }
            }
            else if (getPosRegex.IsMatch(input))
            {
                var position = audio.Position;
                var minutes = Math.Floor(position.Minutes);
                var seconds = Math.Round(position.Seconds, 0);
                var totalSeconds = Math.Round(position.TotalSeconds, 0);

                Console.WriteLine($"Position: {minutes}:{seconds} | Total Seconds: {totalSeconds}");
            }
            else if (listDevicesRegex.IsMatch(input))
            {
                var deviceNames = AudioDevice.AudioDevices;

                WriteBlank();

                for (var i = 0; i < deviceNames.Length; i++)
                {
                    WriteLine($"{i}: {deviceNames[i]}");
                }
            }
            else if (changeDeviceRegex.IsMatch(input))
            {
                var deviceNames = AudioDevice.AudioDevices;

                WriteLine("Enter a number to choose from the list of devices.");
                WriteLine("Enter 'q' to stop the choose device process.", enterBlankBefore: true);

                for (var i = 0; i < deviceNames.Length; i++)
                {
                    TabbedWriteLine($"{i}: {deviceNames[i]}");
                }

                Console.WriteLine(string.Join("\n", deviceNames));

                Write("Enter a device item number: ", enterBlankBefore: true);

                var isNumber = false;

                do
                {
                    var numberInput = Console.ReadLine();

                    if (numberInput == "q")
                    {
                        break;
                    }

                    if (string.IsNullOrEmpty(numberInput) || numberInput.Length > 1)
                    {
                        WriteLine("Invalid device number.  Please use a number from the device list.");
                        continue;
                    }

                    var deviceNumber = numberInput[0];

                    isNumber = Numbers.Contains(numberInput[0]);

                    if (isNumber)
                    {
                        int.TryParse(deviceNumber.ToString(), out var chosenNumber);

                        var chosenDevice = deviceNames[chosenNumber];

                        AudioDevice.SetAudioDevice(chosenDevice);
                        WriteLine($"The audio device set to '{chosenDevice}'.", enterBlankBefore: true, enterBlankAfter: true);
                    }
                }
                while (!isNumber);
            }
            else if (getVolumeRegex.IsMatch(input))
            {
                Console.WriteLine($"Volume: {audio.Volume}");
            }
            else if (setVolumeRegex.IsMatch(input))
            {
                if (float.TryParse(input.Split('=')[1], out var volume))
                {
                    audio.Volume = volume;
                }
            }
            else if (setVolumeRegex.IsMatch(input))
            {
                if (int.TryParse(input.Split('=')[1], out var volumeChange))
                {
                    audio.Volume += volumeChange;
                }
            }
            else if (setPlaySpeedRegex.IsMatch(input))
            {
                if (float.TryParse(input.Split('=')[1], out var speed))
                {
                    audio.PlaySpeed = speed;
                }
            }
            else if (getPlaySpeed.IsMatch(input))
            {
                Console.WriteLine($"Play Speed: {audio.PlaySpeed}");
            }
            else if (toggleLoopPosRegex.IsMatch(input))
            {
                audio.IsLooping = !audio.IsLooping;

                if (audio.IsLooping)
                {
                    Console.WriteLine("Looping enabled");
                }
                else
                {
                    Console.WriteLine("Looping disabled");
                }
            }
            else if (loadSoundRegex.IsMatch(input))
            {
                var soundList = GetValidFiles(soundLibDirPath);

                WriteLine("Enter a number to choose from the list of sounds.", enterBlankBefore: true);
                WriteLine("Enter 'q' to stop the load sound process.", enterBlankBefore: true);

                for (var i = 0; i < soundList.Length; i++)
                {
                    TabbedWriteLine($"{i}: {Path.GetFileName(soundList[i])}");
                }

                Write("Enter a sound item number: ", enterBlankBefore: true);

                var isNumber = false;

                do
                {
                    var numberInput = Console.ReadLine();

                    if (numberInput == "q")
                    {
                        break;
                    }

                    if (string.IsNullOrEmpty(numberInput) || numberInput.Length > 1)
                    {
                        WriteLine("Invalid sound number.  Please use a number from the sound list.");
                        continue;
                    }

                    var soundNumber = numberInput[0];

                    isNumber = Numbers.Contains(numberInput[0]);

                    if (isNumber)
                    {
                        int.TryParse(soundNumber.ToString(), out var chosenNumber);

                        var chosenSound = soundList[chosenNumber];
                        LoadSound(chosenSound, BufferType.Full);
                    }
                }
                while (!isNumber);
            }
            else if (unloadSoundRegex.IsMatch(input))
            {
                audio.Dispose();
            }
            else if (setLibPathRegex.IsMatch(input))
            {
                var sections = input.Split("=");

                if (sections.Length < 2)
                {
                    WriteLine("The set music library command does not contain a path.", enterBlankAfter: true);
                }
                else
                {
                    var path = sections[1];

                    // Make sure that the path does not start or end with single or double quotes
                    path = path.StartsWith('\"') ? path[1..] : path;
                    path = path.StartsWith('\'') ? path[1..] : path;
                    path = path.EndsWith('\"') ? path[..^1] : path;
                    path = path.EndsWith('\'') ? path[..^1] : path;

                    if (ValidMusicLibrary(path))
                    {
                        SetMusicLibDirPath(path);
                        SetDefaultSoundFile();
                    }
                    else
                    {
                        WriteLine($"The music library path '{path}' does not exist.", enterBlankAfter: true);
                    }
                }
            }
            else
            {
                Console.WriteLine($"The command '{input}' is invalid.");
            }
        }

        audioPosTokenSrc.Cancel();
        audio.Dispose();

        string[] GetValidFiles(string path)
        {
            var oggFiles = Directory.GetFiles(path, "*.ogg");
            var mp3Files = Directory.GetFiles(path, "*.mp3");

            var validFiles = new List<string>(oggFiles);
            validFiles.AddRange(mp3Files);

            return validFiles.ToArray();
        }

        bool ValidMusicLibrary(string path)
        {
            var validFiles = GetValidFiles(path);

            return validFiles.Length > 0;
        }

        void SetMusicLibDirPath(string path) => soundLibDirPath = path.EndsWith(@"\") ? path : $@"{path}\";

        void SetDefaultSoundFile()
        {
            var soundFileName = Path.GetFileName(GetValidFiles(soundLibDirPath)[0]);
            var soundLibPath = $"{soundLibDirPath}{soundFileName}";
            audio = new Audio(soundLibPath, BufferType.Stream);

            WriteLine($"Music Library set to '{soundLibDirPath}'.");
            WriteLine($"Default sound file set to '{soundFileName}'", enterBlankAfter: true);
        }

        void LoadSound(string soundFile, BufferType bufferType)
        {

        }
    }

    public static void AddToHelp(string description, Regex regex)
    {
        helpText.AppendLine();
        var regexStr = regex.ToString();
        var requiresValue = regexStr.Contains('=') && (regexStr.Contains("[0-9]+") || regexStr.Contains(".+"));
        var hasMultipleVersions = regexStr.Contains('(') && regexStr.Contains('|') && regexStr.Contains(')');

        regexStr = regexStr.Replace("(-|)", string.Empty);
        regexStr = regexStr.Replace("[0-9]+", string.Empty);
        regexStr = regexStr.Replace("(", string.Empty);
        regexStr = regexStr.Replace(")", string.Empty);
        regexStr = regexStr.Replace("=", string.Empty);
        regexStr = regexStr.Replace(".+", string.Empty);

        if (hasMultipleVersions)
        {
            var sections = regexStr.Split('|');

            var joinStr = requiresValue ? "=<value>, " : ", ";
            regexStr = string.Join(joinStr, sections);
        }

        regexStr += requiresValue ? "=<value>" : string.Empty;

        helpText.AppendLine(regexStr);
        helpText.AppendLine($"  {description}");
    }

    private static void StartPosUpdater()
    {
        audioPosTokenSrc = new CancellationTokenSource();

        audioPosTask = new Task(
            () =>
            {
                while (!audioPosTokenSrc.IsCancellationRequested)
                {
                    audioPosTokenSrc.Token.WaitHandle.WaitOne(250);

                    var minutes = (int)Math.Floor(audio.Position.Minutes);
                    var seconds = (int)Math.Round(audio.Position.Seconds, 0);
                    var minSec = $"{minutes}:{seconds}";
                    var totalSeconds = (int)Math.Round(audio.Position.TotalSeconds, 0);

                    Console.Title = $"{minSec} |  Total Secs: {totalSeconds}";
                }
            }, audioPosTokenSrc.Token);

        audioPosTask.Start();
    }

    private static void ShowHelp()
    {
        Console.WriteLine(helpText.ToString());
        return;
        WriteBlank();

        WriteLine("Commands:");

        TabbedWrite("Shows a list of commands: ");
        TabbedWriteLine("-h | --help", 3);

        TabbedWrite("Clears the screen: ");
        TabbedWriteLine("-c | --clear", 4);

        TabbedWrite("Set Library Location: ");
        TabbedWriteLine("--set-lib=<music library path>", 4);

        TabbedWrite("Load Sound: ");
        TabbedWriteLine("--load-sound", 5);

        TabbedWrite("Plays the song: ");
        TabbedWriteLine("--play", 4);

        TabbedWrite("Pauses the song: ");
        TabbedWriteLine("--pause", 4);

        TabbedWrite("Stops the song: ");
        TabbedWriteLine("-s | --stop", 4);

        TabbedWrite("Turn looping on or off: ");
        TabbedWriteLine("--looping[=<true | false]", 3);
        TabbedWriteLine("No value displays the current status.", 8);

        TabbedWrite("Set the play speed: ");
        TabbedWriteLine("--play-speed[=<value>]", 4);
        TabbedWriteLine("No value displays the current play speed.", 8);

        TabbedWrite("Fast forward the song: ");
        TabbedWriteLine("-f=<value> | --forward=<value>", 4);

        TabbedWrite("Rewind the song: ");
        TabbedWriteLine("-r=<value> | --rewind=<value>", 4);

        TabbedWrite("Increase Volume: ");
        TabbedWriteLine("-v<+ | -><value> | --volume<+ | -><value>", 4);

        TabbedWrite("Set Position: ");
        TabbedWriteLine("--set-pos=<mm:ss>", 5);

        TabbedWrite("Gets a list of audio devices: ");
        TabbedWriteLine("-l | --list", 3);

        TabbedWrite("Starts the process of choosing a device: ");
        TabbedWriteLine("--choose-device");

        TabbedWrite("Quits the application: ");
        TabbedWriteLine("-q | --quit", 4);

        WriteBlank();
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

    private static void TabbedWrite(string msg, int tabCount = 1, bool enterBlankBefore = false, bool enterBlankAfter = false)
    {
        for (var i = 0; i < tabCount; i++)
        {
            msg = $"\t{msg}";
        }

        Write(msg, enterBlankBefore, enterBlankAfter);
    }

    private static void TabbedWriteLine(string msg, int tabCount = 1, bool enterBlankBefore = false, bool enterBlankAfter = false)
    {
        for (var i = 0; i < tabCount; i++)
        {
            msg = $"\t{msg}";
        }

        WriteLine(msg, enterBlankBefore, enterBlankAfter);
    }
}
