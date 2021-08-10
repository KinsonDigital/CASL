[string]$workspace = "K:\SOFTWARE DEVELOPMENT\PERSONAL\CASL";

.\BuildScripts\PackageTestingApp.ps1 -Workspace $workspace `
    -ProjectName "CASL" `
    -ReleaseName "Production" `
    -Version "1.0.0-preview.6"
