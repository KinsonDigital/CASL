# Build Lib Instructions

The only time that building the OpenAL library is necessary is for Linux.
The Windows libraries can be download already prebuilt, so building is not an issue
for this operating system.

For Linux, the library binaries are not available so it must be built from source.

This guide was built from resources listed at the bottom of this document as well as
actually a bunch of research, debugging, and running the CASL application to figure out
the missing pieces to getting this working.

This was proven out with on an actual Linux (Ubuntu) machine, but this could probably be done on
Windows using WSL2.  This has not been attempted yet.

1. Perform all of the following steps on Ubuntu.
   - Try this out on Windows WSL2 if you want.
2. Update Ubuntu system just in case. It is probably not necessary but a good idea.
   ```
   sudo apt update
   sudo apt upgrade
   ```
3. Install GIT if it is not already installed.
   ```
   sudo apt install git -y
   ```
4. Install dependencies for building the library.
   ```
   sudo apt install build-essential cmake libpipewire-0.3-dev libasound2-dev
   ```
   > NOTE: The instructions on the GitHub readme does not do this,
   > but this worked for me so we are going with it.
5. Download the OpenAL Soft source code from the [GitHub repository](https://github.com/kcat/openal-soft.git).
   ```
   git clone https://github.com/kcat/openal-soft.git
   ```
6. Change directory into the open source project folder.
    ```
    cd openal-soft/
    ```
7. Change directory into the `build` directory.
   ```
   cd build/
   ```
8. Execute the following cmake command.
   ```
   cmake ..
   ```
9. Build the library.
   ```
   make
   ```

Once the build is complete, the `build` directory that you are currently in will contain
multiple versions of the library.  Each file will be suffixed with the text `libopenal` and then
are contain a version number at the end.

You want to use the library that is the latest version.
The last time I built the library, I had the following lib files:
1. libopenal.so
2. libopenal.so.1
3. libopenal.so.1.24.2


## Resources
1. [OpenAL Soft](https://github.com/kcat/openal-soft.git)
2. [OpenAL](https://openal.org)
3. [OpenAL Soft GitHub Project](https://github.com/kcat/openal-soft)
