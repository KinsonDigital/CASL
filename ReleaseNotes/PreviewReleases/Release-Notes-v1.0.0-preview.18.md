<h1 align="center" style="color: mediumseagreen;font-weight: bold;">
CASL Preview Release Notes - v1.0.0-preview.18
</h1>

<h2 align="center" style="font-weight: bold;">Quick Reminder</h2>

<div align="center">

As with all software, there is always a chance for issues and bugs, especially for preview releases, which is why your input is greatly appreciated. 🙏🏼
</div>

<h2 align="center" style="font-weight: bold;">New Features ✨</h2>

1. [#158](https://github.com/KinsonDigital/CASL/issues/158) - Added the ability to process sounds as a stream directly from the audio on disk.
2. [#158](https://github.com/KinsonDigital/CASL/issues/158) - Added a new property to the `Sound` class, (now named `Audio`) named `BufferType` to see what type of buffer the audio was set to upon creation.
3. [#158](https://github.com/KinsonDigital/CASL/issues/158) - Added the ability to retain loop settings after switching to a new device.

<h2 align="center" style="font-weight: bold;">Bug Fixes 🐛</h2>

1. [#158](https://github.com/KinsonDigital/CASL/issues/158) - Fixed a bug where if the sound has never been played or was stopped in addition to the play speed being set to a value other than the default value, the sound would not retain its original play speed when switching audio devices.

<h2 align="center" style="font-weight: bold;">Breaking Changes 🧨</h2>

1. [#158](https://github.com/KinsonDigital/CASL/issues/158) - Introduced the following breaking changes:
   - Refactored the name of the `SoundDataException` to `AudioDataException`.
   - Changed the `ALDevice` struct to `internal`.
   - Changed the `ALContext` struct to `internal`.
   - Changed the following enums to `internal`.  These were not meant to be part of the public API.
     - `AlcContextAttributes`
     - `AlcContextAttributes`
     - `AlcGetStringList`
     - `ALGetString`
     - `AlcError`
   - Changed the name of the `ISound` interface to `IAudio` for a more clear and concise public API.
   - Changed the name of the `Sound` class to `Audio` for a more clear and concise public API.
   - Added a parameter to the `Sound` class to choose the type of buffer.
   - Removed the `Sound.Stop` method due to confusion between stop and reset.
   - Refactored the name of the `SoundTime` struct to `AudioTime`.
    - Refactored the `SoundTime`, (now named `AudioTime`) to a read-only record struct for a more clear and concise public API.
   - Refactored the `SoundState` enum to `AudioState` for a more clear and concise public API.
   - Removed deprecated serialization implementation for the following exceptions across the project.  
     NOTE: This is to avoid using the deprecated serialization methods that may pose security vulnerabilities or compatibility issues in the future.
     - `UnknownPlatformException`
     - `StringNullOrEmptyException`
     - `LoadLibraryException`
     - `AudioException`
     - `InitializeDeviceException`
     - `AudioDeviceManagerNotInitializedException`
     - `AudioDeviceDoesNotExistException`
     - `AudioDataException`

<h2 align="center" style="font-weight: bold;">Dependency Updates 📦</h2>

1. [#321](https://github.com/KinsonDigital/CASL/pull/321), [#333](https://github.com/KinsonDigital/CASL/pull/333), [#343](https://github.com/KinsonDigital/CASL/pull/343) - Updated dependency _**system.io.abstractions**_ to _**v21.0.0**_
2. [#331](https://github.com/KinsonDigital/CASL/pull/331), [#327](https://github.com/KinsonDigital/CASL/pull/327) - Updated dependency _**simpleinjector**_ to _**v5.4.4**_
3. [#330](https://github.com/KinsonDigital/CASL/pull/330), [#339](https://github.com/KinsonDigital/CASL/pull/339), [#329](https://github.com/KinsonDigital/CASL/pull/329), [#322](https://github.com/KinsonDigital/CASL/pull/322) - Updated dependency _**xunit**_ to _**v2.7.0**_
4. [#339](https://github.com/KinsonDigital/CASL/pull/339), [#329](https://github.com/KinsonDigital/CASL/pull/329), [#322](https://github.com/KinsonDigital/CASL/pull/322) - Updated dependency _**xunit.runner.visualstudio**_ to _**v2.5.6**_
5. [#325](https://github.com/KinsonDigital/CASL/pull/325), [#326](https://github.com/KinsonDigital/CASL/pull/326), [#328](https://github.com/KinsonDigital/CASL/pull/328) - Updated _**kinsondigital/infrastructure action**_ to _**v13.6.3**_
6. [#341](https://github.com/KinsonDigital/CASL/pull/341) - Updated dependency _**coverlet.msbuild**_ to _**v6.0.2**_
7. [#340](https://github.com/KinsonDigital/CASL/pull/340) - Updated dependency _**coverlet.collector**_ to _**v6.0.2**_
8. [#336](https://github.com/KinsonDigital/CASL/pull/336) - Updated dependency _**microsoft.net.test.sdk**_ to _**v17.9.0**_

<h2 align="center" style="font-weight: bold;">Other 🪧</h2>

1. [#345](https://github.com/KinsonDigital/CASL/issues/345) - Finished replacing _**moq**_ code with _**nsubstitute**_ code.
2. [#332](https://github.com/KinsonDigital/CASL/issues/332) - Updated the README.
3. [#303](https://github.com/KinsonDigital/CASL/issues/303) - Removed Assert Extensions.
