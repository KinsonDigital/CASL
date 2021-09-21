<h1 style="font-weight:bold" align="center">Linux Development Environment Setup</h1>

Setting up the development environment is not as easy with linux systems as it is for windows.  There are many different ways to go about this and I am not an expert on this.  The instructions below are how I went about setting up **Ubuntu** as a development environment so I could do testing on a linux machine.  Recommendations on how to improve are welcome!!

---

### Software Used

1. [VirtualBox](https://www.virtualbox.org/) - Virtual Machine Software
2. [Ubuntu](https://ubuntu.com/) - Linux Operating System
3. [Visual Studio Code](https://code.visualstudio.com/) - Lightweight code editor
4. [OpenAL Package](https://launchpad.net/ubuntu/+source/openal-soft) - Software implementation of **OpenAL** library
5. [GitKraken](https://www.gitkraken.com/) - GIT repository management client
6. [GIT](https://git-scm.com/)

### Setup

When setting up a linux environment, there are some options.  At first I started using **WSL2** but ran into issues when I actually wanted to run the **CASLTesting** project so I could actually hear sound being played.  This did not work because **WSL2** does not have access to the sound hardware.  There are ways to go about this but my investigations and attempts failed.

This is why I decided to just install **VirtualBox** so I could run the the linux **Ubuntu** OS which would give me the sound hardware and a more productive development experience for running and testing the library.  You can absolutely use whatever you want such as **HyperV**, **WSL2** (If you can get sound working), or even other supported linux operating systems that work for **NET 5.0**.  You can of course just have a real installation on another machine of the required linux OS.

This is what I did to set everything up.

1. Goto [VirtualBox](https://www.virtualbox.org/wiki/Download_Old_Builds_6_0) and choose the appropriate version of **VirtualBox** and install it on your windows machine.
2. Go [here](https://brb.nci.nih.gov/seqtools/installUbuntu.html) for instructions on how to create a Ubuntu virtual machine
   * Make sure that you follow all of the instructions and don't skip the **Guest Additions** setup
3. Once you have the **Ubuntu** virtual machine up and running, open the terminal window and enter the commands below.  This will update any current packages and also install the **OpenAL** library
   * `sudo apt-get update`
   * `sudo apt-get install libopenal1`
5. Install the **.NET 5.0 SDK** by following the instructions [here](https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu#2004-)
   * For **Ubuntu 20.04** and many other versions, the order of commands are:
     * `wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb`
     * `sudo dpkg -i packages-microsoft-prod.deb`
     * `sudo apt-get update`
     * `sudo apt-get install -y apt-transport-https`
     * `sudo apt-get update`
     * `sudo apt-get install -y dotnet-sdk-5.0`
   * After the above commands are finished, you should be able to enter the command `dotnet` and get appropriate output verifying that dotnet has been installed
6. Install **GIT** by entering the command below
   * `sudo apt-get install git`


<div align="right">

   [< Windows Dev Env Setup](./WindowsDevEnvSetup.md) | [Branching >](./../Branching.md)
   <br/>
</div>