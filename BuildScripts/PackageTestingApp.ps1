param([string]$Workspace, [string]$ProjectName, [string]$ReleaseName, [string]$Version);

[string]$srcBaseDirPath = "$($Workspace)/$($ProjectName)/OpenAL/libs/";
[string]$destBaseDirPath = "$($Workspace)/Testing/ProductionRelease/";
[string]$winOpenALLibName = "soft_oal.dll";

# Create required runtimes directories
Write-Host "Copying native libraries . . .";
New-Item -Path "$($destBaseDirPath)/runtimes/win-x64/native/" -ItemType "directory" -Force;
New-Item -Path "$($destBaseDirPath)/runtimes/win-x86/native/" -ItemType "directory" -Force;
New-Item -Path "$($destBaseDirPath)/runtimes/linux-x64/native/" -ItemType "directory" -Force;

# Copy proper native libraries to the proper runtime folders
Copy-Item -Path "$($srcBaseDirPath)win-x64/$($winOpenALLibName)" -Destination "$($destBaseDirPath)/runtimes/win-x64/native/" -Recurse -Force;
Copy-Item -Path "$($srcBaseDirPath)win-x86/$($winOpenALLibName)" -Destination "$($destBaseDirPath)/runtimes/win-x86/native/" -Recurse -Force;
Copy-Item -Path "$($srcBaseDirPath)linux-x64/libopenal.so.1" -Destination "$($destBaseDirPath)/runtimes/linux-x64/native/" -Recurse -Force;

# Publish Testing Application
dotnet.exe publish "$($Workspace)/Testing/$($ProjectName)Testing/$($ProjectName)Testing.csproj" -c Debug -o "$($Workspace)/Testing/$($ReleaseName)Release/" --no-restore;

[string]$testingAppPackagePath = "$($Workspace)/$($ProjectName)-TestingApp-$($Version).zip";

# Package Testing Application Into Zip
Write-Host "Packaging Testing Application . . .";
Compress-Archive -Path $destBaseDirPath -DestinationPath $testingAppPackagePath -Force;

Write-Host "\nTesting Application Path: $($testingAppPackagePath)";
