<h1 align="center" style="color: mediumseagreen;font-weight: bold;">
CASL Preview Release Notes - v1.0.0-preview.21
</h1>

<h2 align="center" style="font-weight: bold;">Quick Reminder</h2>

<div align="center">

As with all software, there is always a chance for issues and bugs, especially for preview releases, so your input is greatly appreciated. 🙏🏼
</div>

<h2 align="center" style="font-weight: bold;">New Features ✨</h2>

1. [#381](https://github.com/KinsonDigital/CASL/issues/381) - Updated CASL to run on Linux operating systems.

<h2 align="center" style="font-weight: bold;">Bug Fixes 🐛</h2>

1. [#385](https://github.com/KinsonDigital/CASL/issues/385) - Fixed an issue with failing tests.
2. [#373](https://github.com/KinsonDigital/CASL/issues/373) - Fixed a bug where the native libraries were not being discovered with _**self-contained**_, and _**single-file**_ builds.
3. [#361](https://github.com/KinsonDigital/CASL/issues/361) - Fixed a bug with audio artifacts occurring at the end of an audio file when the audio file has been loaded as a stream.

<h2 align="center" style="font-weight: bold;">Breaking Changes 🧨</h2>

1. [#373](https://github.com/KinsonDigital/CASL/issues/373) - Removed support for 32bit (x86) builds of CASL.

<h2 align="center" style="font-weight: bold;">Dependency Updates 📦</h2>

1. [#384](https://github.com/KinsonDigital/CASL/pull/384) - Updated _**dotnet**_ to _**v9.0.0**_.
2. [#380](https://github.com/KinsonDigital/CASL/pull/380) - Updated the native _**OpenAL**_ library to _**v1.24.2**_.
3. [#372](https://github.com/KinsonDigital/CASL/pull/372) - Updated dependency _**coverlet.msbuild**_ to _**6.0.4**_.
4. [#371](https://github.com/KinsonDigital/CASL/pull/371) - Updated dependency _**coverlet.collector**_ to _**6.0.4**_.
5. [#370](https://github.com/KinsonDigital/CASL/pull/370) - Updated dependency _**xunit.runner.visualstudio**_ to _**v3.0.0**_.
6. [#368](https://github.com/KinsonDigital/CASL/pull/368) - Updated dependency _**microsoft.codeanalysis.netanalyzers**_ to _**v9.0.0**_.
7. [#367](https://github.com/KinsonDigital/CASL/pull/367) - Updated dependency _**system.io.abstractions**_ to _**21.3.1**_.
8. [#366](https://github.com/KinsonDigital/CASL/pull/366) - Updated dependency _**nsubstitute**_ to _**5.3.0**_.
9. [#365](https://github.com/KinsonDigital/CASL/pull/365) - Updated _**kinsondigital/infrastructure**_ action to _**v14.0.0**_.
10. [#364](https://github.com/KinsonDigital/CASL/pull/364) - Updated dependency _**xunit**_ to _**2.9.3**_.
11. [#363](https://github.com/KinsonDigital/CASL/pull/363) - Updated dependency _**fluentassertions**_ to _**6.12.2**_.
12. [#362](https://github.com/KinsonDigital/CASL/pull/362) - Updated dependency _**microsoft.net.test.sdk**_ to _**17.12.0**_.

<h2 align="center" style="font-weight: bold;">Other 🪧</h2>

1. [#377](https://github.com/KinsonDigital/CASL/issues/377) - Updated deno to latest version.
2. [#376](https://github.com/KinsonDigital/CASL/issues/376) - Removed the prepare release workflow.
