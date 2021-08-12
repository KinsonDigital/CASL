#THIS IS USED FOR TESTING THE PACKAGE SCRIPT

Clear-Host

[string]$workspace = "K:\SOFTWARE DEVELOPMENT\PERSONAL\CASL";

.\BuildScripts\PackageTestingApp.ps1 -Workspace $workspace `
    -ProjectName "CASL" `
    -ReleaseName "Production" `
    -BuildConfig "Release" `
    -Version "1.2.3-preview.4"
