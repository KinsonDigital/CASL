<h1 align="center" style='color:mediumseagreen;font-weight:bold'>
    CASL Preview Release Notes - v1.0.0-preview.14
</h1>

<h2 align="center" style='font-weight:bold'>Quick Reminder</h2>

<div align="center">

As with all software, there is always a chance for issues and bugs, especially for preview releases, which is why your input is greatly appreciated. 🙏🏼
</div>

---

<h2 style="font-weight:bold" align="center">New Features ✨</h2>

1. [#144](https://github.com/KinsonDigital/CASL/issues/144) - Change the `Sound` class from `sealed` to unsealed.
   > **Note** This gives users the ability to inherit from the sound class to make it easier to adapt for their own systems.

---

<h2 style="font-weight:bold" align="center">Breaking Changes 🧨</h2>

1. [#120](https://github.com/KinsonDigital/CASL/issues/120) - Introduced the following breaking changes:
   - Changed the return type of the `AudioDevices` property in the `AudioDevice` class from `string[]` to `ImmutableArray<string>`.
   - Removed access to the `OggAudioDataStream` class by internalizing the type.
     > **Note** This class was not meant to be exposed as part of the public API.
   - Refactored the following exceptions from being unsealed to sealed.
     - `AudioException`
     - `SoundDataException`
     - `AudioDeviceDoestNotExistException`
     - `AudioDeviceManagerNotInitializedException`
   - `InitializeDeviceException`
     - `LoadLibraryException`
     - `StringNullOrEmptyException`
     - `UnknownPlatformException`
2. [#120](https://github.com/KinsonDigital/CASL/issues/120) - Made the following improvements:
   - Made all of the custom exceptions in the code base serializable.
        > **Note** This is best practice as it makes it much easier for multiple systems to consume and utilize the exceptions.
   - Changed how interoperability works with the native OpenAL libraries.
        > **Note** Interop was changed from the classic `[DllImport]` system to the new `[LibraryImport]` system introduced  
        > in dotnet v7.0.  This comes with performance benefits as well as allowing to be AOT compiled for systems that do not allow dynamic code generation. Go [here](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke-source-generation) for more information.

---

<h2 style="font-weight:bold" align="center">Internal Changes ⚙️</h2>
<h5 align="center">(Changes that do not affect users.  Not breaking changes, new features, or bug fixes.)</h5>

1. [#125](https://github.com/KinsonDigital/CASL/issues/125) - Refactored the entire code base from using block-scoped namespaces to file-scoped namespaces.

---

<h2 style="font-weight:bold" align="center">Nuget/Library Updates 📦</h2>

1. [#214](https://github.com/KinsonDigital/CASL/pull/214), [#218](https://github.com/KinsonDigital/CASL/pull/218), [#230](https://github.com/KinsonDigital/CASL/pull/230) - Updated the following dependencies:
    - Updated **microsoft.codeanalysis.netanalyzers** from _**v7.0.1**_ to _**v7.0.3**_
    - Updated **Microsoft.NET.Test.Sdk** from _**v17.6.0**_ to _**v17.6.3**_

---

<h2 style="font-weight:bold" align="center">Other 🪧</h2>
<h5 align="center">(Includes anything that does not fit into the categories above)</h5>

1. Changed the branching model for the project.
2. [#228](https://github.com/KinsonDigital/CASL/issues/228) - Fixed badges in the project readme to accommodate the new branching model and CICD changes.
3. [#223](https://github.com/KinsonDigital/CASL/issues/223) - Set up the project to utilize new pull requests to issue syncing system in the organization.
4. [#212](https://github.com/KinsonDigital/CASL/issues/212) - Removed old status check and release CICD system.
5. [#215](https://github.com/KinsonDigital/CASL/issues/215) - Created a new preview release workflow to replace the old [CICD](https://github.com/KinsonDigital/CICD) release system.
6. [#196](https://github.com/KinsonDigital/CASL/issues/196) - Created new build and test status check workflows.
7. [#168](https://github.com/KinsonDigital/CASL/issues/168) - Added rule to the solution-wide **.editorconfig** file.
8. [#76](https://github.com/KinsonDigital/CASL/issues/76) - Updated NuGet package authors.
9. [#120](https://github.com/KinsonDigital/CASL/issues/120) - Fixed various grammar and spelling issues with code documentation.