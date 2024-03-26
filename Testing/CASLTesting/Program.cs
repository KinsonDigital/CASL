// <copyright file="Program.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTesting;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
    private static readonly string AudioFilePath = $@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}{Path.DirectorySeparatorChar}{AudioDirName}{Path.DirectorySeparatorChar}";
    private static readonly char[] Numbers = new[]
    {
        '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
    };

    private static Task posTask;
    private static CancellationTokenSource posCancelTokenSrc;
    private static Sound testSound;

    public static void Main()
    {
        var bytesPerFloat = sizeof(float);
        var extension = ".mp3";
        // var extension = ".ogg";
        var fileName = "pathetic";
        var filePath = $@"K:\SOFTWARE-DEV\PERSONAL\CASL\Testing\CASLTesting\AudioFiles\{fileName}{extension}";
        testSound = new Sound(filePath, BufferType.Stream);

        UpdatePos();

        while (true)
        {
            var fastForwardRegex = new Regex("-f=[0-9]+");
            var rewindRegex = new Regex("-r=[0-9]+");
            var playRegex = new Regex("--play");
            var pauseRegex = new Regex("--pause");
            var resetRegex = new Regex("--reset");
            var setPosRegex = new Regex("--set-pos=(-|)[0-9]+");
            var getPosRegex = new Regex("--get-pos");
            var listDevicesRegex = new Regex("--list-devices");
            var changeDeviceRegex = new Regex("--change-device");
            var setVolumeLongRegex = new Regex("--set-volume=[0-9]+");
            var getVolumeRegex = new Regex("--get-volume");
            var setVolumeShortRegex = new Regex("-v=[0-9]+");
            var setPlaySpeedRegex = new Regex("--set-speed=[0-9]+");
            var getPlaySpeed = new Regex("--get-speed");
            var toggleLoopPosRegex = new Regex("--toggle-loop");
            var unloadSoundRegex = new Regex("--unload");

            var input = Console.ReadLine().ToLower();

            if (input.ToLower() == "-q")
            {
                break;
            }

            if (input.ToLower() == "cls")
            {
                Console.Clear();
            }
            else if (playRegex.IsMatch(input))
            {
                testSound.Play();
            }
            else if (pauseRegex.IsMatch(input))
            {
                testSound.Pause();
            }
            else if (fastForwardRegex.IsMatch(input))
            {
                if (int.TryParse(input.Split('=')[1], out var seconds))
                {
                    testSound.FastForward(seconds);
                }
            }
            else if (rewindRegex.IsMatch(input))
            {
                if (int.TryParse(input.Split('=')[1], out var seconds))
                {
                    testSound.Rewind(seconds);
                }
            }
            else if (resetRegex.IsMatch(input))
            {
                testSound.Reset();
            }
            else if (setPosRegex.IsMatch(input))
            {
                if (int.TryParse(input.Split('=')[1], out var seconds))
                {
                    testSound.SetTimePosition(seconds);
                }
            }
            else if (getPosRegex.IsMatch(input))
            {
                var position = testSound.Position;
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
                Console.WriteLine($"Volume: {testSound.Volume}");
            }
            else if (setVolumeLongRegex.IsMatch(input))
            {
                if (float.TryParse(input.Split('=')[1], out var volume))
                {
                    testSound.Volume = volume;
                }
            }
            else if (setVolumeShortRegex.IsMatch(input))
            {
                if (int.TryParse(input.Split('=')[1], out var volumeChange))
                {
                    testSound.Volume += volumeChange;
                }
            }
            else if (setPlaySpeedRegex.IsMatch(input))
            {
                if (float.TryParse(input.Split('=')[1], out var speed))
                {
                    testSound.PlaySpeed = speed;
                }
            }
            else if (getPlaySpeed.IsMatch(input))
            {
                Console.WriteLine($"Play Speed: {testSound.PlaySpeed}");
            }
            else if (toggleLoopPosRegex.IsMatch(input))
            {
                testSound.IsLooping = !testSound.IsLooping;

                if (testSound.IsLooping)
                {
                    Console.WriteLine("Looping enabled");
                }
                else
                {
                    Console.WriteLine("Looping disabled");
                }
            }
            else if (unloadSoundRegex.IsMatch(input))
            {
                testSound.Dispose();
            }
            else
            {
                Console.WriteLine($"The command '{input}' is invalid.");
            }
        }

        posCancelTokenSrc.Cancel();
        testSound.Dispose();

        return;


        var paused = false;
        var cancelTokenSrc = new CancellationTokenSource();
        var soundLibDirPath = AudioFilePath;


        Sound? sound;

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
            sound = new Sound(soundLibPath, BufferType.Full);

            WriteLine($"Music Library set to '{soundLibDirPath}'.");
            WriteLine($"Default sound file set to '{soundFileName}'", enterBlankAfter: true);
        }

        void LoadSound(string soundFile, BufferType bufferType)
        {
            paused = true;
            if (sound is not null)
            {
                sound.Reset();
                sound.Dispose();
            }

            sound = new Sound(soundFile, bufferType);
            WriteLine($"Loaded sound '{Path.GetFileName(soundFile)}'.");

            paused = false;
        }

        WriteLine("Type the command '-h | -help' for a list of commands.", enterBlankAfter: true);

        if (ValidMusicLibrary(soundLibDirPath))
        {
            SetDefaultSoundFile();
        }
        else
        {
            WriteLine($"No valid sound files exist at the music library path '{soundLibDirPath}'.", enterBlankAfter: true);
            WriteLine("Set a new music library path or type '-q' to exit.");

            while (true)
            {
                var libPathCommand = Console.ReadLine();

                if (string.IsNullOrEmpty(libPathCommand))
                {
                    WriteLine("Invalid command.  Type a path or '-q' to quit.");
                }
                else if (libPathCommand == "-q")
                {
                    return;
                }
                else
                {
                    if (Directory.Exists(libPathCommand))
                    {
                        if (ValidMusicLibrary(libPathCommand))
                        {
                            SetMusicLibDirPath(libPathCommand);
                            SetDefaultSoundFile();
                            break;
                        }
                        else
                        {
                            WriteLine("Not a valid music library path.  Must contain '.ogg' or '.mp3' files.");
                        }
                    }
                    else
                    {
                        WriteLine($"The path '{libPathCommand}' does not exist.\nPlease type a valid directory path.");
                    }
                }
            }
        }

        var getTimeTask = new Task(
            () =>
            {
                while (cancelTokenSrc.IsCancellationRequested is false)
                {
                    cancelTokenSrc.Token.WaitHandle.WaitOne(500);

                    if (paused)
                    {
                        continue;
                    }

                    var seconds = (int)sound.Position.Seconds;

                    var currentTimePos = $"{(int)sound.Position.Minutes}:{(seconds < 10 ? $"0{seconds}" : seconds)}";
                    var timeLen = $"{(int)sound.Length.Minutes}:{(int)sound.Length.Seconds}";

                    Console.Title = $"{currentTimePos} - {timeLen}";
                }
            }, cancelTokenSrc.Token);

        ShowHelp();

        getTimeTask.Start();

        var command = Console.ReadLine()?.ToLower().Trim() ?? string.Empty;

        sound.IsLooping = true;

        while (command != "-q" && command != "--quit")
        {
            if (command == "-h" || command == "--help")
            {
                ShowHelp();
            }
            else if (command == "-c" || command == "--clear")
            {
                Console.Clear();
            }
            else if (command.StartsWith("--set-lib"))
            {
                var sections = command.Split("=");

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
            else if (command.StartsWith("--load-sound"))
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
                var chosenSound = string.Empty;

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

                        chosenSound = soundList[chosenNumber];
                        LoadSound(chosenSound, BufferType.Full);
                    }
                }
                while (isNumber is false);

                WriteLine($"The audio device set to '{chosenSound}'.", enterBlankBefore: true, enterBlankAfter: true);
            }
            else if (command == "--play")
            {
                sound.Play();
            }
            else if (command == "--pause")
            {
                sound.Pause();
            }
            else if (command == "-s" || command == "--stop")
            {
                sound.Reset();
            }
            else if (command.StartsWith("-f=") || command.StartsWith("--forward="))
            {
                var sections = command.Split('=', StringSplitOptions.RemoveEmptyEntries);

                if (sections.Length < 2)
                {
                    WriteLine("The fast forward command contains no value.", enterBlankAfter: true);
                }
                else
                {
                    float.TryParse(sections[1], out var seconds);

                    sound.FastForward(seconds);
                }
            }
            else if (command.StartsWith("-r=") || command.StartsWith("--rewind="))
            {
                var sections = command.Split('=', StringSplitOptions.RemoveEmptyEntries);

                if (sections.Length < 2)
                {
                    WriteLine("The rewind command contains no value.", enterBlankAfter: true);
                }
                else
                {
                    float.TryParse(sections[1], out var seconds);

                    sound.Rewind(seconds);
                }
            }
            else if (command.StartsWith("-v+") || command.StartsWith("--volume+") ||
                     command.StartsWith("-v-") || command.StartsWith("--volume-") ||
                     command.StartsWith("-v=") || command.StartsWith("--volume="))
            {
                var sections = Array.Empty<string>();
                var volumeIncrease = false;
                var volumeDecrease = false;
                var volumeSet = false;

                if (command.StartsWith("-v+"))
                {
                    sections = command.Split("v+");
                    volumeIncrease = true;
                }
                else if (command.StartsWith("--volume+"))
                {
                    sections = command.Split("volume+");
                    volumeIncrease = true;
                }
                else if (command.StartsWith("-v-"))
                {
                    sections = command.Split("v-");
                    volumeDecrease = true;
                }
                else if (command.StartsWith("--volume-"))
                {
                    sections = command.Split("volume-");
                    volumeDecrease = true;
                }
                else if (command.StartsWith("-v="))
                {
                    sections = command.Split("v=");
                    volumeSet = true;
                }
                else if (command.StartsWith("--volume="))
                {
                    sections = command.Split("volume=");
                    volumeSet = true;
                }

                if (sections.Length < 2)
                {
                    WriteLine("The volume command contains no value.", enterBlankAfter: true);
                }
                else
                {
                    float.TryParse(sections[1], out var volume);

                    if (volumeIncrease)
                    {
                        sound.Volume += volume;
                    }
                    else if (volumeDecrease)
                    {
                        sound.Volume -= volume;
                    }
                    else if (volumeSet)
                    {
                        sound.Volume = volume;
                    }
                }
            }
            else if (command.StartsWith("--looping"))
            {
                var sections = command.Split("=");

                if (sections.Length == 1)
                {
                    WriteLine($"Sound looping turned {(sound.IsLooping ? "on" : "off")}.");
                }
                else
                {
                    var parseSuccess = bool.TryParse(sections[1].ToLower(), out var shouldLoop);

                    if (parseSuccess)
                    {
                        sound.IsLooping = shouldLoop;

                        WriteLine($"Looping has been turned {(shouldLoop ? "on" : "off")}.");
                    }
                    else
                    {
                        WriteLine("The '--looping' command must have a 'true' or 'false' value.");
                    }
                }
            }
            else if (command.StartsWith("--play-speed"))
            {
                var sections = command.Split("=");

                if (sections.Length == 1)
                {
                    WriteLine($"Play speed set to {sound.PlaySpeed}.");
                }
                else
                {
                    var parseSuccess = float.TryParse(sections[1].ToLower(), out var playSpeed);

                    if (parseSuccess)
                    {
                        sound.PlaySpeed = playSpeed;

                        WriteLine($"Play speed has been set to {playSpeed}.");
                    }
                    else
                    {
                        WriteLine("The '--play-speed' command must have a floating point value.");
                    }
                }
            }
            else if (command.StartsWith("--set-pos"))
            {
                var sections = command.Split("=");

                if (sections.Length >= 2)
                {
                    var timePos = sections[1];

                    var timePosSections = timePos.Split(":");

                    if (timePosSections.Length >= 2)
                    {
                        var minuteParseSuccess = int.TryParse(timePosSections[0], out var minute);
                        var secondParseSuccess = int.TryParse(timePosSections[1], out var second);

                        if (minuteParseSuccess is false || secondParseSuccess is false)
                        {
                            WriteLine("The minute and second value must be an integer value.", enterBlankAfter: true);
                        }
                        else
                        {
                            sound.SetTimePosition((minute * 60) + second);
                        }
                    }
                    else
                    {
                        WriteLine("The value for the set position command must be in the minute and second format of 'mm:ss'.", enterBlankAfter: true);
                    }
                }
                else
                {
                    WriteLine("The set position command contains no value.", enterBlankAfter: true);
                }
            }
            else if (command == "-l" || command == "--list")
            {
                var deviceList = AudioDevice.AudioDevices;

                WriteLine("System Audio Devices:", enterBlankBefore: true);

                foreach (var device in deviceList)
                {
                    TabbedWriteLine(device);
                }

                WriteBlank();
            }
            else if (command == "--choose-device")
            {
                var deviceList = AudioDevice.AudioDevices;

                WriteLine("Enter a number to choose from the list of devices.");
                WriteLine("Enter 'q' to stop the choose device process.", enterBlankBefore: true);

                for (var i = 0; i < deviceList.Length; i++)
                {
                    TabbedWriteLine($"{i}: {deviceList[i]}");
                }

                Write("Enter a device item number: ", enterBlankBefore: true);

                var isNumber = false;
                var chosenDevice = string.Empty;

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

                        chosenDevice = deviceList[chosenNumber];

                        AudioDevice.SetAudioDevice(chosenDevice);
                    }
                }
                while (isNumber is false);

                WriteLine($"The audio device set to '{chosenDevice}'.", enterBlankBefore: true, enterBlankAfter: true);
            }
            else
            {
                WriteLine($"The command '{command}' is invalid.", enterBlankBefore: true, enterBlankAfter: true);
            }

            command = Console.ReadLine()?.ToLower().Trim() ?? string.Empty;
        }

        cancelTokenSrc.Cancel();
        cancelTokenSrc.Dispose();
        getTimeTask.Wait();
        getTimeTask.Dispose();

        WriteLine("Disposing of audio service. . .", true);

        sound.Dispose();

        WriteLine("Audio services disposed.", true);
    }

    private static void UpdatePos()
    {
        posCancelTokenSrc = new CancellationTokenSource();

        posTask = new Task(
            () =>
            {
                while (!posCancelTokenSrc.IsCancellationRequested)
                {
                    posCancelTokenSrc.Token.WaitHandle.WaitOne(250);

                    var minutes = (int)Math.Floor(testSound.Position.Minutes);
                    var seconds = (int)Math.Round(testSound.Position.Seconds, 0);
                    var minSec = $"{minutes}:{seconds}";
                    var totalSeconds = (int)Math.Round(testSound.Position.TotalSeconds, 0);

                    Console.Title = $"{minSec} |  Total Secs: {totalSeconds}";
                }
            }, posCancelTokenSrc.Token);

        posTask.Start();
    }

    private static void ShowHelp()
    {
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
