<h1 align="center">
    <a href="#"><img align="center" src="./Documentation/Images/casl-logo.png" height="96"></a>
    <br />
</h1>

<h1 style="font-weight:bold" align="center">CASL</h1>

<div align="center">

![](https://img.shields.io/github/workflow/status/KinsonDigital/CASL/%F0%9F%9A%80Preview%20Release?label=QA%20Release%20%F0%9F%9A%80&logo=GitHub&style=plastic)
![](https://img.shields.io/github/workflow/status/KinsonDigital/CASL/%F0%9F%9A%80Preview%20Release?label=Preview%20Release%20%F0%9F%9A%80&logo=GitHub&style=plastic)
![](https://img.shields.io/github/workflow/status/KinsonDigital/CASL/%F0%9F%9A%80Production%20Release?label=Production%20Release%20%F0%9F%9A%80&logo=GitHub&style=plastic)

![](https://img.shields.io/codecov/c/github/KinsonDigital/CASL/master?label=Code%20Coverage&logo=CodeCov&style=plastic)
[![Contributor Covenant](https://img.shields.io/badge/Contributor%20Covenant-2.0-4baaaa.svg?style=plastic)](code_of_conduct.md)

![](https://img.shields.io/nuget/v/KinsonDigital.CASL?logo=nuget&style=plastic)

</div>

<h2 style="font-weight:bold" align="center" >!! NOTICE !!</h2>

This library is still under development and is not at v1.0.0 yet!!  However, all of the major features are available, so we ensourage you to use it and provide feedback.  That is what open source is all about 🥳 

<h2 style="font-weight:bold" align="center">📖 About CASL</h2>

Pronounced *Castle*, **CASL** is an acronym for (C)ross-platform (A)udio and (S)ound (L)ibrary

**CASL** is an easy to use, cross-platform, dotnet library for playing and managing sounds powered by **OpenAL 1.1** using software rendering.  This is accomplished by using low level bindings to the native **OpenAL** library.

This can be used for applications such as video games, sound players, and works on any dotnet compliant platform.

<h2 style="font-weight:bold" align="center">✨Features</h2>

### Audio Formats

Currently this library can play **.ogg** and **.mp3** file formats.  There are plans to support **.wav** files in the future as well as other possible sound formats.

### Cross-Platform

We strive for **CASL** to be a cross platform library by running under the latest possible dotnet version.  

<h2 style="font-weight:bold" align="center">🔧Maintainers</h2>

We currently have the following maintainers:
- [![twitter-logo](https://raw.githubusercontent.com/KinsonDigital/CASL/release/v1.0.0/Documentation/Images/twitter-logo-16x16.svg)Calvin Wilkinson](https://twitter.com/KDCoder) (KinsonDigital GitHub Organization - Owner)
- [![twitter-logo](https://raw.githubusercontent.com/KinsonDigital/CASL/release/v1.0.0/Documentation/Images/twitter-logo-16x16.svg)Kristen Wilkinson](https://twitter.com/kswilky) (KinsonDigital GitHub Organization - Documentation Maintainer & Tester)

<h2 style="font-weight:bold" align="center">🙏🏼Contributing</h2>

**CASL** encourages and uses [Early Pull Requests](https://medium.com/practical-blend/pull-request-first-f6bb667a9b6). Please don't wait until you're finished with your work before creating a PR.

1. Fork the repository
2. Create a feature branch following the feature branch section in the documentation [here](./Documentation/Branching.md)
3. Add an empty commit to the new feature branch to start your work off.
   - Use this git command: `git commit --allow-empty -m "start of [thing you're working on]"`
4. Once you've pushed a commit, open a [**draft pull request**](https://github.blog/2019-02-14-introducing-draft-pull-requests/). Do this **before** you start working.
5. Make your commits in small, incremental steps with clear descriptions.
6. All unit tests must pass before a PR will be completed.
7. Make sure that the code follows the the coding standards.
   - Pay attention to the warnings in **Visual Studio**!!
   - Refer to the *.editorconfig* files in the code base for rules
8. Tag a maintainer when you're done and ask for a review!

If you have any questions, contact a project maintainer

<h2 style="font-weight:bold" align="center">Practices</h2>

1. The code base is highly tested using unit testing with a high level of code coverage.  When contributing, make sure to add or adjust the unit tests appropriately regarding your changes.
2. We use a combination of [StyleCop](https://github.com/DotNetAnalyzers/StyleCopAnalyzers) and [Microsoft.CodeAnalysis.NetAnalyzers](https://github.com/dotnet/roslyn-analyzers) libraries for maintaining coding standards.
   - We understand that there are some exceptions to the rule and not all coding standards fit every situation.  In these scenarios, contact a maintainer and lets discuss it!!  Warnings can always be suppressed if need be.
- We use [semantic versioning 2.0](https://semver.org/) for versioning.

<h2 style="font-weight:bold" align="center">Further Resources</h2>

- A sample project **CASLTesting** can be found in the [Testing Folder](https://github.com/KinsonDigital/CASPL/tree/master/Testing/CASLTesting)
  - This project serves the purpose of a sample project as well as a simple way to do manual testing of the library
- Goto [OpenAL](https://www.openal.org/) for more information and documentation.
- Checkout [NVorbis](https://github.com/NVorbis/NVorbis) for dealing with **.ogg** data.
- Checkout [MP3Sharp](https://github.com/ZaneDubya/MP3Sharp) for dealing with **.mp3** data.

<h2 style="font-weight:bold" align="center">Licensing And Governance</h2>

**CASL** is distributed under the very permissive MIT license and all dependencies are distributed under MIT-compatible licenses.
This project has adopted the code of conduct defined by the [Contributor Covenant](http://contributor-covenant.org/) to clarify expected behavior in our community.
