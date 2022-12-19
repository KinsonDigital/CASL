<h1 align="center" style='color:mediumseagreen;font-weight:bold'>
    CASL Preview Release Notes - v1.0.0-preview.11
</h1>

<h2 align="center" style='font-weight:bold'>Quick Reminder</h2>

<div align="center">

As with all software, there is always a chance for issues and bugs, especially for preview releases, which is why your input is greatly appreciated. 🙏🏼
</div>

---

<h2 style="font-weight:bold" align="center">New Features ✨</h2>

1. [#123](https://github.com/KinsonDigital/CASL/issues/123) - Changed the library to use cross-platform paths when dealing with files and directories.

---

<h2 style="font-weight:bold" align="center">Breaking Changes 🧨</h2>

1. [#115](https://github.com/KinsonDigital/CASL/issues/115) - Changed the `Sound` class to `sealed`.
   > **💡** This prevents anybody from inheriting from the class.



---

<h2 style="font-weight:bold" align="center">Internal Changes ⚙️</h2>
<h5 align="center">(Changes that do not affect users.  Not breaking changes, new features, or bug fixes.)</h5>

1. [#116](https://github.com/KinsonDigital/CASL/issues/116) - Updated the project dotnet and language versions.
   > **💡** Things comes with lots of performance improvements due to dotnet _**v7.0**_
   - Updated dotnet from version _**v5.0**_ to _**v7.0**_
   - Updated C# language version from version _**v9.0**_ to _**v11.0**_
2. [#115](https://github.com/KinsonDigital/CASL/issues/115) - Updated the build system from standard workflows to the [KinsonDigital.CICD](https://kinsondigital.cicd) build system.
---

<h2 style="font-weight:bold" align="center">Nuget/Library Updates 📦</h2>

1. [#117](https://github.com/KinsonDigital/CASL/issues/117) - Updated the following NuGet packages.
   - Updated the NuGet package **System.IO.Abstractions** from version _**v13.2.33**_ to _**v19.1.5**_
   - Updated the NuGet package **Microsoft.CodeAnalysis.NetAnalyzers** from version _**v5.0.3**_ to _**v7.0.0**_

---

<h2 style="font-weight:bold" align="center">Other 🪧</h2>
<h5 align="center">(Includes anything that does not fit into the categories above)</h5>

1. [#113](https://github.com/KinsonDigital/CASL/issues/113) - Made grammar changes to the project readme file.
2. [#85](https://github.com/KinsonDigital/CASL/issues/85) - Made grammar changes to the project readme file.
3. [#100](https://github.com/KinsonDigital/CASL/issues/100) - Removed project-specific issues and pull request templates.
   > **💡** These are not maintained at the organizational level.
4. [#88](https://github.com/KinsonDigital/CASL/issues/88) - Added release note template files to the project for preview and production releases.
5. [#86](https://github.com/KinsonDigital/CASL/issues/86) - Updated the project's code of conduct.
6. [#83](https://github.com/KinsonDigital/CASL/issues/83) - Updated the branching documentation.
7. [#64](https://github.com/KinsonDigital/CASL/issues/64) - Created a backers file.
